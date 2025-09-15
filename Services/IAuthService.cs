using System.Threading.Tasks;

namespace Reservation.Services
{
    public interface IAuthService
    {
        Task<bool> ValidateCredentialAsync(string username, string password);

    }
}
