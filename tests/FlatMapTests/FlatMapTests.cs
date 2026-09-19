using FishPieClient.Utils;

namespace FlatMapTests;

public class FlatMapTests
{
    
    private static FlatMap<Key, int> FlatMapFactory(int count)
    {
        var map = new FlatMap<Key, int>();
        for (int i = 0; i < count; i++)
        {
            var key = new Key((uint)i);
            map.Add(key, i);
        }

        return map;
    }

    [Test]
    public void MoveNext_AfterDisposal()
    {
        // Disposal of enumerator is equivalent to reset call
        IEnumerator<KeyValuePair<Key, int>> enumerator = FlatMapFactory(5).GetEnumerator();
        for (int i = 0; i < 5; i++)
        {
            enumerator.MoveNext();
        }

        enumerator.Dispose();
        Assert.That(enumerator.MoveNext(), Is.True);
    }

    [Test]
    public void Constructor_Capacity()
    {
        var map = new FlatMap<Key, int>(10);
        Assert.That(map, Has.Count.EqualTo(0));
        Assert.That(map.Capacity, Is.EqualTo(10));
    }

    [Test]
    public void Capacity_NegativeValue_ThrowsArgumentOutOfRangeException()
    {
        var map = new FlatMap<Key, int>(10);
        int capacityBefore = map.Capacity;
        Assert.Throws<ArgumentOutOfRangeException>(() => map.Capacity = -1);
        Assert.That(capacityBefore, Is.EqualTo(map.Capacity));
    }
    
    [Test]
    public void Capacity_LessThanCount_ThrowsArgumentOutOfRangeException()
    {
        var map = new FlatMap<Key, int>();
        for (int i = 0; i < 10; i++)
        {
            map.Add(new Key((uint)i), i);
            Assert.Throws<ArgumentOutOfRangeException>(() => map.Capacity = i);
        }
    }

    [Test]
    public void Capacity_GrowsDuringAAdds()
    {
        var map = new FlatMap<Key, int>();
        int capacity = 4;
        for (int i = 0; i < 10; i++)
        {
            map.Add(new Key((uint)i), i);

            if (i == capacity)
            {
                capacity *= 2;
            }

            if (i <= capacity + 1)
            {
                Assert.That(map.Capacity, Is.EqualTo(capacity));
            }
            else
            {
                Assert.That(map.Capacity, Is.EqualTo(i));
            }
        }
    }

    [Test]
    public void GetKeyAtIndex_EveryIndex()
    {
        var map = FlatMapFactory(10);
        for (int i = 0; i < 10; i++)
        {
            Assert.That(map.IndexOfKey(map.GetKeyAtIndex(i)), Is.EqualTo(i));
        }
    }
    
}