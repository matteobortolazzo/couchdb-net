using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using CouchDB.Client.Query.Sort;
using Flurl.Http;
using Newtonsoft.Json;

namespace CouchDB.Client.Query
{
    public interface ICouchQueryable<TSource> where TSource : CouchEntity
    {
        IOrderedCouchQueryable<TSource> OrderBy<TProperty>(Expression<Func<TSource, TProperty>> keySelector);
        IOrderedCouchQueryable<TSource> OrderByDescending<TProperty>(Expression<Func<TSource, TProperty>> keySelector);
        ICouchQueryable<TSource> Select<TProperty>(params Func<TSource, TProperty>[] selectors);
        ICouchQueryable<TSource> Skip(int count);
        ICouchQueryable<TSource> Take(int count);
        Task<List<TSource>> ToListAsync();
    }

    public interface IOrderedCouchQueryable<TSource> : ICouchQueryable<TSource> where TSource : CouchEntity
    {
        IOrderedCouchQueryable<TSource> ThenBy<TProperty>(Expression<Func<TSource, TProperty>> keySelector);
        IOrderedCouchQueryable<TSource> ThenByDescending<TProperty>(Expression<Func<TSource, TProperty>> keySelector);
    }
    
    internal class CouchQueryable<TSource> : IOrderedCouchQueryable<TSource> where TSource : CouchEntity
    {
        private readonly IDictionary<string, object> _selector;
        private readonly IFlurlRequest _baseRequest;
        private readonly string _dbName;

        private int _takeCount = 25;
        private int _skipCount;

        protected List<SortProperty> SortProperties = null;

        internal CouchQueryable(IDictionary<string, object> selector, IFlurlRequest baseRequest, string dbName)
        {
            _selector = selector;
            _baseRequest = baseRequest;
            _dbName = dbName;
        }

        public IOrderedCouchQueryable<TSource> OrderBy<TProperty>(Expression<Func<TSource, TProperty>> keySelector)
        {
            var propName = GetPropertyInfo(keySelector);
            SortProperties = new List<SortProperty>{ new SortProperty(propName, true) };
            return this;
        }

        public IOrderedCouchQueryable<TSource> OrderByDescending<TProperty>(Expression<Func<TSource, TProperty>> keySelector)
        {
            var propName = GetPropertyInfo(keySelector);
            SortProperties = new List<SortProperty> { new SortProperty(propName, false) };
            return this;
        }

        public ICouchQueryable<TSource> Select<TProperty>(params Func<TSource, TProperty>[] selectors)
        {
            return this;
        }

        public ICouchQueryable<TSource> Skip(int count)
        {
            _skipCount = count;
            return this;
        }

        public ICouchQueryable<TSource> Take(int count)
        {
            _takeCount = count;
            return this;
        }

        public async Task<List<TSource>> ToListAsync()
        {
            var j = JsonConvert.SerializeObject(_selector);

            var result = await _baseRequest
                .AppendPathSegment(_dbName)
                .AppendPathSegment("_find")
                .PostJsonAsync(new
                {
                    selector = _selector,
                    limit = _takeCount,
                    skip = _skipCount
                })
                .ReceiveJson<FindResult>();

            return result.Docs;
        }
        
        private class FindResult
        {
            [JsonProperty("docs")]
            public List<TSource> Docs { get; set; }
        }

        public string GetPropertyInfo<TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var type = typeof(TSource);

            if (!(propertyLambda.Body is MemberExpression member))
                throw new ArgumentException(
                    $"Expression '{propertyLambda}' refers to a method, not a property.");

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(
                    $"Expression '{propertyLambda}' refers to a field, not a property.");

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType ?? throw new InvalidOperationException()))
                throw new ArgumentException(
                    $"Expression '{propertyLambda}' refers to a property that is not from type {type}.");

            var jsonPropertyAttributes = propInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
            var jsonProperty = jsonPropertyAttributes.Length > 0 ? jsonPropertyAttributes[0] as JsonPropertyAttribute : null;

            var propName = jsonProperty != null ? jsonProperty.PropertyName : propInfo.Name;

            return propName;
        }

        public IOrderedCouchQueryable<TSource> ThenBy<TProperty>(Expression<Func<TSource, TProperty>> keySelector)
        {
            var propName = GetPropertyInfo(keySelector);
            SortProperties.Add(new SortProperty(propName, true));
            return this;
        }

        public IOrderedCouchQueryable<TSource> ThenByDescending<TProperty>(Expression<Func<TSource, TProperty>> keySelector)
        {
            var propName = GetPropertyInfo(keySelector);
            SortProperties.Add(new SortProperty(propName, false));
            return this;
        }
    }
}
