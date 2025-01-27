using Core.Shared.Models;
using log4net;
using System.Net;
using System.Net.Mail;

namespace Core.Shared.Services
{
    public static class EmailService
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(EmailService));

        public static ResultOf SendEmail(string emailFrom, List<string> emailTo, string subject, string body)
        {
            ResultOf returnValue;

            try
            {
                string emailPassword = "your-email-password"; // Be cautious storing passwords in code

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