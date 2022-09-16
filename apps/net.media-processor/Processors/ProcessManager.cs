using System;
using System.Linq;
using System.Text;

namespace MediaProcessor
{

    /// <summary>
    /// Process manager will coordinate between different tasks
    /// 1. Check message queue to find if there is any new file to process
    /// 2. Check for failed tasks and 
    /// </summary>
    public class ProcessManager: IProcessManager
    {
        public void Dispose()
        {
        }

        public void Run()
        {
            throw new NotImplementedException();
        }
    }
}
