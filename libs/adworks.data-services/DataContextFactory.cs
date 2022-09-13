using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.data_services.Identity;
using adworks.networking;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.data_services
{
    public class DataContextFactory : IDataContextFactory
    {
        private readonly ILogger _logger;

        public DataContextFactory()
        {

        }
        public DataContextFactory(ILogger logger)
        {
            _logger = logger;
        }

        public CommonDbContext Create()
        {
            return new CommonDbContext(_logger);
        }

        public static void Cleanup()
        {
            using (var context = new CommonDbContext())
            {
                context.Database.ExecuteSqlRaw("delete from DeviceStatuses");
                context.Database.ExecuteSqlRaw("delete from Licenses where Type != 'Trial'");
                context.Database.ExecuteSqlRaw("delete from Devices where SerialNumber not like '00000000%'");
                context.Database.ExecuteSqlRaw("delete from Appointments");
                context.Database.ExecuteSqlRaw("delete from PlaylistItems");
                context.Database.ExecuteSqlRaw("delete from SubPlaylists");
                context.Database.ExecuteSqlRaw("delete from PlaylistGroups");
                context.Database.ExecuteSqlRaw("delete from DeviceGroups where Name <> 'Office'");
                context.Database.ExecuteSqlRaw("delete from Locations");
                context.Database.ExecuteSqlRaw("delete from Playlists");
                context.Database.ExecuteSqlRaw("delete from MergeRecords");
                context.Database.ExecuteSqlRaw("delete from Videos");
                context.Database.ExecuteSqlRaw("delete from Images");
                context.Database.ExecuteSqlRaw("delete from Audios");
                context.Database.ExecuteSqlRaw("delete from Records");
                context.Database.ExecuteSqlRaw("delete from Users where Email <> 'admin@eworkspace.ca' AND Email <> 'jeff@jeffjin.com'");
                context.Database.ExecuteSqlRaw("delete from Organizations where Name <> 'kiosho'");
            }

        }

        public static void Cleanup(string clause)
        {
            using (var context = new CommonDbContext())
            {
                context.Database.ExecuteSqlRaw("delete from " + clause);
            }
        }

        public static async Task SeedAuthData(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            if (!roleManager.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role() {Name = RoleTypes.Admin},
                    new Role() {Name = RoleTypes.Manager},
                    new Role() {Name = RoleTypes.User}
                };

                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(role);
                }
            }

            if (!userManager.Users.Any())
            {
                // Initialize default user
                var admin = new User()
                {
                    UserName = "admin@eworkspace.ca",
                    Email = "admin@eworkspace.ca",
                    EmailConfirmed = true
                };

                var user = new User()
                {
                    UserName = "jeff@jeffjin.com",
                    Email = "jeff@jeffjin.com",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(admin, "Pr0t3ct3d!@#");
                await userManager.AddToRolesAsync(admin, new[]
                {
                    RoleTypes.Admin, RoleTypes.Manager, RoleTypes.User
                });

                await userManager.CreateAsync(user, "Pr0t3ct3d!");
                await userManager.AddToRolesAsync(user, new[]
                {
                    RoleTypes.User
                });
            }
        }

        //Warning: this seeding needs to be happened after auth data is setup
        public static async Task SeedAppData(CommonDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            if (!context.Organizations.Any())
            {
                var admin = context.Users.SingleOrDefault(u => u.Email == "admin@eworkspace.ca");
                var user = context.Users.SingleOrDefault(u => u.Email == "jeff@jeffjin.com");

                var organization = new Organization()
                {
                    Name = DtoHelper.DefaultOrganization,
                    CreatedBy = admin
                };
                organization.Users.Add(admin);
                organization.Users.Add(user);

                await context.Organizations.AddAsync(organization);
                await context.SaveChangesAsync();
            }

            if (!context.DeviceGroups.Any())
            {
                var user = context.Users.SingleOrDefault(u => u.Email == "admin@eworkspace.ca");
                var organization = context.Organizations.SingleOrDefault(u => u.Name == DtoHelper.DefaultOrganization);

                var group = new DeviceGroup()
                {
                    Name = "Office",
                    CreatedBy = user,
                    Organization = organization,
                };

                await context.DeviceGroups.AddAsync(@group);
                await context.SaveChangesAsync();
            }

            if (!context.Licenses.Any())
            {
                var user = context.Users.SingleOrDefault(u => u.Email == "admin@eworkspace.ca");

                var trial1 = new License()
                {
                    Type = "Trial",
                    ExpireOn = DateTimeOffset.Now.AddMonths(3),
                    CreatedBy = user
                };

                await context.Licenses.AddAsync(trial1);

                var trial2 = new License()
                {
                    Type = "Trial",
                    ExpireOn = DateTimeOffset.Now.AddMonths(3),
                    CreatedBy = user
                };

                await context.Licenses.AddAsync(trial2);

                var trial3 = new License()
                {
                    Type = "Trial",
                    ExpireOn = DateTimeOffset.Now.AddMonths(3),
                    CreatedBy = user
                };

                await context.Licenses.AddAsync(trial3);

                var trial4 = new License()
                {
                    Type = "Trial",
                    ExpireOn = DateTimeOffset.Now.AddMonths(3),
                    CreatedBy = user
                };

                await context.Licenses.AddAsync(trial4);
                await context.SaveChangesAsync();
            }

            if (!context.Locations.Any())
            {
                var user = context.Users.SingleOrDefault(u => u.Email == "admin@eworkspace.ca");

                var vaughan = new Location()
                {
                    Address = "176 District Ave, Vaughan, Ontario, Canada, L6A 0Y3",
                    Locale = "en-ca",
                    TimezoneOffset = -5,
                    CreatedBy = user
                };

                var newYork = new Location()
                {
                    Address = "Flushing, New York, USA, 11355",
                    Locale = "en-us",
                    TimezoneOffset = -5,
                    CreatedBy = user
                };

                var vancouver = new Location()
                {
                    Address = "Vancouver, British Columbia, Canada, V5K 0A4",
                    Locale = "en-ca",
                    TimezoneOffset = -8,
                    CreatedBy = user
                };

                await context.Locations.AddAsync(vaughan);
                await context.Locations.AddAsync(newYork);
                await context.Locations.AddAsync(vancouver);


                await context.SaveChangesAsync();
            }

            if (!context.Devices.Any())
            {
                var user = context.Users.SingleOrDefault(u => u.Email == "admin@eworkspace.ca");
                var group = context.DeviceGroups.SingleOrDefault(u => u.Name == "Office");
                var licenses = context.Licenses.Where(l => l.Type == "Trial").ToArray();
                var location = context.Locations.SingleOrDefault(l => l.Address == "176 District Ave, Vaughan, Ontario, Canada, L6A 0Y3");
                var trial1 = licenses[0];
                var trial2 = licenses[1];
                var trial3 = licenses[2];
                var trial4 = licenses[3];

                var device1 = new Device()
                {
                    AssetTag = "AD001",
                    SerialNumber = "0000000029fa61cc",
                    DeviceVersion = 1,
                    AppVersion = 1,
                    ActivatedOn = DateTimeOffset.Now,
                    CreatedBy = user,
                    DeviceGroup = group,
                    Location = location,
                    Licenses = {trial1}
                };


                var device2 = new Device()
                {
                    AssetTag = "AD002",
                    SerialNumber = "00000000954f0c90",
                    DeviceVersion = 1,
                    AppVersion = 1,
                    ActivatedOn = DateTimeOffset.Now,
                    CreatedBy = user,
                    DeviceGroup = group,
                    Location = location,
                    Licenses = {trial2}
                };

                var device3 = new Device()
                {
                    AssetTag = "AD003",
                    SerialNumber = "000000004095f08a",
                    DeviceVersion = 1,
                    AppVersion = 1,
                    ActivatedOn = DateTimeOffset.Now,
                    CreatedBy = user,
                    Location = location,
                    DeviceGroup = group,
                    Licenses = {trial3}
                };

                var mac = new Device()
                {
                    AssetTag = "AD004",
                    SerialNumber = "J5909005E4MFB",
                    DeviceVersion = 1,
                    AppVersion = 1,
                    ActivatedOn = DateTimeOffset.Now,
                    CreatedBy = user,
                    Location = location,
                    DeviceGroup = group,
                    Licenses = {trial4}
                };

                await context.Devices.AddAsync(device1);
                await context.Devices.AddAsync(device2);
                await context.Devices.AddAsync(device3);
                await context.Devices.AddAsync(mac);
                await context.SaveChangesAsync();
            }

            if (!context.Categories.Any())
            {
                await context.Categories.AddAsync(new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Hobby",
                });

                await context.Categories.AddAsync(new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Sports"
                });
                await context.Categories.AddAsync(new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Music Videos"
                });
                await context.Categories.AddAsync(new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Drama"
                });
                await context.Categories.AddAsync(new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Education"
                });
                await context.Categories.AddAsync(new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Advertisement"
                });
                await context.Categories.AddAsync(new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Miscellaneous"
                });
                await context.Categories.AddAsync(new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Corals"
                });
                await context.SaveChangesAsync();
                await context.SaveChangesAsync();
            }

            if (!context.SubCategories.Any())
            {
                var lps = await context.SubCategories.AddAsync(new SubCategory()
                {
                    Id = Guid.NewGuid(),
                    Name = "LPS",
                });

                var sps =  await context.SubCategories.AddAsync(new SubCategory()
                {
                    Id = Guid.NewGuid(),
                    Name = "SPS"
                });
                var softies = await context.SubCategories.AddAsync(new SubCategory()
                {
                    Id = Guid.NewGuid(),
                    Name = "Softies"
                });
                var corals = await context.Categories.SingleOrDefaultAsync(c => c.Name == "Corals");
                corals.SubCategories.Add(lps.Entity);
                corals.SubCategories.Add(sps.Entity);
                corals.SubCategories.Add(softies.Entity);
                await context.SaveChangesAsync();
            }

            if (!context.Playlists.Any())
            {
                var user = context.Users.FirstOrDefault(u => u.UserName == "admin@eworkspace.ca");
                var group = context.DeviceGroups.SingleOrDefault(u => u.Name == "Office");

                var basicPlaylist = new Playlist()
                {
                    Name = "Basic (No PiP)",
                    StartDate = DateTimeOffset.Now.AddDays(10),
                    EndDate = DateTimeOffset.Now.AddMonths(10),
                    CreatedBy = user
                };
                basicPlaylist.SubPlaylists.Add(new SubPlaylist()
                {
                    Width = 100,
                    Height = 100,
                    PositionX = 0,
                    PositionY = 0,
                    CreatedBy = user
                });
                basicPlaylist.PlaylistGroups.Add(new PlaylistGroup()
                {
                    Playlist = basicPlaylist,
                    DeviceGroup = group,
                    CreatedBy = user
                });

                await context.Playlists.AddAsync(basicPlaylist);

                var topBottomPl = new Playlist()
                {
                    Name = "Top Bottom",
                    StartDate = DateTimeOffset.Now.AddDays(10),
                    EndDate = DateTimeOffset.Now.AddMonths(10),
                    CreatedBy = user
                };
                topBottomPl.SubPlaylists.Add(new SubPlaylist()
                {
                    Width = 80,
                    Height = 80,
                    PositionX = 20,
                    PositionY = 0,
                    CreatedBy = user
                });
                topBottomPl.SubPlaylists.Add(new SubPlaylist()
                {
                    Width = 100,
                    Height = 20,
                    PositionX = 0,
                    PositionY = 80,
                    CreatedBy = user
                });
                topBottomPl.PlaylistGroups.Add(new PlaylistGroup()
                {
                    Playlist = topBottomPl,
                    DeviceGroup = group,
                    CreatedBy = user
                });
                await context.Playlists.AddAsync(topBottomPl);

                var stdPlaylist = new Playlist()
                {
                    Name = "Standard 3 Section",
                    StartDate = DateTimeOffset.Now.AddDays(10),
                    EndDate = DateTimeOffset.Now.AddMonths(10),
                    CreatedBy = user
                };
                stdPlaylist.SubPlaylists.Add(new SubPlaylist()
                {
                    Width = 80,
                    Height = 80,
                    PositionX = 20,
                    PositionY = 0,
                    CreatedBy = user
                });
                stdPlaylist.SubPlaylists.Add(new SubPlaylist()
                {
                    Width = 20,
                    Height = 80,
                    PositionX = 0,
                    PositionY = 0,
                    CreatedBy = user
                });
                stdPlaylist.SubPlaylists.Add( new SubPlaylist()
                {
                    Width = 100,
                    Height = 20,
                    PositionX = 0,
                    PositionY = 80,
                    CreatedBy = user
                });
                stdPlaylist.PlaylistGroups.Add(new PlaylistGroup()
                {
                    Playlist = stdPlaylist,
                    DeviceGroup = group,
                    CreatedBy = user
                });
                await context.Playlists.AddAsync(stdPlaylist);

                await context.SaveChangesAsync();
            }

            if (!context.Images.Any())
            {
                var user = context.Users.SingleOrDefault(u => u.Email == "jeff@jeffjin.com");
                var admin = context.Users.FirstOrDefault(u => u.UserName == "admin@eworkspace.ca");
                var category = context.Categories.SingleOrDefault(c => c.Name == "Corals");

                var img1 = new Image()
                {
                    CloudUrl = "https://jeffjin.net:5002/images/samples/wd.jpg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/wd.jpg",
                    Title = "Walt Disney",
                    Description = "Beautiful pinks purples, yellow and greens make this tennis a truly magical addition to any tank",
                    Category = category,
                    CreatedBy = user
                };
                await context.Images.AddAsync(img1);

                var img2 = new Image()
                {
                    CloudUrl = "https://jeffjin.net:5002/images/samples/loom.jpg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/loom.jpg",
                    Title = "Rainbow Loom",
                    Description = "Truly cool  Reef Raft captive.  Kind of a pink/red with purple and yellow at the tips.  Definitely a standout.",
                    Category = category,
                    CreatedBy = user
                };
                await context.Images.AddAsync(img2);

                var img3 = new Image()
                {
                    CloudUrl = "https://jeffjin.net:5002/images/samples/op3.jpg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/op3.jpg",
                    Title = "Orange Passion",
                    Description = "A classic piece with beautiful orange polyps that contrasts its striking blue base",
                    Category = category,
                    CreatedBy = user
                };
                await context.Images.AddAsync(img3);

                var img4 = new Image()
                {
                    CloudUrl = "https://jeffjin.net:5002/images/samples/op2.jpg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/op2.jpg",
                    Title = "Orange Passion",
                    Description = "A classic piece with beautiful orange polyps that contrasts its striking blue base",
                    Category = category,
                    CreatedBy = user
                };
                await context.Images.AddAsync(img4);

                var img5 = new Image()
                {
                    CloudUrl = "https://jeffjin.net:5002/images/samples/op1.jpg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/op1.jpg",
                    Title = "Orange Passion",
                    Description = "A classic piece with beautiful orange polyps that contrasts its striking blue base",
                    Category = category,
                    CreatedBy = user
                };
                await context.Images.AddAsync(img5);

                var img6 = new Image()
                {
                    CloudUrl = "https://jeffjin.net:5002/images/samples/green_slimer.jpeg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/green_slimer.jpeg",
                    Title = "Green Slimer",
                    Description = "With thick branches, separate, untidy and with colours that vary from yellowish green to pale brown acropora yongei is an easily recognizable species. This fact, coupled with its incredible abundance, makes this coral a highly ubiquitous animal",
                    Category = category,
                    CreatedBy = user
                };
                await context.Images.AddAsync(img6);

                var img7 = new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/home_wrecker.jpg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/home_wrecker.jpg",
                    Title = "Home Wrecker",
                    Description = "Home Wrecker Description",
                    Category = category,
                    CreatedBy = user
                };
                await context.Images.AddAsync(img7);

                var img8 = new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/digitata2.jpg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/digitata2.jpg",
                    Title = "Fire Digi",
                    Description = "The Forest Fire Montipora digitata has bright red/orange polyps and either a light green or teal base",
                    Category = category,
                    CreatedBy = user
                };
                await context.Images.AddAsync(img8);

                var img9 = new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/digitata3.jpg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/digitata3.jpg",
                    Title = "Fire Digi",
                    Description = "he Forest Fire Montipora digitata has bright red/orange polyps and either a light green or teal base",
                    Category = category,
                    CreatedBy = user
                };
                await context.Images.AddAsync(img9);

                var img10 = new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/purple_bonsai.jpg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/purple_bonsai.jpg",
                    Title = "Purple Bonsai",
                    Description = "The tabling Acroporas have that special allure.  One that comes to mind is  the ability to appear as if they defy gravity as they table outward.",
                    Category = category,
                    CreatedBy = user
                };
                await context.Images.AddAsync(img10);

                var img11 = new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/purple_bonsai1.jpg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/purple_bonsai1.jpg",
                    Title = "Purple Bonsai",
                    Description = "The tabling Acroporas have that special allure.  One that comes to mind is  the ability to appear as if they defy gravity as they table outward.",
                    Category = category,
                    CreatedBy = user
                };
                await context.Images.AddAsync(img11);

                var img12 = new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/redplanet.jpg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/redplanet.jpg",
                    Title = "Red Planet",
                    Description = "Polyp coloration on this coral is a deep red and the new growth is light pink to white.",
                    Category = category,
                    CreatedBy = user
                };
                await context.Images.AddAsync(img12);

                var img13 = new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/cherry_blossom.jpg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/cherry_blossom.jpg",
                    Title = "Cherry Blossom",
                    Description = "This Acropora is pretty common on many inshore reefs, but it was prolific on the millie spot, were Acropora were dominant.",
                    Category = category,
                    CreatedBy = user
                };
                await context.Images.AddAsync(img13);

                var img14 = new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/galaxea_fascicularis.jpg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/galaxea_fascicularis.jpg",
                    Title = "Galaxea fascicularis",
                    Description = "The Galaxy Coral Galaxea fascicularis is a popular large polyp stony (LPS) coral that many reef enthusiasts have or want in their collection.",
                    Category = category,
                    CreatedBy = user
                };
                await context.Images.AddAsync(img14);

                await context.Images.AddAsync(new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/nike-adapt-bb.jpeg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/nike-adapt-bb.jpeg",
                    Title = "Nike Adapt BB",
                    Description = "Nike Adapt BB",
                    Category = category,
                    CreatedBy = admin
                });

                await context.Images.AddAsync(new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/nike2015.png",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/nike2015.png",
                    Title = "Nike 2015",
                    Description = "Nike 2015",
                    Category = category,
                    CreatedBy = admin
                });

                await context.Images.AddAsync(new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/nike2016.png",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/nike2016.png",
                    Title = "Nike 2016",
                    Description = "Nike 2016",
                    Category = category,
                    CreatedBy = admin
                });

                await context.Images.AddAsync(new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/adidas1.png",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/adidas1.png",
                    Title = "adidas1",
                    Description = "adidas1",
                    Category = category,
                    CreatedBy = admin
                });

                await context.Images.AddAsync(new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/adidas2.png",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/adidas2.png",
                    Title = "adidas2",
                    Description = "adidas2",
                    Category = category,
                    CreatedBy = admin
                });
                await context.Images.AddAsync(new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/adidas3.jpeg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/adidas3.jpeg",
                    Title = "adidas3",
                    Description = "adidas3",
                    Category = category,
                    CreatedBy = admin
                });
                await context.Images.AddAsync(new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/audi.jpeg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/audi.jpeg",
                    Title = "audi",
                    Description = "audi",
                    Category = category,
                    CreatedBy = admin
                });
                await context.Images.AddAsync(new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/audi2.jpeg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/audi2.jpeg",
                    Title = "audi2",
                    Description = "audi2",
                    Category = category,
                    CreatedBy = admin
                });
                await context.Images.AddAsync(new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/benz1.jpeg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/benz1.jpeg",
                    Title = "benz1",
                    Description = "benz1",
                    Category = category,
                    CreatedBy = admin
                });
                await context.Images.AddAsync(new Image()
                {
                    CloudUrl = "https://jeffjin.net/images/samples/benz2.jpeg",
                    RawFilePath = "/home/vsftpd/ftpuser/images/samples/benz2.jpeg",
                    Title = "benz2",
                    Description = "benz2",
                    Category = category,
                    CreatedBy = admin
                });

                await context.SaveChangesAsync();
            }

            if (!context.Videos.Any())
            {
                var user = context.Users.SingleOrDefault(u => u.Email == "jeff@jeffjin.com");
                var admin = context.Users.FirstOrDefault(u => u.UserName == "admin@eworkspace.ca");
                var category = context.Categories.SingleOrDefault(c => c.Name == "Corals");

                var vid1 = new Video()
                {
                    VodVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000001/wd.mp4",
                    ProgressiveVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000001/wd.mp4",
                    RawFilePath = "/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000001/wd.mp4",
                    Title = "Walt Disney",
                    Description = "Beautiful pinks purples, yellow and greens make this tennis a truly magical addition to any tank",
                    Category = category,
                    CreatedBy = user,
                    Duration = 100,
                    ThumbnailLink = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000001/thumbnails/wd.jpg"
                };
                await context.Videos.AddAsync(vid1);

                var vid2 = new Video()
                {
                    VodVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000002/Purple_Bonsai.mp4",
                    ProgressiveVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000002/Purple_Bonsai.mp4",
                    RawFilePath = "/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000002/Purple_Bonsai.mp4",
                    Title = "Purple  Bonsai",
                    Description = "The tabling Acroporas have that special allure.  One that comes to mind is  the ability to appear as if they defy gravity as they table outward.",
                    Category = category,
                    CreatedBy = user,
                    Duration = 200,
                    ThumbnailLink = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000002/thumbnails/Purple_Bonsai.jpg"
                };
                await context.Videos.AddAsync(vid2);

                var vid3 = new Video()
                {
                    VodVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000003/Orange_Passion.mp4",
                    RawFilePath = "/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000003/Orange_Passion.mp4",
                    ProgressiveVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000003/Orange_Passion.mp4",
                    Title = "Orange Passion",
                    Description = "A classic piece with beautiful orange polyps that contrasts its striking blue base",
                    Category = category,
                    CreatedBy = user,
                    Duration = 80,
                    ThumbnailLink = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000003/thumbnails/Orange_Passion.jpg"
                };
                await context.Videos.AddAsync(vid3);

                var vid4 = new Video()
                {
                    VodVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000004/Mercedes-Benz1.mp4",
                    RawFilePath = "/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000004/Mercedes-Benz1.mp4",
                    ProgressiveVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000004/Mercedes-Benz1.mp4",
                    Title = "Mercedes-Benz",
                    Description = "Mercedes-Benz",
                    Category = category,
                    CreatedBy = admin,
                    Duration = 88,
                    ThumbnailLink = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000004/thumbnails/benz2.jpeg"
                };
                await context.Videos.AddAsync(vid4);

                var vid5 = new Video()
                {
                    VodVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000005/NIKE MAG-1.mp4",
                    RawFilePath = "/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000005/NIKE MAG-1.mp4",
                    ProgressiveVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000005/NIKE MAG-1.mp4",
                    Title = "nike-adapt-bb",
                    Description = "nike-adapt-bb",
                    Category = category,
                    CreatedBy = admin,
                    Duration = 130,
                    ThumbnailLink = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000005/thumbnails/nike-adapt-bb.jpeg"
                };
                await context.Videos.AddAsync(vid5);

                var vid6 = new Video()
                {
                    VodVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000006/Nike Running Shoes Commercial 2015.mp4",
                    RawFilePath = "/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000006/Nike Running Shoes Commercial 2015.mp4",
                    ProgressiveVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000006/Nike Running Shoes Commercial 2015.mp4",
                    Title = "Nike Running Shoes Commercial 2015",
                    Description = "Nike Running Shoes Commercial 2015",
                    Category = category,
                    CreatedBy = admin,
                    Duration = 780,
                    ThumbnailLink = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000006/thumbnails/nike2015.png"
                };
                await context.Videos.AddAsync(vid6);

                await context.Videos.AddAsync(new Video()
                {
                    VodVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000007/The all-new Audi RS 5.mp4",
                    RawFilePath = "/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000007/The all-new Audi RS 5.mp4",
                    ProgressiveVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000007/The all-new Audi RS 5.mp4",
                    Title = "The all-new Audi RS 5",
                    Description = "The all-new Audi RS 5",
                    Category = category,
                    CreatedBy = admin,
                    Duration = 111,
                    ThumbnailLink = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000007/thumbnails/audi.jpeg"
                });

                await context.Videos.AddAsync(new Video()
                {
                    VodVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000008/adidas1.mp4",
                    RawFilePath = "/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000008/adidas1.mp4",
                    ProgressiveVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000008/adidas1.mp4",
                    Title = "adidas1",
                    Description = "adidas1",
                    Category = category,
                    CreatedBy = admin,
                    Duration = 300,
                    ThumbnailLink = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000008/thumbnails/adidas1.png"
                });


                await context.Videos.AddAsync(new Video()
                {
                    VodVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000009/adidas2.mp4",
                    RawFilePath = "/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000009/adidas2.mp4",
                    ProgressiveVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000009/adidas2.mp4",
                    Title = "adidas2",
                    Description = "adidas2",
                    Category = category,
                    CreatedBy = admin,
                    Duration = 77,
                    ThumbnailLink = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000009/thumbnails/adidas2.png"
                });


                await context.Videos.AddAsync(new Video()
                {
                    VodVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000010/adidas3.mp4",
                    RawFilePath = "/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000010/adidas3.mp4",
                    ProgressiveVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000010/adidas3.mp4",
                    Title = "adidas3",
                    Description = "adidas3",
                    Category = category,
                    CreatedBy = admin,
                    Duration = 211,
                    ThumbnailLink = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000010/thumbnails/adidas3.jpeg"
                });


                await context.Videos.AddAsync(new Video()
                {
                    VodVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000011/nike1.mp4",
                    RawFilePath = "/home/vsftpd/ftpuser/videos/samples/00000000-0000-0000-0000-000000000011/nike1.mp4",
                    ProgressiveVideoUrl = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000011/nike1.mp4",
                    Title = "nike1",
                    Description = "nike1",
                    Category = category,
                    CreatedBy = admin,
                    Duration = 90,
                    ThumbnailLink = "https://jeffjin.net/videos/samples/00000000-0000-0000-0000-000000000011/thumbnails/nike2016.png"
                });

                await context.SaveChangesAsync();
            }

            if (!context.Products.Any())
            {
                var user = context.Users.SingleOrDefault(u => u.Email == "admin@eworkspace.ca");
                var sps = context.SubCategories.SingleOrDefault(c => c.Name == "SPS");
                var lps = context.SubCategories.SingleOrDefault(c => c.Name == "LPS");

                var videos = context.Videos.Where(v => v.Category.Name == "Corals").ToList();
                var images = context.Images.Where(v => v.Category.Name == "Corals").ToList();

                var prod1 = new Product()
                {
                    Title = "Walt Disney",
                    Description = "Beautiful pinks purples, yellow and greens make this tennis a truly magical addition to any tank",
                    ProductCategory = sps,
                    CreatedBy = user,
                    Price = 100,
                    Brand = "ORA",
                    Inventory = 10,
                };
                foreach (var image in images.Where(i => i.Title == prod1.Title))
                {
                    prod1.Images.Add(image);
                }
                foreach (var video in videos.Where(i => i.Title == prod1.Title))
                {
                    prod1.Videos.Add(video);
                }
                await context.Products.AddAsync(prod1);

                var prod2 = new Product()
                {
                    Title = "Galaxea fascicularis",
                    Description = "The Galaxy Coral Galaxea fascicularis is a popular large polyp stony (LPS) coral that many reef enthusiasts have or want in their collection.",
                    ProductCategory = lps,
                    CreatedBy = user,
                    Price = 150,
                    Brand = "ORA",
                    Inventory = 10,
                };
                foreach (var image in images.Where(i => i.Title == prod2.Title))
                {
                    prod2.Images.Add(image);
                }
                foreach (var video in videos.Where(i => i.Title == prod2.Title))
                {
                    prod2.Videos.Add(video);
                }
                await context.Products.AddAsync(prod2);

                var prod3 = new Product()
                {
                    Title = "Green Slimer",
                    Description = "With thick branches, separate, untidy and with colours that vary from yellowish green to pale brown acropora yongei is an easily recognizable species. ",
                    ProductCategory = sps,
                    CreatedBy = user,
                    Price = 40,
                    Brand = "ORA",
                    Inventory = 10,
                };
                foreach (var image in images.Where(i => i.Title == prod3.Title))
                {
                    prod3.Images.Add(image);
                }
                foreach (var video in videos.Where(i => i.Title == prod3.Title))
                {
                    prod3.Videos.Add(video);
                }
                await context.Products.AddAsync(prod3);

                var prod4 = new Product()
                {
                    Title = "Cherry Blossom",
                    Description = "Beautiful pinks purples, yellow and greens make this tennis a truly magical addition to any tank",
                    ProductCategory = sps,
                    CreatedBy = user,
                    Price = 99,
                    Brand = "ORA",
                    Inventory = 10,
                };
                foreach (var image in images.Where(i => i.Title == prod4.Title))
                {
                    prod4.Images.Add(image);
                }
                foreach (var video in videos.Where(i => i.Title == prod4.Title))
                {
                    prod4.Videos.Add(video);
                }
                await context.Products.AddAsync(prod4);

                var prod5 = new Product()
                {
                    Title = "Orange Passion",
                    Description = "A classic piece with beautiful orange polyps that contrasts its striking blue base",
                    ProductCategory = sps,
                    CreatedBy = user,
                    Price = 128,
                    Brand = "ORA",
                    Inventory = 2,
                };
                foreach (var image in images.Where(i => i.Title == prod5.Title))
                {
                    prod5.Images.Add(image);
                }
                foreach (var video in videos.Where(i => i.Title == prod5.Title))
                {
                    prod5.Videos.Add(video);
                }
                await context.Products.AddAsync(prod5);

                var prod6 = new Product()
                {
                    Title = "Purple Bonsai",
                    Description = "The tabling Acroporas have that special allure.  One that comes to mind is  the ability to appear as if they defy gravity as they table outward.",
                    ProductCategory = sps,
                    CreatedBy = user,
                    Price = 99,
                    Brand = "ORA",
                    Inventory = 44,
                };
                foreach (var image in images.Where(i => i.Title == prod6.Title))
                {
                    prod6.Images.Add(image);
                }
                foreach (var video in videos.Where(i => i.Title == prod6.Title))
                {
                    prod6.Videos.Add(video);
                }
                await context.Products.AddAsync(prod6);

                var prod7 = new Product()
                {
                    Title = "Rainbow Loom",
                    Description = "Truly cool  Reef Raft captive.  Kind of a pink/red with purple and yellow at the tips.  Definitely a standout.",
                    ProductCategory = sps,
                    CreatedBy = user,
                    Price = 199,
                    Brand = "ORA",
                    Inventory = 20,
                };
                foreach (var image in images.Where(i => i.Title == prod7.Title))
                {
                    prod7.Images.Add(image);
                }
                foreach (var video in videos.Where(i => i.Title == prod7.Title))
                {
                    prod7.Videos.Add(video);
                }
                await context.Products.AddAsync(prod7);

                var prod8 = new Product()
                {
                    Title = "Fire Digi",
                    Description = "The Forest Fire Montipora digitata has bright red/orange polyps and either a light green or teal base",
                    ProductCategory = sps,
                    CreatedBy = user,
                    Price = 9.9,
                    Brand = "ORA",
                    Inventory = 100,
                };
                foreach (var image in images.Where(i => i.Title == prod8.Title))
                {
                    prod8.Images.Add(image);
                }
                foreach (var video in videos.Where(i => i.Title == prod8.Title))
                {
                    prod8.Videos.Add(video);
                }
                await context.Products.AddAsync(prod8);

                await context.SaveChangesAsync();
            }
        }
    }
}
