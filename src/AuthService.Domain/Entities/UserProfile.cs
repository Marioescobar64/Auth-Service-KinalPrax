using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Domain.Entities
{
    public class UserProfile
    {
        public string Id { get; set; } = default!;

        public string UserId { get; set; } = default!;

        public string? ProfilePicture { get; set; }

        public string? Phone { get; set; }

        public User? User { get; set; }
    }
}


