using System.ComponentModel.DataAnnotations;

namespace JWTAuthServer.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string ClientId { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string ClientName { get; set; } = string.Empty;
        [Required]
        [MaxLength(200)]
        public string ClientURL { get; set; } = string.Empty;
    }
}
