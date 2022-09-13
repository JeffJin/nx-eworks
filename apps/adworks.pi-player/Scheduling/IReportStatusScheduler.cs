using System.Threading.Tasks;

namespace adworks.pi_player.Scheduling
{
    public interface IReportStatusScheduler: IPlayerScheduler
    {
        Task ScheduleStatusReport(string serialNumber = Constants.StatusReportGroupName);
    }
}