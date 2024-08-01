using I2Editor.Common;
using I2LanguagesLib;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace I2Editor.ViewModels;

public sealed class MainViewModel : BaseViewModel
{
    private string? _title;
    public string Title
    {
        get => _title ??= "I2Editor";
        set => SetField(ref _title, value);
    }

    private ExportedFile? _project;
    public ExportedFile? Project
    {
        get => _project;
        set => SetField(ref _project, value);
    }

    private RelayCommand? _importCommand;
    public RelayCommand ImportCommand => _importCommand ??= new RelayCommand(Import);

    private void Import()
    {
        FileDialog dialog = new OpenFileDialog
        {
            Filter = "I2Languages file (*.dat)|*.dat|All files (*.*)|*.*",
            Title = "Select a I2Languages file"
        };

        if (dialog.ShowDialog() != true)
            return;

        var file = dialog.FileName;
        var reader = new BinaryReader(File.OpenRead(file));
        Project = new ExportedFile(FileStructure.ReadFile(reader));

        Title = $"I2Editor - Unsaved";
    }

    private RelayCommand? _saveCommand;
    public RelayCommand SaveCommand => _saveCommand ??= new RelayCommand(Save);

    private void Save()
    {
        if (Project is null) return;

        using var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
        if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            return;

        var folder = folderDialog.SelectedPath;
        ExportUtils.SaveProject(Project!, folder);

        Title = $"I2Editor - {folder}";
    }

    private RelayCommand? _openProjectCommand;
    public RelayCommand OpenProjectCommand => _openProjectCommand ??= new RelayCommand(OpenProject);

    private void OpenProject()
    {
        using var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
        if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            return;

        var folder = folderDialog.SelectedPath;
        try
        {
            Project = ExportUtils.OpenProject(folder);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error opening project!", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Title = $"I2Editor - {folder}";
    }

    private RelayCommand? _exportCommand;
    public RelayCommand ExportCommand => _exportCommand ??= new RelayCommand(Export);

    private void Export()
    {
        if (Project is null) return;

        var file = Project.Reconstruct();

        // Ask a file to export to
        FileDialog dialog = new SaveFileDialog
        {
            Filter = "I2Languages file (*.dat)|*.dat|All files (*.*)|*.*",
            Title = "Select a I2Languages file"
        };

        if (dialog.ShowDialog() != true)
            return;

        var path = dialog.FileName;
        var writer = new BinaryWriter(File.OpenWrite(path));
        file.WriteFile(writer);
    }

    private RelayCommand? _exportCsvCommand;
    public RelayCommand ExportCsvCommand => _exportCsvCommand ??= new RelayCommand(ExportCsv);

    private void ExportCsv()
    {
        if (Project is null) return;

        // Ask a file to export to
        FileDialog dialog = new SaveFileDialog
        {
            Filter = "CSV file (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Select a CSV file"
        };

        if (dialog.ShowDialog() != true)
            return;

        var path = dialog.FileName;
        try
        {
            ExportUtils.ExportToCSV(path, Project);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error exporting CSV!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private RelayCommand? _importCsvCommand;
    public RelayCommand ImportCsvCommand => _importCsvCommand ??= new RelayCommand(ImportCsv);

    private void ImportCsv()
    {
        if (Project is null) return;

        // Ask a file to export to
        FileDialog dialog = new OpenFileDialog
        {
            Filter = "CSV/TSV file (*.csv,*.tsv)|*.csv;*.tsv|All files (*.*)|*.*",
            Title = "Select a CSV file"
        };

        if (dialog.ShowDialog() != true)
            return;

        var path = dialog.FileName;

        try
        {
            ExportUtils.ImportFromCsv(Project, path);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error loading CSV!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private RelayCommand? _mergeCommand;
    public RelayCommand MergeCommand => _mergeCommand ??= new RelayCommand(Merge);

    private void Merge()
    {
        // Allows to copy json values from one file to another, if the keys match.
        // Useful when you need to migrate to a new version of the file.

        // Ask for new file with new structure
        FileDialog dialog = new OpenFileDialog
        {
            Filter = "JSON file (*.json)|*.json|All files (*.*)|*.*",
            Title = "Select locale file from NEW version"
        };

        if (dialog.ShowDialog() != true)
            return;

        var template = dialog.FileName;

        // Ask for old file with old values
        dialog = new OpenFileDialog
        {
            Filter = "JSON file (*.json)|*.json|All files (*.*)|*.*",
            Title = "Select locale with your changes"
        };

        if (dialog.ShowDialog() != true)
            return;

        var values = dialog.FileName;

        try
        {
            // Load both files
            var templateJson = File.ReadAllText(template);
            var valuesJson = File.ReadAllText(values);

            // Deserialize them
            var templateObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(templateJson);
            var valuesObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(valuesJson);

            if (templateObj is null || valuesObj is null)
                throw new Exception("Failed to deserialize files!");

            var merged = ExportUtils.Merge(templateObj, valuesObj);
            // Serialize merged file to json and save it
            var json = JsonConvert.SerializeObject(merged, Formatting.Indented);
            var saveDialog = new SaveFileDialog
            {
                Filter = "JSON file (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select where to save merged file"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            var path = saveDialog.FileName;
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error merging files!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
