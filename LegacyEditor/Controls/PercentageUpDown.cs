using System.Windows.Forms;

namespace WalkerSim.Editor.Controls
{
    public partial class PercentageUpDown : NumericUpDown
    {
        protected override void UpdateEditText()
        {
            base.UpdateEditText();

            ChangingText = true;
            Text += "%";
        }
    }
}
