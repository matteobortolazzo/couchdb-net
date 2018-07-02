using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using CouchDB.Client.Query.Extensions;
using CouchDB.Client.Query.Sort;

namespace CouchDB.Client.Query
{
    public interface ICouchIndexSelector<T> where T : CouchEntity
    {
        ICouchAscendingIndexSelector<T> Ascending(Expression<Func<T, object>> fieldSelector);
        ICouchDescendingIndexSelector<T> Descending(Expression<Func<T, object>> fieldSelector);
    }

    public interface ICouchAscendingIndexSelector<T> : ICouchIndexSelector<T> where T : CouchEntity
    {
        ICouchAscendingIndexSelector<T> ThenAscending(Expression<Func<T, object>> fieldSelector);
    }

    public interface ICouchDescendingIndexSelector<T> : ICouchIndexSelector<T> where T : CouchEntity
    {
        ICouchDescendingIndexSelector<T> ThenDescending(Expression<Func<T, object>> fieldSelector);
    }

    internal class CouchIndexSelector<T> : ICouchIndexSelector<T>, ICouchAscendingIndexSelector<T>, ICouchDescendingIndexSelector<T> where T : CouchEntity
    {
        internal List<SortProperty> IndexFields;

        public ICouchAscendingIndexSelector<T> Ascending(Expression<Func<T, object>> fieldSelector)
        {
            var propertyName = fieldSelector.GetJsonPropertyName();
            IndexFields = new List<SortProperty> { new SortProperty(propertyName, true) };
            return this;
        }

        public ICouchDescendingIndexSelector<T> Descending(Expression<Func<T, object>> fieldSelector)
        {
            var propertyName = fieldSelector.GetJsonPropertyName();
            IndexFields = new List<SortProperty> { new SortProperty(propertyName, false) };
            return this;
        }

        public ICouchAscendingIndexSelector<T> ThenAscending(Expression<Func<T, object>> fieldSelector)
        {
            var propertyName = fieldSelector.GetJsonPropertyName();
            IndexFields.Add(new SortProperty(propertyName, true));
            return this;
        }

        public ICouchDescendingIndexSelector<T> ThenDescending(Expression<Func<T, object>> fieldSelector)
        {
            var propertyName = fieldSelector.GetJsonPropertyName();
            IndexFields.Add(new SortProperty(propertyName, false));
            return this;
        }
    }
}
