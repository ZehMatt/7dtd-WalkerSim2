using Avalonia;
using Avalonia.Controls;
using Editor.ViewModels;

namespace Editor.Views
{
    public partial class ColorPickerControl : UserControl
    {
        public static readonly StyledProperty<string> ColorStringProperty =
            AvaloniaProperty.Register<ColorPickerControl, string>(
                nameof(ColorString),
                defaultValue: "#808080",
                defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public string ColorString
        {
            get => GetValue(ColorStringProperty);
            set => SetValue(ColorStringProperty, value);
        }

        private readonly ColorPickerViewModel _vm;
        private bool _updatingFromVm;
        private bool _updatingStyledProp;

        public ColorPickerControl()
        {
            _vm = new ColorPickerViewModel();

            // The control owns its DataContext, so the Flyout content will also
            // inherit it regardless of the parent's DataContext.
            DataContext = _vm;
            InitializeComponent();

            _vm.ColorChanged = str =>
            {
                if (_updatingStyledProp) return;
                _updatingFromVm = true;
                SetValue(ColorStringProperty, str);
                _updatingFromVm = false;
            };
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ColorStringProperty && !_updatingFromVm)
            {
                _updatingStyledProp = true;
                _vm.ColorString = change.GetNewValue<string>() ?? "#808080";
                _updatingStyledProp = false;
            }
        }
    }
}
