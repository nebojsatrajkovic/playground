using Core.Shared.Models;
using log4net;
using System.Net;
using System.Net.Mail;

namespace Core.Shared.Services
{
    public static class EmailService
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(EmailService));

        public static ResultOf SendEmail(List<string> emailTo, string subject, string body)
        {
            ResultOf returnValue;

            try
            {
                string? emailFrom = Environment.GetEnvironmentVariable("CORE_EMAIL_ADDRESS");
                string? emailPassword = Environment.GetEnvironmentVariable("CORE_EMAIL_PASS");

                if (string.IsNullOrEmpty(emailFrom) || string.IsNullOrEmpty(emailPassword))
                {
                    return new ResultOf(CORE_OperationStatus.FAILED, "Email service credentials are not configured");
                }

                using var mail = new MailMessage
                {
                    From = new MailAddress(emailFrom),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                foreach (var email in emailTo)
                {
                    mail.To.Add(email);
                }

                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(emailFrom, emailPassword),
                    EnableSsl = true
                };

                smtp.Send(mail);

                returnValue = new ResultOf(CORE_OperationStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to send email: ", ex);

                returnValue = new ResultOf(ex);
            }

            return returnValue;
        }
    }
}