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
    public partial class MenuUserControl : UserControl
    {
        public string TableControlTitle
        {
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
        }
        
        public MenuUserControl()
        {
            InitializeComponent();
        }
    }
}
