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

        public ConsumptionProxy()
        {
            ChannelFactory<IConsumptionService> factory = new ChannelFactory<IConsumptionService>("WCFClient");
            client = factory.CreateChannel();
            communicationObj = (ICommunicationObject)client;
        }

        public void Send(DailyConsumptionSample sample)
        {
            client.PushSample(sample);
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
