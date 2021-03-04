using System;
using System.Collections.Generic;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Options
{
    public class CouchDatabaseBuilder
    {
        internal readonly Dictionary<Type, CouchDocumentBuilder> DocumentBuilders;

        internal CouchDatabaseBuilder()
        {
            DocumentBuilders = new Dictionary<Type, CouchDocumentBuilder>();
        }

        public CouchDocumentBuilder<TSource> Document<TSource>()
            where TSource : CouchDocument
        {
            Type documentType = typeof(TSource);
            if (!DocumentBuilders.ContainsKey(documentType))
            {
                DocumentBuilders.Add(documentType, new CouchDocumentBuilder<TSource>());
            }

            return (CouchDocumentBuilder<TSource>)DocumentBuilders[documentType];
        }
    }
}