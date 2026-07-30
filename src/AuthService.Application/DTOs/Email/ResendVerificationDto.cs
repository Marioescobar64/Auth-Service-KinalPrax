using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs.Email;

    public class ResetPasswordDto
    {
        public string Token { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
    }