using System.Collections.Generic;
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

    public static IEnumerable<ProjectItem> GetProjectItems(EnvDTE.ProjectItems projectItems)
    {
      foreach (EnvDTE.ProjectItem item in projectItems)
      {
        yield return item;

        if (item.SubProject != null)
        {
          foreach (ProjectItem childItem in GetProjectItems(item.SubProject.ProjectItems))
            yield return childItem;
        }
        else
        {
          foreach (ProjectItem childItem in GetProjectItems(item.ProjectItems))
            yield return childItem;
        }
      }
    }
  }
}
