using DtoGenerator.Logic.Models;

namespace DtoGenerator.Logic.Interfaces
{
  public interface ICodeGenerator
  {
    string GenerateSourcecode(FileInfo fileInfo);
  }
}