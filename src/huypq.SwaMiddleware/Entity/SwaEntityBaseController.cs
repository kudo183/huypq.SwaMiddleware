using Microsoft.EntityFrameworkCore;
using QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace huypq.SwaMiddleware
{
    public abstract class SwaEntityBaseController<ContextType, DtoType, EntityType, UserEntityType> : SwaController, IDisposable
        where UserEntityType : SwaUser
        where ContextType : DbContext, SwaIDbContext<UserEntityType>
        where DtoType : SwaIDto<EntityType>, new()
        where EntityType : class, SwaIEntity
    {
        #region define class
        protected class PagingResult
        {
            public int TotalItemCount { get; set; }
            public int PageIndex { get; set; }
            public int PageCount { get; set; }
            public List<DtoType> Items { get; set; }
        }

        protected class ChangeState
        {
            public const string Insert = "a";
            public const string Delete = "d";
            public const string Update = "u";
        }

        protected class ChangedItem
        {
            public string State { get; set; }
            public DtoType Data { get; set; }
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
            return CreateJsonResult("OK");
        }

        public void Dispose()
        {
            if (DBContext != null)
            {
                DBContext.Dispose();
            }
        }

        public SwaActionResult GetAll(IQueryable<EntityType> includedQuery)
        {
            var result = new PagingResult();
            result.Items = includedQuery.AsEnumerable().Select(p =>
            {
                var dto = new DtoType();
                dto.FromEntity(p);
                return dto;
            }).ToList();

            result.TotalItemCount = result.Items.Count;
            result.PageCount = 1;
            result.PageIndex = 1;
            return CreateJsonResult(result);
        }

        protected SwaActionResult Get(string json, IQueryable<EntityType> includedQuery)
        {
            var filter = SwaSettings.Instance.JsonSerializer.Deserialize<QueryExpression>(json);

            return Get(filter, includedQuery);
        }

        protected SwaActionResult Get(QueryExpression filter, IQueryable<EntityType> includedQuery)
        {
            int pageCount;

            var query = QueryExpression.AddQueryExpression(
                includedQuery, filter, SwaSettings.Instance.DefaultPageSize, out pageCount);

            var result = new PagingResult
            {
                PageIndex = filter.PageIndex,
                PageCount = pageCount,
                Items = query.AsEnumerable().Select(p =>
                {
                    var a = new DtoType();
                    a.FromEntity(p);
                    return a;
                }).ToList()
            };

            return CreateJsonResult(result);
        }

        protected SwaActionResult Save(string json)
        {
            var items = SwaSettings.Instance.JsonSerializer.Deserialize<List<ChangedItem>>(json);

            return Save(items);
        }

        protected SwaActionResult Save(List<ChangedItem> items)
        {
            foreach (var changeItem in items)
            {
                var dto = changeItem.Data;

                switch (changeItem.State)
                {
                    case ChangeState.Insert:
                        DBContext.Set<EntityType>().Add(dto.ToEntity());
                        break;
                    case ChangeState.Update:
                        DBContext.Entry(dto.ToEntity()).State = EntityState.Modified;
                        break;
                    case ChangeState.Delete:
                        DBContext.Set<EntityType>().Remove(dto.ToEntity());
                        break;
                    default:
                        return CreateStatusResult(System.Net.HttpStatusCode.InternalServerError);
                }
            }

            return SaveChanges();
        }
    }
}
