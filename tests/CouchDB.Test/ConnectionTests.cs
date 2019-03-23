using CouchDB.Client;
using CouchDB.Test.Models;
using System.Linq;
using Xunit;
using CouchDB.Client.Extensions;
using System.Text.RegularExpressions;

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
                        //h.Owner.Name == "Bobby" &&
                        //(h.Floors.All(f => f.Area < 120) || h.Floors.Any(f => f.Area > 500)) &&
                        //|| h.Numbers.ContainsAll(new[] { 1, 2 }) ||
                        //h.Numbers.ContainsNone(new[] { 3, 4 }) && 
                        //h.Address.FieldExists(true) && 
                        //h.Address.IsCouchType(CouchType.String)
                        //h.Numbers.In(new[] { 1, 2 }) &&
                        //h.Numbers.NotIn(new[] { 3, 4 })
                        //h.Numbers.Count == 3
                        h.Address.IsMatch("[a-zA-Z]{0,2}")
                    );
                    //.OrderByDescending(h => h.Owner.Name)
                    //.ThenByDescending(h => h.ConstructionDate)
                    //.Skip(0)
                    //.Take(50)
                    //.Select(h =>
                    //    new
                    //    {
                    //        h.Owner.Name,
                    //        h.Address,
                    //        h.ConstructionDate
                    //    })
                    //.UseBookmark("g1AAAABweJzLY...")
                    //.WithReadQuorum(150)
                    //.UpdateIndex(true)
                    //.FromStable(true)
                    //.UseIndex("design_document", "index_name");

                var list = query.ToList();
            }
        }
    }
}