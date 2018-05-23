using System.Collections.Generic;

namespace DtoGenerator.Logic.Models
{

  public class ClassInfo
  {
    public string Name { get; set; }

    public List<PropertyInfo> Properties { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool NeedFromModelMethod { get; set; } = true;

    public bool NeedToModelMethod { get; set; } = true;
  }
}
