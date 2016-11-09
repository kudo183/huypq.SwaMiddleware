namespace huypq.SwaMiddleware
{
    public interface SwaIUser
    {
        int Ma { get; }
        string Email { get; set; }
        string PasswordHash { get; set; }
        System.DateTime NgayTao { get; set; }
    }
}
