using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace huypq.SwaMiddleware
{
    public static class SwaServiceCollectionExtensions
    {
        /// <summary>
        /// With Token Authentication and sql Trusted connection
        /// </summary>
        /// <typeparam name="ContextType"></typeparam>
        /// <typeparam name="UserEntity"></typeparam>
        /// <param name="services"></param>
        /// <param name="dbName">Sql connection string to store user login info</param>
        /// <param name="tokenEncryptKeyDirectoryPath">Directory to store key use to encrypt Token</param>
        /// <returns></returns>
        public static IServiceCollection AddSwaWithTrustedConnection<ContextType, UserEntity, GroupEntity, UserGroupEntity>(
            this IServiceCollection services, string dbName, string tokenEncryptKeyDirectoryPath)
            where UserEntity : class, SwaIUser
            where GroupEntity : class, SwaIGroup
            where UserGroupEntity : class, SwaIUserGroup
            where ContextType : DbContext, SwaIDbContext<UserEntity, GroupEntity, UserGroupEntity>
        {
            var connection = string.Format(@"Server=.;Database={0};Trusted_Connection=True;", dbName);

            return AddSwa<ContextType, UserEntity, GroupEntity, UserGroupEntity>(services, connection, tokenEncryptKeyDirectoryPath);
        }

        /// <summary>
        /// With Token Authentication
        /// </summary>
        /// <typeparam name="ContextType"></typeparam>
        /// <typeparam name="UserEntity"></typeparam>
        /// <param name="services"></param>
        /// <param name="connection">Sql connection string to store user login info</param>
        /// <param name="tokenEncryptKeyDirectoryPath">Directory to store key use to encrypt Token</param>
        /// <returns></returns>
        public static IServiceCollection AddSwa<ContextType, UserEntity, GroupEntity, UserGroupEntity>(
            this IServiceCollection services, string connection, string tokenEncryptKeyDirectoryPath)
            where UserEntity : class, SwaIUser
            where GroupEntity : class, SwaIGroup
            where UserGroupEntity : class, SwaIUserGroup
            where ContextType : DbContext, SwaIDbContext<UserEntity, GroupEntity, UserGroupEntity>
        {
            services.AddDbContext<ContextType>(options => options.UseSqlServer(connection), ServiceLifetime.Scoped);
            services.AddDataProtection()
                .PersistKeysToFileSystem(new System.IO.DirectoryInfo(tokenEncryptKeyDirectoryPath))
                .ProtectKeysWithDpapi();
            return AddSwa(services);
        }

        /// <summary>
        /// Without Token Authentication and sql Trusted connection
        /// </summary>
        /// <typeparam name="ContextType"></typeparam>
        /// <typeparam name="UserEntity"></typeparam>
        /// <param name="services"></param>
        /// <param name="dbName">Sql connection string to store user login info</param>
        /// <param name="tokenEncryptKeyDirectoryPath">Directory to store key use to encrypt Token</param>
        /// <returns></returns>
        public static IServiceCollection AddSwaWithTrustedConnection<ContextType>(
            this IServiceCollection services, string dbName)
            where ContextType : DbContext
        {
            var connection = string.Format(@"Server=.;Database={0};Trusted_Connection=True;", dbName);

            return AddSwa<ContextType>(services, connection);
        }

        /// <summary>
        /// Without Token Authentication
        /// </summary>
        /// <typeparam name="ContextType"></typeparam>
        /// <typeparam name="UserEntity"></typeparam>
        /// <param name="services"></param>
        /// <param name="connection">Sql connection string to store user login info</param>
        /// <param name="tokenEncryptKeyDirectoryPath">Directory to store key use to encrypt Token</param>
        /// <returns></returns>
        public static IServiceCollection AddSwa<ContextType>(
            this IServiceCollection services, string connection)
            where ContextType : DbContext
        {
            services.AddDbContext<ContextType>(options => options.UseSqlServer(connection));
            return AddSwa(services);
        }

        /// <summary>
        /// without authentication
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSwa(this IServiceCollection services)
        {
            services.AddRouting();

            return services;
        }
    }
}
