using Common.Contracts;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly int[] CONNECTION_BREAK_TARGET_SCOPE = new int[] { int.MaxValue-1, int.MaxValue };

        public ConsumptionProxy()
        {
            ChannelFactory<IConsumptionService> factory = new ChannelFactory<IConsumptionService>("WCFClient");
            client = factory.CreateChannel();
            communicationObj = (ICommunicationObject)client;
            CONNECTION_BREAK_TARGET = new Random().Next(CONNECTION_BREAK_TARGET_SCOPE[0], CONNECTION_BREAK_TARGET_SCOPE[1]);
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

            Stopwatch watch = Stopwatch.StartNew();
            client.PushSample(sample);
            watch.Stop();
            Console.WriteLine($"Sample {sample.Date:yyyy-MM-dd},{sample.CountryCode} sent, Latency: {watch.ElapsedMilliseconds} ms");
            _pushedNumber++;
        }

        public void EndSession()
        {
            client.EndSession();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if(disposing)
            {
                try
                {
                    if(communicationObj != null)
                    {
                        if (communicationObj.State == CommunicationState.Faulted)
                        {
                            communicationObj.Abort();
                            Console.WriteLine("ConsumptionProxy aborted connection succesfully after it was in a Faulted state");
                        }
                        else
                        {
                            communicationObj.Close();
                            Console.WriteLine("ConsumptionProxy closed successfully");
                        }
                    }
                }
                catch
                {
                    communicationObj.Abort();
                    Console.WriteLine("ConsumptionProxy threw an Error: Communication aborted successfully");
                }
            }

            _disposed = true;
        }

        ~ConsumptionProxy()
        {
            Dispose(false);
        }
    }
}
