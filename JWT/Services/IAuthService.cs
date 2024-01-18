using JWT.Dtos;
using JWT.Models;

namespace JWT.Services
{
	public interface IAuthService
	{
		Task<AuthenticationDto> RegisterAsync(RegisterDto model);
		Task<AuthenticationDto> GetTokenAsync(LoginDto model);
		Task<string> AddRoleAsync(RoleDto model);
	}
}
