using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
namespace TicketDesk.Localization
{
    public class CommonHelper
    {
        /// <summary>
        ///     Format nội dung gửi mail khi User quên mật khẩu 
        /// </summary>
        /// <param name="EmailTo"></param>
        /// <param name="Content"></param>
        /// <param name="Name"></param>
        /// <param name="Subject"></param>
        /// <param name="imgUrl"></param>
        /// <param name="attachment"></param>
        /// <returns></returns>
        public static bool SendContentMail(string EmailTo, string Content, string Name, string Subject, string imgUrl = "", string attachment = "")
        {
            bool isOk = false;
            try
            {

                string email = ConfigurationManager.AppSettings["EmailSend"];
                string passWord = ConfigurationManager.AppSettings["PassEmailSend"];
                string smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
                int port = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
                int sslPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpSslPort"]);
                if (email != "" && passWord != "")
                {
                    MailMessage mail = new MailMessage(email, EmailTo);
                    mail.Subject = Subject;
                    mail.Body = Content;
                    mail.IsBodyHtml = true;
                    if (!string.IsNullOrEmpty(imgUrl))
                        mail.Body = string.Format("<div><img src='{0}'/><div>", imgUrl);

                    SmtpClient client = new SmtpClient();
                    client.Port = port;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(email, passWord);
                    client.Host = smtpServer;
                    client.Timeout = 10000;
                    client.EnableSsl = true;
                    if (!string.IsNullOrEmpty(attachment))
                        mail.Attachments.Add(new System.Net.Mail.Attachment(attachment));
                    client.Send(mail);
                    isOk = true;
                }
            }
            catch (Exception ex)
            {
                //NSLog.Logger.Error("Send Mail Error", ex);
            }
            return isOk;
        }
    }
}
