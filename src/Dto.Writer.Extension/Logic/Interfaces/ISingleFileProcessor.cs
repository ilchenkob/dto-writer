using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dto.Writer.Logic.Models;

namespace Dto.Writer.Logic.Interfaces
{
  public interface ISingleFileProcessor
  {
    Task<FileInfo> Analyze(
      string selectedFilePath,
      IEnumerable<string> allProjectSourcesExceptSelected,
      Action<int> onProgressChanged);
  }
}