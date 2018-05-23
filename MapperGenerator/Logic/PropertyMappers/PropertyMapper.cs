namespace DtoGenerator.Logic.PropertyMappers
{
  public abstract class PropertyMapper
  {
    internal const string FromModelParamName = "model";
    internal const string FromModelMethodName = "FromModel";
    internal const string ToModelMethodName = "ToModel";

    protected string PropertyName { get; }

    protected bool HasSetter { get; }

    protected PropertyMapper(string propertyName, bool hasSetter)
    {
      PropertyName = propertyName;
      HasSetter = hasSetter;
    }

    protected abstract string BuildToModelMappingSyntax();

    public abstract string GetFromModelMappingSyntax();

    public string GetToModelMappingSyntax()
    {
      return HasSetter ? BuildToModelMappingSyntax() : string.Empty;
    }
  }
}
