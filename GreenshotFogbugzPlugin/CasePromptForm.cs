using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GreenshotFogbugzPlugin
{
    public partial class CasePromptForm : Form
    {

        public CasePromptForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public string Instruction
        {
            get
            {
                return this.lblInstruction.Text;
            }
            set
            {
                this.lblInstruction.Text = value;
            }
        }

        public string CaseId
        {
            get
            {
                return this.txtCaseId.Text;
            }
            set
            {
                this.txtCaseId.Text = value;
            }
        }
    }
}
