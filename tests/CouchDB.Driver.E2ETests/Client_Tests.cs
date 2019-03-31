using CouchDB.Driver.E2E.Models;
using CouchDB.Driver.Types;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Driver.E2E
{
    public class E2E
    {
        [Fact]
        public async Task Crud()
        {
            using (var client = new CouchClient("http://localhost:5984"))
            {
                var dbs = await client.GetDatabasesNamesAsync();
                var rebels = client.GetDatabase<Rebel>();

                if (dbs.Contains(rebels.Database))
                {
                    await client.DeleteDatabaseAsync<Rebel>();
                }

                rebels = await client.CreateDatabaseAsync<Rebel>();

                var luke = await rebels.CreateAsync(new Rebel { Name = "Luke", Age = 19 });
                Assert.Equal("Luke", luke.Name);

                luke.Surname = "Skywalker";
                luke = await rebels.CreateOrUpdateAsync(luke);
                Assert.Equal("Skywalker", luke.Surname);

                luke = await rebels.FindAsync(luke.Id);
                Assert.Equal(19, luke.Age);

                await rebels.DeleteAsync(luke);
                luke = await rebels.FindAsync(luke.Id);
                Assert.Null(luke);

                await client.DeleteDatabaseAsync<Rebel>();
            }
        }
        [Fact]
        public async Task Users()
        {
            using (var client = new CouchClient("http://localhost:5984"))
            {
                var dbs = await client.GetDatabasesNamesAsync();
                var users = client.GetUsersDatabase();

                if (!dbs.Contains(users.Database))
                {
                    users = await client.CreateDatabaseAsync<CouchUser>();
                }
                
                var luke = await users.CreateAsync(new CouchUser(name: "luke", password: "lasersword"));
                Assert.Equal("luke", luke.Name);
                
                luke = await users.FindAsync(luke.Id);
                Assert.Equal("luke", luke.Name);

                await users.DeleteAsync(luke);
                luke = await users.FindAsync(luke.Id);
                Assert.Null(luke);

                await client.DeleteDatabaseAsync<CouchUser>();
            }
        }
    }
}
