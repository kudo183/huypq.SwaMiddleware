using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace huypq.SwaMiddleware
{
    public class SwaUserBaseController<ContextType, UserEntityType, DtoType> : SwaEntityBaseController<ContextType, UserEntityType, DtoType>
        where UserEntityType : class, SwaIUser, new()
        where ContextType : DbContext, SwaIDbContext<UserEntityType>
    {
        public override SwaActionResult ActionInvoker(string actionName, Dictionary<string, object> parameter)
        {
            SwaActionResult result = null;

            switch (actionName)
            {
                case "token":
                    result = Token(parameter["user"].ToString(), parameter["pass"].ToString());
                    break;
                case "register":
                    result = Register(parameter["user"].ToString(), parameter["pass"].ToString());
                    break;
                default:
                    break;
            }

            return result;
        }

        public override DtoType ConvertToDto(UserEntityType dto)
        {
            throw new NotImplementedException();
        }

        public override UserEntityType ConvertToEntity(DtoType dto)
        {
            throw new NotImplementedException();
        }

        public SwaActionResult Register(string user, string pass)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                return CreateStatusResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (DBContext.User.Any(p => p.Email == user))
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Conflict);
            }
            var hasher = new huypq.Crypto.PasswordHash();
            var entity = new UserEntityType()
            {
                Email = user,
                PasswordHash = hasher.HashedBase64String(pass),
                NgayTao = DateTime.UtcNow
            };
            DBContext.User.Add(entity);
            return SaveChanges();
        }

        public SwaActionResult Token(string user, string pass)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }

            var entity = DBContext.User.FirstOrDefault(p => p.Email == user);
            if (entity == null)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }

            var result = huypq.Crypto.PasswordHash.VerifyHashedPassword(entity.PasswordHash, pass);
            if (result == false)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }

            return CreateObjectResult(new SwaTokenModel() { User = entity.Email, UserId = entity.Ma });
        }
    }
}
