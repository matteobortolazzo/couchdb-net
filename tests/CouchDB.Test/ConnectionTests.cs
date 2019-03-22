using CouchDB.Client;
using CouchDB.Test.Models;
using System.Linq;
using Xunit;

namespace CouchDB.Test
{
    public class ConnectionTest
    {
        [Fact]
        public void CreateConnection()
        {
            using (var connection = new CouchConnection(""))
            {
                QueryProvider provider = new CouchQueryProvider(connection);

                var houses = new Query<House>(provider);

                var query = houses
                    .Where(h =>
                    h.Owner.Name == "Bobby" &&
                    (h.Floors.All(f => f.Area < 120) || h.Floors.Any(f => f.Area > 500)))
                    .Skip(0)
                    .Take(50);


                var list = query.ToList();
            }
        }
    }
}