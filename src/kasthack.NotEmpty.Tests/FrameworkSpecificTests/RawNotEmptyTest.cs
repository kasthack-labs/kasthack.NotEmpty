namespace kasthack.NotEmpty.Tests.FrameworkSpecificTests;

using System.Collections.Generic;
using global::Xunit;

public class RawNotEmptyTest : NotEmptyTestBase
{
    public RawNotEmptyTest()
        : base((x, o) => Raw.NotEmptyExtensions.NotEmpty(x, o))
    {
    }

    [Fact]
    public void RawAssertThrowsAnExceptionOfACorrectType() => Assert.Throws<Raw.EmptyException>(() => this.Action(new { Value = 0, }));

    [Fact]
    public void PathIsConstructedProperlyForTheRootObject() => this.AssertExceptionPath("(value)", 0);

    [Fact]
    public void PathIsConstructedProperlyForProperties() => this.AssertExceptionPath("(value).Value", new { Value = 0, });

    [Fact]
    public void PathIsConstructedProperlyForNestedProperties() => this.AssertExceptionPath("(value).Value.A", new { Value = new { A = 0 } });

    [Fact]
    public void PathIsConstructedProperlyForArrays() => this.AssertExceptionPath("(value).Value[0].A", new { Value = new[] { new { A = 0 } } });

    [Fact]
    public void PathIsConstructedProperlyForDictionaries() => this.AssertExceptionPath("(value).Value[str]", new { Value = new Dictionary<string, int> { { "str", 0 } } });

    private void AssertExceptionPath(string expectedPath, object value) => Assert.Equal(
        expectedPath,
        Assert.Throws<Raw.EmptyException>(() => this.Action(value)).Path);
}