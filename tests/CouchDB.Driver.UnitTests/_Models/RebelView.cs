using CouchDB.Driver.UnitTests.Models;
using CouchDB.Driver.Views;

#nullable disable

namespace CouchDB.Driver.UnitTests._Models
{
    public class RebelView : CouchView<string[], Rebel>
    {
        public int NumberOfBattles { get; set; }
    }
}
#nullable restore