using CouchDB.Client;
using CouchDB.Test.Models;
using System.Linq;
using Xunit;
using CouchDB.Client.Extensions;
using System.Text.RegularExpressions;
using System;
using System.Linq.Expressions;

namespace CouchDB.Test
{
    public class ConnectionTest
    {
        [Fact]
        public void CreateConnection()
        {
            using (var client = new CouchClient("http://127.0.0.1:5984/"))
            {
                var houses = client.GetDatabase<House>();
                var customers = client.GetDatabase<Customer>();

                //const int one = 1;
                //Expression<Func<House, bool>> funcQ1 = t => t.Number == one;

                //var comparisonHouse = new House { Number = 1, Address = "AAA" };
                //Expression<Func<House, bool>> funcQ2 = t => t.Number >= comparisonHouse.Number;
                // const string book = "adsa";
                // var q = customers.Where(c => c.Name == "Matteo").ToList();
                var q2 = customers.ToList();
                // var x = customers.FindAsync("6bda3050d3d60002983be15d290006a9");

                //var flats = houses.AsQueryable()
                //   .Where(h => h.Type == HouseType.Flat).ToList();
                //.Where(h =>
                //    h.Owner.Name == "Bobby" &&
                //    h.Owner.Name != "AA"
                //    //(h.Floors.All(f => f.Area < 120) || h.Floors.Any(f => f.Area > 500)) &&
                //    //|| h.Numbers.ContainsAll(new[] { 1, 2 }) ||
                //    //h.Numbers.ContainsNone(new[] { 3, 4 }) && 
                //    //h.Address.FieldExists(true) && 
                //    //h.Address.IsCouchType(CouchType.String)
                //    //h.Numbers.In(new[] { 1, 2 }) &&
                //    //h.Numbers.NotIn(new[] { 3, 4 })
                //    //h.Numbers.Count == 3
                //    //h.Address.IsMatch("[a-zA-Z]{0,2}")
                //);
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

                //var json = query.ToString();
                //var result = query.ToList();
            }
        }
    }
}