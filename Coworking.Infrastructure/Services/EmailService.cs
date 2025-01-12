using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace Coworking.Infrastructure.Services;

    public interface IEmailService
    {
        Task SendReservationConfirmation(string toEmail, string reservationInfo);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendReservationConfirmation(string toEmail, string reservationInfo)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Coworking System", "noreply@coworking.com"));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Confirmación de Reserva";
            message.Body = new TextPart("plain")
            {
                Text = $"Tu reserva ha sido confirmada.\n\nDetalles:\n{reservationInfo}"
            };

            // Ajustar según el servidor SMTP a usar
            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.server.com", 587, false);
            await client.AuthenticateAsync("smtp_user", "smtp_password");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
