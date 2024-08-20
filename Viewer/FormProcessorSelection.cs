using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WalkerSim.Viewer
{
    public partial class FormProcessorSelection : Form
    {
        public Config.MovementProcessorType Choice = Config.MovementProcessorType.Invalid;
        private List<Config.MovementProcessorType> _choices = new List<Config.MovementProcessorType>();

        public FormProcessorSelection()
        {
            InitializeComponent();

            var startChoices = Enum.GetValues(typeof(Config.MovementProcessorType)).Cast<Config.MovementProcessorType>();
            foreach (var choice in startChoices)
            {
                if (choice == Config.MovementProcessorType.Invalid)
                    continue;

                listProcessors.Items.Add(choice);
                _choices.Add(choice);
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            var idx = listProcessors.SelectedIndex;
            if (idx == -1)
            {
                buttonOk.Enabled = false;
                return;
            }

            buttonOk.Enabled = true;
            Choice = _choices[idx];
        }
    }
}
