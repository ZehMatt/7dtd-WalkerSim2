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
            InitializeComponent();

            // Set DataContext on the INNER grid only — not on the UserControl itself.
            // If we set it on the UserControl, the external binding
            // ColorString="{Binding Color, Mode=TwoWay}" would resolve against _vm
            // (which has no Color property) instead of the MovementProcessorGroupModel.
            InnerGrid.DataContext = _vm;

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
