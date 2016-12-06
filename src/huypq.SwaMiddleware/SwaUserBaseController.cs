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
                case "login":
                    result = Login(parameter["user"].ToString(), parameter["pass"].ToString());
                    break;
                case "accesstoken":
                    result = Token(parameter["group"].ToString());
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

            if (DBContext.SwaUser.Any(p => p.Email == user))
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Conflict);
            }
            var hasher = new huypq.Crypto.PasswordHash();
            var entity = new UserEntityType()
            {
                Email = user,
                PasswordHash = hasher.HashedBase64String(pass),
                CreateDate  = DateTime.UtcNow
            };
            DBContext.SwaUser.Add(entity);
            DBContext.SaveChanges();
            DBContext.SwaUserGroup.Add(new UserGroupEntityType()
            {
                IsGroupOwner = false,
                UserID = entity.ID,
                GroupID = TokenModel.GroupId
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

            if (DBContext.SwaUser.Any(p => p.Email == user))
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Conflict);
            }

            if (DBContext.SwaGroup.Any(p => p.GroupName == group))
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Conflict);
            }

            var hasher = new huypq.Crypto.PasswordHash();
            var entity = new UserEntityType()
            {
                Email = user,
                PasswordHash = hasher.HashedBase64String(pass),
                CreateDate = DateTime.UtcNow
            };
            DBContext.SwaUser.Add(entity);

            var groupEntity = new GroupEntityType()
            {
                GroupName = group,
                CreateDate = DateTime.UtcNow
            };
            DBContext.SwaGroup.Add(groupEntity);

            DBContext.SaveChanges();

            DBContext.SwaUserGroup.Add(new UserGroupEntityType()
            {
                IsGroupOwner = true,
                UserID = entity.ID,
                GroupID = groupEntity.ID
            });
            DBContext.SaveChanges();

            return CreateObjectResult("OK");
        }

        public SwaActionResult Login(string user, string pass)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }

            var userEntity = DBContext.SwaUser.FirstOrDefault(p => p.Email == user);
            if (userEntity == null)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }
            
            var result = huypq.Crypto.PasswordHash.VerifyHashedPassword(userEntity.PasswordHash, pass);
            if (result == false)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }

            return CreateObjectResult(new SwaTokenModel() { User = userEntity.Email, UserId = userEntity.ID });
        }

        public SwaActionResult Token(string group)
        {
            var groupEntity = DBContext.SwaGroup.FirstOrDefault(p => p.GroupName == group);
            if (groupEntity == null)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }

            var userGroupEntity = DBContext.SwaUserGroup.FirstOrDefault(p => p.UserID == TokenModel.UserId && p.GroupID == groupEntity.ID);
            if (userGroupEntity == null)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }
            
            return CreateObjectResult(new SwaTokenModel() { User = TokenModel.User, UserId = TokenModel.UserId, GroupId = groupEntity.ID, IsGroupOwner = userGroupEntity.IsGroupOwner });
        }

        public SwaActionResult GetGroups(string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                return CreateObjectResult("");
            }

            var userEntity = DBContext.SwaUser.FirstOrDefault(p => p.Email == user);
            if (userEntity == null)
            {
                return CreateObjectResult("");
            }

            var maGroups = DBContext.SwaUserGroup.Where(p => p.UserID == userEntity.ID).Select(p => p.GroupID);
            if (maGroups == null)
            {
                return CreateObjectResult("");
            }

            var groups = DBContext.SwaGroup.Where(p => maGroups.Contains(p.ID));

            var result = string.Empty;
            foreach (var item in groups.Select(p => p.GroupName))
            {
                result = result + item + "*&*";
            }

            return CreateObjectResult(result);
        }
    }
}
