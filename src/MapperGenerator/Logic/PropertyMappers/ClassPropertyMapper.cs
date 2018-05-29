namespace DtoWriter.Logic.PropertyMappers
{
  public class ClassPropertyMapper : PropertyMapper
  {
    private readonly string _propertyTypeName;

    public ClassPropertyMapper(string propertyTypeName, string propertyName, bool hasSetter) : base(propertyName, hasSetter)
    {
      _propertyTypeName = propertyTypeName;
    }

    public override string GetFromModelMappingSyntax()
    {
      return $"{PropertyName} = {_propertyTypeName}.{FromModelMethodName}({FromModelParamName}.{PropertyName}),\n";
    }

    protected override string BuildToModelMappingSyntax()
    {
      return $"{PropertyName} = {PropertyName}.{ToModelMethodName}(),\n";
    }
  }
}