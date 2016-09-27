using Microsoft.EntityFrameworkCore;
using QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace huypq.SwaMiddleware
{
    public abstract class SwaEntityBaseController<ContextType, EntityType> : SwaController, IDisposable
        where ContextType : DbContext
        where EntityType : class, SwaIEntity
    {
        #region define class
        public class PagingResult<T>
        {
            public int TotalItemCount { get; set; }
            public int PageIndex { get; set; }
            public int PageCount { get; set; }
            public List<T> Items { get; set; }
        }

        public class ChangeState
        {
            public const string Insert = "a";
            public const string Delete = "d";
            public const string Update = "u";
        }

        public class ChangedItem<T>
        {
            public string State { get; set; }
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
            return CreateJsonResult("OK");
        }

        public void Dispose()
        {
            if (DBContext != null)
            {
                DBContext.Dispose();
            }
        }

        protected PagingResult<EntityType> GetAll(IQueryable<EntityType> includedQuery)
        {
            var result = new PagingResult<EntityType>();
            result.Items = includedQuery.ToList();

            result.TotalItemCount = result.Items.Count;
            result.PageCount = 1;
            result.PageIndex = 1;
            return result;
        }

        protected PagingResult<EntityType> Get(string json, IQueryable<EntityType> includedQuery)
        {
            var filter = SwaSettings.Instance.JsonSerializer.Deserialize<QueryExpression>(json);

            return Get(filter, includedQuery);
        }

        protected PagingResult<EntityType> Get(QueryExpression filter, IQueryable<EntityType> includedQuery)
        {
            int pageCount;

            var query = QueryExpression.AddQueryExpression(
                includedQuery, filter, SwaSettings.Instance.DefaultPageSize, out pageCount);

            var result = new PagingResult<EntityType>
            {
                PageIndex = filter.PageIndex,
                PageCount = pageCount,
                Items = query.ToList()
            };

            return result;
        }

        protected SwaActionResult Save(string json)
        {
            var items = SwaSettings.Instance.JsonSerializer.Deserialize<List<ChangedItem<EntityType>>>(json);

            return Save(items);
        }

        protected SwaActionResult Save(List<ChangedItem<EntityType>> items)
        {
            foreach (var changeItem in items)
            {
                var entity = changeItem.Data;

                switch (changeItem.State)
                {
                    case ChangeState.Insert:
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
