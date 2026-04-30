using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Domain.Entities;

public class UserPasswordReset
{
    [Key]
    [MaxLength(16)]
    public string Id { get; set; } = null!;
 
    [Required]
    [MaxLength(16)]
    [ForeignKey(nameof(User))] // Clave foránea que referencia a la entidad User, indicando que cada restablecimiento de contraseña está asociado a un usuario específico.
    public string UserId { get; set; } = string.Empty;
 
    [MaxLength(256)]
    public string? PasswordResetToken { get; set; } = null!;
 
    public DateTime? PasswordResetTokenExpiry { get; set; } = null!;
 
    [Required]
    public User User { get; set; } = null!;
}