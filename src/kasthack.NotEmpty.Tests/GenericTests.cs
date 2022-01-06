namespace kasthack.NotEmpty.Tests
{
    public class XunitNotEmptyTest : NotEmptyTestBase
    {
        public XunitNotEmptyTest()
            : base((x, o) => kasthack.NotEmpty.Xunit.NotEmptyExtensions.NotEmpty(x, o))
        {
        }
    }

    public class NunitNotEmptyTest : NotEmptyTestBase
    {
        public NunitNotEmptyTest()
            : base((x, o) => kasthack.NotEmpty.Nunit.NotEmptyExtensions.NotEmpty(x, o))
        {
        }
    }

    public class MsTestNotEmptyTest : NotEmptyTestBase
    {
        public MsTestNotEmptyTest()
            : base((x, o) => kasthack.NotEmpty.MsTest.NotEmptyExtensions.NotEmpty(x, o))
        {
        }
    }
}