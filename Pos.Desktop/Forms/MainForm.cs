using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Configuration;

using Pos.Desktop.Controls;
namespace Pos.Desktop.Forms
{
    public partial class MainForm : Form
    {
        private KassaUserControl _ucKassa = new KassaUserControl();
        private List<TableUserControl> _listTables = new List<TableUserControl>();
        private POSDataContext ctx = new POSDataContext(); 
        private int _show = -1;
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            Initialize();            
        }

        private void Initialize()
        {            
            PopulateTables();
        }        
        public void PopulateTables()
        {
            POSDataContext db = new POSDataContext();
            panelKassa.Controls.Clear();
            var tables = from a in db.TableSeats where a.IsDelete == false select a;
            if (tables.Count() > 0)
            {
                int length = panelKassa.Width;
                int totalRowTable = (length - 5) / 210;
                int index = 0;
                int row = 0;
                foreach (var table in tables)
                {
                    TableUserControl uc = new TableUserControl();

                    uc.LabelControl.MouseClick += new MouseEventHandler(LabelControl_MouseClick);  
                                     
                    var orderName = Helper.GetOrderName(db, table.TableID);

                    if(!string.IsNullOrEmpty(orderName))
                        uc.TableControlTitle = string.Format("{0}: {1}", table.TableName, orderName);

                    else uc.TableControlTitle = table.TableName;

                    uc.LabelControl.Tag = table;

                    panelKassa.Controls.Add(uc);

                    if (index == 0)
                        uc.Left = 5;
                    else
                        uc.Left = 5 + (index * 210);

                    uc.ConfigureBackground(table.TableStatus);

                    uc.Top = 10 + (row * 90);

                    index++;

                    if ((index + 1) > totalRowTable)
                    {
                        index = 0;
                        row++;
                    }
                }

                dataGridTables.DataSource = ctx.TableSeats.ToArray();
                cboTables.DataSource = ctx.TableSeats.ToArray();
                //dataGridMenus.DataSource = ctx.MenuCard;
                List<MenuGroup> list = new List<MenuGroup>(ctx.MenuGroups.ToArray());
                
                MenuGroup g = new MenuGroup();
                g.id = 0;
                g.GroupName = "ALL";
                list.Add(g);
                cboFilter.DataSource = list;                
                cboFilter.SelectedValue = 0;
            }
            
        }

        private void LabelControl_MouseClick(object sender, MouseEventArgs e)
        {             
            TableSeat uc = (TableSeat)((Label)sender).Tag;

            OrderUserControl ucOrder = new OrderUserControl();
            ucOrder.BackToTablesHandler += new EventHandler<EventArgs>(ucOrder_BackToTablesHandler);
            ucOrder.SelectedTable = uc;
            ucOrder.ctx = this.ctx;

            panelKassa.Controls.Clear();
            panelKassa.Controls.Add(ucOrder);
            ucOrder.Dock = DockStyle.Fill;
            ucOrder.PopulateMenus();
        }

        void ucOrder_BackToTablesHandler(object sender, EventArgs e)
        {
            PopulateTables();
        }     

         private void btnAdd_Click(object sender, EventArgs e)
        {
            TableForm frm = new TableForm();
            frm.ctx = this.ctx;
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //ctx.DetectChanges();
                //dataGridTables.DataSource = ctx.TableSeat.ToArray();
                Initialize();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridTables.SelectedRows.Count > 0)
            {
                var ret = MessageBox.Show("This will delete all orders from this table, are you sure?", 
                    "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (ret == System.Windows.Forms.DialogResult.OK)
                {                    
                    TableSeat item = (TableSeat)dataGridTables.SelectedRows[0].DataBoundItem;
                    var selected = ctx.TableSeats.First(m => m.TableID == item.TableID);
                    var orders = from a in ctx.OrderKassas where a.TableID == selected.TableID select a;
                    foreach (var order in orders)
                    {
                        var details = from a in ctx.OrderDetails where a.OrderID == order.OrderID select a;
                        foreach (var detail in details)
                            ctx.OrderDetails.DeleteOnSubmit(detail);
                        ctx.OrderKassas.DeleteOnSubmit(order);
                    }
                    ctx.TableSeats.DeleteOnSubmit(selected);
                    ctx.SubmitChanges();
                    
                    dataGridTables.DataSource = ctx.TableSeats.ToArray();
                    Initialize();

                }
            }
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            if (dataGridTables.SelectedRows.Count > 0)
            {                
                TableSeat item = (TableSeat)dataGridTables.SelectedRows[0].DataBoundItem;
                var selected = ctx.TableSeats.First(m => m.TableID == item.TableID);
                selected.IsDelete = !selected.IsDelete;
                ctx.SubmitChanges();
                
                dataGridTables.DataSource = ctx.TableSeats.ToArray();
                Initialize();
            }
        }

