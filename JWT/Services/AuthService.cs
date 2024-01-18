using JWT.Dtos;
using JWT.Helpers;
using JWT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWT.Services
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly HelperJWT helperJWT;
		private readonly RoleManager<IdentityRole> roleManager;

		public AuthService(UserManager<ApplicationUser> userManager,
			IOptions<HelperJWT> helperJWT,
			RoleManager<IdentityRole> roleManager)
		{
			this.userManager = userManager;
			this.helperJWT = helperJWT.Value;
			this.roleManager = roleManager;
		}
		public async Task<string> AddRoleAsync(RoleDto model)
		{
			var user = await userManager.FindByNameAsync(model.Username);

			if (user is null || !await roleManager.RoleExistsAsync(model.Role))
				return "Invalid user ID or Role";

			if (await userManager.IsInRoleAsync(user, model.Role))
				return "User already assigned to this role";

			var result = await userManager.AddToRoleAsync(user, model.Role);

			return result.Succeeded ? string.Empty : "Something went wrong";
		}

		public async Task<AuthenticationDto> GetTokenAsync(LoginDto model)
		{
			var authModel = new AuthenticationDto();

			var user = await userManager.FindByEmailAsync(model.Email);

			if (user is null || !await userManager.CheckPasswordAsync(user, model.Password))
			{
				authModel.Message = "Email or Password is incorrect!";
				return authModel;
			}

			var jwtSecurityToken = await CreateJwtToken(user);
			var rolesList = await userManager.GetRolesAsync(user);

			authModel.IsAuthenticated = true;
			authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
			authModel.Email = user.Email;
			authModel.Username = user.UserName;
			authModel.ExpiresOn = jwtSecurityToken.ValidTo;
			authModel.Roles = rolesList.ToList();

			return authModel;
		}

		public async Task<AuthenticationDto> RegisterAsync(RegisterDto model)
		{
			if (await userManager.FindByEmailAsync(model.Email) is not null)
				return new AuthenticationDto { Message = "Email is exist" };

			if (await userManager.FindByNameAsync(model.Username) is not null)
				return new AuthenticationDto { Message = "Username is exist" };

			var user = new ApplicationUser
			{
				FirstName = model.FirstName,
				LastName = model.LastName,
				UserName = model.Username,
				Email = model.Email,
			};

			var result = await userManager.CreateAsync(user, model.Password);

			if (!result.Succeeded)
			{
				var errors = string.Empty;

				foreach (var error in result.Errors)
				{
					errors += $"{error.Description},";
				}
				return new AuthenticationDto { Message = "Error" };
			}

			await userManager.AddToRoleAsync(user, "User");

			var jwtSecurityToken = await CreateJwtToken(user);

			return new AuthenticationDto
			{
				Email = user.Email,
				ExpiresOn = jwtSecurityToken.ValidTo,
				IsAuthenticated = true,
				Roles = new List<string> { "User" },
				Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
				Username = user.UserName
			};
		}

		private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
		{
			var userClaims = await userManager.GetClaimsAsync(user);
			var roles = await userManager.GetRolesAsync(user);
			var roleClaims = new List<Claim>();

			foreach (var role in roles)
				roleClaims.Add(new Claim("roles", role));

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim("uid", user.Id)
			}
			.Union(userClaims)
			.Union(roleClaims);

			var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(helperJWT.Key));
			var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

			var jwtSecurityToken = new JwtSecurityToken(
				issuer: helperJWT.Issuer,
				audience: helperJWT.Audience,
				claims: claims,
				expires: DateTime.Now.AddDays(helperJWT.DurationInDays),
				signingCredentials: signingCredentials);

			return jwtSecurityToken;
		}
	}
}
