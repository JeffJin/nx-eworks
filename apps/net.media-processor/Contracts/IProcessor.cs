using System;

namespace adworks.media_processor
{
    public interface IProcessor: IDisposable
    {
        void Run();
    }
}