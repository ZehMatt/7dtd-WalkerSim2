using Avalonia;
using Avalonia.Controls;
using Editor.Audio;
using System;

namespace Editor.Views
{
    // Thin wrapper that hosts AboutVFXGl. On shader init failure the GL
    // control clears magenta as a visible signal rather than crashing.
    public sealed class AboutVFXHost : ContentControl
    {
        private WavPlayer _synth;
        private AboutVFXGl _gl;

        public WavPlayer Synth
        {
            get => _synth;
            set
            {
                _synth = value;
                if (_gl != null)
                    _gl.Synth = value;
            }
        }

        // Forwarded from AboutVFXGl — fires after the first frame renders.
        public event Action Ready;

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            if (_gl == null)
            {
                _gl = new AboutVFXGl { Synth = _synth };
                _gl.Ready += () => Ready?.Invoke();
                Content = _gl;
            }
        }
    }
}
