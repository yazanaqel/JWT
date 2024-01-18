﻿namespace JWT.Dtos;

public class AuthenticationDto
{
	public string Message { get; set; } = string.Empty;
	public bool IsAuthenticated { get; set; }
	public string Username { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public List<string> Roles { get; set; } = new List<string>();
	public string Token { get; set; } = string.Empty;
	public DateTime ExpiresOn { get; set; }
}
