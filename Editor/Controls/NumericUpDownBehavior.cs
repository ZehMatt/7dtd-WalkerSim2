using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using System;
using System.Globalization;

namespace Editor.Controls
{
    /// <summary>
    /// Attached behavior that immediately clamps a NumericUpDown's displayed value to its
    /// Minimum/Maximum as the user types, rather than only on commit.
    ///
    /// Avalonia coerces Value internally before firing ValueChanged, so watching ValueChanged
    /// is too late — the text still shows the out-of-range input.  Instead we subscribe to the
    /// inner PART_TextBox.TextChanged and post a dispatcher update, which runs after Avalonia's
    /// own handler clears its _updateFromTextInput flag and therefore allows the text to refresh.
    /// </summary>
    public static class NumericUpDownBehavior
    {
        public static readonly AttachedProperty<bool> ClampImmediatelyProperty =
            AvaloniaProperty.RegisterAttached<NumericUpDown, bool>(
                "ClampImmediately", typeof(NumericUpDownBehavior));

        static NumericUpDownBehavior()
        {
            ClampImmediatelyProperty.Changed.AddClassHandler<NumericUpDown>(OnChanged);
        }

        public static bool GetClampImmediately(NumericUpDown element) => element.GetValue(ClampImmediatelyProperty);
        public static void SetClampImmediately(NumericUpDown element, bool value) => element.SetValue(ClampImmediatelyProperty, value);

        private static void OnChanged(NumericUpDown nud, AvaloniaPropertyChangedEventArgs e)
        {
            nud.TemplateApplied -= OnTemplateApplied;
            if (e.NewValue is true)
                nud.TemplateApplied += OnTemplateApplied;
        }

        private static void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
        {
            if (sender is not NumericUpDown nud)
                return;
            var textBox = e.NameScope.Find<TextBox>("PART_TextBox");
            if (textBox == null)
                return;

            textBox.TextChanged += (_, _) =>
            {
                var text = textBox.Text;
                if (string.IsNullOrEmpty(text))
                    return;
                // Only act on text that parses as a number; partial inputs like "-" or "." are left alone.
                if (!decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var parsed))
                    return;
                var clamped = Math.Clamp(parsed, nud.Minimum, nud.Maximum);
                if (clamped == parsed)
                    return;
                // Post so this runs after Avalonia's own TextChanged handler has finished
                // and cleared its _updateFromTextInput flag, allowing the text to be refreshed.
                Dispatcher.UIThread.Post(() => nud.Value = clamped);
            };
        }
    }
}
