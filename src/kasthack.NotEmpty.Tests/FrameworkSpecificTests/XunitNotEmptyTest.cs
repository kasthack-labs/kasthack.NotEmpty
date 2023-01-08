namespace kasthack.NotEmpty.Tests.FrameworkSpecificTests;

public class XunitNotEmptyTest : NotEmptyTestBase
{
    public XunitNotEmptyTest()
        : base((x, o) => Xunit.NotEmptyExtensions.NotEmpty(x, o))
    {
    }
}