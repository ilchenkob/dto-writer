using DtoWriter.Logic.Models;

namespace DtoWriter.Logic.Interfaces
{
  public interface ICodeGenerator
  {
    string GenerateSourcecode(FileInfo fileInfo);
  }
}