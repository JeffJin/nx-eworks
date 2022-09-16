using System;

namespace adworks.message_bus
{
    public interface ISubscription : IDisposable
    {
        bool Closed { get; }
        
        void AddTearDown(Action teardownLogic);
        void AddTearDown(ISubscription subscription);
        void Remove(ISubscription subscription);
        void Unsubscribe();
    }
}
