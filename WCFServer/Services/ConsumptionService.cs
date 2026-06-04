using Common.Contracts;
using Common.Enums;
using Common.Events;
using Common.Faults;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;

namespace WCFServer.Services
{
    public delegate void CustomEventHandler<CustomEventArgs>(object sender, CustomEventArgs args) where CustomEventArgs : EventArgs;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ConsumptionService : IConsumptionService
    {
        private SessionMeta _currentSession;

        private int _sampleCount;
        private double _actualMeanSum;
        private List<double> _recentActual;

        public event CustomEventHandler<TransferStartedEventArgs> OnTransferStarted;
        public event CustomEventHandler<SampleReceivedEventArgs> OnSampleReceived;
        public event CustomEventHandler<WarningRaisedEventArgs> OnWarningRaised;
        public event CustomEventHandler<TransferCompletedEventArgs> OnTransferCompleted;

        private readonly double _forecastDeviationPct;
        private readonly double _outOfBandPct;
        private readonly int _riseWindowDays;
        private readonly double _riseThresholdMWh;

        public ConsumptionService()
        {
            _forecastDeviationPct = double.Parse(ConfigurationManager.AppSettings["ForecastDeviationPct"]);
            _outOfBandPct = double.Parse(ConfigurationManager.AppSettings["OutOfBandPct"]);
            _riseWindowDays = int.Parse(ConfigurationManager.AppSettings["RiseWindowDays"]);
            _riseThresholdMWh = double.Parse(ConfigurationManager.AppSettings["RiseThresholdMWh"]);
            _recentActual = new List<double>();
        }

        public void StartSession(SessionMeta meta)
        {
            _currentSession = meta;
            _sampleCount = 0;
            _actualMeanSum = 0;
            _recentActual = new List<double>();
            OnTransferStarted?.Invoke(this, new TransferStartedEventArgs(meta));
        }

        public void PushSample(DailyConsumptionSample sample)
        {
            Console.WriteLine("SERVER: Transfering... " + sample.Date.ToString("yyyy-MM-dd"));
            ValidateSample(sample);
            OnSampleReceived?.Invoke(this, new SampleReceivedEventArgs(sample, ReceivedSampleState.Valid, _currentSession, ""));
            CheckAnalytics(sample);
        }

        public void EndSession()
        {
            OnTransferCompleted?.Invoke(this, new TransferCompletedEventArgs(_currentSession));
            Console.WriteLine("SERVER: Transfer completed");
        }

        private void ValidateSample(DailyConsumptionSample sample)
        {
            if (double.IsNaN(sample.TotalActualMWh) || double.IsNaN(sample.TotalForecastMWh))
            {
                ReportSampleValidationFailed(sample, "NaN value for date");
                throw new FaultException<DataFormatFault>(new DataFormatFault { Message = "NaN value in sample for date " + sample.Date.ToString("yyyy-MM-dd") });
            }

            if (sample.TotalActualMWh < 0)
            {
                ReportSampleValidationFailed(sample, "Negative TotalActualMWh");
                throw new FaultException<ValidationFault>(new ValidationFault { Message = "TotalActualMWh cannot be negative for date " + sample.Date.ToString("yyyy-MM-dd") });
            }

            if (sample.TotalForecastMWh < 0)
            {
                ReportSampleValidationFailed(sample, "Negative TotalForecastMWh");
                throw new FaultException<ValidationFault>(new ValidationFault { Message = "TotalForecastMWh cannot be negative for " + sample.Date.ToString("yyyy-MM-dd") });
            }

            if (sample.PeakTime.Date != sample.Date.Date)
            {
                ReportSampleValidationFailed(sample, "Wrong day for PeakTime");
                throw new FaultException<ValidationFault>(new ValidationFault { Message = "PeakTime (" + sample.PeakTime.ToString("yyyy-MM-dd") + ") is not inside day " + sample.Date.ToString("yyyy-MM-dd") });
            }
        }

        private void ReportSampleValidationFailed(DailyConsumptionSample sample, string reason)
        {
            OnSampleReceived?.Invoke(this, new SampleReceivedEventArgs(sample, ReceivedSampleState.Rejected, _currentSession, reason));
        }

        private void CheckAnalytics(DailyConsumptionSample sample)
        {
            if (sample.TotalForecastMWh > 0)
            {
                double deviationPct = Math.Abs(sample.TotalActualMWh - sample.TotalForecastMWh) / sample.TotalForecastMWh * 100.0;
                if (deviationPct > _forecastDeviationPct)
                {
                    OnWarningRaised?.Invoke(this, new WarningRaisedEventArgs(WarningType.ForecastDeviationWarning, sample, deviationPct));
                }
            }

            if (_sampleCount > 0)
            {
                double actualMean = _actualMeanSum / _sampleCount;
                double lower = (1.0 - _outOfBandPct / 100.0) * actualMean;
                double upper = (1.0 + _outOfBandPct / 100.0) * actualMean;

                if (sample.TotalActualMWh < lower || sample.TotalActualMWh > upper)
                {
                    OnWarningRaised?.Invoke(this, new WarningRaisedEventArgs(WarningType.ConsumptionOutOfBandWarning, sample));
                }
            }

            _sampleCount++;
            _actualMeanSum += sample.TotalActualMWh;

            _recentActual.Add(sample.TotalActualMWh);
            if (_recentActual.Count > _riseWindowDays + 1)
                _recentActual.RemoveAt(0);

            if (_recentActual.Count == _riseWindowDays + 1)
            {
                bool rising = true;
                for (int i = 1; i < _recentActual.Count; i++)
                {
                    if (_recentActual[i] - _recentActual[i - 1] <= _riseThresholdMWh)
                    {
                        rising = false;
                        break;
                    }
                }

                if (rising)
                {
                    OnWarningRaised?.Invoke(this, new WarningRaisedEventArgs(WarningType.ConsumptionRiseWarning, sample));
                }
            }
        }
    }
}
