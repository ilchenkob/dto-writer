using System;
using System.Collections.ObjectModel;

namespace DtoGenerator.UI.ViewModels.TreeNodes
{
  public class AttributesNodeViewModel : NodeViewModel
  {
    public AttributesNodeViewModel(Action<bool> dataMemberStateChangedCallback, Action<bool> jsonPropStateChangedCallback)
    {
      Title = "Attributes";
      Childs = new ObservableCollection<NodeViewModel>
      {
        new NodeViewModel { Title = "DataMember", StateChanged = dataMemberStateChangedCallback },
        new NodeViewModel { Title = "JsonProperty", StateChanged = jsonPropStateChangedCallback }
      };
      
      base.ChangeState(false);
      Childs[0].ChangeState(IsEnabled);
      Childs[1].ChangeState(IsEnabled);
    }

    public override void ChangeState(bool isEnabled)
    {
      base.ChangeState(isEnabled);
      foreach (var attribute in Childs)
      {
        attribute.ChangeState(IsEnabled);
      }
    }
  }
}