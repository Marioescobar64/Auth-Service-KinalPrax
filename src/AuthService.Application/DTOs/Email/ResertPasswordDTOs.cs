


using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs.Email
{
    public class ResendVerificationDto
    {
        public string Email { get; set; } = string.Empty;
    }
}