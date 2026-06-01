using Common.Contracts;
using Common.Enums;
using Common.Events;
using Common.Faults;
using Common.Models;
using System;
using System.ServiceModel;

namespace WCFServer.Services
{
    public delegate void CustomEventHandler<CustomEventArgs>(object sender, CustomEventArgs args) where CustomEventArgs : EventArgs;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ConsumptionService : IConsumptionService
    {
        private SessionMeta _currentSession;

        public event CustomEventHandler<TransferStartedEventArgs> OnTransferStarted;
        public event CustomEventHandler<SampleReceivedEventArgs> OnSampleReceived;
        public event CustomEventHandler<WarningRaisedEventArgs> OnWarningRaised;
        public event CustomEventHandler<TransferCompletedEventArgs> OnTransferCompleted;

        public void StartSession(SessionMeta meta)
        {
            _currentSession = meta;
            OnTransferStarted?.Invoke(this, new TransferStartedEventArgs(meta));
        }

        public void PushSample(DailyConsumptionSample sample)
        {
            Console.WriteLine($"Transfering... {sample.Date}");
            ValidateSample(sample);
            OnSampleReceived?.Invoke(this, new SampleReceivedEventArgs(sample, ReceivedSampleState.Valid, ""));
            //Console.WriteLine("SERVER: Received [" + sample.RowIndex + "]: " + sample.Date.ToString("yyyy-MM-dd") + " | Actual: " + sample.TotalActualMWh.ToString("F2") + " MWh | Forecast: " + sample.TotalForecastMWh.ToString("F2") + " MWh");
        }

        public void EndSession()
        {
            OnTransferCompleted?.Invoke(this, new TransferCompletedEventArgs(_currentSession));
            Console.WriteLine("Transfer completed");
        }

        private void ValidateSample(DailyConsumptionSample sample)
        {
            if (double.IsNaN(sample.TotalActualMWh) || double.IsNaN(sample.TotalForecastMWh))
            {
                OnSampleReceived?.Invoke(this, new SampleReceivedEventArgs(sample, ReceivedSampleState.Rejected, "NaN value for date"));
                throw new FaultException<DataFormatFault>( new DataFormatFault { Message = "NaN value in sample for date " + sample.Date.ToString("yyyy-MM-dd")});
            }

            if (sample.TotalActualMWh < 0)
            {
                OnSampleReceived?.Invoke(this, new SampleReceivedEventArgs(sample, ReceivedSampleState.Rejected, "Negative TotalAcutalMWh"));
                throw new FaultException<ValidationFault>(new ValidationFault { Message = "TotalActualMWh cannot be negative for date " + sample.Date.ToString("yyyy-MM-dd") });
            }

            if (sample.TotalForecastMWh < 0)
            {
                OnSampleReceived?.Invoke(this, new SampleReceivedEventArgs(sample, ReceivedSampleState.Rejected, "Negative TotalForecastMWh"));
                throw new FaultException<ValidationFault>( new ValidationFault { Message = "TotalForecastMWh cannot be negative for " + sample.Date.ToString("yyyy-MM-dd")});
            }

            if (sample.PeakTime.Date != sample.Date.Date)
            {
                OnSampleReceived?.Invoke(this, new SampleReceivedEventArgs(sample, ReceivedSampleState.Rejected, "Wrong day for PeakTime"));
                throw new FaultException<ValidationFault>( new ValidationFault { Message = "PeakTime (" + sample.PeakTime.ToString("yyyy-MM-dd") + ") is not inside day " + sample.Date.ToString("yyyy-MM-dd")});
            }
        }
    }
}
