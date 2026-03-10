using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Editor.Views
{
    public partial class PreferencesWindow : Window
    {
        private readonly ObservableCollection<string> _folders = new ObservableCollection<string>();

        public bool SettingsSaved { get; private set; }

        public PreferencesWindow()
        {
            InitializeComponent();

            // Populate combo boxes
            PanButtonCombo.ItemsSource = Enum.GetValues<MouseButton>();
            ZoomModifierCombo.ItemsSource = Enum.GetValues<ZoomModifier>();

            FoldersList.ItemsSource = _folders;

            // Show auto-detected game paths
            var detected = WorldLocator.FindGamePaths();
            DetectedFoldersList.ItemsSource = detected.Count > 0
                ? detected
                : new[] { "(none detected)" };

            LoadFromSettings();
        }

        private void LoadFromSettings()
        {
            var settings = EditorSettings.Instance;
            PanButtonCombo.SelectedItem = settings.PanButton;
            ZoomModifierCombo.SelectedItem = settings.ZoomModifier;

            _folders.Clear();
            foreach (var folder in settings.GameFolders)
                _folders.Add(folder);
        }

        private void ApplyToSettings()
        {
            var settings = EditorSettings.Instance;
            if (PanButtonCombo.SelectedItem is MouseButton pan)
                settings.PanButton = pan;
            if (ZoomModifierCombo.SelectedItem is ZoomModifier zoom)
                settings.ZoomModifier = zoom;

            settings.GameFolders.Clear();
            settings.GameFolders.AddRange(_folders);
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            ApplyToSettings();
            EditorSettings.Instance.Save();
            SettingsSaved = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            PanButtonCombo.SelectedItem = MouseButton.Right;
            ZoomModifierCombo.SelectedItem = ZoomModifier.Ctrl;
            _folders.Clear();
        }

        private async void OnAddFolderClick(object sender, RoutedEventArgs e)
        {
            var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Game Folder",
                AllowMultiple = false
            });

            if (result.Count > 0)
            {
                var path = result[0].TryGetLocalPath();
                if (!string.IsNullOrEmpty(path) && !_folders.Contains(path))
                    _folders.Add(path);
            }
        }

        private void OnRemoveFolderClick(object sender, RoutedEventArgs e)
        {
            if (FoldersList.SelectedItem is string selected)
                _folders.Remove(selected);
        }
    }
}
