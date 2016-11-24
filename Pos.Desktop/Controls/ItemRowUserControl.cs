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
    public partial class ItemRowUserControl : UserControl
    {

        public event EventHandler<SelectedItemEventArgs> DeleteItemHandler;
        public event EventHandler<EventArgs> RecalculateTotalHandler;
        public OrderDetail OrderDetailSelected { set; get; }
        public int OrderDetailId { set; get; }

        public string ItemTitle
        {
            set
            {
                this.lbItem.Text = value;
            }
            get
            {
                return this.lbItem.Text;
            }
        }
        public string Quantity
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.txtQuantity.Text = "0";
                else
                    this.txtQuantity.Text = value;
                
            }
            get
            {
                if (string.IsNullOrEmpty(this.txtQuantity.Text.Trim()))
                    return "0";
                else
                    return this.txtQuantity.Text;                
            }
        }
        public Decimal Price
        {
            set
            {
                this.lbPrice.Text = value.ToString("N0");
            }
            get
            {
                return Convert.ToDecimal(this.lbPrice.Text);
            }
        }
        public Decimal SubTotal
        {
            set
            {
                this.lbSubTotal.Text = value.ToString("N0");
            }
            get
            {
                if (string.IsNullOrEmpty(this.lbSubTotal.Text))
                    return 0;
                return Convert.ToDecimal(this.lbSubTotal.Text);
            }
        }
        protected virtual void OnDeleteItemHandler(SelectedItemEventArgs e)
        {
            if (DeleteItemHandler != null)
                DeleteItemHandler(this, e);
        }
        protected virtual void OnRecalculateTotalHandler(EventArgs e)
        {
            if (RecalculateTotalHandler != null)
                RecalculateTotalHandler(this, e);
        }
        public ItemRowUserControl()
        {
            InitializeComponent();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            SelectedItemEventArgs ar = new SelectedItemEventArgs(OrderDetailSelected);            
            this.OnDeleteItemHandler(ar);
        }

        private void txtQuantity_TextChanged(object sender, EventArgs e)
        {
            int quantity = 0;
            if (!string.IsNullOrEmpty(txtQuantity.Text))
            {
                quantity = Convert.ToInt32(this.txtQuantity.Text);
                SubTotal = quantity * Price;
            }
            else
            {
                SubTotal = 0;
            }
            using (POSDataContext ctx = new POSDataContext())
            {
                if (OrderDetailSelected != null)
                {
                    var item = from a in ctx.OrderDetails
                               where a.OrderDetailID == OrderDetailSelected.OrderDetailID
                               select a;
                    if (item != null)
                    {
                        if (item.Count() > 0)
                        {
                            var orderDetail = item.ToArray()[0];
                            orderDetail.Quantity = quantity;
                            ctx.SubmitChanges();
                        }
                    }
                }
            }
            OnRecalculateTotalHandler(e);
        }

        private void txtQuantity_KeyPress(object sender, KeyPressEventArgs e)
        {
            const char Delete = (char)8;
            e.Handled = !Char.IsDigit(e.KeyChar) && e.KeyChar != Delete;
        }
    }
}
