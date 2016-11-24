namespace huypq.SwaMiddleware
{
    public interface SwaIUserGroup
    {
        bool IsGroupOwner { get; set; }
        int ID { get; set; }
        int GroupID { get; set; }
        int UserID { get; set; }
    }
}
