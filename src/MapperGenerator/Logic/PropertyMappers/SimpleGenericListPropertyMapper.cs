namespace DtoWriter.Logic.PropertyMappers
{
  public class SimpleGenericListPropertyMapper : PropertyMapper
  {
    public SimpleGenericListPropertyMapper(string propertyName, bool hasSetter) : base(propertyName, hasSetter)
    {
    }

    protected override string BuildToModelMappingSyntax()
    {
      return $"{PropertyName} = {PropertyName}.ToList(),\n";
    }

    public override string GetFromModelMappingSyntax()
    {
      return $"{PropertyName} = model.{PropertyName}.ToList(),\n";
    }
  }
}