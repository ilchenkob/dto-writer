namespace Dto.Writer.Logic.PropertyMappers
{
  public class UnknownTypePropertyMapper : PropertyMapper
  {
    public UnknownTypePropertyMapper(string propertyName, bool hasSetter) : base(propertyName, hasSetter)
    {
    }

    public override string GetFromModelMappingSyntax()
    {
      return $"// TODO: Add mapping for {PropertyName}\n";
    }

    protected override string BuildToModelMappingSyntax()
    {
      return $"// TODO: Add mapping for {PropertyName}\n";
    }
  }
}