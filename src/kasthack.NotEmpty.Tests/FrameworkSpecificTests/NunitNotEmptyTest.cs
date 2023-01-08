namespace kasthack.NotEmpty.Tests.FrameworkSpecificTests;

public class NunitNotEmptyTest : NotEmptyTestBase
{
    public NunitNotEmptyTest()
        : base((x, o) => Nunit.NotEmptyExtensions.NotEmpty(x, o))
    {
    }
}