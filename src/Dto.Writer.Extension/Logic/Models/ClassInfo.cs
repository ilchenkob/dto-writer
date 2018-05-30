using System.Collections.Generic;

namespace Dto.Writer.Logic.Models
{

  public class ClassInfo
  {
    public string Name { get; set; }

    public List<PropertyInfo> Properties { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool NeedFromModelMethod { get; set; } = true;

    public bool NeedToModelMethod { get; set; } = true;

    public bool NeedJsonPropertyAttribute { get; set; }

    public bool NeedDataMemberPropertyAttribute { get; set; }
  }
}
