using Dto.Writer.Logic.Models;

namespace Dto.Writer.Logic.Interfaces
{
  public interface ICodeGenerator
  {
    string GenerateSourcecode(FileInfo fileInfo);
  }
}