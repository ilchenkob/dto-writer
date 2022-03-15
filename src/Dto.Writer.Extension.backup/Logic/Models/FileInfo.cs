using System.Collections.Generic;

namespace Dto.Writer.Logic.Models
{
  public class FileInfo
  {
    public string ModelNamespace { get; set; }

    public string Namespace { get; set; }

    public List<ClassInfo> Classes { get; set; }
  }
}
