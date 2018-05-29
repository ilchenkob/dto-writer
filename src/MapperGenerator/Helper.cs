using System.IO;
using System.Threading.Tasks;

namespace DtoWriter
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
