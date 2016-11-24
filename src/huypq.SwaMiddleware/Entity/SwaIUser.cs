namespace huypq.SwaMiddleware
{
    public interface SwaIUser
    {
        int ID { get; }
        string Email { get; set; }
        string PasswordHash { get; set; }
        System.DateTime CreateDate { get; set; }
    }
}
