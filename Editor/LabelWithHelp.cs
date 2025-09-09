using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WalkerSim.Editor
{
    public partial class LabelWithHelp : UserControl
    {
        private Label mainLabel;
        private LinkLabel helpLink;

        public LabelWithHelp()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            mainLabel = new Label
            {
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 0),
                Location = new Point(0, 0)
            };

            helpLink = new LinkLabel
            {
                Text = "?",
                AutoSize = true,
                Margin = new Padding(0, 0, 2, 0),
                Location = new Point(mainLabel.Right, 0)
            };
            helpLink.LinkClicked += HelpLink_LinkClicked;

            this.Controls.Add(mainLabel);
            this.Controls.Add(helpLink);

            this.AutoSize = true;
            this.Padding = new Padding(4, 0, 0, 2);
            this.Margin = new Padding(4, 0, 0, 2);
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        private void HelpLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OnHelpClicked(EventArgs.Empty);
        }

        // Event to allow subscribing to the ? click
        public event EventHandler HelpClicked;
        protected virtual void OnHelpClicked(EventArgs e)
        {
            HelpClicked?.Invoke(this, e);
        }

        // Property to set the label text
        [Category("Appearance")]
        [Description("The text displayed by the label.")]
        public string LabelText
        {
            get => mainLabel.Text;
            set
            {
                mainLabel.Text = value;
                // Update help link location
                helpLink.Location = new Point(mainLabel.Right, mainLabel.Top);
            }
        }

        [Category("Behavior")]
        [Description("The URL opened when the help link is clicked.")]
        public string HelpUrl { get; set; }
    }
}
