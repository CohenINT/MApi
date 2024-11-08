using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace AvaloniaApplication1.Views;

public partial class MainWindow : Window
{
    private string ChosenPath { get; set; } = string.Empty;
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ClickHandler(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Clicked!");
    }
    private async Task SelectFolder()
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
               // Do something with the selected folder path
            }
        }
    }
    private async void OnSelectFolderButtonClick(object? sender, RoutedEventArgs e)
    {
        await SelectFolder();
    }
}