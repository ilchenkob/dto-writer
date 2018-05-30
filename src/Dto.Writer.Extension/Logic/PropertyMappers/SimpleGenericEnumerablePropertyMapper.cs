namespace Dto.Writer.Logic.PropertyMappers
{
  public class SimpleGenericEnumerablePropertyMapper : PropertyMapper
  {
    public SimpleGenericEnumerablePropertyMapper(string propertyName, bool hasSetter) : base(propertyName, hasSetter)
    {
    }

    public override string GetFromModelMappingSyntax()
    {
      return $"{PropertyName} = model.{PropertyName}.ToArray(),\n";
    }

    protected override string BuildToModelMappingSyntax()
    {
      return $"{PropertyName} = model.{PropertyName}.ToList(),  // TODO: Check this property mapping\n";
    }
  }
}
