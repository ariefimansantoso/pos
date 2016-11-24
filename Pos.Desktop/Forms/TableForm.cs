using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Pos.Desktop.Forms
{
    public partial class TableForm : Form
    {
        public TableForm()
        {
            InitializeComponent();
        }
        public POSDataContext ctx { set; get; }
        private bool InputValidate()
        {
            if (cboNum.SelectedItem == null)
                return false;
            if (string.IsNullOrEmpty(txtName.Text))
                return false;
            return true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (!InputValidate())
            {
                MessageBox.Show("Please fill the required fields");
                return;
            }

            TableSeat table = new TableSeat();
            table.IsDelete = !chkActivate.Checked;
            table.TableName = txtName.Text;
            table.TableNumber = Convert.ToInt32(cboNum.SelectedItem.ToString());
            table.TableStatus = "Free";
            if (cboType.SelectedItem != null)
                table.TableType = cboType.SelectedItem.ToString();
            else
                table.TableType = "Table";

            ctx.TableSeats.InsertOnSubmit(table);
            ctx.SubmitChanges();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
