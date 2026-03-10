using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Editor.ViewModels;

namespace Editor.Views
{
    public partial class MovementSystemsView : UserControl
    {
        public MovementSystemsView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private EditorViewModel? _vm;

        private void OnDataContextChanged(object? sender, System.EventArgs e)
        {
            if (_vm != null)
                _vm.PropertyChanged -= OnVmPropertyChanged;

            _vm = DataContext as EditorViewModel;

            if (_vm != null)
                _vm.PropertyChanged += OnVmPropertyChanged;
        }

        private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(EditorViewModel.TreeSelectedItem))
                return;

            if (_vm?.TreeSelectedItem is Models.MovementProcessorModel proc)
            {
                // Find the parent system and expand its tree node.
                var parentSystem = _vm.MovementSystems.FirstOrDefault(s => s.Processors.Contains(proc));
                if (parentSystem != null)
                    ExpandSystem(parentSystem);
            }
        }

        private void ExpandSystem(Models.MovementProcessorGroupModel system)
        {
            var tree = this.FindControl<TreeView>("SystemsTree");
            if (tree == null) return;

            foreach (var item in tree.GetRealizedContainers())
            {
                if (item is TreeViewItem tvi && tvi.DataContext == system)
                {
                    tvi.IsExpanded = true;
                    break;
                }
            }
        }
    }
}
