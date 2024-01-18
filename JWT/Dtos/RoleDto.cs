using System.ComponentModel.DataAnnotations;

namespace JWT.Dtos;

public class RoleDto
{
	[Required]
	public string Username { get; set; } = string.Empty;

	[Required]
	public string Role { get; set; } = string.Empty;
}
