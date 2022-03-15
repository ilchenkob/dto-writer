namespace Dto.Writer.UI.ViewModels
{
  public class LoadingViewModel : BaseViewModel
  {
    private int _maxValue;
    public int MaxValue
    {
      get => _maxValue;
      set
      {
        _maxValue = value;
        NotifyPropertyChanged(() => MaxValue);
        NotifyPropertyChanged(() => IsCompleted);
      }
    }

    private int _value;
    public int Value
    {
      get => _value;
      set
      {
        _value = value;
        NotifyPropertyChanged(() => Value);
        NotifyPropertyChanged(() => IsCompleted);
      }
    }

    public bool IsCompleted => Value >= MaxValue;
  }
}
