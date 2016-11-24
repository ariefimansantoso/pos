using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Pos.Desktop.Controls
{
    public partial class TableUserControl : UserControl
    {
        public string TableControlTitle
        {
            get
            {
                return this.lbTableName.Text;
            }
            set
            {
                this.lbTableName.Text = value;
            }
        }
        public Label LabelControl
        {
            get
            {
                return this.lbTableName;
            }
            set
            {
                this.lbTableName = value;
            }
        }
        public void ConfigureBackground(string status)
        {
            if (status.ToLower() == "occupied")
            {
                this.BackColor = Color.Salmon;
            }
            else
            {
                this.BackColor = Color.GreenYellow;                     
            }
        }
        public TableUserControl()
        {
            InitializeComponent();
        }

    }
}
