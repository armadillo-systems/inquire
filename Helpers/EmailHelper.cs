using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace iNQUIRE.Helper
{
    public static class EmailHelper
    {
        public static string FromAddress;
        public static string Subject;
        public static string SmtpHost;
        public static int SmtpPort;

        private static string MailAccount = ConfigurationManager.AppSettings["mailAccount"];
        private static string MailPassword = ConfigurationManager.AppSettings["mailPassword"];

        // key = local path to the image, value = body
        public static void SendEmail(string to_email_address, string email_html, List<LinkedResource> email_resources)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress(FromAddress);
                mail.To.Add(to_email_address);
                mail.Subject = Subject;

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(email_html.ToString(), Encoding.Default, "text/html");

                foreach (LinkedResource lr in email_resources)
                    htmlView.LinkedResources.Add(lr);

                mail.AlternateViews.Add(htmlView);

                //var smtp = new SmtpClient(SmtpHost, SmtpPort);
                //smtp.Send(mail);

                // Init SmtpClient and send
                SmtpClient smtpClient = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));
                System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(MailAccount, MailPassword);
                smtpClient.Credentials = credentials;

                smtpClient.Send(mail);

                email_resources.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Failed to send DREExportEmail: {0}", ex.Message));
            }
        }

    }
}
