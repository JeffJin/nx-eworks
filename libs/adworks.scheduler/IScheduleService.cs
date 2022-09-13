using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using Newtonsoft.Json;

namespace adworks.scheduler
{
    public interface IScheduleService
    {
        Task<Guid> AddAppointment(Appointment appointment);
        IEnumerable<Appointment>  GetAppointments(DateTimeOffset from, DateTimeOffset to);
        Task<Appointment> FindAppointment(Guid id);
        Task<Guid> MarkAppointmentAsComplete(Guid id);
        bool DeleteAppointment(Guid id);
        void UpdateAppointment(Appointment appointment);
    }
}