using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace adworks.pi_player.Services
{
    public interface IShellService
    {
        void Command(string cmd, int timeout = 500);
        
        string Bash(string cmd, int timeout = 500);

        string StopVideoPlayer();
        
        string PlayVideo(string url, int x, int y, int w, int h, int startTime = 0, int duration = 0);

        string PlayAudio(string url, int startTime = 0);

        string ShowImage(string url, int x, int y, int w, int h);
    }
}