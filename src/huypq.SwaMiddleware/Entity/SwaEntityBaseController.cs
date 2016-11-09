using Microsoft.EntityFrameworkCore;
using QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace huypq.SwaMiddleware
{
    public abstract class SwaEntityBaseController<ContextType, EntityType, DtoType> : SwaController, IDisposable
        where ContextType : DbContext
        where EntityType : class, SwaIEntity
    {
        #region define class
        [ProtoBuf.ProtoContract]
        public class PagingResultDto<T>
        {
            [ProtoBuf.ProtoMember(1)]
            public int TotalItemCount { get; set; }
            [ProtoBuf.ProtoMember(2)]
            public int PageIndex { get; set; }
            [ProtoBuf.ProtoMember(3)]
            public int PageCount { get; set; }
            [ProtoBuf.ProtoMember(4)]
            public List<T> Items { get; set; }
            [ProtoBuf.ProtoMember(5)]
            public long VersionNumber { get; set; }
            [ProtoBuf.ProtoMember(6)]
            public string ErrorMsg { get; set; }
        }

        public class ChangeState
        {
            public const string Add = "a";
            public const string Delete = "d";
            public const string Update = "u";
        }

        [ProtoBuf.ProtoContract]
        public class ChangedItem<T>
        {
            [ProtoBuf.ProtoMember(1)]
            public string State { get; set; }
            [ProtoBuf.ProtoMember(2)]
            public T Data { get; set; }
        }
        #endregion

        private static long VersionNumber = DateTime.UtcNow.Ticks;

        protected ContextType DBContext
        {
            get
            {
                var context = (ContextType)Context.RequestServices.GetService(typeof(ContextType));
                return context;
            }
        }

        protected SwaActionResult SaveChanges()
        {
            try
            {
                var changeCount = DBContext.SaveChanges();
                if (changeCount > 0)
                {
                    System.Threading.Interlocked.Increment(ref VersionNumber);
                }
            }
            catch (Exception ex)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.InternalServerError);
            }
            //need return an json object, if just return status code, jquery will treat as fail.
            return CreateObjectResult("OK");
        }

        public override SwaActionResult ActionInvoker(string actionName, Dictionary<string, object> parameter)
        {
            SwaActionResult result = null;

            switch (actionName)
            {
                case "get":
                    result = Get(parameter["body"] as System.IO.Stream, GetQuery());
                    break;
                case "save":
                    result = Save(parameter["body"] as System.IO.Stream);
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

        protected virtual IQueryable<EntityType> GetQuery()
        {
            return DBContext.Set<EntityType>().Where(p => p.MaGroup == TokenModel.GroupId);
        }

        protected SwaActionResult Get(System.IO.Stream requestBody, IQueryable<EntityType> includedQuery)
        {
            QueryExpression filter = null;
            switch (RequestObjectType)
            {
                case "json":
                    filter = SwaSettings.Instance.JsonSerializer.Deserialize<QueryExpression>(requestBody);
                    break;
                case "protobuf":
                    filter = SwaSettings.Instance.BinarySerializer.Deserialize<QueryExpression>(requestBody);
                    break;
            }
            return CreateObjectResult(Get(filter, includedQuery));
        }

        protected PagingResultDto<DtoType> Get(QueryExpression filter, IQueryable<EntityType> includedQuery)
        {
            int pageCount = 1;
            int pageIndex = 1;
            var query = includedQuery;
            var result = new PagingResultDto<DtoType>
            {
                Items = new List<DtoType>()
            };

            result.VersionNumber = VersionNumber;
            if (result.VersionNumber == filter.VersionNumber)
            {
                return result;
            }

            if (filter != null && filter.PageIndex > 0)//paging
            {
                var pageSize = GetPageSize();
                query = QueryExpression.AddQueryExpression(
                query, filter, pageSize, out pageCount);
                pageIndex = filter.PageIndex;
            }
            else//no paging
            {
                if (filter != null)
                {
                    query = WhereExpression.AddWhereExpression(query, filter.WhereOptions);
                }

                var itemCount = query.Count();
                var maxItem = GetMaxItemAllowed();

                if (itemCount > maxItem)
                {
                    result.ErrorMsg = "Entity set too large, please use paging";
                    return result;
                }
            }

            result.PageIndex = pageIndex;
            result.PageCount = pageCount;

            foreach (var entity in query)
            {
                result.Items.Add(ConvertToDto(entity));
            }

            return result;
        }

        protected virtual int GetMaxItemAllowed()
        {
            return SwaSettings.Instance.MaxItemAllowed;
        }

        protected virtual int GetPageSize()
        {
            return SwaSettings.Instance.DefaultPageSize;
        }

        protected SwaActionResult Save(System.IO.Stream requestBody)
        {
            List<ChangedItem<DtoType>> items = null;
            switch (RequestObjectType)
            {
                case "json":
                    items = SwaSettings.Instance.JsonSerializer.Deserialize<List<ChangedItem<DtoType>>>(requestBody);
                    break;
                case "protobuf":
                    items = SwaSettings.Instance.BinarySerializer.Deserialize<List<ChangedItem<DtoType>>>(requestBody);
                    break;
            }

            return Save(items);
        }

        public abstract EntityType ConvertToEntity(DtoType dto);
        public abstract DtoType ConvertToDto(EntityType entity);

        protected SwaActionResult Save(List<ChangedItem<DtoType>> items)
        {
            foreach (var changeItem in items)
            {
                var entity = ConvertToEntity(changeItem.Data);

                switch (changeItem.State)
                {
                    case ChangeState.Add:
                        DBContext.Set<EntityType>().Add(entity);
                        break;
                    case ChangeState.Update:
                        DBContext.Entry(entity).State = EntityState.Modified;
                        break;
                    case ChangeState.Delete:
                        DBContext.Set<EntityType>().Remove(entity);
                        break;
                    default:
                        return CreateStatusResult(System.Net.HttpStatusCode.InternalServerError);
                }
            }

            return SaveChanges();
        }
    }
}
