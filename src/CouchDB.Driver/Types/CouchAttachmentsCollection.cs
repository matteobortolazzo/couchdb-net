using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CouchDB.Driver.Types
{
    public class CouchAttachmentsCollection : IEnumerable<CouchAttachment>
    {
        internal Dictionary<string, CouchAttachment> _attachments;

        internal CouchAttachmentsCollection() 
        {
            _attachments = new Dictionary<string, CouchAttachment>();
        }

        internal CouchAttachmentsCollection(Dictionary<string, CouchAttachment> attachments)
        {
            _attachments = attachments;
            foreach (var item in _attachments)
            {
                item.Value.Name = item.Key;
            }
        }

        public void AddOrUpdate(string path, string contentType)
        {
            var info = GetFileInfo(path, contentType);
            AddOrUpdate(info.Name, path, contentType);
        }

        public void AddOrUpdate(string attachmentName, string path, string contentType)
        {
            var info = GetFileInfo(path, contentType);

            if (!_attachments.ContainsKey(attachmentName))
            {
                _attachments.Add(attachmentName, new CouchAttachment());
            }

            var attachment = _attachments[attachmentName];
            attachment.Name = attachmentName;
            attachment.FileInfo = info;
            attachment.ContentType = contentType;
        }

        public void Delete(string attachmentName)
        {
            var attachment = _attachments[attachmentName];
            attachment.Deleted = true;
        }

        public CouchAttachment this[string key]
        {
            get => _attachments[key];
        }

        public IEnumerator<CouchAttachment> GetEnumerator()
        {
            return _attachments
                .Select(kv => kv.Value)
                .Where(at => !at.Deleted)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static FileInfo GetFileInfo(string path, string contentType)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentNullException(nameof(contentType));
            }
            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"File does not exists: {path}");
            }

            return new FileInfo(path);
        }
    }
}
