namespace Esprima.Tests;

public class ArrayListTests
{
    [Theory]
    [InlineData(new int[] { 1, 2, 3 }, -1, null)]
    [InlineData(new int[] { 1, 2, 3 }, 0, new int[] { 2, 3 })]
    [InlineData(new int[] { 1, 2, 3 }, 1, new int[] { 1, 3 })]
    [InlineData(new int[] { 1, 2, 3 }, 2, new int[] { 1, 2 })]
    [InlineData(new int[] { 1, 2, 3 }, 3, null)]
    public void RemoveAt(int[] items, int index, int[]? expectedItems)
    {
        var list = new ArrayList<int>(items);
        if (expectedItems is not null)
        {
            list.RemoveAt(index);
            Assert.Equal(expectedItems.AsEnumerable(), list.AsEnumerable());
        }
        else
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(index));
        }
    }
}
