using System;
using System.Threading.Tasks;
using CouchDB.Client;
using Newtonsoft.Json;

namespace CouchDB.Test
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new CouchClient("http://127.0.0.1:5984");
            // .UseSSL()
            client.ConfigureAuthentication("root", "relax");

            // await client.AddDatabaseAsync("cars");

            var carsDb = await client.GetDatabaseAsync<Car>("cars");
            var e81 = await carsDb.FindAsync<Car>(JsonConvert.SerializeObject(new
            {
                selector = new {
                    Brand = "BMW"
                }
            }));
            // await carsDb.AddDocumentAsync("E81", new Car { Brand = "BMW", CC = 2000, Seats = 4 });
        }
    }
}
