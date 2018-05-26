using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DtoGenerator.UI.ViewModels;
using DtoGenerator.UI.ViewModels.TreeNodes;

namespace DtoGenerator.UI.Views
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
      _viewModel.SyntaxTreeReady = () => expandAllTreeNodes(tree);
      DataContextChanged += (s, e) =>
      {
        if (_viewModel.IsSyntaxTreeReady)
        {
          Task.Delay(400).ContinueWith(t => Application.Current.Dispatcher.Invoke(() => expandAllTreeNodes(tree)));
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

    private void expandAllTreeNodes(ItemsControl items)
    {
      foreach (var obj in items.Items)
      {
        var childControl = items.ItemContainerGenerator.ContainerFromItem(obj);
        if (childControl is TreeViewItem item)
        {
          item.ExpandSubtree();
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
