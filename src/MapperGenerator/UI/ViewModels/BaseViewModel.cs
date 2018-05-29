using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DtoWriter.UI.ViewModels
{
  public abstract class BaseViewModel : INotifyPropertyChanged
  {
    protected void NotifyPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected void NotifyPropertyChanged(Expression<Func<object>> propertyExpression)
    {
      var unaryExpression = propertyExpression.Body as UnaryExpression;
      var memberExpression = unaryExpression == null ? (MemberExpression)propertyExpression.Body : (MemberExpression)unaryExpression.Operand;

      NotifyPropertyChanged(memberExpression.Member.Name);
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
