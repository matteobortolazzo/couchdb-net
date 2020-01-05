using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CouchDB.Driver.Types
{
    public class CouchAttachmentsCollection : IEnumerable<CouchAttachment>
    {
        private Dictionary<string, CouchAttachment> _attachments;

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

        public void Add(FileInfo fileInfo, string contentType)
        {
            Add(fileInfo.Name, fileInfo, contentType);
        }

        public void Add(string attachmentName, FileInfo fileInfo, string contentType)
        {
            _attachments.Add(attachmentName, new CouchAttachment
            {
                Name = attachmentName,
                ContentType = contentType,
                FileInfo = fileInfo
            });
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
