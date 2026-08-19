using FishPieClient.Utils;

namespace TestTest;

public class Tests
{
    
    [Test]
    public void ExpectTrue()
    {
        Assert.DoesNotThrow(() => Asserts.Expect(true, "This should not throw!"));
    }

    [Test]
    public void EnsureTrue()
    {
        Assert.DoesNotThrow(() => Asserts.Ensure(true, "This should not throw!"));
    }

    [Test]
    public void EnsureFalse()
    {
        _ = Assert.Catch(() => Asserts.Ensure(false, "This should throw!"));
    }
    
}