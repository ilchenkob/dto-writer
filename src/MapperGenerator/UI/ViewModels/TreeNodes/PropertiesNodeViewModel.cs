using System.Collections.ObjectModel;

namespace DtoGenerator.UI.ViewModels.TreeNodes
{
  public class PropertiesNodeViewModel : NodeViewModel
  {
    public PropertiesNodeViewModel()
    {
      Title = "Properties";
      Childs = new ObservableCollection<NodeViewModel>();
    }

    public override void ChangeState(bool isEnabled)
    {
      base.ChangeState(isEnabled);
      foreach(var prop in Childs)
      {
        prop.ChangeState(IsEnabled);
      }
    }
  }
}