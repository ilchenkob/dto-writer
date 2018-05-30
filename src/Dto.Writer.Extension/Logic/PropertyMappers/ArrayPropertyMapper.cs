namespace Dto.Writer.Logic.PropertyMappers
{
  public class ArrayPropertyMapper : PropertyMapper
  {
    private readonly string _typeName;

    public ArrayPropertyMapper(string typeName, string propertyName, bool hasSetter) : base(propertyName, hasSetter)
    {
      _typeName = typeName;
    }

    protected override string BuildToModelMappingSyntax()
    {
      return $"{PropertyName} = {PropertyName}.Select(dto => dto.{ToModelMethodName}()).ToArray(),\n";
    }

    public override string GetFromModelMappingSyntax()
    {
      return $"{PropertyName} = model.{PropertyName}.Select({_typeName}.{FromModelMethodName}).ToArray(),\n";
    }
  }
}