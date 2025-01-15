using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Coworking.Infrastructure.Services
{
    public interface IEmailService
    {
        Task SendReservationConfirmationAsync(string toEmail, string reservationDetails);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendReservationConfirmationAsync(string toEmail, string reservationDetails)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Coworking System", _configuration["EmailSettings:FromEmail"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Confirmación de Reserva";
            message.Body = new TextPart("plain")
            {
                Text = $"Tu reserva ha sido confirmada.\n\nDetalles:\n{reservationDetails}"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _configuration["EmailSettings:SmtpServer"],
                int.Parse(_configuration["EmailSettings:SmtpPort"]),
                bool.Parse(_configuration["EmailSettings:UseSsl"])
            );

            await client.AuthenticateAsync(
                _configuration["EmailSettings:SmtpUsername"],
                _configuration["EmailSettings:SmtpPassword"]
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}