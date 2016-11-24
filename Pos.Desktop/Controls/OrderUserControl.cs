using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Configuration;
using Pos.Desktop.Forms;
namespace Pos.Desktop.Controls
{
    public partial class OrderUserControl : UserControl
    {
        public TableSeat SelectedTable { set; get; }
        private int _currentOrderId = 0;
        private OrderKassa _currentOrder;
        public event EventHandler<EventArgs> BackToTablesHandler;
        public POSDataContext ctx { set; get; } 
        public OrderUserControl()
        {
            InitializeComponent();
        }

        protected virtual void OnBackToTablesHandler(EventArgs e)
        {            
            if (BackToTablesHandler != null)
                BackToTablesHandler(this, e);
        }

        private void Initialize()
        {
            //PopulateMenus();
            POSDataContext db = new POSDataContext();
            panelRightContent.Controls.Clear();
            TableSeat seat = SelectedTable;
            //ctx.DetectChanges();
            ctx.SubmitChanges();
            if (seat.TableStatus.ToLower() == "occupied")
            {
                int id = seat.TableID;
                var order = (from a in db.OrderKassas
                             where (a.TableID == id && a.IsComplete == false && a.OrderTotal == 0)
                             select a).ToList<OrderKassa>().LastOrDefault();
                if (order != null)
                {
                    _currentOrderId = order.OrderID;
                    OrderDetailPopulate(order.OrderID);
                    _currentOrder = order;
                    txtNamaPemesan.Text = order.Name;
                }
            }
            else
            {   
                int id = seat.TableID;
                var table = from a in ctx.TableSeats where a.TableID == id select a;
                if (table.Count() > 0)
                {
                    var item = table.ToArray()[0];
                    item.TableStatus = "Occupied";

                    OrderKassa order = new OrderKassa();
                    order.OrderDate = DateTime.Now;
                    order.IsComplete = false;
                    order.Remarks = string.Empty;
                    order.TableID = id;
                    order.CompletedDate = DateTime.Now.AddDays(-1);

                    ctx.OrderKassas.InsertOnSubmit(order);
                    ctx.SubmitChanges();
                    _currentOrderId = order.OrderID;
                    _currentOrder = order;
                }
            }
            
        }

        private void row_DeleteItemHandler(object sender, SelectedItemEventArgs e)
        {
            int id = ((OrderDetail)e.Id).MenuCardID;
            string name = ((OrderDetail)e.Id).CustomMenuName.Trim();
            if (!string.IsNullOrEmpty(name) || id != 1124)
            {
                var item = from a in ctx.OrderDetails
                           where a.MenuCardID == id && a.OrderID == _currentOrderId
                           select a;
                if (item.Count() > 0)
                {
                    var deletedItem = item.ToArray()[0];
                    var qty = deletedItem.Quantity;
                    var menuCardToUpdate = Helper.GetMenuCard(id);

                    UpdateMenuCard(id, menuCardToUpdate.Stock.Value + qty);
                    
                    ctx.OrderDetails.DeleteOnSubmit(deletedItem);
                    ctx.SubmitChanges();

                    OrderDetailPopulate(_currentOrderId);
                }
            }
            else               
            {                    
                var item = from a in ctx.OrderDetails
                            where a.CustomMenuName.Trim() == name && a.OrderID == _currentOrderId
                            select a;
                if (item.Count() > 0)
                {
                    var deletedItem = item.ToArray()[0];
                    ctx.OrderDetails.DeleteOnSubmit(deletedItem);
                    ctx.SubmitChanges();

                    OrderDetailPopulate(_currentOrderId);
                }
            }
            
        }

