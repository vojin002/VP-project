using Common.Contracts;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFServer.Services
{
    public class ConsumptionService : IConsumptionService
    {
        public void StartSession(SessionMeta meta)
        {
            throw new NotImplementedException();
        }

        public void EndSession()
        {
            throw new NotImplementedException();
        }

        public void PushSample(DailyConsumptionSample sample)
        {
            Console.WriteLine(sample.CountryCode);
        }
    }
}
