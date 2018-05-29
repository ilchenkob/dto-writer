namespace DtoWriter.Logic.PropertyMappers
{
  public class SimpleArrayPropertyMapper : PropertyMapper
  {
    public SimpleArrayPropertyMapper(string propertyName, bool hasSetter) : base(propertyName, hasSetter)
    {
    }

    protected override string BuildToModelMappingSyntax()
    {
      return $"{PropertyName} = {PropertyName}.ToArray(),\n";
    }

    public override string GetFromModelMappingSyntax()
    {
      return $"{PropertyName} = {FromModelParamName}.{PropertyName}.ToArray(),\n";
    }
  }
}
