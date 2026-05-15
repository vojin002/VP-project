using Common.Contracts;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCFClient.Proxies
{
    public class ConsumptionProxy : IDisposable
    {
        private IConsumptionService client;
        private ICommunicationObject communicationObj;
        private bool _disposed = false;

        private int _pushedNumber = 0;
        private readonly int CONNECTION_BREAK_TARGET;

        public ConsumptionProxy()
        {
            ChannelFactory<IConsumptionService> factory = new ChannelFactory<IConsumptionService>("WCFClient");
            client = factory.CreateChannel();
            communicationObj = (ICommunicationObject)client;
            CONNECTION_BREAK_TARGET = new Random().Next(1, 3000);
        }

        public void StartSession(SessionMeta meta)
        {
            client.StartSession(meta);
        }

        public void PushSample(DailyConsumptionSample sample)
        {
            if(_pushedNumber == CONNECTION_BREAK_TARGET)
            {
                throw new CommunicationException("Simulated connection break after " + _pushedNumber + " pushed samples");
            }
            client.PushSample(sample);
            _pushedNumber++;
        }

        public void EndSession()
        {
            client.EndSession();
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                if (communicationObj != null)
                {
                    if (communicationObj.State == CommunicationState.Faulted)
                    {
                        communicationObj.Abort();
                    }
                    else
                    {
                        communicationObj.Close();
                    }
                }
            }
            catch
            {
                communicationObj.Abort();
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
