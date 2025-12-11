using System;

namespace CouchDB.Driver.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DatabaseNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}