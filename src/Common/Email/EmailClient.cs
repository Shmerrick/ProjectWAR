using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Net;
using System;
using AuthenticationServer;

namespace AuthenticationServer.Email
{
    public class EmailClient
    {
        public string EmailServer;
        public string User;
        public string Pass;
        public string Address;

        public SmtpClient Client { get; set; }
        public MailMessage MM { get; set; }

        public EmailClient(string emailServer, string user, string pass, string addr)
        {
            EmailServer = emailServer;
            User = user;
            Pass = pass;
            Address = addr;
            Client = new SmtpClient(emailServer);
            Client.Credentials = new NetworkCredential(user, pass);

            MM = new MailMessage();
            MM.From = new MailAddress(addr);
        }

        public static void SendMail(EmailEventArgs e)
        {
            bool single = e.Single;

            if (single)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendSingal), e);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendMultiple), e);
            }

            return;
        }

        private static void SendMultiple(object e)
        {
            EmailEventArgs eea = (EmailEventArgs)e;

            List<MailAddress> emails = (List<MailAddress>)eea.Emails;
            string sub = (string)eea.Subject;
            string msg = (string)eea.Message;
            SmtpClient _client = eea.Client.Client;
            MailMessage _mm = eea.Client.MM;
            _mm.Subject = eea.Subject;
            for (int i = 0; i < emails.Count; ++i)
            {
                MailAddress ma = (MailAddress)emails[i];

                _mm.To.Add(ma);
            }

            _mm.Subject += " - " + sub;
            _mm.Body = msg;

            try
            {
                _client.Send(_mm);
            }
            catch { }
            _mm.To.Clear();
            _mm.Body = "";
            _mm.Subject = eea.Subject;

            return;
        }

        private static void SendSingal(object e)
        {
            EmailEventArgs eea = (EmailEventArgs)e;

            string to = (string)eea.To;
            string sub = (string)eea.Subject;
            string msg = (string)eea.Message;
            SmtpClient _client = eea.Client.Client;
            MailMessage _mm = eea.Client.MM;
            _mm.Subject = eea.Subject;
            _mm.To.Add(to);
            _mm.Subject += " " + sub;
            _mm.Body = msg;

            try
            {
                _client.Send(_mm);
            }
            catch (Exception ex) { Console.WriteLine(ex); }
            Console.WriteLine("Email sent");

            _mm.To.Clear();
            _mm.Body = "";
            _mm.Subject = eea.Subject;

            return;
        }
    }

    public class EmailEventArgs
    {
        public bool Single;
        public List<MailAddress> Emails;
        public string To;
        public string Subject;
        public string Message;
        public EmailClient Client;

        public EmailEventArgs(bool single, List<MailAddress> list, string to, string sub, string msg, EmailClient client)
        {
            Single = single;
            Emails = list;
            To = to;
            Subject = sub;
            Message = msg;
            Client = client;
        }
    }
}