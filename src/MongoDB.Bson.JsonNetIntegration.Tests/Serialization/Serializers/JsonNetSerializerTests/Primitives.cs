using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using Shouldly;

namespace MongoDB.Bson.Serialization.Serializers.JsonNetSerializerTests
{
    [TestFixture]
    public class Primitives : Base
    {
        [Test]
        public void Should_serialize_primitives()
        {
            var foo = new Foo
            {
                A = 5,
                B = "Yay!",
                C = DateTime.SpecifyKind(new DateTime(2014, 1, 1), DateTimeKind.Utc),
                D = 7,
                E = 5.2m,
                F = 2.5
            };

            var json = SerializeToJson(foo);

            var expected = "{ \"a\" : 5, \"B\" : \"Yay!\", \"C\" : ISODate(\"2014-01-01T00:00:00Z\"), \"D\" : NumberLong(7), \"E\" : 5.2, \"F\" : 2.5 }";
            json.ShouldBe(expected);
        }

        [Test]
        public void Should_deserialize_primitives()
        {
            var json = "{ \"a\" : 5, \"B\" : \"Yay!\", \"C\" : ISODate(\"2014-01-01T00:00:00Z\"), \"D\" : NumberLong(7), \"E\" : 5.2, \"F\" : 2.5 }";

            var foo = DeserializeFromJson<Foo>(json);

            foo.A.ShouldBe(5);
            foo.B.ShouldBe("Yay!");
            foo.C.ShouldBe(DateTime.SpecifyKind(new DateTime(2014, 1, 1), DateTimeKind.Utc));
            foo.D.ShouldBe(7);
            foo.E.ShouldBe(5.2m);
            foo.F.ShouldBe(2.5);
        }

        private class Foo
        {
            [JsonProperty("a")]
            public int A { get; set; }

            public string B { get; set; }

            public DateTime C { get; set; }

            public long D { get; set; }

            public decimal E { get; set; }

            public double F { get; set; }
        }

    }
}