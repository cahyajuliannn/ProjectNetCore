using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace LearnNetCore.Verify
{
    public class Verification
    {
        public string EmailV = "your email";
        public string PasswordV = "your password";
        public string VerificationCode;
    }

    public class RandomGenerator
    {
        private Random Rand = new Random();
        public int GenerateRandom()
        {
            return Rand.Next(1000, 9999);
        }
    }
    public class ServiceEmail
    {
        //RandomGenerator randomGenerator = new RandomGenerator();
        public void SendEmail(string sendEmail, string theCode)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            mail.To.Add(new MailAddress(sendEmail));
            mail.From = new MailAddress("cjdeveloper123@gmail.com", "NET CORE API");
            mail.Subject = "VERIFICATION CORE" +DateTime.Now.ToString();
            mail.Body = "Dear User, <br><br>Please sign up using this code to your application :<br><br><b>" + theCode + "</b><br><br> Thank you, <br> CJ Education";
            mail.IsBodyHtml = true;

            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Port = 587;
            SmtpServer.Credentials = new NetworkCredential("cjdeveloper123@gmail.com", "b1smillah123!");
            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);

        }
    }
}
