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

        public void AddOrUpdate(FileInfo fileInfo)
        {
            AddOrUpdate(fileInfo.Name, fileInfo, null);
        }

        public void AddOrUpdate(string attachmentName, FileInfo fileInfo)
        {
            AddOrUpdate(attachmentName, fileInfo, null);
        }

        public void AddOrUpdate(FileInfo fileInfo, string contentType)
        {
            AddOrUpdate(fileInfo.Name, fileInfo, contentType);
        }

        public void AddOrUpdate(string attachmentName, FileInfo fileInfo, string contentType)
        {
            if (!_attachments.ContainsKey(attachmentName))
            {
                _attachments.Add(attachmentName, new CouchAttachment());
            }

            var attachment = _attachments[attachmentName];
            attachment.Name = attachmentName;
            attachment.FileInfo = fileInfo;
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
    }
}
