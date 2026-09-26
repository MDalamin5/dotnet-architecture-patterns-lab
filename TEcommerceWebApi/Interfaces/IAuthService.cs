using System.Threading.Tasks;
using TEcommerceWebApi.DTOs;

namespace TEcommerceWebApi.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(AuthRegisterDto registerData);
        Task<AuthResponseDto?> LoginAsync(AuthLoginDto loginData);
    }
}