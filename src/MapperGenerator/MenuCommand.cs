using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Gets the service provider from the owner package.
    /// </summary>
    private IServiceProvider serviceProvider => this.package;

    private readonly SingleFileCommandExecutor singleFileCommandExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private MenuCommand(Package package)
    {
      this.package = package ?? throw new ArgumentNullException("package");
      if (this.serviceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
      {
        var menuCommandId = new CommandID(CommandSet, solutionExplorerItemCommandId);
        var menuItem = new OleMenuCommand(this.MenuItemCallback, menuCommandId);
        menuItem.BeforeQueryStatus += OnCanShowMenuItem;
        commandService.AddCommand(menuItem);


        var ide = (EnvDTE80.DTE2)this.serviceProvider.GetService(typeof(DTE));
        singleFileCommandExecutor = new SingleFileCommandExecutor(ide);
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
        var isCsharp = singleFileCommandExecutor.CanHandleSelectedItem();

        menuCommand.Visible = isCsharp;
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
        singleFileCommandExecutor.Run();
      }
    }
  }
}
