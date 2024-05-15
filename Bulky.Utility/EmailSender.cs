using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public class EmailSender : IEmailSender
    {

        //public string SendGridSecret { get; set; }
        //public EmailSender(IConfiguration _config)
        //{
        //    SendGridSecret = _config.GetValue<string>("SendGrid:SecretKey");
        //}
        //Task IEmailSender.SendEmailAsync(string email, string subject, string htmlMessage)
        //{
        //    var Client =new  SendGridClient(SendGridSecret);
        //    var from = new EmailAddress("hello@dotnetmastery.com", "Bullky book");
        //    var to = new EmailAddress(email);
        //    var message = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);
        //    return Client.SendEmailAsync(message);
        //}
       

     

        Task IEmailSender.SendEmailAsync(string email, string subject, string htmlMessage)
        { 
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse("hello@dotnetmastery.com"));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                client.Authenticate("dotnetmastery@gmail.com", "DotNet123$");
                client.Send(message);
                client.Disconnect(true);
            }
            return Task.CompletedTask;
        }
    }
}
