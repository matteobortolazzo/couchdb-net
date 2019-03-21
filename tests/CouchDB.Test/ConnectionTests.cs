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
            using(var connection = new CouchConnection(""))
            {
                QueryProvider provider = new CouchQueryProvider(connection);

                var customers = new Query<Customer>(provider);

                var query = customers.Where(c => 
                c.Card.Number == "0000" && c.Points.All(d => d == 0));

                var list = query.ToList();

                Assert.All(list, c => Assert.Equal("Matteo", c.Name));
            }
        }
    }
}
