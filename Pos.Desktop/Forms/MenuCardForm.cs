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
    public partial class MenuCardForm : Form
    {
        public int FormModel { set; get; } //1 add, 2 edit;
        public MenuCard SelectedMenu { set; get; }
        public POSDataContext context { set; get; }        
        public MenuCardForm()
        {
            InitializeComponent();
        }

        private void MenuCardForm_Load(object sender, EventArgs e)
        {
            cboGroup.DataSource = context.MenuGroups;
            if (FormModel == 2)            
            {
                cboGroup.SelectedValue = SelectedMenu.MenuGroupId;
                txtDes.Text = SelectedMenu.Description;
                txtName.Text = SelectedMenu.MenuName;
                txtNum.Text = SelectedMenu.Number.ToString();
                txtPrice.Text = SelectedMenu.Price.ToString("00.00");
                txtStock.Text = SelectedMenu.Stock.HasValue ? SelectedMenu.Stock.Value.ToString() : "0";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        private bool InputValidate()
        {
            if (cboGroup.SelectedItem == null)
                return false;
            if (string.IsNullOrEmpty(txtName.Text))
                return false;
            return true;
        }
        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (!InputValidate())
            {
                MessageBox.Show("Please fill the required fields");
                return;
            }

            MenuCard menu = null;
            if (FormModel == 2)
                menu = context.MenuCards.First(m => m.id == SelectedMenu.id);
            else
                menu = new MenuCard();

            menu.MenuGroup = context.MenuGroups.First(x => x.id == (int)cboGroup.SelectedValue);
            menu.MenuGroupId = (int)cboGroup.SelectedValue;
            menu.MenuName = txtName.Text;

            if (!string.IsNullOrEmpty(txtNum.Text))
                menu.Number = Convert.ToInt32(txtNum.Text);
            else
                menu.Number = null;

            if (!string.IsNullOrEmpty(txtStock.Text))
                menu.Stock = Convert.ToInt32(txtStock.Text);
            else
                menu.Number = null;

            try
            {
                if (!string.IsNullOrEmpty(txtPrice.Text))
                    menu.Price = Convert.ToDecimal(txtPrice.Text);
                else
                    menu.Price = 0;
            }
            catch (Exception)
            {
                menu.Price = 0;
            }
            menu.Description = txtDes.Text;

            if (FormModel == 1)
                context.MenuCards.InsertOnSubmit(menu);


            context.SubmitChanges();                
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void txtNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            const char Delete = (char)8;
            e.Handled = !Char.IsDigit(e.KeyChar) && e.KeyChar != Delete;
        }

        private void txtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            const char Delete = (char)8;
            e.Handled = !Char.IsDigit(e.KeyChar) && e.KeyChar != Delete && e.KeyChar!='.';
        }
    }
}
