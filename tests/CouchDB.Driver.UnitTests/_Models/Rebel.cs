using System;
using System.Collections.Generic;

namespace CouchDB.UnitTests.Models;

public class SimpleRebel : CouchDocument
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class Rebel : CouchDocument
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public int Age { get; set; }
    public bool IsJedi { get; set; }
    public Species Species { get; set; }
    public Guid Guid { get; set; }
    public List<string> Skills { get; set; }
    public List<Battle> Battles { get; set; }
    public Vehicle Vehicle { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is Rebel r)
        {
            return r.Id == Id;
        }
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}