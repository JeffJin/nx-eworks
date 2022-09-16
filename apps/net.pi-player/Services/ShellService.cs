using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.pi_player.Services
{
    public class ShellService: IShellService
    {
        private readonly ILogger _logger;

        public ShellService(ILogger logger)
        {
            _logger = logger;
        }

        public void Command(string cmd, int timeout = 3000)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "/bin/bash";
                    process.StartInfo.Arguments = $"-c \"{escapedArgs} >/dev/null 2>&1\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    if (process.WaitForExit(timeout))
                    {
                        _logger.Information("Process exit code: {0}", process.ExitCode);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Execute command {cmd} failed", cmd);
            }

        }

        public string Bash(string cmd, int timeout = 3000)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            long peakWorkingSet = 0,
                peakVirtualMem = 0;
            Process process = null;
            string result = null;
            try
            {
                process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{escapedArgs}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();

                do
                {
                    if (!process.HasExited)
                    {
                        // Refresh the current process property values.
                        process.Refresh();
                        // Display current process statistics.
                        try
                        {
                            _logger.Information("{0} -", process.ToString());
                            _logger.Information("  physical memory usage: {0}",
                                process.WorkingSet64);
                            _logger.Information("  user processor time: {0}",
                                process.UserProcessorTime);
                            _logger.Information("  total processor time: {0}",
                                process.TotalProcessorTime);
                            // Update the values for the overall peak memory statistics.
                            peakVirtualMem = process.PeakVirtualMemorySize64;
                            peakWorkingSet = process.PeakWorkingSet64;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Reading process statistics failed, {cmd}", cmd);
                        }
                    }

                    result += process.StandardOutput.ReadToEnd();

                } while (!process.WaitForExit(timeout));

                _logger.Information("Process exit code: {0}",
                    process.ExitCode);

                // Display peak memory statistics for the process.
                _logger.Information("Peak physical memory usage of the process: {0}",
                    peakWorkingSet);
                _logger.Information("Peak virtual memory usage of the process: {0}",
                    peakVirtualMem);
            }
            finally
            {
                try
                {
                    if (process != null)
                    {
                        process.Close();
                    }
                }
                catch
                {
                }
            }

            return result;
        }

        public string StopVideoPlayer()
        {
            try
            {
                _logger.Information("stop omx player");
                var output = Bash("killall omxplayer.bin");
                _logger.Information(output);
                return output;
            }
            catch (Exception e)
            {
                _logger.Information("Failed to stop omxplayer", e);
            }

            return null;
        }

        public string PlayVideo(string path, int x, int y, int w, int h, int startTime = 0, int duration = 0)
        {
            _logger.Information("Start playing video {path}, {x}, {y}, {w}, {h}, {t}, {d}",  path, x, y, w, h, startTime, duration);
            try
            {
                PlayLocalVideo(path, duration);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to play videlo {path}", path);
            }
            return string.Empty;
        }

        public string PlayAudio(string url, int startTime = 0)
        {
            _logger.Information("Start playing audio " + url);
            try
            {
                return string.Empty;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to play audio {path}", url);
            }
            return string.Empty;
        }

        public string ShowImage(string url, int x, int y, int w, int h)
        {
            _logger.Information("Start displaying image {url}, {x}, {y}, {w}, {h}",  url, x, y, w, h);
            try
            {
                return string.Empty;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to show image {path}", url);
            }
            return string.Empty;
        }

        private string PlayRemoteVideo(string url)
        {
            try
            {
                _logger.Information("start playing remote video", url);
                var output = Bash(string.Format("omxplayer -r -o both {0}", url));
                _logger.Information(output);
                return output;
            }
            catch (Exception e)
            {
                _logger.Information("Failed to play remote video", e);
            }

            return null;
        }

        private void PlayLocalVideo(string filePath, int timeout)
        {
            try
            {
                _logger.Information("-------------------------------------------------------");
                string cmd = string.Format("omxplayer -r -o both {0}", filePath);
                _logger.Information(cmd);
                _logger.Information("start playing local videoat {time}", DateTime.Now.ToString("h:mm:ss tt zz"));
                Command(cmd, timeout);
                _logger.Information("finish playing local video at {time}", DateTime.Now.ToString("h:mm:ss tt zz"));
                _logger.Information("-------------------------------------------------------");
            }
            catch (Exception e)
            {
                _logger.Information("Failed to play local video", e);
            }
            finally
            {
                StopVideoPlayer();
            }
        }

        private string PlayYoutubeVideo(string youtubeUrl)
        {
            try
            {
                _logger.Information("start playing youtube video", youtubeUrl);
                string youtubeHash = Bash(string.Format("youtube-dl -g '{0}'", youtubeUrl));
                var output = Bash(string.Format("omxplayer -r -o both '{0}'", youtubeHash));
                _logger.Information(output);
                return output;
            }
            catch (Exception e)
            {
                _logger.Information("Failed to play youtube video", e);
            }

            return null;
        }
    }
}
