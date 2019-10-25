using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Calculator
{
    internal class MainForm : Form
    {
        public MainForm()
        {
            SetupForm();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetupForm()
        {    
            Size = new Size(300, 400);
            MinimumSize = new Size(300, 400);

            BackColor = Color.FromArgb(30, 30, 30);

            ShowIcon = false;
            Text = "Calculator";
            
            Controls.Add(new CalculatorView
            {
                Location = new Point(0, 0),
                Size = new Size(ClientSize.Width, ClientSize.Height),
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Right |
                         AnchorStyles.Bottom | 
                         AnchorStyles.Left
            });
        }
    }
}