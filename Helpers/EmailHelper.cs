using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace iNQUIRE.Helper
{
    public static class EmailHelper
    {
        public static string FromAddress;
        public static string SmtpHost;
        public static int SmtpPort;

        private static string MailAccount = ConfigurationManager.AppSettings["mailAccount"];
        private static string MailPassword = ConfigurationManager.AppSettings["mailPassword"];

        // key = local path to the image, value = body
        public async static Task SendEmail(string to_email_address, string subject, string email_html, List<LinkedResource> email_resources)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress(FromAddress);
                mail.To.Add(to_email_address);
                mail.Subject = subject;

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(email_html.ToString(), Encoding.Default, "text/html");

                if (email_resources != null)
                {
                    foreach (LinkedResource lr in email_resources)
                        htmlView.LinkedResources.Add(lr);
                }

                mail.AlternateViews.Add(htmlView);

                var smtpClient = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));
                System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(MailAccount, MailPassword);
                smtpClient.Credentials = credentials;
                await smtpClient.SendMailAsync(mail);
                if (email_resources != null)
                    email_resources.Clear();
            }
            catch (Exception ex)
            {
                Helper.LogHelper.StatsLog(null, "EmailHelper.SendEmail()", "Send email", ex.Message, ex.InnerException?.Message);
            }
        }

    }
}
