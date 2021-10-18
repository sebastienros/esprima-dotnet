using System;
using System.Globalization;
using System.Linq;
using Esprima.Ast;
using Xunit;

namespace Esprima.Tests
{
    public class NodeListTests
    {
        public static TheoryData<int, int, Lazy<NodeList<Literal>>>
            CreateTestData(int start, int count)
        {
            var array =
                Enumerable
                    .Range(start, count)
                    .Select(x => new Literal(x, x.ToString(CultureInfo.InvariantCulture)))
                    .ToArray();

            return new TheoryData<int, int, Lazy<NodeList<Literal>>> { { start, count, Lazy.Create("Sequence", () => NodeList.Create(array.Select(x => x))) }, { start, count, Lazy.Create("Collection", () => NodeList.Create(new BreakingCollection<Literal>(array))) }, { start, count, Lazy.Create("ReadOnlyList", () => NodeList.Create(new BreakingReadOnlyList<Literal>(array))) } };
        }

        [Theory]
        [MemberData(nameof(CreateTestData), 1, 0)]
        [MemberData(nameof(CreateTestData), 1, 3)]
        [MemberData(nameof(CreateTestData), 1, 4)]
        [MemberData(nameof(CreateTestData), 1, 7)]
        [MemberData(nameof(CreateTestData), 1, 10)]
        [MemberData(nameof(CreateTestData), 1, 22)]
        public void Create(int start, int count, Lazy<NodeList<Literal>> xs)
        {
            var list = xs.Value;

            Assert.Equal(count, list.Count);

            for (var i = 0; i < count; i++)
            {
                Assert.Equal(start + i, list[i].NumericValue);
            }

            using (var e = list.GetEnumerator())
            {
                for (var i = 0; i < count; i++)
                {
                    Assert.True(e.MoveNext());
                    Assert.Equal(start + i, e.Current.NumericValue);
                }

                Assert.False(e.MoveNext());
            }
        }
    }
}
