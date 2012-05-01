using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GreenshotFogbugzPlugin
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
        }

        public string Url
        {
            get
            {
                return this.txtUrl.Text;
            }
            set
            {
                this.txtUrl.Text = value;
            }
        }

        public string Email
        {
            get
            {
                return this.txtEmail.Text;
            }
            set
            {
                this.txtEmail.Text = value;
            }
        }

        public string Password
        {
            get
            {
                return this.txtPassword.Text;
            }
            set
            {
                this.txtPassword.Text = value;
            }
        }
    }
}
