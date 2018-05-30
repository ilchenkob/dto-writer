using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dto.Writer.Logic;
using Dto.Writer.UI.ViewModels;
using Dto.Writer.UI.Views;
using EnvDTE;

namespace Dto.Writer
{
  public class SingleFileCommandExecutor
  {
    private readonly EnvDTE80.DTE2 _ide;
    public SingleFileCommandExecutor(EnvDTE80.DTE2 ide)
    {
      _ide = ide ?? throw new NullReferenceException("IDE");
    }

    public void Run(bool calledFromCodeEditor = false)
    {
      var selectedItem = calledFromCodeEditor ? getCodeEditorItem() : getSelectedSolutionExplorerItem();
      var filePath = selectedItem.Properties.Item("FullPath").Value.ToString();
      if (!string.IsNullOrWhiteSpace(filePath))
      {
        var selectedProject = selectedItem.ContainingProject;
        var allSources = getAllProjectItems(selectedProject.ProjectItems)
          .Where(v => v.Name.Contains(".cs"))
          .Select(s => s.Properties.Item("FullPath").Value.ToString())
          .Except(new[] { filePath });

        var allProjects = getAllSolutionProjects(selectedProject);

        SingleFileDto singleFileDialog = null;
        Action<string, string> saveFile = (projectName, dtoFilePath) =>
        {
          if (!string.IsNullOrWhiteSpace(dtoFilePath))
          {
            try
            {
              var dtoProject = getProjectByName(projectName);
              dtoProject.ProjectItems.AddFromFile(dtoFilePath);
            }
            catch (Exception ex)
            {
              var message = ex.Message;
            }
            finally
            {
              singleFileDialog?.Close();
            }
          }
        };

        var viewModel = new SingleFileDtoViewModel(
          new SingleFileProcessor(),
          new CodeGenerator(),
          filePath,
          allProjects,
          allSources,
          saveFile);
        singleFileDialog = new SingleFileDto(viewModel);
        singleFileDialog.ShowModal();
      }
    }

    public bool CanHandleSelectedItem()
    {
      var selectedItem = getSelectedSolutionExplorerItem();
      return selectedItem != null && selectedItem.Name.EndsWith(".cs");
    }

    public ProjectItem getSelectedSolutionExplorerItem()
    {
      var solutionExplorer = _ide.ToolWindows.SolutionExplorer;
      if (solutionExplorer.SelectedItems is object[] items)
      {
        if (items.Length == 1 && items[0] is UIHierarchyItem hierarchyItem)
        {
          var projectItem = _ide.Solution.FindProjectItem(hierarchyItem.Name);
          if (projectItem != null)
            return projectItem;
        }
      }

      return null;
    }

    private ProjectItem getCodeEditorItem()
    {
      return _ide.ActiveDocument.ProjectItem;
    }

    private IEnumerable<ProjectItem> getAllProjectItems(ProjectItems projectItems)
    {
      foreach (ProjectItem item in projectItems)
      {
        yield return item;

        if (item.SubProject != null)
        {
          foreach (ProjectItem childItem in getAllProjectItems(item.SubProject.ProjectItems))
            yield return childItem;
        }
        else
        {
          foreach (ProjectItem childItem in getAllProjectItems(item.ProjectItems))
            yield return childItem;
        }
      }
    }

    private IReadOnlyCollection<Logic.Models.Project> getAllSolutionProjects(Project selectedProject)
    {
      var selectedProjectName = selectedProject.Name;
      var allProjects = new List<Logic.Models.Project>();
      for (var i = 1; i < _ide.Solution.Projects.Count + 1; i++)
      {
        try
        {
          var project = _ide.Solution.Projects.Item(i);
          if (!string.IsNullOrWhiteSpace(project.Name) && !string.IsNullOrWhiteSpace(project.FullName))
            allProjects.Add(new Logic.Models.Project
            {
              Name = project.Name,
              Path = project.FullName.Remove(project.FullName.LastIndexOf('\\')),
              DefaultNamespace = project.Properties.Item("DefaultNamespace").Value.ToString(),
              IsSelected = project.Name.Equals(selectedProjectName, StringComparison.InvariantCultureIgnoreCase)
            });
        }
        catch (Exception e)
        {
          // TODO: handle unexpected cases
        }
      }

      return new ReadOnlyCollection<Logic.Models.Project>(allProjects);
    }

    private Project getProjectByName(string name)
    {
      for (var i = 1; i < _ide.Solution.Projects.Count + 1; i++)
      {
        var project = _ide.Solution.Projects.Item(i);
        if (project.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
        {
          return project;
        }
      }

      return null;
    }
  }
}
