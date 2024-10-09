using System;
using System.Windows.Forms;

namespace Serial
{
    public partial class MainFormSerial : Form
    {
        public const int TexBoxWidth_adjust = 167; //112

        private void LeftRightSplitterChanged(object sender, SplitterEventArgs e)
        {
            //richTextBox1.Width = this.Width - (LeftRight_SplitC.SplitterDistance) - TexBoxWidth_adjust;
        }

        /***********************************************************************************************/
        private void MainForm_SplitC_DoubleClick(object sender, EventArgs e)
        {
            MainForm_SplitC.SplitterDistance = 0;
        }
        private void LeftRightSplitterDoubleClick(object sender, EventArgs e)
        {
            LeftRight_SplitC.SplitterDistance = this.Width;
        }

        private void MacroSplitPannel_DoubleClick(object sender, EventArgs e)
        {
            MacroSplitPannel.SplitterDistance = 0;
        }

        /***********************************************************************************************/

        private void MacroSplitterMoved(object sender, SplitterEventArgs e)
        {
            MacroReAdjust();
        }

        void MacroReAdjust()
        {
            int SizeAdjust;
            if (this.MacroSplitPannel.SplitterDistance >= 250)
            {
                this.MacroSplitPannel.SplitterDistance = 250; // clamp
            }
            if (this.MacroSplitPannel.SplitterDistance > 153)
            {
                SizeAdjust = this.MacroSplitPannel.SplitterDistance - 35;
            }
            else
            {
                SizeAdjust = 125;
            }

            // now readjust the buttons widths..
            // Macrobutton1.Width = SizeAdjust;

        }


        /***********************************************************************************************/
        private void TopSplitterMoved(object sender, SplitterEventArgs e)
        {
            if (this.MainForm_SplitC.SplitterDistance >= 115)
                this.MainForm_SplitC.SplitterDistance = 115; // clamp
        }

    }
}