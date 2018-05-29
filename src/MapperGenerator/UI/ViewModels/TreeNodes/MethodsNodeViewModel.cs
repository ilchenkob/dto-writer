using System;
using System.Collections.ObjectModel;

namespace DtoWriter.UI.ViewModels.TreeNodes
{
  public class MethodsNodeViewModel : NodeViewModel
  {
    public MethodsNodeViewModel(Action<bool> fromModelStateChangedCallback, Action<bool> toModelStateChangedCallback)
    {
      Title = "Methods";
      Childs = new ObservableCollection<NodeViewModel>
      {
        new NodeViewModel { Title = "FromModel", StateChanged = fromModelStateChangedCallback },
        new NodeViewModel { Title = "ToModel", StateChanged = toModelStateChangedCallback }
      };
    }

    public override void ChangeState(bool isEnabled)
    {
      base.ChangeState(isEnabled);
      foreach (var method in Childs)
      {
        method.ChangeState(IsEnabled);
      }
    }
  }
}