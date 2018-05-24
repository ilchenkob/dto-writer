using System;
using System.Linq;
using DtoGenerator.Logic;
using DtoGenerator.UI.Views;
using EnvDTE;

namespace DtoGenerator
{
  public class SingleFileCommandExecutor
  {
    private readonly ProjectItem _selectedItem;
    public SingleFileCommandExecutor(ProjectItem selectedItem)
    {
      _selectedItem = selectedItem ?? throw new NullReferenceException("Selected Document");
    }

    public void Run()
    {
      var filePath = _selectedItem.Properties.Item("FullPath").Value.ToString();
      if (!string.IsNullOrWhiteSpace(filePath))
      {
        var selectedProject = _selectedItem.ContainingProject;
        var allSources = Helper.GetProjectItems(selectedProject.ProjectItems)
          .Where(v => v.Name.Contains(".cs"))
          .Select(s => s.Properties.Item("FullPath").Value.ToString())
          .Except(new[] { filePath });

        SingleFileDto singleFileDialog = null;
        Action<string> saveFile = (dtoFilePath) =>
        {
          if (!string.IsNullOrWhiteSpace(dtoFilePath))
          {
            try
            {
              selectedProject.ProjectItems.AddFromFile(dtoFilePath);
            }
            catch (Exception ex)
            {
              var a = ex.Message;
            }
            finally
            {
              singleFileDialog?.Close();
            }
          }
        };

        var viewModel = new UI.ViewModels.SingleFileDtoViewModel(new SingleFileProcessor(), new CodeGenerator(), filePath, allSources, saveFile);
        singleFileDialog = new SingleFileDto(viewModel);
        singleFileDialog.ShowModal();
      }
    }
  }
}
