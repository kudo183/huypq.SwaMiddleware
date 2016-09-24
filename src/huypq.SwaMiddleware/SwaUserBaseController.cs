using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace huypq.SwaMiddleware
{
    public class SwaUserBaseController<ContextType, DtoType, EntityType, UserEntityType> : SwaEntityBaseController<ContextType, DtoType, EntityType, UserEntityType>
        where UserEntityType : SwaUser, new ()
        where ContextType : DbContext, SwaIDbContext<UserEntityType>
        where DtoType : SwaIDto<EntityType>, new()
        where EntityType : class, SwaIEntity
    {
        public override SwaActionResult ActionInvoker(string actionName, Dictionary<string, object> parameter)
        {
            SwaActionResult result = null;

            switch (actionName)
            {
                case "token":
                    result = Token(parameter["json"].ToString());
                    break;
                case "register":
                    result = Register(parameter["json"].ToString());
                    break;
                default:
                    break;
            }

            return result;
        }

        private class JsonParameterModel
        {
            public string User { get; set; }
            public string Password { get; set; }

            public static JsonParameterModel FromJson(string json)
            {
                var result = SwaSettings.Instance.JsonSerializer.Deserialize<JsonParameterModel>(json);
                return result;
            }
        }

        public SwaActionResult Register(string json)
        {
            var model = JsonParameterModel.FromJson(json);

            if (DBContext.User.Any(p => p.Email == model.User))
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Conflict);
            }
            var hasher = new huypq.Crypto.PasswordHash();
            var entity = new UserEntityType()
            {
                Email = model.User,
                PasswordHash = hasher.HashedBase64String(model.Password),
                NgayTao = DateTime.UtcNow
            };
            DBContext.User.Add(entity);
            return SaveChanges();
        }

        public SwaActionResult Token(string json)
        {
            var model = JsonParameterModel.FromJson(json);
            if (model == null)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }

            var entity = DBContext.User.FirstOrDefault(p => p.Email == model.User);
            if (entity == null)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }

            var result = huypq.Crypto.PasswordHash.VerifyHashedPassword(entity.PasswordHash, model.Password);
            if (result == false)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.Unauthorized);
            }

            return CreateJsonResult(new SwaTokenModel() { User = entity.Email, UserId = entity.Ma });
        }
    }
}
