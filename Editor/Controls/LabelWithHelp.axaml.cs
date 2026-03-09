using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;

namespace Editor.Controls
{
    public partial class LabelWithHelp : UserControl
    {
        public static readonly StyledProperty<string> LabelTextProperty =
            AvaloniaProperty.Register<LabelWithHelp, string>(nameof(LabelText), "");

        public static readonly StyledProperty<string> HelpUrlProperty =
            AvaloniaProperty.Register<LabelWithHelp, string>(nameof(HelpUrl), "");

        public string LabelText
        {
            get => GetValue(LabelTextProperty);
            set => SetValue(LabelTextProperty, value);
        }

        public string HelpUrl
        {
            get => GetValue(HelpUrlProperty);
            set => SetValue(HelpUrlProperty, value);
        }

        public LabelWithHelp()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == LabelTextProperty)
            {
                if (PART_Label != null)
                    PART_Label.Text = (string)change.NewValue;
            }
            else if (change.Property == HelpUrlProperty)
            {
                if (PART_HelpButton != null)
                    PART_HelpButton.IsVisible = !string.IsNullOrEmpty((string)change.NewValue);
            }
        }

        private void OnHelpClick(object sender, RoutedEventArgs e)
        {
            var url = HelpUrl;
            if (string.IsNullOrEmpty(url)) return;

            var version = WalkerSim.BuildInfo.Version == "0.0.0" ? "nightly" : WalkerSim.BuildInfo.Version;
            url = url.Replace("<version>", version);

            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch { }
        }
    }
}
