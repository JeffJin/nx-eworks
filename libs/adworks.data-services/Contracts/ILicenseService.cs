using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.media_common;

namespace adworks.data_services
{
    public interface ILicenseService
    {
        Task<License> UpdateLicense(LicenseDto dto);
        Task<License> FindLicense(Guid id);
        bool DeleteLicense(Guid id);
        Task<Guid> AddLicense(LicenseDto dto);
        Task<IList<License>> GetLicenses();
        License CreateTrialLicense(int months = 3);
        Task<IList<License>> FindLicenses(IEnumerable<Guid> licenseIds);
    }
}