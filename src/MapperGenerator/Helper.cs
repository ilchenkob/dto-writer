using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using EnvDTE;

namespace DtoGenerator
{
  internal class Helper
  {
    public static Task<string> ReadFile(string path)
    {
      return Task.Run(() =>
      {
        using (var reader = new StreamReader(path))
        {
          return reader.ReadToEnd();
        }
      });
    }
  }
}
