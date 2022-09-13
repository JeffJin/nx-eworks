namespace adworks.media_common.Services
{
    public interface ITextService
    {
        void LogDeviceStatus(DeviceStatusDto statusDto);
        void WriteToFile(string filePath, string content);
        void WriteToFile(string filePath, params string[] contents);
        string[] LoadContents(string serialNumber);
        string LoadLastLine(string serialNumber);
        void Archive(string path, string dest);
        void ArchiveDeviceStatuses(string serialNumber);
    }
}