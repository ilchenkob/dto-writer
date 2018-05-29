using System;
using System.Collections.ObjectModel;

namespace DtoWriter.UI.ViewModels.TreeNodes
{
  public class NodeViewModel : BaseViewModel
  {
    public NodeViewModel()
    {
      Title = string.Empty;
      Childs = new ObservableCollection<NodeViewModel>();
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

    public ObservableCollection<NodeViewModel> Childs { get; protected set; }

    public virtual void ChangeState(bool isEnabled)
    {
      IsEnabled = isEnabled;
      StateChanged?.Invoke(IsEnabled);
    }
  }
}
