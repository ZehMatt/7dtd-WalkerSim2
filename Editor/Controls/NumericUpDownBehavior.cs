using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Globalization;

namespace Editor.Controls
{
    public enum NumericInputMode
    {
        /// <summary>Derive from control properties: integer if Increment is whole, decimal otherwise.</summary>
        Auto,
        /// <summary>Digits only (and minus if Minimum &lt; 0).</summary>
        Integer,
        /// <summary>Digits, dot, comma (and minus if Minimum &lt; 0).</summary>
        Decimal,
    }

    /// <summary>
    /// Attached behavior for NumericUpDown that:
    /// 1. Filters keyboard input so only valid numeric characters are accepted.
    /// 2. Immediately clamps the displayed value to Minimum/Maximum as the user types.
    /// </summary>
    public static class NumericUpDownBehavior
    {
        public static readonly AttachedProperty<bool> ClampImmediatelyProperty =
            AvaloniaProperty.RegisterAttached<NumericUpDown, bool>(
                "ClampImmediately", typeof(NumericUpDownBehavior));

        public static readonly AttachedProperty<NumericInputMode> InputModeProperty =
            AvaloniaProperty.RegisterAttached<NumericUpDown, NumericInputMode>(
                "InputMode", typeof(NumericUpDownBehavior), NumericInputMode.Auto);

        static NumericUpDownBehavior()
        {
            ClampImmediatelyProperty.Changed.AddClassHandler<NumericUpDown>(OnChanged);
        }

        public static bool GetClampImmediately(NumericUpDown element) => element.GetValue(ClampImmediatelyProperty);
        public static void SetClampImmediately(NumericUpDown element, bool value) => element.SetValue(ClampImmediatelyProperty, value);

        public static NumericInputMode GetInputMode(NumericUpDown element) => element.GetValue(InputModeProperty);
        public static void SetInputMode(NumericUpDown element, NumericInputMode value) => element.SetValue(InputModeProperty, value);

        private static void OnChanged(NumericUpDown nud, AvaloniaPropertyChangedEventArgs e)
        {
            nud.TemplateApplied -= OnTemplateApplied;
            if (e.NewValue is true)
                nud.TemplateApplied += OnTemplateApplied;
        }

        private static bool IsDecimalMode(NumericUpDown nud)
        {
            var mode = GetInputMode(nud);
            if (mode == NumericInputMode.Integer) return false;
            if (mode == NumericInputMode.Decimal) return true;
            // Auto: infer from Increment — if it has a fractional part, allow decimals.
            return nud.Increment % 1 != 0;
        }

        private static void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
        {
            if (sender is not NumericUpDown nud)
                return;
            var textBox = e.NameScope.Find<TextBox>("PART_TextBox");
            if (textBox == null)
                return;

            // Filter input at the tunnel level — invalid characters never reach the TextBox.
            textBox.AddHandler(InputElement.TextInputEvent, (_, args) =>
            {
                var input = args.Text;
                if (string.IsNullOrEmpty(input))
                    return;

                var currentText = textBox.Text ?? "";
                var caretIndex = textBox.CaretIndex;
                var allowDecimal = IsDecimalMode(nud);
                var allowNegative = nud.Minimum < 0;

                foreach (var ch in input)
                {
                    if (char.IsDigit(ch))
                        continue;

                    // Decimal separator: allow dot and comma as interchangeable decimal separators.
                    if (allowDecimal && (ch == '.' || ch == ','))
                    {
                        // Only allow one decimal separator in the text.
                        if (!currentText.Contains('.') && !currentText.Contains(','))
                            continue;

                        args.Handled = true;
                        return;
                    }

                    // Minus: only at position 0, only one, only if field allows negative.
                    if (ch == '-' && allowNegative && caretIndex == 0 && !currentText.Contains("-"))
                        continue;

                    args.Handled = true;
                    return;
                }
            }, RoutingStrategies.Tunnel);

            // Intercept Value becoming null (empty/invalid text) and restore the last good value.
            // This runs before the two-way binding propagates null to the ViewModel.
            decimal lastGoodValue = nud.Value ?? nud.Minimum;
            bool restoring = false;
            nud.PropertyChanged += (_, args) =>
            {
                if (args.Property != NumericUpDown.ValueProperty || restoring)
                    return;
                if (nud.Value.HasValue)
                {
                    lastGoodValue = nud.Value.Value;
                }
                else
                {
                    // Value went null — block it from reaching the binding.
                    restoring = true;
                    nud.Value = lastGoodValue;
                    restoring = false;
                }
            };

            // Restore last good value when the user leaves the field with invalid text.
            textBox.LostFocus += (_, _) =>
            {
                var text = textBox.Text;
                if (string.IsNullOrEmpty(text) ||
                    !decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out _))
                {
                    nud.Value = lastGoodValue;
                }
            };

            // Clamp value as the user types.
            textBox.TextChanged += (_, _) =>
            {
                var text = textBox.Text;
                if (string.IsNullOrEmpty(text))
                    return;
                if (!decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var parsed))
                    return;
                var clamped = Math.Clamp(parsed, nud.Minimum, nud.Maximum);
                if (clamped == parsed)
                    return;
                Dispatcher.UIThread.Post(() => nud.Value = clamped);
            };
        }
    }
}
