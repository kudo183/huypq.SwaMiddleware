using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace huypq.SwaMiddleware
{
    public class SwaUserBaseController<ContextType, UserEntityType, GroupEntityType, UserGroupEntityType, DtoType> : SwaController, IDisposable
        where UserEntityType : class, SwaIUser, new()
        where GroupEntityType : class, SwaIGroup, new()
        where UserGroupEntityType : class, SwaIUserGroup, new()
        where ContextType : DbContext, SwaIDbContext<UserEntityType, GroupEntityType, UserGroupEntityType>
    {
        protected ContextType DBContext
        {
            get
            {
                var context = (ContextType)Context.RequestServices.GetService(typeof(ContextType));
                return context;
            }
        }

        public override SwaActionResult ActionInvoker(string actionName, Dictionary<string, object> parameter)
        {
            SwaActionResult result = null;

            switch (actionName)
            {
                case "token":
                    result = Token(parameter["user"].ToString(), parameter["pass"].ToString(), parameter["group"].ToString());
                    break;
                case "getgroups":
                    result = GetGroups(parameter["user"].ToString());
                    break;
                case "register":
                    result = Register(parameter["user"].ToString(), parameter["pass"].ToString(), parameter["group"].ToString());
                    break;
                case "createuser":
                    result = CreateUser(parameter["user"].ToString(), parameter["pass"].ToString());
                    break;
                default:
                    break;
            }

            return result;
        }

        public void Dispose()
        {
            if (DBContext != null)
            {
                DBContext.Dispose();
            }
        }

        public SwaActionResult CreateUser(string user, string pass)
        {
            if (TokenModel.IsGroupOwner == false)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.BadRequest);
            }

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
            DBContext.SaveChanges();
            DBContext.UserGroup.Add(new UserGroupEntityType()
            {
                LaChuGroup = false,
                MaUser = entity.Ma,
                MaGroup = TokenModel.GroupId
            });
            DBContext.SaveChanges();
            return CreateObjectResult("OK");
        }

        public SwaActionResult Register(string user, string pass, string group)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                return CreateStatusResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (DBContext.User.Any(p => p.Email == user))
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Conflict);
            }

            if (DBContext.Group.Any(p => p.TenGroup == group))
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

            var groupEntity = new GroupEntityType()
            {
                TenGroup = group,
                NgayTao = DateTime.UtcNow
            };
            DBContext.Group.Add(groupEntity);

            DBContext.SaveChanges();

            DBContext.UserGroup.Add(new UserGroupEntityType()
            {
                LaChuGroup = true,
                MaUser = entity.Ma,
                MaGroup = groupEntity.Ma
            });
            DBContext.SaveChanges();

            return CreateObjectResult("OK");
        }

        public SwaActionResult Token(string user, string pass, string group)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }

            var userEntity = DBContext.User.FirstOrDefault(p => p.Email == user);
            if (userEntity == null)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }

            var groupEntity = DBContext.Group.FirstOrDefault(p => p.TenGroup == group);
            if (groupEntity == null)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }

            var userGroupEntity = DBContext.UserGroup.FirstOrDefault(p => p.MaUser == userEntity.Ma && p.MaGroup == groupEntity.Ma);
            if (userGroupEntity == null)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }

            var result = huypq.Crypto.PasswordHash.VerifyHashedPassword(userEntity.PasswordHash, pass);
            if (result == false)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }

            return CreateObjectResult(new SwaTokenModel() { User = userEntity.Email, UserId = userEntity.Ma, GroupId = groupEntity.Ma, IsGroupOwner = userGroupEntity.LaChuGroup });
        }

        public SwaActionResult GetGroups(string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                return CreateObjectResult("");
            }

            var userEntity = DBContext.User.FirstOrDefault(p => p.Email == user);
            if (userEntity == null)
            {
                return CreateObjectResult("");
            }

            var maGroups = DBContext.UserGroup.Where(p => p.MaUser == userEntity.Ma).Select(p => p.MaGroup);
            if (maGroups == null)
            {
                return CreateObjectResult("");
            }

            var groups = DBContext.Group.Where(p => maGroups.Contains(p.Ma));

            var result = string.Empty;
            foreach (var item in groups.Select(p => p.TenGroup))
            {
                result = result + item + "*&*";
            }

            return CreateObjectResult(result);
        }
    }
}
