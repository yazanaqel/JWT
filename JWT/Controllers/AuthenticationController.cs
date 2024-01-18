using JWT.Dtos;
using JWT.Models;
using JWT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JWT.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly IAuthService authService;

		public AuthenticationController(IAuthService authService)
        {
			this.authService = authService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterDto model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await authService.RegisterAsync(model);

			if (!result.IsAuthenticated)
				return BadRequest(result.Message);

			return Ok(result);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await authService.GetTokenAsync(model);

			if (!result.IsAuthenticated)
				return BadRequest(result.Message);

			return Ok(result);
		}

		[Authorize(Roles = "Admin")]
		[HttpPost("addrole")]
		public async Task<IActionResult> AddRole([FromBody] RoleDto model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await authService.AddRoleAsync(model);

			if (!string.IsNullOrEmpty(result))
				return BadRequest(result);

			return Ok(model);
		}
	}
}
