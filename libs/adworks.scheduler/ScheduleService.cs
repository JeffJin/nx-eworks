using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.data_services.DbModels;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.scheduler
{
    public class ScheduleService: IScheduleService
    {
        private readonly IDataContextFactory _dbContextFactory;
        private ILogger _logger;

        public ScheduleService(ILogger logger, IDataContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }


        public async Task<Guid> AddAppointment(Appointment appointment)
        {
            if (string.IsNullOrEmpty(appointment.Email) && string.IsNullOrEmpty(appointment.Phone))
            {
                throw new InvalidDataException("Either Email or Phone number must be valid");
            }

            using (var _dbContext = _dbContextFactory.Create())
            {
                _dbContext.Appointments.Add(appointment);

                await _dbContext.SaveChangesAsync();
            }

            return appointment.Id;
        }

        public IEnumerable<Appointment> GetAppointments(DateTimeOffset @from, DateTimeOffset to)
        {
            using (var _dbContext = _dbContextFactory.Create())
            {
                var results = _dbContext.Appointments.Where(v => v.StartTime >= @from && v.EndTime <= to).ToList();
                return results;
            };
        }

        public async Task<Appointment> FindAppointment(Guid id)
        {
            using (var _dbContext = _dbContextFactory.Create())
            {
                var appt = await _dbContext.Appointments.FindAsync(id);
                return appt;
            }
        }

        public Task<Guid> MarkAppointmentAsComplete(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool DeleteAppointment(Guid id)
        {
            using (var _dbContext = _dbContextFactory.Create())
            {
                var appt = _dbContext.Appointments.Find(id);
                if (appt == null)
                {
                    return false;
                }
                else
                {
                    _dbContext.Appointments.Remove(appt);
                }
                _dbContext.SaveChanges();
                return true;
            }
        }

        public void UpdateAppointment(Appointment appointment)
        {
            using (var _dbContext = _dbContextFactory.Create())
            {
                var temp = _dbContext.Appointments.Find(appointment.Id);
                if (temp == null)
                {
                    throw new AppointmentNotFoundException(appointment.Id);
                }
                temp.Email = appointment.Email;
                temp.Description = appointment.Description;
                temp.EndTime = appointment.EndTime;
                temp.Name = appointment.Name;
                temp.Organizer = appointment.Organizer;
                temp.Phone = appointment.Phone;
                temp.StartTime = appointment.StartTime;
                temp.UpdatedOn = DateTimeOffset.Now;
                _dbContext.SaveChanges();
            }
        }
    }

    public class AppointmentNotFoundException : Exception
    {
        public Guid AppointmentId { get; }

        public AppointmentNotFoundException(Guid appointmentId)
        {
            AppointmentId = appointmentId;
        }
    }
}
