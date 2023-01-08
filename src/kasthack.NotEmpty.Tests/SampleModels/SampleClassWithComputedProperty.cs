namespace kasthack.NotEmpty.Tests.SampleModels;

internal class SampleClassWithComputedProperty
{
    private readonly int value;

    public SampleClassWithComputedProperty(int value = 1) => this.value = value;

#pragma warning disable RCS1085 // We don't want it to be an auto property
    public int Value => this.value;
#pragma warning restore RCS1085
}
