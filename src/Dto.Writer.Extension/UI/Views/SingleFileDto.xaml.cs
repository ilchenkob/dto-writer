using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dto.Writer.UI.ViewModels;
using Dto.Writer.UI.ViewModels.TreeNodes;

namespace Dto.Writer.UI.Views
{
  /// <summary>
  /// Interaction logic for SingleFileDto.xaml
  /// </summary>
  public partial class SingleFileDto
  {
    private readonly SingleFileDtoViewModel _viewModel;

    public SingleFileDto(SingleFileDtoViewModel viewModel)
    {
      InitializeComponent();

      _viewModel = viewModel;
      _viewModel.PropertyChanged += (sender, eventArgs) =>
      {
        if (eventArgs.PropertyName.Equals(nameof(_viewModel.SyntaxTreeItems)))
        {
          expandAllTreeNodes(tree);
        }
      };
      DataContextChanged += (s, e) =>
      {
        if (_viewModel.SyntaxTreeItems != null && _viewModel.SyntaxTreeItems.Count > 0)
        {
          expandAllTreeNodes(tree);
        }
      };
      DataContext = viewModel;

      this.KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape)
      {
        Close();
      }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void TreeCheckBox_Checked(object sender, RoutedEventArgs e)
    {
      setNodeState(sender, true);
    }

    private void TreeCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
      setNodeState(sender, false);
    }

    private async void expandAllTreeNodes(ItemsControl control)
    {
      await Task.Delay(400).ConfigureAwait(false);
      foreach (var obj in control.Items)
      {
        var childControl = control.ItemContainerGenerator.ContainerFromItem(obj);
        if (childControl is TreeViewItem item)
        {
          Application.Current.Dispatcher.Invoke(() => item.ExpandSubtree());
        }
      }
    }

    private void setNodeState(object sender, bool isChecked)
    {
      if (sender is CheckBox checkBox)
      {
        if (checkBox.DataContext is NodeViewModel vm)
        {
          vm.ChangeState(isChecked);
        }
      }
    }
  }
}
