using FishPieClient.Graphics.Buffers;

namespace BufferTests;

public class MultiBufferTests
{
    
    [Test]
    public void SingleWrite()
    {
        Span<byte> dataView = [0x0, 0x1, 0x2];
        
        var mb = new MultiBuffer<FakeBuffer<byte>, byte>((uint)dataView.Length, "test_buffer", null!,
            (size, name, _) => new FakeBuffer<byte>(size, name), 3);
        mb.Write(dataView, 0);

        var buffer = mb.Buffer;
        
        List<(byte[], int)> expected = [ (dataView.ToArray(), dataView.Length * 0) ];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(buffer.Size, Is.EqualTo(dataView.Length * 3));
            Assert.That(buffer.WriteCalls, Is.EquivalentTo(expected));
            Assert.That(buffer.Name, Is.EqualTo("test_buffer"));
            Assert.That(mb.OriginalSize, Is.EqualTo(dataView.Length));
        }

        mb.Dispose();
    }

    [Test]
    public void TripleWrite()
    {
        Span<byte> dataView = [0x0, 0x1, 0x2];
        
        var mb = new MultiBuffer<FakeBuffer<byte>, byte>((uint)dataView.Length, "test_buffer", null!,
            (size, name, _) => new FakeBuffer<byte>(size, name), 3);
        mb.Write(dataView, 0);
        mb.Advance();
        mb.Write(dataView, 0);
        mb.Advance();
        mb.Write(dataView, 0);

        var buffer = mb.Buffer;
        
        List<(byte[], int)> expected =
        [
            (dataView.ToArray(), dataView.Length * 0),
            (dataView.ToArray(), dataView.Length * 1),
            (dataView.ToArray(), dataView.Length * 2),
        ];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(buffer.Size, Is.EqualTo(dataView.Length * 3));
            Assert.That(buffer.WriteCalls, Is.EquivalentTo(expected));
        }
    }
    
    [Test]
    public void QuadWrite()
    {
        Span<byte> dataView = [0x0, 0x1, 0x2];
        
        var mb = new MultiBuffer<FakeBuffer<byte>, byte>((uint)dataView.Length, "test_buffer", null!,
            (size, name, _) => new FakeBuffer<byte>(size, name), 3);
        mb.Write(dataView, 0);
        mb.Advance();
        mb.Write(dataView, 0);
        mb.Advance();
        mb.Write(dataView, 0);
        mb.Advance();
        mb.Write(dataView, 0);

        var buffer = mb.Buffer;
        
        List<(byte[], int)> expected =
        [
            (dataView.ToArray(), dataView.Length * 0),
            (dataView.ToArray(), dataView.Length * 1),
            (dataView.ToArray(), dataView.Length * 2),
            (dataView.ToArray(), dataView.Length * 0)
        ];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(buffer.Size, Is.EqualTo(dataView.Length * 3));
            Assert.That(buffer.WriteCalls, Is.EquivalentTo(expected));
        }
    }

    [Test]
    public void MultiWriteOffset()
    {
        Span<byte> dataView = [0x0, 0x1, 0x2, 0x3, 0x4];
        
        var mb = new MultiBuffer<FakeBuffer<byte>, byte>((uint)dataView.Length, "test_buffer", null!,
            (size, name, _) => new FakeBuffer<byte>(size, name), 3);
        mb.Write(dataView, 1);
        mb.Advance();
        mb.Write(dataView, 2);
        mb.Advance();
        mb.Write(dataView, 3);
        mb.Advance();
        mb.Write(dataView, 4);

        var buffer = mb.Buffer;
        
        List<(byte[], int)> expected =
        [
            (dataView.ToArray(), dataView.Length * 0 + 1),
            (dataView.ToArray(), dataView.Length * 1 + 2),
            (dataView.ToArray(), dataView.Length * 2 + 3),
            (dataView.ToArray(), dataView.Length * 0 + 4)
        ];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(buffer.Size, Is.EqualTo(dataView.Length * 3));
            Assert.That(buffer.WriteCalls, Is.EquivalentTo(expected));
        }
    }
    
}