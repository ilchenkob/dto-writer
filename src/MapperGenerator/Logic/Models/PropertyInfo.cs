using DtoWriter.Logic.PropertyMappers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DtoWriter.Logic.Models
{
  public class PropertyInfo
  {
    public string Name { get; set; }

    public TypeSyntax Type { get; set; }

    public string TypeName { get; set; }

    public bool HasSetter { get; set; }

    public bool IsEnumerableType { get; set; }

    public bool IsGenericType { get; set; }

    public bool IsEnabled { get; set; } = true;

    public PropertyMapper Mapper { get; set; }
  }
}
