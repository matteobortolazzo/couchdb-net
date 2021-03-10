using CouchDB.Driver.UnitTests.Models;
using System.Linq;
using Xunit;

namespace CouchDB.Driver.UnitTests.Find
{
    public class Find_Discriminator
    {
        private const string _databaseName = "allrebels";
        private readonly ICouchDatabase<Rebel> _rebels;
        private readonly ICouchDatabase<SimpleRebel> _simpleRebels;

        public Find_Discriminator()
        {
            var client = new CouchClient("http://localhost");
            _rebels = client.GetDatabase<Rebel>(_databaseName, nameof(Rebel));
            _simpleRebels = client.GetDatabase<SimpleRebel>(_databaseName, nameof(SimpleRebel));
        }

        [Fact]
        public void Discriminator_WithoutFilter()
        {
            var json1 = _rebels.ToString();
            var json2 = _simpleRebels.ToString();
            Assert.Equal(@"{""selector"":{""split_discriminator"":""Rebel""}}", json1);
            Assert.Equal(@"{""selector"":{""split_discriminator"":""SimpleRebel""}}", json2);
        }

        [Fact]
        public void Discriminator_WithFilter()
        {
            var json1 = _rebels.Where(c => c.Age == 19).ToString();
            var json2 = _simpleRebels.Where(c => c.Age == 19).ToString();
            Assert.Equal(@"{""selector"":{""$and"":[{""age"":19},{""split_discriminator"":""Rebel""}]}}", json1);
            Assert.Equal(@"{""selector"":{""$and"":[{""age"":19},{""split_discriminator"":""SimpleRebel""}]}}", json2);
        }
    }
}
