using Microsoft.EntityFrameworkCore;

namespace huypq.SwaMiddleware
{
    public interface SwaIDbContext<T> where T : class, SwaIUser
    {
        DbSet<T> User { get; set; }
    }
}
