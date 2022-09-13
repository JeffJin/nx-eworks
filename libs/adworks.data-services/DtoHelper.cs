using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using adworks.data_services.DbModels;
using adworks.data_services.Identity;
using adworks.media_common;

namespace adworks.data_services
{
    public static class DtoHelper
    {
        public static string DefaultOrganization = "kiosho";
        public static MediaAssetDto Convert(MediaAsset model)
        {
            if (model.GetType() == typeof(Video))
            {
                return Convert((Video)model);
            }

            if (model.GetType() == typeof(Audio))
            {
                return Convert((Audio)model);
            }

            if (model.GetType() == typeof(Image))
            {
                return Convert((Image)model);
            }

            return null;
        }

        public static ProductDto Convert(Product model)
        {
            if (model == null)
            {
                return null;
            }
            return new ProductDto
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                CreatedBy = model.CreatedBy?.Email,
                UpdatedBy = model.UpdatedBy?.Email,
                SubCategory = model.ProductCategory?.Name,
                Category = model.ProductCategory?.Category?.Name,
                CreatedOn = model.CreatedOn,
                UpdatedOn = model.UpdatedOn,
                Price = model.Price,
                Brand = model.Brand,
                Inventory = model.Inventory,
                Images = model.Images.Select(i => i.CloudUrl).ToList(),
                Videos = model.Videos.Select(i => i.VodVideoUrl).ToList()
            };
        }
        public static Product Convert(ProductDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            var model = new Product
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Brand = dto.Brand,
                Inventory = dto.Inventory,
                UpdatedOn = dto.UpdatedOn,
            };
            //use the id generated from file upload
            if (dto.Id != Guid.Empty)
            {
                model.Id = dto.Id;
            }

