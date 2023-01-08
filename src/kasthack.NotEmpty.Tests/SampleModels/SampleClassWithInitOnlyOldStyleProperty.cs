namespace kasthack.NotEmpty.Tests.SampleModels;

internal class SampleClassWithInitOnlyOldStyleProperty
{
    public SampleClassWithInitOnlyOldStyleProperty(int value = 1) => this.Value = value;

    public int Value { get; }
}