        private void dataGridMenus_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (this.dataGridMenus.Columns[e.ColumnIndex].Name == "menuGroupDataGridViewTextBoxColumn")
            {
                if (e.Value != null)
                {
                    MenuCard menu = (MenuCard)this.dataGridMenus.Rows[e.RowIndex].DataBoundItem;
                    e.Value = menu.MenuGroup.GroupName;
                }
            }
        }

        private void btnMenuAdd_Click(object sender, EventArgs e)
        {
            MenuCardForm frm = new MenuCardForm();
            frm.FormModel = 1;
            frm.Text = "Add New Menu";
            frm.context = ctx;
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                dataGridMenus.DataSource = ctx.MenuCards.ToArray();
                cboFilter.SelectedValue = 0;
            }
        }

        private void btnMenuEdit_Click(object sender, EventArgs e)
        {
            if (dataGridMenus.SelectedRows.Count > 0)
            {
                MenuCard item = (MenuCard)dataGridMenus.SelectedRows[0].DataBoundItem;
                MenuCardForm frm = new MenuCardForm();
                frm.FormModel = 2;
                frm.Text = "Edit Menu";
                frm.context = ctx;
                frm.SelectedMenu = item;
                if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    dataGridMenus.DataSource = ctx.MenuCards.ToArray();
                    cboFilter.SelectedValue = 0;
                   
                }
            }
        }

        private void btnMenuDelete_Click(object sender, EventArgs e)
        {
            if (dataGridMenus.SelectedRows.Count > 0)
            {
                var ret = MessageBox.Show("This will delete all orders related to this menu, are you sure?", 
                    "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (ret == System.Windows.Forms.DialogResult.OK)
                {                                    
                    MenuCard item = (MenuCard)dataGridMenus.SelectedRows[0].DataBoundItem;
                    var selected = ctx.MenuCards.First(m => m.id == item.id);
                    var orderdetails = from a in ctx.OrderDetails where a.MenuCardID == item.id select a;
                    foreach (var o in orderdetails)
                        ctx.OrderDetails.DeleteOnSubmit(o);

                    ctx.MenuCards.DeleteOnSubmit(selected);
                    ctx.SubmitChanges();// System.Data.Objects.SaveOptions.DetectChangesBeforeSave);


                    dataGridMenus.DataSource = ctx.MenuCards.ToArray();
                    cboFilter.SelectedValue = 0;
                }
                
            }
        }

        private void cboFilter_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cboFilter.SelectedValue != null)
            {
                dataGridMenus.DataSource = null;
                ctx.SubmitChanges();
                int id = (int)cboFilter.SelectedValue;
                if (id == 0)
                    dataGridMenus.DataSource = ctx.MenuCards.ToArray();
                else
                    dataGridMenus.DataSource = ctx.MenuCards.Where(m => m.MenuGroupId == id).ToArray();

            }
        }

        private void btnPrintTables_Click(object sender, EventArgs e)
        {
            int tableId = (int)cboTables.SelectedValue;
            DateTime selectedDate = dateTimePickerTable.Value;
            var orders = from a in ctx.OrderKassas where a.TableID == tableId 
                             && a.OrderDate.Day == selectedDate.Day
                             && a.OrderDate.Month == selectedDate.Month
                             && a.OrderDate.Year == selectedDate.Year 
                             select a;
            if (orders.Count() > 0)
            {
                float fontSize = Convert.ToSingle(ConfigurationManager.AppSettings["FontSize"]);
                Font printFont = new Font("Arial", fontSize);
                PrintDocument pd = new PrintDocument();
                pd.PrinterSettings.PrinterName = ConfigurationManager.AppSettings["PrinterName"];

                List<string> lines = new List<string>();

                lines.Add("Meat Compiler\n");
                lines.Add("\nAll order from table: " + cboTables.SelectedText);
                lines.Add("\nDate: " + selectedDate.ToLongDateString());

                decimal grandTotal = 0;
                foreach (var order in orders)
                {
                    lines.Add("\n-------------------------------------------");
                    lines.Add("\nOrder ID: " + order.OrderID);
                    lines.Add("\nTime: " + order.OrderDate.ToShortTimeString());
                    lines.Add("\nTotal: " + Helper.FormatPrice(order.OrderTotal));
                    grandTotal += order.OrderTotal;
                }
                lines.Add("\n-------------------------------------------");
                lines.Add("\n\nGrand Total:   " + Helper.FormatPrice(grandTotal));

                int lineIndex = 0;
                pd.PrintPage += (s, ev) =>
                {
                    float linesPerPage = 0;
                    float yPos = 0;
                    int count = 0;
                    float leftMargin = 10;
                    float topMargin = 10;
                    string currentLine = null;

                    // Calculate the number of lines per page.
                    linesPerPage = ev.MarginBounds.Height / printFont.GetHeight(ev.Graphics);
                    // Print each line of the file.
                    if (lineIndex < lines.Count)
                    {
                        while (count < linesPerPage && (lines[lineIndex] != null))
                        {
                            yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                            ev.Graphics.DrawString(lines[lineIndex], printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                            count++;

                            currentLine = lines[lineIndex];
                            lineIndex++;

                            if (lineIndex >= lines.Count)
                                break;
                        }
                    }

                    // If more lines exist, print another page.
                    if (currentLine != null)
                    {
                        ev.HasMorePages = true;
                        currentLine = null;
                    }
                    else
                        ev.HasMorePages = false;
                };

                pd.EndPrint += (s, ev) =>
                {

                };

                pd.Print();
            }
            else
            {
                MessageBox.Show("There is no orders on the seleted table and date");
            }
        }

        private void btnPrint2_Click(object sender, EventArgs e)
        {
            int month = Helper.GetMonth(cboMonth.SelectedItem.ToString());
            int year = Convert.ToInt32(cboYear.SelectedItem.ToString());

            float fontSize = Convert.ToSingle(ConfigurationManager.AppSettings["FontSize"]);
            Font printFont = new Font("Arial", fontSize);
            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = ConfigurationManager.AppSettings["PrinterName"];

            List<string> lines = new List<string>();

            lines.Add("Meat Compiler\n");
            lines.Add("\nAll order for: " + cboMonth.SelectedItem.ToString() + " " + cboYear.SelectedItem.ToString());
            lines.Add("\n-------------------------------------------");

            //var orderd=ctx.OrderKassa.GroupBy(m=>m.OrderDate)
            //var orders = ctx.OrderKassa.Where(m=>m.OrderDate.Month == month && m=>m.OrderDate.Year == year).OrderByDescending(o => o.OrderDate).GroupBy(o => o.OrderDate.Date).ToList();
            var orders = (from a in ctx.OrderKassas where a.OrderDate.Month == month && a.OrderDate.Year == year select a).AsQueryable().OrderByDescending(o => o.OrderDate).GroupBy(o => o.OrderDate).ToList();


            if (orders.Count() <= 0)
            {
                MessageBox.Show("There is no order on the selected month/year.");
                return;
            }

            decimal grandTotal = 0;
            decimal grandSubtotal = 0;
            decimal grandSubtotal6 = 0;
            decimal grandTotalBTW6 = 0;
            decimal grandSubtotalDrink = 0;
            decimal grandTotalBTW19 = 0;
            decimal grandSubtotal19 = 0;


            foreach (var keys in orders)
            {
                decimal orderTotal = 0;
                decimal orderTotal6 = 0;
                decimal orderTotal19 = 0;
                foreach (var o in keys)
                {
                    orderTotal += o.OrderTotal;
                    grandTotal += orderTotal;


                    var orderDetails = o.OrderDetails.ToList();

                    foreach (OrderDetail orderDetail in orderDetails)
                    {
                        if (orderDetail.MenuCard.MenuGroupId != 16 && orderDetail.MenuCard.MenuGroupId != 19 && orderDetail.MenuCard.MenuGroupId != 20)
                        {
                            if (orderDetail.CustomMenuPrice > 0)
                            {
                                orderTotal6 += orderDetail.CustomMenuPrice * orderDetail.Quantity;
                            }
                            else
                            {
                                orderTotal6 += orderDetail.MenuCard.Price * orderDetail.Quantity;
                            }
                        }
                        else
                        {
                            if (orderDetail.CustomMenuPrice > 0)
                            {
                                orderTotal19 += orderDetail.CustomMenuPrice * orderDetail.Quantity;
                            }
                            else
                            {
                                orderTotal19 += orderDetail.MenuCard.Price * orderDetail.Quantity;
                            }
                        }

                    }
                }

                decimal btwAmount6 = orderTotal6 * (decimal)((decimal)6 / (decimal)106);
                decimal subTotal6 = orderTotal6 - btwAmount6;

                decimal btwAmount19 = orderTotal19 * (decimal)((decimal)19 / (decimal)119);
                decimal subTotal19 = orderTotal19 - btwAmount19;

                grandSubtotal += orderTotal6;
                grandSubtotal6 += subTotal6;
                grandTotalBTW6 += btwAmount6;

                grandSubtotalDrink += orderTotal19;
                grandSubtotal19 += subTotal19;
                grandTotalBTW19 += btwAmount19;

                var todayTotal = orderTotal;
                lines.Add("\n" + keys.Key.ToLongDateString() + ":   " + Helper.FormatPrice(todayTotal));
                if (orderTotal6 > 0)
                {
                    lines.Add("\nSubtotal No Alcohol: " + Helper.FormatPrice(subTotal6));
                    lines.Add("\nBTW 6%: " + Helper.FormatPrice(btwAmount6));
                    lines.Add("\nTotal No Alcohol: " + Helper.FormatPrice(orderTotal6));
                }

                if (orderTotal19 > 0)
                {
                    lines.Add("\nSubtotal Alcoholic Drink: " + Helper.FormatPrice(subTotal19));
                    lines.Add("\nBTW 19%: " + Helper.FormatPrice(btwAmount19));
                    lines.Add("\nTotal Alcoholic Drink: " + Helper.FormatPrice(orderTotal19));
                }
                lines.Add("\n");
            }

            lines.Add("\n-------------------------------------------\n");
            if (grandSubtotal > 0)
            {
                lines.Add("\nSubtotal: " + Helper.FormatPrice(grandSubtotal6));
                lines.Add("\nBTW 6%: " + Helper.FormatPrice(grandTotalBTW6));
            }

            if (grandSubtotalDrink > 0)
            {
                lines.Add("\nSubtotal Alcoholic Drink: " + Helper.FormatPrice(grandSubtotal19));
                lines.Add("\nBTW 19%: " + Helper.FormatPrice(grandTotalBTW19));
            }
            lines.Add("\nGrand Total:   " + Helper.FormatPrice(grandSubtotal + grandSubtotalDrink));


            int lineIndex = 0;
            pd.PrintPage += (s, ev) =>
            {
                float linesPerPage = 0;
                float yPos = 0;
                int count = 0;
                float leftMargin = 10;
                float topMargin = 10;
                string currentLine = null;

                // Calculate the number of lines per page.
                linesPerPage = ev.MarginBounds.Height / printFont.GetHeight(ev.Graphics);
                // Print each line of the file.
                if (lineIndex < lines.Count)
                {
                    while (count < linesPerPage && (lines[lineIndex] != null))
                    {
                        yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                        ev.Graphics.DrawString(lines[lineIndex], printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                        count++;

                        currentLine = lines[lineIndex];
                        lineIndex++;

                        if (lineIndex >= lines.Count)
                            break;
                    }
                }

                // If more lines exist, print another page.
                if (currentLine != null)
                {
                    ev.HasMorePages = true;
                    currentLine = null;
                }
                else
                    ev.HasMorePages = false;
            };

            pd.EndPrint += (s, ev) =>
            {

            };

            pd.Print();
        }

        private void btnShowOrder_Click(object sender, EventArgs e)
        {
            _show = 1;
            DateTime selectedDate = dateTimePickerOrder.Value;
            var orders = from a in ctx.OrderKassas where a.OrderDate.Day == selectedDate.Day
                             && a.OrderDate.Month == selectedDate.Month
                             && a.OrderDate.Year == selectedDate.Year 
                             select a;
            if (orders.Count() > 0)
            {
                dataGridViewOrders.DataSource = orders.ToArray();
            }
        }

        private void btnShowToday_Click(object sender, EventArgs e)
        {
            _show = 2;
            DateTime selectedDate = DateTime.Now;
            var orders = from a in ctx.OrderKassas
                         where a.OrderDate.Day == selectedDate.Day
                             && a.OrderDate.Month == selectedDate.Month
                             && a.OrderDate.Year == selectedDate.Year
                         select a;
            if (orders.Count() > 0)
            {
                dataGridViewOrders.DataSource = orders.ToArray();
            }
        }

        private void dataGridViewOrders_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (this.dataGridViewOrders.Columns[e.ColumnIndex].Name == "tableIDDataGridViewTextBoxColumn")
            {
                OrderKassa order = (OrderKassa)this.dataGridViewOrders.Rows[e.RowIndex].DataBoundItem;
                e.Value = order.TableSeat.TableName;
            }
            //orderTotalDataGridViewTextBoxColumn
            if (this.dataGridViewOrders.Columns[e.ColumnIndex].Name == "orderTotalDataGridViewTextBoxColumn")
            {
                if(Convert.ToDecimal(e.Value)== 0 || e.Value==null)
                    e.Value="0";

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //print selected
            if (dataGridViewOrders.SelectedRows.Count > 0)
            {                
                OrderKassa order = (OrderKassa)dataGridViewOrders.SelectedRows[0].DataBoundItem;
                Helper.PrintOrder(this.ctx, order, true,false);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //print selected without btw
            if (dataGridViewOrders.SelectedRows.Count > 0)
            {                
                OrderKassa order = (OrderKassa)dataGridViewOrders.SelectedRows[0].DataBoundItem;
                Helper.PrintOrder(this.ctx, order, false,false);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // mark paid
            if (dataGridViewOrders.SelectedRows.Count > 0)
            {
                var ret = MessageBox.Show("Are you sure you want to mark paid this Order?", 
                    "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (ret != System.Windows.Forms.DialogResult.OK)
                    return;
                                
                OrderKassa order = (OrderKassa)dataGridViewOrders.SelectedRows[0].DataBoundItem;
                if (!order.IsComplete)
                {
                    var itemOrder = ctx.OrderKassas.First(m => m.OrderID == order.OrderID);
                    itemOrder.IsComplete = true;

                    ctx.SubmitChanges();
                }

                if (_show == 1)
                    this.btnShowOrder_Click(sender, e);
                else
                    this.btnShowToday_Click(sender, e);

                MessageBox.Show("Done");
            }
        }

        private void btnPrintAll_Click(object sender, EventArgs e)
        {
            // print all without btw
            if (dataGridViewOrders.DataSource != null)
            {                
                OrderKassa[] orders = (OrderKassa[])dataGridViewOrders.DataSource;
                Helper.PrintAllOrder(orders, dateTimePickerOrder.Value);
                //foreach(var order in orders)
                //    Helper.PrintOrder(this.ctx, order, false, false);

            }
        }

        private void btnPrintAll_Click_1(object sender, EventArgs e)
        {
            // print all
            if (dataGridViewOrders.DataSource != null)
            {                
                OrderKassa[] orders = (OrderKassa[])dataGridViewOrders.DataSource;
                Helper.PrintAllOrderWithBTW(orders, dateTimePickerOrder.Value);
                //foreach (var order in orders)
                //    Helper.PrintOrder(this.ctx, order, true, false);
            }
        }

        private void btnDeleteOrder_Click(object sender, EventArgs e)
        {                        
            if (dataGridViewOrders.SelectedRows.Count > 0)
            {
                //ctx.DetectChanges();
                OrderKassa order = (OrderKassa)dataGridViewOrders.SelectedRows[0].DataBoundItem;
                var orderdetails = from a in ctx.OrderDetails where a.OrderID == order.OrderID select a;
                List<OrderDetail> orderDetails = new List<OrderDetail>(orderdetails.ToArray());
                foreach (OrderDetail orderDetail in orderDetails)
                {
                    ctx.OrderDetails.DeleteOnSubmit(orderDetail);
                    ctx.SubmitChanges();
                }
                ctx.OrderKassas.DeleteOnSubmit(order);
                ctx.SubmitChanges();

                if (_show == 1)
                    this.btnShowOrder_Click(sender, e);
                else
                    this.btnShowToday_Click(sender, e);

                MessageBox.Show("Done");
            }
            
        }

        private void btnTidy_Click(object sender, EventArgs e)
        {
            var allOrders = (from o in ctx.OrderKassas
                             orderby o.OrderNumber
                             select o).ToList();

            for (int i = 0; i < allOrders.Count; i++)
            {
                int orderNumber = i + 1;
                allOrders[i].OrderNumber = orderNumber;
            }

            ctx.SubmitChanges();

            if (_show == 1)
                this.btnShowOrder_Click(sender, e);
            else
                this.btnShowToday_Click(sender, e);

            MessageBox.Show("Done");
        }    

        private void button4_Click(object sender, EventArgs e)
        {
            if (btnDeleteOrder.Visible == false && btnTidy.Visible == false)
            {
                btnDeleteOrder.Visible = true;
                btnTidy.Visible = true;
            }
            else
            {
                btnDeleteOrder.Visible = false;
                btnTidy.Visible = false;
            }
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            switch (e.TabPage.Name)
            {
                case "":
                    break;
                
                case "tabPageMenus": // menu
                    menuCardBindingSource.ResetBindings(true);
                    break;
               
            }
                
        }
    }
}
