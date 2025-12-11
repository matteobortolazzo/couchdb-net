using System;
using System.Collections.Generic;

namespace CouchDB.UnitTests.Models;

public class Battle
{
    public bool DidWin { get; set; }
    public string Planet { get; set; }
    public DateTime Date { get; set; }
    public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}