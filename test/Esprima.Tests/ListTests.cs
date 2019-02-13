using System;
using System.Linq;
using Esprima.Ast;
using Xunit;

namespace Esprima.Tests
{
    public class ListTests
    {
        public static TheoryData<int, int, Lazy<List<int>>>
            CreateTestData(int start, int count)
        {
            var sequence = Enumerable.Range(start, count);
            var array = sequence.ToArray();

            return new TheoryData<int, int, Lazy<List<int>>>
            {
                { start, count, Lazy.Create("Sequence"    , () => List.Create(sequence.AsEnumerable())) },
                { start, count, Lazy.Create("Collection"  , () => List.Create(new BreakingCollection<int>(array))) },
                { start, count, Lazy.Create("ReadOnlyList", () => List.Create(new BreakingReadOnlyList<int>(array))) },
            };
        }

        [Theory]
        [MemberData(nameof(CreateTestData), 1, 0)]
        [MemberData(nameof(CreateTestData), 1, 3)]
        [MemberData(nameof(CreateTestData), 1, 4)]
        [MemberData(nameof(CreateTestData), 1, 7)]
        [MemberData(nameof(CreateTestData), 1, 10)]
        [MemberData(nameof(CreateTestData), 1, 22)]
        public void Create(int start, int count, Lazy<List<int>> xs)
        {
            var list = xs.Value;

            Assert.Equal(count, list.Count);

            for (var i = 0; i < count; i++)
            {
                Assert.Equal(start + i, list[i]);
            }

            using (var e = list.GetEnumerator())
            {
                for (var i = 0; i < count; i++)
                {
                    Assert.True(e.MoveNext());
                    Assert.Equal(start + i, e.Current);
                }

                Assert.False(e.MoveNext());
            }
        }
    }
}