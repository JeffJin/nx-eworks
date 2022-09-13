using System;
using System.Diagnostics;
using adworks.data_services.DbModels;
using adworks.networking;

namespace adworks.scheduler
{
    public class SendNotificationsJob : INotificationJob
    {
        private readonly ISmsService _smsService;
        private readonly IEmailService _emailService;
        private readonly IScheduleService _scheduleService;

        private const string SubjectTemplate =
            "Reminder for your appointment coming up at {0}.";

        private readonly string MessageTemplate =
            "Hi {0}," + Environment.NewLine +
            "This is just a friendly reminder for your appointment coming up at {1}." +
            Environment.NewLine + "Sincerely," +
            Environment.NewLine + "eWorkspace Solutions Inc.";

        public SendNotificationsJob(ISmsService smsService, IEmailService emailService,
            IScheduleService scheduleService)
        {
            _smsService = smsService;
            _emailService = emailService;
            _scheduleService = scheduleService;
        }

        public void Execute()
        {
            Debug.WriteLine("Send notifications", DateTimeOffset.Now);
            //Find all upcoming appointments and send sms message
            var appointments = _scheduleService.GetAppointments(DateTimeOffset.UtcNow, DateTimeOffset.MaxValue);
            foreach (Appointment appointment in appointments)
            {
                //TODO check user notifincation preference
                //_smsService.Send(appointment.Phone, appointment.Description);
                _emailService.SendNotificationEmail(appointment.Email);
            }
        }
    }
}