using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;


namespace Reservation.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;

        public AuthService(IConfiguration config)
        {
            _config = config;
        }

        public Task<bool> ValidateCredentialAsync(string username, string password)
        {
            var configuredUser = _config["Auth:admin"];
            var configuredPass = _config["Auth:password"];

            return Task.FromResult(username == configuredUser && password == configuredPass);
        }
    }
}
