using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DtoGenerator.Logic.Models;

namespace DtoGenerator.Logic.Interfaces
{
  public interface ISingleFileProcessor
  {
    Task<FileInfo> Analyze(
      string selectedFilePath,
      IEnumerable<string> allProjectSourcesExceptSelected,
      Action<int> onProgressChanged);
  }
}