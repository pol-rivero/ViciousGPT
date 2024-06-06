﻿using System.Windows;
using System.Windows.Input;
using ViciousGPT.Properties;
using static ViciousGPT.GlobalHotkeyManager;
using Microsoft.Win32;
using System.IO;


namespace ViciousGPT;

public partial class MainWindow : Window
{
    private GlobalHotkeyManager? triggerHotkey;
    private GlobalHotkeyManager? cancelHotkey;
    private readonly GlobalController globalController;

    public MainWindow()
    {
        InitializeComponent();
        globalController = new GlobalController(this);
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;

        openAiKeyBox.Password = Settings.Default.OpenAiKey;
        openAiKeyBox.PasswordChanged += (sender, e) => Settings.Default.OpenAiKey = openAiKeyBox.Password;
        RefreshSelectFileDoneVisibility();
    }

    private void RefreshSelectFileDoneVisibility()
    {
        selectFileDone.Visibility = Settings.Default.GoogleServiceAccountPath.Length > 0 ? Visibility.Visible : Visibility.Hidden;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Settings.Default.PropertyChanged += (sender, e) => Settings.Default.Save();
        triggerHotkey = new GlobalHotkeyManager(this, Modifier.Control, Key.Enter, () => globalController.OnTriggered());
        cancelHotkey = new GlobalHotkeyManager(this, Modifier.Control|Modifier.Alt, Key.Enter, () => globalController.OnCancelled());
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        triggerHotkey?.Dispose();
        cancelHotkey?.Dispose();
    }

    protected override void OnClosed(EventArgs e)
    {
        globalController.Dispose();
        base.OnClosed(e);
    }

    private void selectFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog();
        bool success = dialog.ShowDialog() ?? false;
        if (success)
        {
            string newFilePath = Path.Combine(GetPersistentFolder(), "service-account.json");
            File.Copy(dialog.FileName, newFilePath, true);
            Settings.Default.GoogleServiceAccountPath = newFilePath;
        }
        RefreshSelectFileDoneVisibility();
    }

    private static string GetPersistentFolder()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string persistentFolder = Path.Combine(appData, "ViciousGPT");
        if (!Directory.Exists(persistentFolder))
        {
            Directory.CreateDirectory(persistentFolder);
        }
        return persistentFolder;
    }
}
