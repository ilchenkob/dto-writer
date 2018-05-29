using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DtoWriter.Logic.Models;

namespace DtoWriter.Logic.Interfaces
{
  public interface ISingleFileProcessor
  {
    Task<FileInfo> Analyze(
      string selectedFilePath,
      IEnumerable<string> allProjectSourcesExceptSelected,
      Action<int> onProgressChanged);
  }
}