using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using DtoGenerator.Logic;
using DtoGenerator.Logic.Models;

namespace DtoGenerator.UI.ViewModels
{
  public class SingleFileDtoViewModel : BaseViewModel
  {
    private readonly Action<string> _dtoFileCreated;
    private readonly SingleFileProcessor _fileProcessor;

    public SingleFileDtoViewModel(string selectedFilePath, IEnumerable<string> allProjectSourcesExceptSelected, Action<string> onCreateCallback)
    {
      var sourceFiles = allProjectSourcesExceptSelected.ToList();

      _dtoFileCreated = onCreateCallback;
      _fileProcessor = new SingleFileProcessor();
      DtoFileContent = string.Empty;
      OutputFilePath = string.Empty;
      LoadingProgress = new LoadingViewModel()
      {
        MaxValue = sourceFiles.Count * 2
      };
      Items = new ObservableCollection<NodeViewModel>();
      CreateCommand = new Command(createExecute, () => IsCreateEnabled);
      
#pragma warning disable 4014 // don't need to wait async operation
      loadSourceFile(selectedFilePath, sourceFiles);
#pragma warning restore 4014
    }

    public ObservableCollection<NodeViewModel> Items { get; }

    public bool IsSyntaxTreeReady { get; private set; }

    public Action SyntaxTreeReady { get; set; }

    public ICommand CreateCommand { get; }

    public bool IsCreateEnabled => !string.IsNullOrWhiteSpace(OutputFilePath) && !string.IsNullOrWhiteSpace(DtoFileContent);

    public string DtoNamespace
    {
      get => FileInfo != null ? FileInfo.Namespace : string.Empty;
      set
      {
        if (FileInfo != null && !string.IsNullOrWhiteSpace(DtoFileContent) && !FileInfo.Namespace.Equals(value))
        {
          DtoFileContent = DtoFileContent.Replace(FileInfo.Namespace, value);
          FileInfo.Namespace = value;
        }
      }
    }

    public LoadingViewModel LoadingProgress { get; }

    private string _outputFilePath;
    public string OutputFilePath
    {
      get => _outputFilePath;
      set
      {
        _outputFilePath = value;
        NotifyPropertyChanged(() => OutputFilePath);
        NotifyPropertyChanged(() => IsCreateEnabled);
      }
    }

    private string _dtoFileContent;
    public string DtoFileContent
    {
      get => _dtoFileContent;
      set
      {
        _dtoFileContent = value;
        NotifyPropertyChanged(() => DtoFileContent);
        NotifyPropertyChanged(() => IsCreateEnabled);
      }
    }

    public Logic.Models.FileInfo FileInfo { get; private set; }

    private async Task loadSourceFile(string selectedFilePath, IEnumerable<string> allProjectSourcesExceptSelected)
    {
      var modelFileName = Path.GetFileNameWithoutExtension(selectedFilePath);
      Dispatcher.CurrentDispatcher.Invoke(() => 
        OutputFilePath = $"{selectedFilePath.Remove(selectedFilePath.LastIndexOf(modelFileName))}{modelFileName}{Constants.DtoSuffix}.cs");

      FileInfo = await _fileProcessor.Analyze(selectedFilePath, allProjectSourcesExceptSelected, onLoadingProgressChanged);
      Dispatcher.CurrentDispatcher.Invoke(() => DtoFileContent = _fileProcessor.GenerateSourcecode(FileInfo));

      if (FileInfo?.Classes != null)
      {
        foreach (var classItem in FileInfo.Classes)
        {
          var classNode = new ClassNodeViewModel(classItem.Name,
            state => classStateChanged(classItem, state),
            state => fromModelMethodStateChanged(classItem, state),
            state => toModelMethodStateChanged(classItem, state));

          classNode.AddProperties(classItem.Properties != null
            ? classItem.Properties.Select(p => new NodeViewModel(p.Name, state => propertyStateChanged(p, state)))
            : new List<NodeViewModel>());
          Dispatcher.CurrentDispatcher.Invoke(() => Items.Add(classNode));
        }
      }

      Dispatcher.CurrentDispatcher.Invoke(() =>
      {
        NotifyPropertyChanged(() => DtoNamespace);
        LoadingProgress.Value = LoadingProgress.MaxValue;
        IsSyntaxTreeReady = true;
        SyntaxTreeReady?.Invoke();
      });
    }

    private void createExecute()
    {
      var saveCompleted = false;
      while (!saveCompleted)
      {
        try
        {
          File.WriteAllText(OutputFilePath, DtoFileContent);
          _dtoFileCreated?.Invoke(OutputFilePath);
          saveCompleted = true;
        }
        catch (DirectoryNotFoundException)
        {
          Directory.CreateDirectory(Path.GetDirectoryName(OutputFilePath));
        }
        catch (Exception ex)
        {
          var a = ex.Message;
          saveCompleted = true;
        }
      }
    }

    private void classStateChanged(ClassInfo model, bool isEnabled)
    {
      model.IsEnabled = isEnabled;
      DtoFileContent = _fileProcessor.GenerateSourcecode(FileInfo);
    }

    private void propertyStateChanged(PropertyInfo property, bool isEnabled)
    {
      property.IsEnabled = isEnabled;
      DtoFileContent = _fileProcessor.GenerateSourcecode(FileInfo);
    }

    private void toModelMethodStateChanged(ClassInfo model, bool isEnabled)
    {
      model.NeedToModelMethod = isEnabled;
      DtoFileContent = _fileProcessor.GenerateSourcecode(FileInfo);
    }

    private void fromModelMethodStateChanged(ClassInfo model, bool isEnabled)
    {
      model.NeedFromModelMethod = isEnabled;
      DtoFileContent = _fileProcessor.GenerateSourcecode(FileInfo);
    }

    private void onLoadingProgressChanged(int value)
    {
      Dispatcher.CurrentDispatcher.Invoke(() =>
      {
        LoadingProgress.Value = value;
        NotifyPropertyChanged(() => LoadingProgress);
      });
    }
  }
}
