using System.Collections.Generic;

namespace DtoGenerator.Logic.Models
{
  public class FileInfo
  {
    public string Namespace { get; set; }

    public List<ClassInfo> Classes { get; set; }
  }
}
