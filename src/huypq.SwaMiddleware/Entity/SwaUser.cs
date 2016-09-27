using System.ComponentModel.DataAnnotations;

namespace huypq.SwaMiddleware
{
    public class SwaUser : SwaIEntity
    {
        [Key]
        public int Ma { get; set; }
        [Required]
        [MaxLength(256)]
        public string Email { get; set; }
        [Required]
        [MaxLength(128)]
        public string PasswordHash { get; set; }
        public System.DateTime NgayTao { get; set; }
    }
}
