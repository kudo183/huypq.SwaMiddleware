using Microsoft.EntityFrameworkCore;

namespace huypq.SwaMiddleware
{
    public interface SwaIDbContext<T> where T : SwaUser
    {
        DbSet<T> User { get; set; }
    }
}
