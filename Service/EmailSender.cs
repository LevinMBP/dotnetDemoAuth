using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace DemoAuth.Service
{
    public class EmailSender : IEmailSender
    {
        private readonly string _smtpServer;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly int _smtpPort;
        private readonly bool _useSSL;
        public EmailSender(IConfiguration configuration)
        {
            _smtpServer = configuration["EmailSettings:SmtpServer"];
            _smtpUsername = configuration["EmailSettings:SmtpUsername"];
            _smtpPassword = configuration["EmailSettings:SmtpPassword"];
            _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
            _useSSL = bool.Parse(configuration["EmailSettings:UseSSL"]);
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("No Reply", _smtpUsername)); // Set sender name and address
            message.To.Add(new MailboxAddress("", email)); // Set recipient email
            message.Subject = subject;


            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using var client = new SmtpClient();
                {
                    // Connect to the SMTP server
                    // await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                    await client.ConnectAsync(_smtpServer, _smtpPort, _useSSL ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);

                    // Authenticate if necessary
                    await client.AuthenticateAsync(_smtpUsername, _smtpPassword);

                    // Send the email
                    await client.SendAsync(message);

                    // Disconnect and quit
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;  // Optionally rethrow the exception
            }
        }
    }
}