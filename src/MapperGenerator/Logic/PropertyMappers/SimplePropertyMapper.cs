namespace DtoWriter.Logic.PropertyMappers
{
  public class SimplePropertyMapper : PropertyMapper
  {
    public SimplePropertyMapper(string propertyName, bool hasSetter) : base(propertyName, hasSetter)
    {
    }

    public override string GetFromModelMappingSyntax()
    {
      return $"{PropertyName} = {FromModelParamName}.{PropertyName},\n";
    }

    protected override string BuildToModelMappingSyntax()
    {
      return $"{PropertyName} = {PropertyName},\n";
    }
  }
}
