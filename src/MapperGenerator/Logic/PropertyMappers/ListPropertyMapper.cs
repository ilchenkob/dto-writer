namespace DtoWriter.Logic.PropertyMappers
{
  public class ListPropertyMapper : PropertyMapper
  {
    private readonly string _typeName;

    public ListPropertyMapper(string typeName, string propertyName, bool hasSetter) : base(propertyName, hasSetter)
    {
      _typeName = typeName;
    }

    protected override string BuildToModelMappingSyntax()
    {
      return $"{PropertyName} = {PropertyName}.Select(dto => dto.{ToModelMethodName}()).ToList(),\n";
    }

    public override string GetFromModelMappingSyntax()
    {
      return $"{PropertyName} = model.{PropertyName}.Select({_typeName}.{FromModelMethodName}).ToList(),\n";
    }
  }
}