        private void OrderDetailPopulate(int orderId)
        {
            POSDataContext db = new POSDataContext();
            panelRightContent.Controls.Clear();
            int index = 0;
            var details = from a in db.OrderDetails
                          where a.OrderID == orderId
                          select a;

            decimal totalAll = 0;
            foreach (var detail in details)
            {
                ItemRowUserControl row = new ItemRowUserControl();
                row.Name = detail.MenuCardID.ToString();

                if (!string.IsNullOrEmpty(detail.CustomMenuName))
                {
                    row.ItemTitle = detail.CustomMenuName;
                    row.SubTotal = detail.CustomMenuPrice * detail.Quantity;
                }
                else
                {
                    row.SubTotal = detail.MenuCard.Price * detail.Quantity;
                    row.ItemTitle = detail.MenuCard.MenuName;
                }


                if(detail.CustomMenuPrice > 0)
                    row.Price = detail.CustomMenuPrice;
                else
                    row.Price = detail.MenuCard.Price;

                totalAll = totalAll + (row.SubTotal);
                row.OrderDetailSelected = detail;
                row.Quantity = detail.Quantity.ToString();

                row.DeleteItemHandler += new EventHandler<SelectedItemEventArgs>(row_DeleteItemHandler);
                row.RecalculateTotalHandler += new EventHandler<EventArgs>(row_RecalculateTotalHandler);
                panelRightContent.Controls.Add(row);

                row.Top = 2 + index * 64;
                row.Left = 5;
                index++;
            }
            lbTotal.Text = "Rp. " + totalAll.ToString("N0");
        }

        private void Recalculate()
        {
            decimal totalAll = 0;
            foreach (var item in panelRightContent.Controls)
            {
                ItemRowUserControl row = (ItemRowUserControl)item;
                totalAll = totalAll + row.SubTotal;
            }
            lbTotal.Text = "Rp. " + totalAll.ToString("N0");
        }

        public void PopulateMenus()
        {
            Initialize();

            PopulateMenuCard();
        }

        private void PopulateMenuCard()
        {
            POSDataContext posDb = new POSDataContext();
            panelLeft.Controls.Clear();
            this.lbTableTitle.Text = SelectedTable.TableName;
            int length = this.Width - 560;
            int totalPerRow = 5;
            if (length <= 1024)
                totalPerRow = 4;

            var menus = posDb.MenuCards.Where(x => x.id != 1124).OrderBy(m => m.MenuGroupId);
            Dictionary<int, Color> colorKey = new Dictionary<int, Color>();
            Color[] colors = new Color[] { Color.Blue, Color.Green,
                    Color.Yellow, Color.Pink, Color.BlueViolet, Color.DarkRed,
                    Color.Aqua,Color.GreenYellow,Color.Cyan};
            int index = 0;
            int row = 0;
            int indexCol = 0;
            foreach (var menu in menus)
            {
                MenuUserControl uc = new MenuUserControl();
                uc.LabelControl.MouseClick += new MouseEventHandler(LabelControl_MouseClick);

                if (!colorKey.ContainsKey(menu.MenuGroupId))
                {
                    if (indexCol <= 8)
                    {
                        colorKey.Add(menu.MenuGroupId, colors[indexCol]);
                        indexCol++;
                    }
                }

                if (indexCol <= 8)
                    uc.BackColor = colorKey[menu.MenuGroupId];
                else
                    uc.BackColor = Color.DarkSalmon;

                var menuLabel = menu.MenuName + (menu.Stock.HasValue ? "(" + menu.Stock.Value.ToString() + ")" : " (0)");

                uc.TableControlTitle = menuLabel;
                uc.LabelControl.Tag = menu;
                uc.Name = "Menu" + menu.id.ToString();
                panelLeft.Controls.Add(uc);
                if (index == 0)
                    uc.Left = 5;
                else
                    uc.Left = 5 + (index * 160);

                uc.Top = 10 + (row * 110);
                index++;
                if ((index + 1) > totalPerRow)
                {
                    index = 0;
                    row++;
                }
            }
        }

