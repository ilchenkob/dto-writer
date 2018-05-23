using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Design;
using DtoGenerator.UI.Views;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace DtoGenerator
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class MenuCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int solutionExplorerItemCommandId = 0x0100;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("2c2abdcc-52e1-4096-b408-1ccb207c9e9f");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly Package package;



    /// <summary>
    /// Initializes a new instance of the <see cref="MenuCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private MenuCommand(Package package)
    {
      if (package == null)
      {
        throw new ArgumentNullException("package");
      }

      this.package = package;
      if (this.ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
      {
        var menuCommandId = new CommandID(CommandSet, solutionExplorerItemCommandId);
        var menuItem = new OleMenuCommand(this.MenuItemCallback, menuCommandId);
        menuItem.BeforeQueryStatus += OnCanShowMenuItem;
        commandService.AddCommand(menuItem);
      }
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static MenuCommand Instance
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the service provider from the owner package.
    /// </summary>
    private IServiceProvider ServiceProvider => this.package;

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static void Initialize(Package package)
    {
      Instance = new MenuCommand(package);
    }

    private void OnCanShowMenuItem(object sender, EventArgs e)
    {
      // get the menu that fired the event
      if (sender is OleMenuCommand menuCommand)
      {
        // start by assuming that the menu will not be shown
        menuCommand.Visible = false;
        menuCommand.Enabled = false;

        var ide = (EnvDTE80.DTE2)this.ServiceProvider.GetService(typeof(DTE));
        var selectedItem = GetSelectedSolutionExplorerItem(ide);
        if (selectedItem == null)
          return;
        
        var isCsharp = selectedItem.Name.EndsWith(".cs");
        
        menuCommand.Visible = true;
        menuCommand.Enabled = isCsharp;
      }
    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void MenuItemCallback(object senderObj, EventArgs args)
    {
      if (senderObj is OleMenuCommand && args is OleMenuCmdEventArgs)
      {
        var ide = (EnvDTE80.DTE2)this.ServiceProvider.GetService(typeof(DTE));
        var selectedItem = this.GetSelectedSolutionExplorerItem(ide);
        var filePath = selectedItem.Properties.Item("FullPath").Value.ToString();
        if (!string.IsNullOrWhiteSpace(filePath))
        {
          var selectedProject = selectedItem.ContainingProject;
          var allSources = GetProjectItems(selectedProject.ProjectItems)
                                .Where(v => v.Name.Contains(".cs"))
                                .Select(s => s.Properties.Item("FullPath").Value.ToString())
                                .Except(new []{ filePath });

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

          var viewModel = new UI.ViewModels.SingleFileDtoViewModel(filePath, allSources, saveFile);
          singleFileDialog = new SingleFileDto(viewModel);
          singleFileDialog.ShowModal();
        }
      }
    }

    private ProjectItem GetSelectedSolutionExplorerItem(EnvDTE80.DTE2 ide)
    {
      var solutionExplorer = ide.ToolWindows.SolutionExplorer;
      if (solutionExplorer.SelectedItems is object[] items)
      {
        if (items.Length == 1 && items[0] is EnvDTE.UIHierarchyItem hierarchyItem)
        {
          var projectItem = ide.Solution.FindProjectItem(hierarchyItem.Name);
          if (projectItem != null)
            return projectItem;
        }
      }

      return null;
    }

    /// <summary>
    /// Recursively gets all the ProjectItem objects in a list of projectitems from a Project
    /// </summary>
    /// <param name="projectItems">The project items.</param>
    /// <returns></returns>
    public IEnumerable<ProjectItem> GetProjectItems(EnvDTE.ProjectItems projectItems)
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
