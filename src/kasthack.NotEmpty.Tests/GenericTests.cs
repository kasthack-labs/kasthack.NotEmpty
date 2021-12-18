namespace kasthack.NotEmpty.Tests
{
    public class XunitNotEmptyTest : NotEmptyTestBase
    {
        public XunitNotEmptyTest()
            : base(x => kasthack.NotEmpty.Xunit.NotEmptyExtensions.NotEmpty(x))
        {
        }
    }

    public class NunitNotEmptyTest : NotEmptyTestBase
    {
        public NunitNotEmptyTest()
            : base(x => kasthack.NotEmpty.Nunit.NotEmptyExtensions.NotEmpty(x))
        {
        }
    }

    public class MsTestNotEmptyTest : NotEmptyTestBase
    {
        public MsTestNotEmptyTest()
            : base(x => kasthack.NotEmpty.MsTest.NotEmptyExtensions.NotEmpty(x))
        {
        }
    }

    public class RawNotEmptyTest : NotEmptyTestBase
    {
        public RawNotEmptyTest()
            : base(x => kasthack.NotEmpty.Raw.NotEmptyExtensions.NotEmpty(x))
        {
        }
    }
}