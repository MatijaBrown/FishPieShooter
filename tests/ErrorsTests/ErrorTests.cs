using FishPieClient.Utils;

namespace TestTest;

public class Tests
{
    
    [Test]
    public void ExpectTrue()
    {
        Assert.DoesNotThrow(() => Errors.Expect(true, "This should not throw!"));
    }

    [Test]
    public void EnsureTrue()
    {
        Assert.DoesNotThrow(() => Errors.Ensure(true, "This should not throw!"));
    }

    [Test]
    public void EnsureFalse()
    {
        _ = Assert.Catch(() => Errors.Ensure(false, "This should throw!"));
    }
    
}