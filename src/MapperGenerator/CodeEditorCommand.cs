using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using DtoGenerator.UI.Views;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DtoGenerator
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class CodeEditorCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 0x0100;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("cbb4e331-1562-4fc1-b7d0-eaee9ed27e56");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly Package package;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeEditorCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private CodeEditorCommand(Package package)
    {
      if (package == null)
      {
        throw new ArgumentNullException("package");
      }

      this.package = package;

      if (this.ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
      {
        var menuCommandId = new CommandID(CommandSet, CommandId);
        var menuItem = new OleMenuCommand(this.MenuItemCallback, menuCommandId);
        commandService.AddCommand(menuItem);
      }
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static CodeEditorCommand Instance
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
      Instance = new CodeEditorCommand(package);
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
        var selectedItem = this.GetOpenedCodeEditorItem(ide);
        new SingleFileCommandExecutor(selectedItem).Run();
      }
    }

    private ProjectItem GetOpenedCodeEditorItem(EnvDTE80.DTE2 ide)
    {
      return ide.ActiveDocument.ProjectItem;
    }
  }
}
