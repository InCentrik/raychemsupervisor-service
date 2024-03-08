using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IC.RCS.RCSCore
{
    public class RCSFormWCFService : IRCSFormWCFService
    {
        private RichTextBox _formBox;

        public RCSFormWCFService(RichTextBox txtBox) { 
            _formBox = txtBox;
        }

        public void LogForm(string message)
        {
            _formBox.Text = message;
        }
    }
}
