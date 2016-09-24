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

    public class SwaUserDto : SwaIDto<SwaUser>
    {
        public int Ma { get; set; }
        public string Email { get; set; }
        public System.DateTime NgayTao { get; set; }

        public void FromEntity(SwaUser entity)
        {
            Ma = entity.Ma;
            Email = entity.Email;
            NgayTao = entity.NgayTao;
        }

        public SwaUser ToEntity()
        {
            return new SwaUser()
            {
                Ma = Ma,
                Email = Email,
                NgayTao = NgayTao
            };
        }
    }
}