        private void LabelControl_MouseClick(object sender, MouseEventArgs e)
        {
            POSDataContext posDb = new POSDataContext();
            MenuCard menu = Helper.GetMenuCard(((MenuCard)((Label)sender).Tag).id);

            if(menu.Stock.HasValue)
            {
                if(menu.Stock.Value <= 0)
                {
                    MessageBox.Show("Stok Habis!!!");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Stok Habis!!!");
                return;
            }

            if (!panelRightContent.Controls.ContainsKey(menu.id.ToString()))
            {
                OrderDetail orderDetail = new OrderDetail();
                orderDetail.OrderID = _currentOrderId;
                orderDetail.MenuCardID = menu.id;
                orderDetail.Quantity = 1;
                orderDetail.Remarks = string.Empty;
                orderDetail.CustomMenuName = string.Empty;
                orderDetail.CustomMenuPrice = 0;

                posDb.OrderDetails.InsertOnSubmit(orderDetail);
                posDb.SubmitChanges();

                ItemRowUserControl row = new ItemRowUserControl();
                row.Name = menu.id.ToString();
                row.ItemTitle = menu.MenuName;
                row.Price = menu.Price;
                row.SubTotal = menu.Price;
                row.Quantity = "1";
                decimal currentTotal = 0;
                if (lbTotal.Text.Trim().Length > 2)
                    currentTotal = Convert.ToDecimal(lbTotal.Text.Replace("Rp.",""));
                decimal totalAll = currentTotal + row.SubTotal;
                row.OrderDetailSelected = orderDetail;
                //row.OrderDetailId = orderDetail.OrderDetailID;

                row.DeleteItemHandler += new EventHandler<SelectedItemEventArgs>(row_DeleteItemHandler);
                row.RecalculateTotalHandler += new EventHandler<EventArgs>(row_RecalculateTotalHandler);
                panelRightContent.Controls.Add(row);

                row.Top = 2 + (panelRightContent.Controls.Count - 1) * 64;
                row.Left = 5;

                lbTotal.Text = "Rp. " + totalAll.ToString("N0");
            }
            else
            {
                var item = from a in posDb.OrderDetails
                           where a.MenuCardID == menu.id && a.OrderID == _currentOrderId
                           select a;
                if (item.Count() > 0)
                {
                    var orderDetail = item.ToArray()[0];
                    orderDetail.Quantity = orderDetail.Quantity + 1;
                    posDb.SubmitChanges();

                    ((ItemRowUserControl)panelRightContent.Controls[menu.id.ToString()]).Quantity = orderDetail.Quantity.ToString();
                    ((ItemRowUserControl)panelRightContent.Controls[menu.id.ToString()]).OrderDetailSelected = orderDetail;
                }
            }

            var stock = menu.Stock.HasValue ? (menu.Stock.Value - 1) : 0;
            
            UpdateMenuCard(menu.id, stock);
        }

        private void UpdateMenuCard(int menuId, int newStock)
        {
            var updatedMenu = Helper.UpdateMenuStock(menuId, newStock);

            var menuCard = panelLeft.Controls.Find("Menu" + menuId.ToString(), true)[0] as MenuUserControl;

            var menuLabel = updatedMenu.MenuName + (updatedMenu.Stock.HasValue ? "(" + newStock + ")" : " (0)");

            menuCard.Tag = updatedMenu;

            menuCard.TableControlTitle = menuLabel;
        }

        private void row_RecalculateTotalHandler(object sender, EventArgs e)
        {
            Recalculate();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {            
            // back to tables
            this.OnBackToTablesHandler(e);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            POSDataContext db = new POSDataContext();
            //close table
            var ret = MessageBox.Show("Are you sure you want to close this table?",
                    "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (ret != System.Windows.Forms.DialogResult.OK)
                return;

            int tableId = SelectedTable.TableID;
            var order = from a in db.OrderKassas
                        where a.OrderID == _currentOrderId && a.TableID == tableId
                        select a;

            if (order.Count() > 0)
            {
                OrderKassa itemOrder = order.ToArray()[0];
                var tab = db.TableSeats.First(m => m.TableID == tableId);
                tab.TableStatus = "Free";
                
                var details = from a in db.OrderDetails where a.OrderID == itemOrder.OrderID select a;
                decimal total = 0;

                if (details.Count() > 0)
                {
                    foreach (var detail in details)
                    {
                        if (detail.CustomMenuPrice > 0)
                            total = total + detail.Quantity * detail.CustomMenuPrice;
                        else
                            total = total + detail.Quantity * detail.MenuCard.Price;
                    }
                    itemOrder.OrderTotal = total;
                    itemOrder.IsComplete = false;
                    itemOrder.CompletedDate = DateTime.Now;
                }
                else
                {
                    db.OrderKassas.DeleteOnSubmit(itemOrder);
                }

                db.SubmitChanges();
                this.OnBackToTablesHandler(e);
            }
            else
                MessageBox.Show("Order was not found for this table");
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            Pos.Desktop.Forms.Helper.PrintOrder(this.ctx,_currentOrder, true,false);
        }

        private void btnPrintWithout_Click(object sender, EventArgs e)
        {
            Pos.Desktop.Forms.Helper.PrintOrder(this.ctx, _currentOrder, false,false);
        }

        private void btnKitchen_Click(object sender, EventArgs e)
        {
            Pos.Desktop.Forms.Helper.PrintOrder(this.ctx, _currentOrder, false,true);
        }

        private void btnAddCustom_Click(object sender, EventArgs e)
        {
            // add custom menu
            if (string.IsNullOrEmpty(txtCustomName.Text) || string.IsNullOrEmpty(txtPrice.Text))
            {
                MessageBox.Show("Custom Menu and Price are required fields");
                return;
            }

            OrderDetail orderDetail = new OrderDetail();
            orderDetail.OrderID = _currentOrderId;
            orderDetail.MenuCardID = 1124; //dummy
            if (cboMenuQuantity.SelectedItem != null)
                orderDetail.Quantity = Convert.ToInt32(cboMenuQuantity.SelectedItem);
            else
                orderDetail.Quantity = 0;
            orderDetail.Remarks = string.Empty;
            orderDetail.CustomMenuName = txtCustomName.Text;
            orderDetail.CustomMenuPrice = Convert.ToDecimal(txtPrice.Text);

            ctx.OrderDetails.InsertOnSubmit(orderDetail);
            ctx.SubmitChanges();

            ItemRowUserControl row = new ItemRowUserControl();
            row.Name = "1";
            row.ItemTitle = txtCustomName.Text;
            row.Price = Convert.ToDecimal(txtPrice.Text);
            row.SubTotal = orderDetail.Quantity * row.Price;
            row.Quantity = orderDetail.Quantity.ToString();
            decimal currentTotal = 0;
            if (lbTotal.Text.Trim().Length > 2)
                currentTotal = Convert.ToDecimal(lbTotal.Text.Replace("Rp.", ""));
            decimal totalAll = currentTotal + row.SubTotal;
            row.OrderDetailSelected = orderDetail;

            row.DeleteItemHandler += new EventHandler<SelectedItemEventArgs>(row_DeleteItemHandler);
            row.RecalculateTotalHandler += new EventHandler<EventArgs>(row_RecalculateTotalHandler);
            panelRightContent.Controls.Add(row);

            row.Top = 2 + (panelRightContent.Controls.Count - 1) * 64;
            row.Left = 5;

            lbTotal.Text = "Rp. " + totalAll.ToString("N0");

        }

        private void txtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            const char Delete = (char)8;
            e.Handled = !Char.IsDigit(e.KeyChar) && e.KeyChar != Delete && e.KeyChar != '.';
        }

        private void btnSaveName_Click(object sender, EventArgs e)
        {
            POSDataContext db = new POSDataContext();
            var currentOrder = db.OrderKassas.SingleOrDefault(x => x.OrderID == _currentOrderId);
            if(currentOrder != null)
            {
                currentOrder.Name = txtNamaPemesan.Text;
                db.SubmitChanges();
                MessageBox.Show("Nama Pemesan Telah Tersimpan.");
            }
            else
            {
                MessageBox.Show("Order tidak ditemukan, coba kembali.");
            }
        }

        private void btnBNI_Click(object sender, EventArgs e)
        {            
            Pos.Desktop.Forms.Helper.PrintOrder(this.ctx, _currentOrder, false, false, true);
        }

        private void btnDiskon_Click(object sender, EventArgs e)
        {
            int inputDiskon;
            if (!int.TryParse(txtDiskon.Text, out inputDiskon))
            {
                MessageBox.Show("Angka diskon salah!!!");
                return;
            }

            POSDataContext db = new POSDataContext();
            var currentOrder = db.OrderKassas.SingleOrDefault(x => x.OrderID == _currentOrderId);
            if (currentOrder != null)
            {
                currentOrder.Discount = inputDiskon;
                db.SubmitChanges();
                MessageBox.Show("Diskon Telah Tersimpan.");
            }
            else
            {
                MessageBox.Show("Order tidak ditemukan, coba kembali.");
            }
        }
    }
}
