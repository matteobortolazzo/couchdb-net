using System.Linq;
using System.Threading.Tasks;
using CouchDB.Driver.Example.Models;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Options;

namespace CouchDB.Driver.Example;

public class MyDeathStarContext : CouchContext
{
    public MyDeathStarContext(CouchOptions<MyDeathStarContext> options)
        : base(options)
    {
        SeedDataAsync().GetAwaiter().GetResult();
    }

    private async Task SeedDataAsync()
    {
        var docs = await Rebels.ToListAsync();
        if (docs.All(d => d.Id != "luke"))
        {
            await Rebels.AddAsync(new Rebel
            {
                Id = "luke",
                Name = "Luke",
                Surname = "Skywalker",
                Age = 19
            });
        }

        if (docs.All(d => d.Id != "leia"))
        {
            await Rebels.AddAsync(new Rebel
            {
                Id = "leia",
                Name = "Leia",
                Surname = "Organa",
                Age = 19
            });
        }
    }

    public CouchDatabase<Rebel> Rebels { get; set; }
}