using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DtoGenerator.UI.ViewModels
{
  public class NodeViewModel : BaseViewModel
  {
    public NodeViewModel()
    {
      Title = string.Empty;
      Items = new ObservableCollection<NodeViewModel>();
      IsEnabled = true;
    }

    public Action<bool> StateChanged { get; set; }

    public NodeViewModel(string title, Action<bool> stateChangedCallback) : this()
    {
      Title = title;
      StateChanged = stateChangedCallback;
    }

    private bool _isEnabled;
    public bool IsEnabled
    {
      get => _isEnabled;
      private set
      {
        _isEnabled = value;
        NotifyPropertyChanged(() => IsEnabled);
      }
    }

    private string _title;
    public string Title
    {
      get => _title;
      set
      {
        _title = value;
        NotifyPropertyChanged(() => Title);
      }
    }

    public ObservableCollection<NodeViewModel> Items { get; protected set; }

    public virtual void ChangeState(bool isEnabled)
    {
      IsEnabled = isEnabled;
      StateChanged?.Invoke(IsEnabled);
    }
  }

  public class ClassNodeViewModel : NodeViewModel
  {
    public ClassNodeViewModel(string title,
      Action<bool> stateChangeCallback,
      Action<bool> fromModelStateChangedCallback,
      Action<bool> toModelStateChangedCallback)
    {
      Title = title;
      Items = new ObservableCollection<NodeViewModel>()
      {
        new PropertiesNodeViewModel(),
        new MethodsNodeViewModel(fromModelStateChangedCallback, toModelStateChangedCallback)
      };
      StateChanged = stateChangeCallback;
    }

    public override void ChangeState(bool isEnabled)
    {
      base.ChangeState(isEnabled);
      foreach (var prop in Items)
      {
        prop.ChangeState(IsEnabled);
      }
    }

    public void AddProperties(IEnumerable<NodeViewModel> properties)
    {
      foreach(var property in properties)
        Items.ElementAt(0).Items.Add(property);
    }
  }

  public class PropertiesNodeViewModel : NodeViewModel
  {
    public PropertiesNodeViewModel()
    {
      Title = "Properties";
      Items = new ObservableCollection<NodeViewModel>();
    }

    public override void ChangeState(bool isEnabled)
    {
      base.ChangeState(isEnabled);
      foreach(var prop in Items)
      {
        prop.ChangeState(IsEnabled);
      }
    }
  }

  public class MethodsNodeViewModel : NodeViewModel
  {
    public MethodsNodeViewModel(Action<bool> fromModelStateChangedCallback, Action<bool> toModelStateChangedCallback)
    {
      Title = "Methods";
      Items = new ObservableCollection<NodeViewModel>
      {
        new NodeViewModel { Title = "FromModel", StateChanged = fromModelStateChangedCallback },
        new NodeViewModel { Title = "ToModel", StateChanged = toModelStateChangedCallback }
      };
    }

    public override void ChangeState(bool isEnabled)
    {
      base.ChangeState(isEnabled);
      foreach (var method in Items)
      {
        method.ChangeState(IsEnabled);
      }
    }
  }
}
