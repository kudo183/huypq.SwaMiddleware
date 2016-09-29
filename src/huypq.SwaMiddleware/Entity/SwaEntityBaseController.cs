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

        protected ContextType DBContext
        {
            get
            {
                return (ContextType)App.ApplicationServices.GetService(typeof(ContextType)); ;
            }
        }

        protected SwaActionResult SaveChanges()
        {
            try
            {
                DBContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return CreateStatusResult(System.Net.HttpStatusCode.InternalServerError);
            }
            //need return an json object, if just return status code, jquery will treat as fail.
            return CreateObjectResult("OK");
        }

        public void Dispose()
        {
            if (DBContext != null)
            {
                DBContext.Dispose();
            }
        }

        protected PagingResultDto<DtoType> GetAll(IQueryable<EntityType> includedQuery)
        {
            var result = new PagingResultDto<DtoType>();
            result.Items = new List<DtoType>();
            foreach (var entity in includedQuery)
            {
                result.Items.Add(ConvertToDto(entity));
            }

            result.TotalItemCount = result.Items.Count;
            result.PageCount = 1;
            result.PageIndex = 1;
            return result;
        }

        protected PagingResultDto<DtoType> Get(System.IO.Stream requestBody, IQueryable<EntityType> includedQuery)
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
            return Get(filter, includedQuery);
        }

        protected PagingResultDto<DtoType> Get(QueryExpression filter, IQueryable<EntityType> includedQuery)
        {
            int pageCount;

            var query = QueryExpression.AddQueryExpression(
                includedQuery, filter, SwaSettings.Instance.DefaultPageSize, out pageCount);

            var result = new PagingResultDto<DtoType>
            {
                PageIndex = filter.PageIndex,
                PageCount = pageCount,
                Items = new List<DtoType>()
            };

            foreach (var entity in query)
            {
                result.Items.Add(ConvertToDto(entity));
            }

            return result;
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
