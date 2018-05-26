using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using DtoGenerator.Logic;
using DtoGenerator.Logic.Interfaces;
using DtoGenerator.Logic.Models;
using DtoGenerator.UI.ViewModels.TreeNodes;

namespace DtoGenerator.UI.ViewModels
{
  public class SingleFileDtoViewModel : BaseViewModel
  {
    private readonly Action<string, string> _dtoFileCreated;
    private IReadOnlyCollection<Project> _allSolutionProjects;
    private readonly ISingleFileProcessor _fileProcessor;
    private readonly ICodeGenerator _codeGenerator;
    
    public SingleFileDtoViewModel(
      ISingleFileProcessor fileProcessor,
      ICodeGenerator codeGenerator,
      string selectedFilePath,
      IReadOnlyCollection<Project> allSolutionProjects,
      IEnumerable<string> allProjectSourcesExceptSelected,
      Action<string, string> onCreateCallback)
    {
      var sourceFiles = allProjectSourcesExceptSelected.ToList();

      _allSolutionProjects = allSolutionProjects;
      _dtoFileCreated = onCreateCallback;
      _fileProcessor = fileProcessor;
      _codeGenerator = codeGenerator;

      DtoFileContent = string.Empty;
      OutputFilePath = string.Empty;
      LoadingProgress = new LoadingViewModel
      {
        MaxValue = sourceFiles.Count * 2
      };

      ProjectNames = allSolutionProjects.Select(p => p.Name).ToList();
      SelectedProjectName = allSolutionProjects.FirstOrDefault(p => p.IsSelected)?.Name;

      SyntaxTreeItems = new ObservableCollection<NodeViewModel>();
      CreateCommand = new Command(createExecute, () => IsCreateEnabled);
      
#pragma warning disable 4014 // don't need to wait async operation
      loadSourceFile(selectedFilePath, sourceFiles);
#pragma warning restore 4014
    }

    private string _selectedProjectName;

    public string SelectedProjectName
    {
      get => _selectedProjectName;
      set
      {
        var prevProject = _allSolutionProjects.FirstOrDefault(p =>
          p.Name.Equals(_selectedProjectName, StringComparison.InvariantCultureIgnoreCase));
        var newProject = _allSolutionProjects.FirstOrDefault(p =>
          p.Name.Equals(value, StringComparison.InvariantCultureIgnoreCase));

        if (prevProject != null && newProject != null && DtoNamespace.Contains(prevProject.DefaultNamespace))
        {
          DtoNamespace = $"{newProject.DefaultNamespace}{DtoNamespace.Remove(0, prevProject.DefaultNamespace.Length)}";
        }

        _selectedProjectName = value;
        NotifyPropertyChanged(() => SelectedProjectName);
      }
    }

    public List<string> ProjectNames { get; }  

    public ObservableCollection<NodeViewModel> SyntaxTreeItems { get; }

    public bool IsSyntaxTreeReady { get; private set; }

    public Action SyntaxTreeReady { get; set; }

    public ICommand CreateCommand { get; }

    public bool IsCreateEnabled => !string.IsNullOrWhiteSpace(OutputFilePath) &&
                                   !string.IsNullOrWhiteSpace(DtoFileContent) &&
                                   LoadingProgress.IsCompleted;

