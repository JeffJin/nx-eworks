using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace adworks.message_bus
{

    internal class RabbitChannelSubscription : ISubscription
    {

        private class TeardownSubscription : ISubscription
        {
            private readonly Action _teardownLogic;

            internal TeardownSubscription(Action teardownLogic)
            {
                _teardownLogic = teardownLogic;
            }
            
            public void Dispose()
            {
                Unsubscribe();
            }

            public bool Closed { get; private set; } = false;

            public void AddTearDown(Action teardownLogic)
            {
                throw new NotImplementedException();
            }

            public void AddTearDown(ISubscription subscription)
            {
                throw new NotImplementedException();
            }

            public void Remove(ISubscription subscription)
            {
                throw new NotImplementedException();
            }

            public void Unsubscribe()
            {
                _teardownLogic();
                Closed = true;
            }
        }
        
        private readonly IModel _channel;
        private readonly IList<ISubscription> _teardownLogicSubscriptions;

        internal RabbitChannelSubscription(IModel channel) {
            _channel = channel;
            _teardownLogicSubscriptions = new List<ISubscription>();
        }

        public bool Closed { get; private set; } = false;

        public void AddTearDown(Action teardownLogic)
        {
            _teardownLogicSubscriptions.Add(new TeardownSubscription(teardownLogic));
        }

        public void AddTearDown(ISubscription subscription)
        {
            _teardownLogicSubscriptions.Add(subscription);
        }

        public void Remove(ISubscription subscription)
        {
            _teardownLogicSubscriptions.Remove(subscription);
        }

        public void Unsubscribe()
        {
            if (_channel.IsOpen)
            {
                _channel.Close();
            }
        }
        
        public void Dispose()
        {
            Unsubscribe();
            Closed = true;
        }
    }

}