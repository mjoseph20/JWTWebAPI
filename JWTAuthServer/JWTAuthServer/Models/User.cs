using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace JWTAuthServer.Models
{
    [Index(nameof(Email), Name = "IX_Unique_Email", IsUnique = true)]
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