    public string DtoNamespace
    {
      get => FileInfo != null ? FileInfo.Namespace : string.Empty;
      set
      {
        if (FileInfo != null && !FileInfo.Namespace.Equals(value))
        {
          FileInfo.Namespace = value;
          DtoFileContent = _codeGenerator.GenerateSourcecode(FileInfo);
          NotifyPropertyChanged(() => DtoNamespace);
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
      var selectedProject = _allSolutionProjects.FirstOrDefault(p => p.IsSelected);
      if (selectedProject == null)
        throw new InvalidOperationException("At least one project should be marked as selected");

      var modelFileName = Path.GetFileNameWithoutExtension(selectedFilePath);
      var modelFileDirectory = Path.GetDirectoryName(selectedFilePath);
      if (!string.IsNullOrWhiteSpace(modelFileDirectory))
        modelFileDirectory = modelFileDirectory.Remove(0, selectedProject.Path.Length);
      
      Dispatcher.CurrentDispatcher.Invoke(() => OutputFilePath = $"{modelFileDirectory}\\{modelFileName}{Constants.DtoSuffix}.cs");

      FileInfo = await _fileProcessor.Analyze(selectedFilePath, allProjectSourcesExceptSelected, onLoadingProgressChanged);
      Dispatcher.CurrentDispatcher.Invoke(() => DtoFileContent = _codeGenerator.GenerateSourcecode(FileInfo));

      if (FileInfo?.Classes != null)
      {
        foreach (var classItem in FileInfo.Classes)
        {
          var classNode = new ClassNodeViewModel(classItem.Name,
            state => classStateChanged(classItem, state),
            state => fromModelMethodStateChanged(classItem, state),
            state => toModelMethodStateChanged(classItem, state),
            state => dataMemberStateChangedCallback(classItem, state),
            state => jsonPropStateChangedCallback(classItem, state));

          classNode.AddProperties(classItem.Properties != null
            ? classItem.Properties.Select(p => new NodeViewModel(p.Name, state => propertyStateChanged(p, state)))
            : new List<NodeViewModel>());
          Dispatcher.CurrentDispatcher.Invoke(() => SyntaxTreeItems.Add(classNode));
        }
      }

      Dispatcher.CurrentDispatcher.Invoke(() =>
      {
        NotifyPropertyChanged(() => ProjectNames);
        NotifyPropertyChanged(() => DtoNamespace);
        LoadingProgress.Value = LoadingProgress.MaxValue;
        IsSyntaxTreeReady = true;
        SyntaxTreeReady?.Invoke();
      });
    }

    private void createExecute()
    {
      var saveCompleted = false;
      var selectedProject = _allSolutionProjects.FirstOrDefault(p =>
        p.Name.Equals(SelectedProjectName, StringComparison.InvariantCultureIgnoreCase));

      var dtoFilePath = Path.Combine(selectedProject.Path, OutputFilePath.TrimStart('\\'));
      while (!saveCompleted)
      {
        try
        {
          File.WriteAllText(dtoFilePath, DtoFileContent);
          _dtoFileCreated?.Invoke(selectedProject.Name, dtoFilePath);
          saveCompleted = true;
        }
        catch (DirectoryNotFoundException)
        {
          Directory.CreateDirectory(Path.GetDirectoryName(dtoFilePath));
        }
        catch (Exception ex)
        {
          saveCompleted = true;
        }
      }
    }

    private void classStateChanged(ClassInfo model, bool isEnabled)
    {
      model.IsEnabled = isEnabled;
      DtoFileContent = _codeGenerator.GenerateSourcecode(FileInfo);
    }

    private void propertyStateChanged(PropertyInfo property, bool isEnabled)
    {
      property.IsEnabled = isEnabled;
      DtoFileContent = _codeGenerator.GenerateSourcecode(FileInfo);
    }

    private void toModelMethodStateChanged(ClassInfo model, bool isEnabled)
    {
      model.NeedToModelMethod = isEnabled;
      DtoFileContent = _codeGenerator.GenerateSourcecode(FileInfo);
    }

    private void fromModelMethodStateChanged(ClassInfo model, bool isEnabled)
    {
      model.NeedFromModelMethod = isEnabled;
      DtoFileContent = _codeGenerator.GenerateSourcecode(FileInfo);
    }

    private void dataMemberStateChangedCallback(ClassInfo model, bool isEnabled)
    {
      model.NeedDataMemberPropertyAttribute = isEnabled;
      DtoFileContent = _codeGenerator.GenerateSourcecode(FileInfo);
    }

    private void jsonPropStateChangedCallback(ClassInfo model, bool isEnabled)
    {
      model.NeedJsonPropertyAttribute = isEnabled;
      DtoFileContent = _codeGenerator.GenerateSourcecode(FileInfo);
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
