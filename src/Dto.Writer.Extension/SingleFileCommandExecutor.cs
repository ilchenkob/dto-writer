using System;
using System.Collections;
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
    private const string ProjectFolderID = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";

    private readonly EnvDTE80.DTE2 _ide;
    private readonly IDictionary<string, Project> _projects;

    public SingleFileCommandExecutor(EnvDTE80.DTE2 ide)
    {
      _projects = new Dictionary<string, Project>();
      _ide = ide ?? throw new NullReferenceException("IDE");
    }

    public void Run(bool calledFromCodeEditor = false)
    {
      var selectedItem = calledFromCodeEditor ? getCodeEditorItem() : getSelectedSolutionExplorerItem();
      var filePath = selectedItem.Properties.Item("FullPath").Value.ToString();
      if (!string.IsNullOrWhiteSpace(filePath))
      {
        _projects.Clear();

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
              var dtoProject = _projects[projectName];
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
          foreach (var childItem in getAllProjectItems(item.SubProject.ProjectItems))
            yield return childItem;
        }
        else
        {
          foreach (var childItem in getAllProjectItems(item.ProjectItems))
            yield return childItem;
        }
      }
    }

    private IReadOnlyCollection<Logic.Models.Project> getAllSolutionProjects(Project selectedProject)
    {
      var projects = getProjects(_ide.Solution.Projects, selectedProject.UniqueName);
      return new ReadOnlyCollection<Logic.Models.Project>(projects.OrderBy(p => p.Name).ToList());
    }

    private List<Logic.Models.Project> getProjects(IEnumerable projects, string selectedProjectName, string parentFolder = "")
    {
      var result = new List<Logic.Models.Project>();
      foreach (var item in projects)
      {
        var currentProject = item is ProjectItem pi ? pi.Object as Project : item as Project;
        result.AddRange(processProjectItem(currentProject, selectedProjectName, parentFolder));
      }
      return result;
    }

    private IEnumerable<Logic.Models.Project> processProjectItem(Project projectItem, string selectedProjectName, string parentFolder = "")
    {
      try
      {
        if (projectItem.Kind.Equals(ProjectFolderID))
        {
          return getProjects(projectItem.ProjectItems, selectedProjectName, $"{parentFolder}{projectItem.Name}\\");
        }
        if (!string.IsNullOrWhiteSpace(projectItem.Name) && !string.IsNullOrWhiteSpace(projectItem.FullName))
        {
          var projectDisplayName = string.IsNullOrWhiteSpace(parentFolder)
                                        ? projectItem.Name
                                        : $"{parentFolder}{projectItem.Name}";
          _projects.Add(projectDisplayName, projectItem);

          return new List<Logic.Models.Project>
          {
            new Logic.Models.Project
            {
              DisplayName = projectDisplayName,
              Name = projectItem.Name,
              Path = projectItem.FullName.Remove(projectItem.FullName.LastIndexOf('\\')),
              DefaultNamespace = projectItem.Properties.Item("DefaultNamespace").Value.ToString(),
              IsSelected = projectItem.UniqueName.Equals(selectedProjectName, StringComparison.InvariantCultureIgnoreCase)
            }
          };
        }
      }
      catch (Exception e)
      {
        // TODO: handle unexpected cases
      }

      return new List<Logic.Models.Project>();
    }
  }
}
