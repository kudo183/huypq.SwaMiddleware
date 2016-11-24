using Microsoft.EntityFrameworkCore;

namespace huypq.SwaMiddleware
{
    public interface SwaIDbContext<T, T1, T2>
        where T : class, SwaIUser
        where T1 : class, SwaIGroup
        where T2 : class, SwaIUserGroup
    {
        DbSet<T> SwaUser { get; set; }
        DbSet<T1> SwaGroup { get; set; }
        DbSet<T2> SwaUserGroup { get; set; }
    }
}
