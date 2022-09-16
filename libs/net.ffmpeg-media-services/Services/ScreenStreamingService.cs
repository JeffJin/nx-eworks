using adworks.media_common;
using NReco.VideoConverter;

namespace adworks.ffmpeg_media_services
{
    public class ScreenStreamingService : IStreamingService
    {
        private readonly FFMpegConverter _ffMpegConverter;
        private bool isStreamingStarted;
        public ScreenStreamingService()
        {
            _ffMpegConverter = new FFMpegConverter();
        }

        public void StopStreamingScreenCapture()
        {
            if (_ffMpegConverter == null || !this.isStreamingStarted)
            {
                return;
            }

            if (!_ffMpegConverter.Stop())
            { // always check Stop result! process should be aborted if sending Ctrl+C failed for some reason
                _ffMpegConverter.Abort(); // kill process. output file may be corrupted
            }
            this.isStreamingStarted = false;
        }

        public void StartStreamingScreenCapture(string outputPath)
        {
            this._ffMpegConverter.ConvertMedia(
                "desktop", // whole screen. it is possible to capture specific window by title (title=window_title)
                "gdigrab", // special GDI-based capture device, see https://www.ffmpeg.org/ffmpeg-devices.html#gdigrab
                outputPath, // output file. As alternative video may be streamed to live streaming server.
                null,
                new ConvertSettings()
                {
                    MaxDuration = 6000, // you may explicitly specify recording duration
                    VideoFrameSize = FrameSize.hd720,  // if not set, original screen resolution is used
                    CustomInputArgs = " -draw_mouse 1 "  // enable (1) or disable (0) mouse pointer in recording
                    // see more options at https://www.ffmpeg.org/ffmpeg-devices.html#gdigrab
                    // framerate, show_region, offset_x, offset_y
                }
            );
            this.isStreamingStarted = true;
        }
    }
}
