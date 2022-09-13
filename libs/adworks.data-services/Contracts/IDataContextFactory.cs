using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.data_services
{
    public interface IDataContextFactory
    {
        CommonDbContext Create();
    }
}
