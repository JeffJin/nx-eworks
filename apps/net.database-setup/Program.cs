using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using adworks.data_services;
using adworks.data_services.Identity;
using adworks.media_common;
using adworks.networking;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace database.setup
{
    class Program
    {
        static void Main(string[] args)
        {

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == "Development")
            {
              environment = "dev";
            }
            Console.WriteLine($"Initializing database CommonApp for environment {environment}...");

            var sqlFile = $"dbtest-seed-data-{environment}.sql";
            var workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var sqlPath = Path.Combine(workingDirectory ?? throw new InvalidOperationException(), sqlFile);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: true);
            if (environment == "Development")
            {
              builder
                    .AddJsonFile(
                        Path.Combine(AppContext.BaseDirectory, string.Format("..{0}..{0}..{0}", Path.DirectorySeparatorChar), $"appsettings.{environment}.json"),
                        optional: true
                    );
            }
            else
            {
                builder
                    .AddJsonFile($"appsettings.{environment}.json", optional: false);
            }
            var configuration = builder.Build();

            var logger = new LoggerConfiguration().CreateLogger();
            var sampleImageFolder = Path.Combine(workingDirectory, configuration["Image:SampleFolder"]);
            var sampleVideoFolder = Path.Combine(workingDirectory, configuration["Video:SampleFolder"]);
            var _ftpStreamingAddress = configuration["Ftp:StreamingAddress"];
            var connStr = configuration.GetConnectionString("DefaultConnection");

            if (args.Length >= 2)
            {
                sqlPath = args[1];
            }

            // read the file
            var seedData = File.ReadAllText(sqlPath);

            var dbFactory = new DataContextFactory();
            using (var context = dbFactory.Create())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                //context.Database.ExecuteSqlRaw(seedData);
                Console.WriteLine("Database initialized successfully");
            }

            Console.WriteLine("Application database is successfully setup");

            Console.WriteLine("Initializing sample media files upload");

            //upload sample files
            var msgIdentity = new MessageIdentity(DtoHelper.DefaultOrganization, "Office", "id");
            var ftpService = new FtpService(logger, null, configuration);
            ftpService.UploadFolder(sampleImageFolder, "/images/samples",  _ftpStreamingAddress, msgIdentity);
            ftpService.UploadFolder(sampleVideoFolder, "/videos/samples", _ftpStreamingAddress, msgIdentity);

            Console.WriteLine("Sample media files are successfully uploaded");
        }
    }
}
