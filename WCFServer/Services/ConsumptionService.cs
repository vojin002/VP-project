using Common.Contracts;
using Common.Faults;
using Common.Models;
using System;
using System.ServiceModel;

namespace WCFServer.Services
{
    public class ConsumptionService : IConsumptionService
    {

        public void StartSession(SessionMeta meta)
        {
            Console.WriteLine("SERVER: Session started: " + meta.CountryCode + ", " + meta.YearMonth + ", total days: " + meta.TotalDays + ", file: " + meta.SourceFileName);
        }

        public void PushSample(DailyConsumptionSample sample)
        {
            ValidateSample(sample);
            Console.WriteLine("SERVER: Received [" + sample.RowIndex + "]: " + sample.Date.ToString("yyyy-MM-dd") + " | Actual: " + sample.TotalActualMWh.ToString("F2") + " MWh | Forecast: " + sample.TotalForecastMWh.ToString("F2") + " MWh");
        }

        public void EndSession()
        {
            Console.WriteLine("SERVER: Session ended.");
        }

        private void ValidateSample(DailyConsumptionSample sample)
        {
            if (double.IsNaN(sample.TotalActualMWh) || double.IsNaN(sample.TotalForecastMWh))
            {
                throw new FaultException<DataFormatFault>( new DataFormatFault { Message = "NaN value in sample for date " + sample.Date.ToString("yyyy-MM-dd")});
            }

            if (sample.TotalActualMWh < 0)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Message = "TotalActualMWh cannot be negative for date " + sample.Date.ToString("yyyy-MM-dd")});
            }

            if (sample.TotalForecastMWh < 0)
            {
                throw new FaultException<ValidationFault>( new ValidationFault { Message = "TotalForecastMWh cannot be negative for " + sample.Date.ToString("yyyy-MM-dd")});
            }

            if (sample.PeakTime.Date != sample.Date.Date)
            {
                throw new FaultException<ValidationFault>( new ValidationFault { Message = "PeakTime (" + sample.PeakTime.ToString("yyyy-MM-dd") + ") is not inside day " + sample.Date.ToString("yyyy-MM-dd")});
            }
        }
    }
}
