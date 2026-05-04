using Common.Contracts;
using Common.Faults;
using Common.Models;
using System;
using System.ServiceModel;

namespace WCFServer.Services
{
    public class ConsumptionService : IConsumptionService
    {
        private SessionMeta _trenutnaSesija;

        public void StartSession(SessionMeta meta)
        {
            _trenutnaSesija = meta;
            Console.WriteLine("[SERVER] Sesija zapoceta: " + meta.CountryCode + ", " + meta.YearMonth + ", ukupno dana: " + meta.TotalDays + ", fajl: " + meta.SourceFileName);
        }

        public void PushSample(DailyConsumptionSample uzorak)
        {
            ValidirajUzorak(uzorak);
            Console.WriteLine("[SERVER] Primljeno [" + uzorak.RowIndex + "]: " + uzorak.Date.ToString("yyyy-MM-dd") + " | Actual: " + uzorak.TotalActualMWh.ToString("F2") + " MWh | Forecast: " + uzorak.TotalForecastMWh.ToString("F2") + " MWh");
        }

        public void EndSession()
        {
            Console.WriteLine("[SERVER] Sesija zavrsena za: " + _trenutnaSesija?.CountryCode);
            _trenutnaSesija = null;
        }

        private void ValidirajUzorak(DailyConsumptionSample uzorak)
        {
            if (double.IsNaN(uzorak.TotalActualMWh) || double.IsNaN(uzorak.TotalForecastMWh))
            {
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault { Message = "NaN vrednost u uzorku za datum " + uzorak.Date.ToString("yyyy-MM-dd") + "." });
            }

            if (uzorak.TotalActualMWh < 0)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Message = "TotalActualMWh ne sme biti negativno za datum " + uzorak.Date.ToString("yyyy-MM-dd") + "." });
            }

            if (uzorak.TotalForecastMWh < 0)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Message = "TotalForecastMWh ne sme biti negativno za datum " + uzorak.Date.ToString("yyyy-MM-dd") + "." });
            }

            if (uzorak.PeakTime.Date != uzorak.Date.Date)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Message = "PeakTime (" + uzorak.PeakTime.ToString("yyyy-MM-dd") + ") nije unutar dana " + uzorak.Date.ToString("yyyy-MM-dd") + "." });
            }
        }
    }
}
