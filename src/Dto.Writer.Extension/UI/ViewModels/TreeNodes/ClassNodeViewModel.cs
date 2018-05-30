using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dto.Writer.UI.ViewModels.TreeNodes
{
  public class ClassNodeViewModel : NodeViewModel
  {
    public ClassNodeViewModel(string title,
      Action<bool> stateChangeCallback,
      Action<bool> fromModelStateChangedCallback,
      Action<bool> toModelStateChangedCallback,
      Action<bool> dataMemberStateChangedCallback,
      Action<bool> jsonPropStateChangedCallback)
    {
      Title = title;
      Childs = new ObservableCollection<NodeViewModel>()
      {
        new PropertiesNodeViewModel(),
        new MethodsNodeViewModel(fromModelStateChangedCallback, toModelStateChangedCallback),
        new AttributesNodeViewModel(dataMemberStateChangedCallback, jsonPropStateChangedCallback)
      };
      StateChanged = stateChangeCallback;
    }

    public override void ChangeState(bool isEnabled)
    {
      base.ChangeState(isEnabled);
      foreach (var prop in Childs)
      {
        prop.ChangeState(IsEnabled);
      }
    }

    public void AddProperties(IEnumerable<NodeViewModel> properties)
    {
      foreach(var property in properties)
        Childs.ElementAt(0).Childs.Add(property);
    }
  }
}