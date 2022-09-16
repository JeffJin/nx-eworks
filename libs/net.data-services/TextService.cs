using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_common.Services
{
    public class TextService: ITextService
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private const int NumberOfRetries = 3;
        private const int DelayOnRetry = 1000;

        public TextService(IConfiguration config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        private string GetStatusFolder(string serialNumber)
        {
            var path = Path.Combine(_config["BaseAssetFolder"], "device_statuses", serialNumber);

            return path;
        }

        private string GetDeviceStatusFilePath(string serialNumber)
        {
            var fileName = DateTimeOffset.UtcNow.ToString("yyyy-MM") + ".log";
            var path = GetStatusFolder(serialNumber);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Path.Combine(path, fileName);
        }

        public void LogDeviceStatus(DeviceStatusDto statusDto)
        {
            var filePath = GetDeviceStatusFilePath(statusDto.SerialNumber);

            ArchiveDeviceStatuses(statusDto.SerialNumber);

            WriteToFile(filePath, statusDto.SerialNumber, statusDto.Status, statusDto.CreatedOn.ToString(), statusDto.CreatedBy, statusDto.ReceivedOn.ToString());
        }

        public void WriteToFile(string filePath, string content)
        {
            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(content);
            }
        }

        public void WriteToFile(string filePath, params string[] contents)
        {
            using (StreamWriter sw = File.AppendText(filePath))
            {
                foreach (var content in contents)
                {
                    sw.Write(content + ", ");
                }
                sw.WriteLine();
            }
        }

        public string[] LoadContents(string serialNumber)
        {
            var filePath = GetDeviceStatusFilePath(serialNumber);

            return File.ReadAllLines(filePath);
        }

        public string LoadLastLine(string serialNumber)
        {
            var filePath = GetDeviceStatusFilePath(serialNumber);
            return File.ReadLines(filePath).Last();
        }

        public void Archive(string path, string dest)
        {
            //TODO
            //create snapshot of the heartbeat history and archive the old files.
            _logger.Warning("File {filePath} exceeded max configured size", path);
            _logger.Warning("Archiving File {filePath}", path);

            for (int i=1; i <= NumberOfRetries; ++i) {
                try {
                    if (File.Exists(dest))
                    {
                        File.Delete(dest);
                    }

                    ZipFile.CreateFromDirectory(path, dest);
                    break; // When done we can break loop
                }
                catch (IOException e) when (i <= NumberOfRetries) {
                    // You may check error code to filter some exceptions, not every error
                    // can be recovered.
                    _logger.Error(e, "Archiving file error, {path}, {dest}", path, dest);
                    Thread.Sleep(DelayOnRetry);
                }
            }


            if (!File.Exists(dest))
            {
                _logger.Warning("Failed to archive file {path}", path);
            }

            //remove all old files except todays log file
            foreach (var file in Directory.GetFiles(path))
            {
                if (!file.EndsWith(DateTimeOffset.UtcNow.ToString("yyyy-MM") + ".log"))
                {
                    File.Delete(file);
                }
            }
        }

        public void ArchiveDeviceStatuses(string serialNumber)
        {
            var sourceFolder = GetStatusFolder(serialNumber);
            long maxSize = Convert.ToInt64(_config["MaxFileSize"]);

            if (GetDirectorySize(sourceFolder) >= maxSize)
            {
                var destFolder = Path.Combine(_config["BaseAssetFolder"], "device_statuses", "archives", serialNumber);
                if (!Directory.Exists(destFolder))
                {
                    Directory.CreateDirectory(destFolder);
                }
                var destPath = Path.Combine(destFolder, DateTimeOffset.UtcNow.ToString("yyyy-MM-dd-hh-m-ss") + ".zip");
                Archive(sourceFolder, destPath);
            }
        }

        private long GetDirectorySize(string p)
        {
            // 1.
            // Get array of all file names.
            string[] a = Directory.GetFiles(p, "*.*");

            // 2.
            // Calculate total bytes of all files in a loop.
            long b = 0;
            foreach (string name in a)
            {
                // 3.
                // Use FileInfo to get length of each file.
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }
            // 4.
            // Return total size
            return b;
        }
    }
}
