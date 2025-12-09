using System;

namespace CouchDB.Driver.Helpers;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DatabaseNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}