            return model;
        }

        public static VideoDto Convert(Video video)
        {
            if (video == null)
            {
                return null;
            }
            return new VideoDto
            {
                Id = video.Id,
                CloudUrl = video.VodVideoUrl,
                Title = video.Title,
                Description = video.Description,
                Duration = video.Duration,
                CreatedBy = video.CreatedBy?.Email,
                UpdatedBy = video.UpdatedBy?.Email,
                ProgressiveUrl = video.ProgressiveVideoUrl,
                RawFilePath = video.RawFilePath,
                Tags = video.Tags,
                Category = video.Category?.Name,
                CreatedOn = video.CreatedOn,
                UpdatedOn = video.UpdatedOn,
                EncodedFilePath = video.EncodedVideoPath,
                MainThumbnail = string.IsNullOrEmpty(video.ThumbnailLink) ? "" : video.ThumbnailLink,
                VisibleId = video.Id.ToString("N")
                // FileSize = new FileInfo(video.RawFilePath).Length
            };
        }

        public static Video Convert(VideoDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            var model = new Video
            {
                Description = dto.Description,
                RawFilePath = dto.RawFilePath,
                Duration = dto.Duration,
                Tags = dto.Tags,
                UpdatedOn = dto.UpdatedOn,
                ThumbnailLink = dto.MainThumbnail ?? "",
                Title = dto.Title,
                VodVideoUrl = dto.CloudUrl,
                EncodedVideoPath = dto.EncodedFilePath,
            };
            //use the id generated from file upload
            if (dto.Id != Guid.Empty)
            {
                model.Id = dto.Id;
            }

            return model;
        }

        public static UserDto Convert(User model)
        {
            if (model == null)
            {
                return null;
            }

            return new UserDto()
            {
                Id = model.Id,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                UserName = model.UserName,
                ProfileLogo = model.ProfileLogo,
            };
        }

        public static ImageDto Convert(Image model)
        {
            if (model == null)
            {
                return null;
            }
            return new ImageDto
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                CreatedBy = model.CreatedBy?.Email,
                UpdatedBy = model.UpdatedBy?.Email,
                RawFilePath = model.RawFilePath,
                Tags = model.Tags,
                Category = model.Category?.Name,
                CreatedOn = model.CreatedOn,
                UpdatedOn = model.UpdatedOn,
                CloudUrl = model.CloudUrl,
                ImageType = DetectImageType(model.RawFilePath)
                // FileSize = new FileInfo(model.RawFilePath).Length
            };
        }

        private static string DetectImageType(string filePath)
        {
            if (filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                filePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                return "jpeg";
            }
            else if (filePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
            {
                return "gif";
            }
            else if (filePath.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
            {
                return "bmp";
            }
            else if (filePath.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase))
            {
                return "tiff";
            }
            else if (filePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                return "png";
            }
            else
            {
                return "unknown";
            }
        }

        public static Image Convert(ImageDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            var model = new Image
            {
                Description = dto.Description,
                RawFilePath = dto.RawFilePath,
                Tags = dto.Tags,
                UpdatedOn = dto.UpdatedOn,
                Title = dto.Title,
                CloudUrl = dto.CloudUrl
            };
            //use the id generated from file upload
            if (dto.Id != Guid.Empty)
            {
                model.Id = dto.Id;
            }

            return model;
        }

        public static Organization Convert(OrganizationDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            return new Organization
            {
                Name = dto.Name,
            };
        }

        public static AudioDto Convert(Audio audio)
        {
            if (audio == null)
            {
                return null;
            }
            return new AudioDto
            {
                Id = audio.Id,
                CloudUrl = audio.CloudUrl,
                Title = audio.Title,
                Description = audio.Description,
                Duration = audio.Duration,
                CreatedBy = audio.CreatedBy?.Email,
                UpdatedBy = audio.UpdatedBy?.Email,
                RawFilePath = audio.RawFilePath,
                Tags = audio.Tags,
                Category = audio.Category?.Name,
                CreatedOn = audio.CreatedOn,
                UpdatedOn = audio.UpdatedOn,
                EncodedFilePath = audio.EncodedFilePath,
                // FileSize = new FileInfo(audio.RawFilePath).Length
            };
        }

        public static Audio Convert(AudioDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            var model = new Audio
            {
                Description = dto.Description,
                RawFilePath = dto.RawFilePath,
                Duration = dto.Duration,
                Tags = dto.Tags,
                UpdatedOn = dto.UpdatedOn,
                Title = dto.Title,
                CloudUrl = dto.CloudUrl,
                EncodedFilePath = dto.EncodedFilePath,
            };
            //use the id generated from file upload
            if (dto.Id != Guid.Empty)
            {
                model.Id = dto.Id;
            }

            return model;
        }

        public static DeviceDto Convert(Device model)
        {
            if (model == null)
            {
                return null;
            }
            var result = new DeviceDto
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy?.Email,
                UpdatedBy = model.UpdatedBy?.Email,
                CreatedOn = model.CreatedOn,
                UpdatedOn = model.UpdatedOn,
                ActivatedOn = model.ActivatedOn,
                AppVersion = model.AppVersion,
                AssetTag = model.AssetTag,
                DeviceVersion = model.DeviceVersion,
                LocationId = model.Location?.Id,
                DeviceGroupId = model.DeviceGroup?.Id,
                DeviceGroupName = model.DeviceGroup?.Name,
                OrganizationName = model.DeviceGroup?.Organization?.Name,
                Address = model.Location?.Address,
                Locale = model.Location?.Locale,
                TimezoneOffset = model.Location?.TimezoneOffset,
                SerialNumber = model.SerialNumber
            };

            return result;
        }

        public static OrganizationDto Convert(Organization model)
        {
            if (model == null)
            {
                return null;
            }
            return new OrganizationDto
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy?.Email,
                UpdatedBy = model.UpdatedBy?.Email,
                CreatedOn = model.CreatedOn,
                UpdatedOn = model.UpdatedOn,
                Name = model.Name
            };
        }

        public static Location Convert(LocationDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            return new Location()
            {
                Address = dto.Address,
                Locale = dto.Locale,
                TimezoneOffset = dto.TimezoneOffset,
            };
        }

        public static LocationDto Convert(Location model)
        {
            if (model == null)
            {
                return null;
            }
            return new LocationDto()
            {
                Id = model.Id,
                Address = model.Address,
                Locale = model.Locale,
                TimezoneOffset = model.TimezoneOffset,
                CreatedOn = model.CreatedOn,
                CreatedBy = model.CreatedBy?.Email,
                UpdatedBy = model.UpdatedBy?.Email,
            };
        }


        public static LicenseDto Convert(License model)
        {
            if (model == null)
            {
                return null;
            }
            return new LicenseDto()
            {
                Id = model.Id,
                Type = model.Type,
                ExpireOn = model.ExpireOn,
                CreatedOn = model.CreatedOn,
                CreatedBy = model.CreatedBy?.Email,
                UpdatedBy = model.UpdatedBy?.Email,
                UpdatedOn = model.UpdatedOn
            };
        }


        public static License Convert(LicenseDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            return new License()
            {
                Id = dto.Id,
                Type = dto.Type,
                ExpireOn = dto.ExpireOn,
            };
        }

        public static Device Convert(DeviceDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            return new Device
            {
                ActivatedOn = dto.ActivatedOn,
                AppVersion = dto.AppVersion,
                AssetTag = dto.AssetTag,
                DeviceVersion = dto.DeviceVersion,
                SerialNumber = dto.SerialNumber,
            };
        }


        public static DeviceGroupDto Convert(DeviceGroup model)
        {
            if (model == null)
            {
                return null;
            }
            return new DeviceGroupDto
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy?.Email,
                UpdatedBy = model.UpdatedBy?.Email,
                CreatedOn = model.CreatedOn,
                UpdatedOn = model.UpdatedOn,
                Name = model.Name,
            };
        }


        public static DeviceGroup Convert(DeviceGroupDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            return new DeviceGroup
            {
                UpdatedOn = dto.UpdatedOn,
                Name = dto.Name,
            };
        }

        public static Playlist Convert(PlaylistDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            var playlistGroupDtos = dto.DeviceGroups.Select(deviceGroup => new PlaylistGroupDto()
            {
                Playlist = dto, 
                PlaylistId = dto.Id, 
                DeviceGroup = deviceGroup,
                DeviceGroupId = deviceGroup.Id
            }).ToList();
            
            var result = new Playlist()
            {
                UpdatedOn = dto.UpdatedOn,
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
            };
            result.PlaylistGroups.Concat(playlistGroupDtos.Select(Convert));
            return result;
        }

        public static PlaylistGroup Convert(PlaylistGroupDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            return new PlaylistGroup()
            {
                UpdatedOn = null,
                DeviceGroup = Convert(dto.DeviceGroup),
                Playlist = Convert(dto.Playlist),
            };
        }

        public static SubPlaylist Convert(SubPlaylistDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            return new SubPlaylist()
            {
                UpdatedOn = dto.UpdatedOn,
                PositionX = dto.PositionX,
                PositionY = dto.PositionY,
                Width = dto.Width,
                Height = dto.Height
            };
        }

        public static PlaylistDto Convert(Playlist model)
        {
            if (model == null)
            {
                return null;
            }
            return new PlaylistDto
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy?.Email,
                UpdatedBy = model.UpdatedBy?.Email,
                UpdatedOn = model.UpdatedOn,
                CreatedOn = model.CreatedOn,
                Name = model.Name,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime
            };
        }

        public static SubPlaylistDto Convert(SubPlaylist model)
        {
            if (model == null)
            {
                return null;
            }
            return new SubPlaylistDto
            {
                Id = model.Id,
                CreatedBy = model.CreatedBy?.Email,
                UpdatedBy = model.UpdatedBy?.Email,
                CreatedOn = model.CreatedOn,
                UpdatedOn = model.UpdatedOn,
                PositionX = model.PositionX,
                PositionY = model.PositionY,
                Width = model.Width,
                Height = model.Height
            };
        }

        public static PlaylistItemDto Convert(PlaylistItem model)
        {
            if (model == null)
            {
                return null;
            }
            return new PlaylistItemDto
            {
                Id = model.Id,
                SubPlaylistId = model.SubPlaylist.Id,
                Index = model.Index,
                CreatedBy = model.CreatedBy?.Email,
                UpdatedBy = model.UpdatedBy?.Email,
                CreatedOn = model.CreatedOn,
                UpdatedOn = model.UpdatedOn,
                MediaAssetId = model.MediaAssetId,
                AssetDiscriminator = model.AssetDiscriminator,
                Duration = model.Duration
            };
        }

        public static PlaylistItem Convert(PlaylistItemDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            return new PlaylistItem
            {
                Index = dto.Index,
                UpdatedOn = dto.UpdatedOn,
                MediaAssetId = dto.MediaAssetId,
                AssetDiscriminator = dto.AssetDiscriminator,
                Duration = dto.Duration
            };
        }

        public static DeviceStatus Convert(DeviceStatusDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            return new DeviceStatus
                        {
                            Status = dto.Status,
                            UpdatedOn = dto.ReceivedOn
                        };
        }

        public static DeviceStatusDto Convert(DeviceStatus dto)
        {
            if (dto == null)
            {
                return null;
            }
            return new DeviceStatusDto
                        {
                            Id = dto.Id,
                            CreatedOn = dto.CreatedOn,
                            CreatedBy = dto.CreatedBy?.Email,
                            UpdatedBy = dto.UpdatedBy?.Email,
                            SerialNumber = dto.Device?.SerialNumber,
                            Status = dto.Status
                        };
        }
    }
}