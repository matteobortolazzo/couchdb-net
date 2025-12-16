using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace CouchDB.Driver.Types;

public sealed class CouchAttachmentsCollection : IReadOnlyList<CouchAttachment>
{
    private readonly Dictionary<string, CouchAttachment> _attachments;
    private List<CouchAttachment>? _cachedList;

    internal CouchAttachmentsCollection()
    {
        _attachments = new Dictionary<string, CouchAttachment>();
    }

    internal CouchAttachmentsCollection(Dictionary<string, CouchAttachment> attachments)
    {
        _attachments = attachments;
        foreach (KeyValuePair<string, CouchAttachment> item in _attachments)
        {
            item.Value.Name = item.Key;
        }
    }

    public void AddOrUpdate(string path, string contentType)
    {
        FileInfo info = GetFileInfo(path);
        AddOrUpdate(info.Name, path, contentType);
    }

    public void AddOrUpdate(string attachmentName, string path, string contentType)
    {
        FileInfo info = GetFileInfo(path);

        if (!_attachments.TryGetValue(attachmentName, out CouchAttachment? attachment))
        {
            attachment = new CouchAttachment();
            _attachments.Add(attachmentName, attachment);
        }

        attachment.Name = attachmentName;
        attachment.FileInfo = info;
        attachment.ContentType = contentType;
        _cachedList = null;
    }

    public void Delete(string attachmentName)
    {
        CouchAttachment attachment = _attachments[attachmentName];
        attachment.Deleted = true;
        _cachedList = null;
    }

    public CouchAttachment this[string key]
    {
        get => _attachments[key];
    }

    public CouchAttachment this[int index]
    {
        get
        {
            _cachedList ??= _attachments.Values.Where(at => !at.Deleted).ToList();
            return _cachedList[index];
        }
    }

    public int Count
    {
        get
        {
            _cachedList ??= _attachments.Values.Where(at => !at.Deleted).ToList();
            return _cachedList.Count;
        }
    }

    public IEnumerator<CouchAttachment> GetEnumerator()
    {
        _cachedList ??= _attachments.Values.Where(at => !at.Deleted).ToList();
        return _cachedList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    internal CouchAttachment[] GetAddedAttachments()
    {
        return _attachments.Values.ToArray();
    }

    internal CouchAttachment[] GetDeletedAttachments()
    {
        return _attachments
            .Where(kv => kv.Value.Deleted)
            .Select(kv => kv.Value)
            .ToArray();
    }

    internal void RemoveAttachment(CouchAttachment attachment)
    {
        _ = _attachments.Remove(attachment.Name);
        _cachedList = null;
    }

    private static FileInfo GetFileInfo(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return !File.Exists(path)
            ? throw new InvalidOperationException($"File does not exists: {path}")
            : new FileInfo(path);
    }
}