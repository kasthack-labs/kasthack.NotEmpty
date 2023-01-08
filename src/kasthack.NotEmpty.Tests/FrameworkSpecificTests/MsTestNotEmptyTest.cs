namespace kasthack.NotEmpty.Tests.FrameworkSpecificTests;

public class MsTestNotEmptyTest : NotEmptyTestBase
{
    public MsTestNotEmptyTest()
        : base((x, o) => MsTest.NotEmptyExtensions.NotEmpty(x, o))
    {
    }
}