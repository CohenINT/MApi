using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Avalonia;
using Microsoft.Extensions.Logging;

namespace AvaloniaApplication1.Views;

public partial class MainWindow : Window
{
    
    private readonly DuplicateFinder dupFinder;
    private readonly IServiceProvider _services;
    private string ChosenPath { get; set; } = string.Empty;
    private string ExportPath { get; set; } = string.Empty;
    public MainWindow()
    {
        InitializeComponent();
        this._services = BuildServiceProvider((x) =>
        {
            x.AddSingleton<DuplicateFinder>();
            x.AddLogging(b =>
            {
                b.AddConsole();
                b.AddDebug();
            });
        });
        this.dupFinder = this._services.GetRequiredService<DuplicateFinder>();
    }

    public static IServiceProvider BuildServiceProvider(Action<IServiceCollection> configureServices = null)
    {
        var services = new ServiceCollection();
        configureServices.Invoke(services);
        return services.BuildServiceProvider();
    }
    private async void ClickHandler(object? sender, RoutedEventArgs e)
    {
        await this.dupFinder.IndexAllFilesAsyncV2(SearchBox.Text ?? "", ProgressBarIndex);
    }
     
    private async void OnSelectFolderButtonClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
    
        if (topLevel != null)
        {
            
            var folderDialog = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Folder",
                AllowMultiple = false
            });

            if (folderDialog.Count > 0)
            {
                var selectedFolder = folderDialog[0];
                this.ChosenPath = selectedFolder.Path.LocalPath;
                SearchBox.Text = this.ChosenPath;
                ProgressBarIndex.Value = 10;
                // Do something with the selected folder path
            }
        }
    }

    private async void OnExportJsonButtonClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
    
        if (topLevel != null)
        {
            
            var folderDialog = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Folder",
                AllowMultiple = false
            });

            if (folderDialog.Count > 0)
            {
                var selectedFolder = folderDialog[0];
                this.ExportPath = selectedFolder.Path.LocalPath;
               await this.dupFinder.ExportIndexToJson($"{this.ExportPath}/result.json",ProgressBarJson);
            }
        }
    }

    private  async void OnProcessClick(object? sender, RoutedEventArgs e)
    {
        await this.dupFinder.ProcessFilesAsync(ProgressBarFileProcess);
    }
}