using System.Windows.Forms;

namespace Serial
{
    public partial class MainFormSerial : Form
    {
        public MainFormSerial()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            InitializeComponent();

            MacroReAdjust();

        }
    } // closing brace of public partial class Form1 : Form
}// closing brace of namesapce Serial


