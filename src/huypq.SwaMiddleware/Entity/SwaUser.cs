namespace huypq.SwaMiddleware
{
    public interface SwaIUser : SwaIEntity
    {
        string Email { get; set; }
        string PasswordHash { get; set; }
        System.DateTime NgayTao { get; set; }
    }
}
