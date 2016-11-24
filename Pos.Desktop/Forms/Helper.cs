using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Configuration;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Text;

namespace Pos.Desktop.Forms
{
    public class Helper
    {
        public static string FormatPrice(decimal price)
        {
            return price.ToString("N0");
        }

        public static int GetMonth(string month)
        {
            if (month.ToLower() == "january")
                return 1;
            if (month.ToLower() == "february")
                return 2;
            if (month.ToLower() == "march")
                return 3;
            if (month.ToLower() == "april")
                return 4;
            if (month.ToLower() == "may")
                return 5;
            if (month.ToLower() == "june")
                return 6;
            if (month.ToLower() == "july")
                return 7;
            if (month.ToLower() == "august")
                return 8;
            if (month.ToLower() == "september")
                return 9;
            if (month.ToLower() == "october")
                return 10;
            if (month.ToLower() == "november")
                return 11;
            if (month.ToLower() == "december")
                return 12;

            return 0;
        }

        public static decimal GetTotal(int orderId)
        {
            POSDataContext ctx = new POSDataContext();
            var orderdetails = from a in ctx.OrderDetails where a.OrderID == orderId select a;
            List<OrderDetail> orderDetails = new List<OrderDetail>(orderdetails.ToArray());
            
            decimal grandTotal = 0;            
            decimal orderTotal = 0;
            decimal orderTotalDrink = 0;
            
            foreach (OrderDetail orderDetail in orderDetails)
            {
                if (orderDetail.CustomMenuPrice > 0)
                {
                    orderTotal += orderDetail.CustomMenuPrice * orderDetail.Quantity;
                }
                else
                {
                    orderTotal += orderDetail.MenuCard.Price * orderDetail.Quantity;
                }
            }

            grandTotal = orderTotalDrink + orderTotal;

            return grandTotal;
        }            

        public static void PrintOrder(POSDataContext ctx, OrderKassa order, bool btw,bool isKitchen, bool bni = false)
        {
            var orderdetails = from a in ctx.OrderDetails where a.OrderID == order.OrderID select a;
            List<OrderDetail> orderDetails = new List<OrderDetail>(orderdetails.ToArray());

            float fontSize = Convert.ToSingle(ConfigurationManager.AppSettings["FontSize"]);

            Font printFont = new Font("Arial", fontSize);
            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = ConfigurationManager.AppSettings["PrinterName"];

            if(isKitchen)
                pd.PrinterSettings.PrinterName = ConfigurationManager.AppSettings["PrinterNameKitchen"];

            string tableNameHeader = order.TableSeat.TableName + ": " + order.Name;
            string dateHeader = "\nTanggal : " + order.OrderDate.ToLongDateString();

            decimal grandTotal = 0;
            string strBillSubtotal = "";
            string strBillSubtotalAmount = "";
            string strBillBTW = "";
            string strBillBTWAmount = "";
            string strBillDrink = "";
            string strBillDrinkAmount = "";
            string strBillBTWDrink = "";
            string strBillBTWDrinkAmount = "";
            string strBillTotal = "";
            string strBillTotalAmount = "";
            string strBillFooter = "Terima kasih dan sampai jumpa";
            string strBillDiskon = "";
            string strBillDiskonAmount = "";

            pd.PrintPage += (s, ev) =>
            {
                float linesPerPage = 0;
                //not using
                //float yPos = 0;
                //int count = 0;
                float leftMargin = 10;
                float topMargin = 10;
                string currentLine = null;
                Pen p = new Pen(Color.White, 1);
                float lineHeight = printFont.GetHeight(ev.Graphics);
                int intLineHeight = (int)lineHeight;

                // Calculate the number of lines per page.
                linesPerPage = ev.MarginBounds.Height / printFont.GetHeight(ev.Graphics);

                StringFormat sfHeader = new StringFormat();
                sfHeader.Alignment = StringAlignment.Center;

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Near;

                StringFormat sfFar = new StringFormat();
                sfFar.Alignment = StringAlignment.Far;

                // header
                Rectangle rectHeader = new Rectangle((int)leftMargin, (int)topMargin, 260, 50);
                Rectangle rectHeader2 = new Rectangle((int)leftMargin, rectHeader.Height + 20, 260, 35);
                ev.Graphics.DrawString("Meat Compiler", printFont, Brushes.Black, rectHeader, sfHeader);
                ev.Graphics.DrawString("\nTel: 0817-6328-000", printFont, Brushes.Black, rectHeader, sfHeader);
                ev.Graphics.DrawString("\n\nwww.meatcompiler.id", printFont, Brushes.Black, rectHeader, sfHeader);
                ev.Graphics.DrawString(tableNameHeader, printFont, Brushes.Black, rectHeader2, sf);
                ev.Graphics.DrawString(dateHeader, printFont, Brushes.Black, rectHeader2, sf);
                ev.Graphics.DrawRectangle(p, rectHeader);
                ev.Graphics.DrawRectangle(p, rectHeader2);

                int topMarginBody1 = 100;

                ev.Graphics.DrawLine(Pens.Black, (int)leftMargin, topMarginBody1, (int)leftMargin + 260, topMarginBody1);

                decimal orderTotal = 0;
                decimal orderTotalDrink = 0;

                int topMarginBody = topMarginBody1 + 10;

                foreach (OrderDetail orderDetail in orderDetails)
                {
                    Rectangle rectBodyLeft = new Rectangle((int)leftMargin, topMarginBody, 200, intLineHeight);
                    Rectangle rectBodyright = new Rectangle(rectBodyLeft.Width + 5 - 40, topMarginBody, 100, intLineHeight);

                    string menuNameBill = "";
                    string menuAmountBill = "";

                    if (orderDetail.CustomMenuPrice > 0)
                    {
                        orderTotal += orderDetail.CustomMenuPrice * orderDetail.Quantity;
                        menuNameBill = orderDetail.Quantity + "x " + orderDetail.CustomMenuName;
                        menuAmountBill = Helper.FormatPrice(orderDetail.CustomMenuPrice * orderDetail.Quantity);
                    }
                    else
                    {
                        orderTotal += orderDetail.MenuCard.Price * orderDetail.Quantity;
                        menuNameBill = orderDetail.Quantity + "x " + orderDetail.MenuCard.MenuName;
                        menuAmountBill = Helper.FormatPrice(orderDetail.MenuCard.Price * orderDetail.Quantity);
                    }

                    ev.Graphics.DrawString(menuNameBill, printFont, Brushes.Black, rectBodyLeft, sf);
                    ev.Graphics.DrawString(menuAmountBill, printFont, Brushes.Black, rectBodyright, sfFar);

                    ev.Graphics.DrawRectangle(p, rectBodyLeft);
                    ev.Graphics.DrawRectangle(p, rectBodyright);

                    topMarginBody = topMarginBody + intLineHeight + 5;
                }


                grandTotal = orderTotalDrink + orderTotal;
                strBillTotal = "Total ";
                strBillTotalAmount = Helper.FormatPrice(grandTotal);

                int topMarginFooter1 = topMarginBody + 15; // bodyHeight + 5;
                int topMarginFooter = topMarginFooter1 + 15;
                int topMarginFooterFinal = topMarginFooter;

                ev.Graphics.DrawLine(Pens.Black, (int)leftMargin, topMarginFooter1, (int)leftMargin + 260, topMarginFooter1);

                POSDataContext db = new POSDataContext();
                var theOrder = db.OrderKassas.FirstOrDefault(x => x.OrderID == order.OrderID);
                if (theOrder.Discount > 0)
                {
                    strBillSubtotal = "Subtotal: ";
                    strBillSubtotalAmount = Helper.FormatPrice(grandTotal);
                    int subTotalTopMargin = topMarginFooter + intLineHeight + 5;

                    Rectangle rectFooterSubtotal = new Rectangle((int)leftMargin, subTotalTopMargin, 200, intLineHeight);
                    Rectangle rectFooterSubtotalAmount = new Rectangle(rectFooterSubtotal.Width + 5 - 40, topMarginFooter, 100, intLineHeight);                    
                    
                    ev.Graphics.DrawString(strBillSubtotal, printFont, Brushes.Black, rectFooterSubtotal, sf);
                    ev.Graphics.DrawString(strBillSubtotalAmount, printFont, Brushes.Black, rectFooterSubtotalAmount, sfFar);

                    strBillDiskon = "Diskon: ";
                    strBillDiskonAmount = (grandTotal * (order.Discount / 100)).ToString("N0");
                    int diskonTopMargin = subTotalTopMargin + intLineHeight + 5;

                    grandTotal = grandTotal - (grandTotal * (order.Discount / 100));
                    strBillTotalAmount = Helper.FormatPrice(grandTotal);

                    Rectangle rectFooterDiskon = new Rectangle((int)leftMargin, diskonTopMargin, 200, intLineHeight);
                    Rectangle rectFooterDiskonAmount = new Rectangle(rectFooterDiskon.Width + 5 - 40, diskonTopMargin, 100, intLineHeight);

                    ev.Graphics.DrawString(strBillDiskon, printFont, Brushes.Black, rectFooterDiskon, sf);
                    ev.Graphics.DrawString(strBillDiskonAmount, printFont, Brushes.Black, rectFooterDiskonAmount, sfFar);
                }

                if(bni)
                {
                    decimal cardChargeAmount = grandTotal * (decimal)(0.02);
                    decimal subTotal = grandTotal;

                    strBillSubtotal = "Subtotal: ";
                    strBillSubtotalAmount = Helper.FormatPrice(subTotal);

                    strBillBTW = "Card Charge 2%: ";
                    strBillBTWAmount = Helper.FormatPrice(cardChargeAmount);

                    Rectangle rectFooterSubtotal = new Rectangle((int)leftMargin, topMarginFooter, 200, intLineHeight);
                    Rectangle rectFooterSubtotalAmount = new Rectangle(rectFooterSubtotal.Width + 5 - 40, topMarginFooter, 100, intLineHeight);
                    int btwTopMargin = topMarginFooter + intLineHeight + 5;

                    Rectangle rectFooterBTW = new Rectangle((int)leftMargin, btwTopMargin, 200, intLineHeight);
                    Rectangle rectFooterBTWAmount = new Rectangle(rectFooterBTW.Width + 5 - 40, btwTopMargin, 100, intLineHeight);

                    ev.Graphics.DrawString(strBillSubtotal, printFont, Brushes.Black, rectFooterSubtotal, sf);
                    ev.Graphics.DrawString(strBillSubtotalAmount, printFont, Brushes.Black, rectFooterSubtotalAmount, sfFar);
                    ev.Graphics.DrawString(strBillBTW, printFont, Brushes.Black, rectFooterBTW, sf);
                    ev.Graphics.DrawString(strBillBTWAmount, printFont, Brushes.Black, rectFooterBTWAmount, sfFar);

                    ev.Graphics.DrawRectangle(p, rectFooterSubtotal);
                    ev.Graphics.DrawRectangle(p, rectFooterSubtotalAmount);
                    ev.Graphics.DrawRectangle(p, rectFooterBTW);
                    ev.Graphics.DrawRectangle(p, rectFooterBTWAmount);

                    topMarginFooterFinal = btwTopMargin + intLineHeight + 5;

                    grandTotal = subTotal + cardChargeAmount;
                    strBillTotalAmount = Helper.FormatPrice(grandTotal);
                }                

                if (btw)
                {
                    decimal btwAmount = orderTotal * (decimal)((decimal)6 / (decimal)106);
                    decimal subTotal = orderTotal - btwAmount;

                    strBillSubtotal = "Subtotal: ";
                    strBillSubtotalAmount = Helper.FormatPrice(subTotal);

                    strBillBTW = "BTW 6%: ";
                    strBillBTWAmount = Helper.FormatPrice(btwAmount);

                    Rectangle rectFooterSubtotal = new Rectangle((int)leftMargin, topMarginFooter, 200, intLineHeight);
                    Rectangle rectFooterSubtotalAmount = new Rectangle(rectFooterSubtotal.Width + 5, topMarginFooter, 60, intLineHeight);
                    int btwTopMargin = topMarginFooter + intLineHeight + 5;

                    Rectangle rectFooterBTW = new Rectangle((int)leftMargin, btwTopMargin, 200, intLineHeight);
                    Rectangle rectFooterBTWAmount = new Rectangle(rectFooterBTW.Width + 5, btwTopMargin, 60, intLineHeight);

                    ev.Graphics.DrawString(strBillSubtotal, printFont, Brushes.Black, rectFooterSubtotal, sf);
                    ev.Graphics.DrawString(strBillSubtotalAmount, printFont, Brushes.Black, rectFooterSubtotalAmount, sfFar);
                    ev.Graphics.DrawString(strBillBTW, printFont, Brushes.Black, rectFooterBTW, sf);
                    ev.Graphics.DrawString(strBillBTWAmount, printFont, Brushes.Black, rectFooterBTWAmount, sfFar);

                    ev.Graphics.DrawRectangle(p, rectFooterSubtotal);
                    ev.Graphics.DrawRectangle(p, rectFooterSubtotalAmount);
                    ev.Graphics.DrawRectangle(p, rectFooterBTW);
                    ev.Graphics.DrawRectangle(p, rectFooterBTWAmount);

                    topMarginFooterFinal = btwTopMargin + intLineHeight + 5;

                    if (orderTotalDrink > 0)
                    {
                        decimal btwAmountDrink = orderTotalDrink * (decimal)((decimal)19 / (decimal)119);
                        decimal subTotalDrink = orderTotalDrink - btwAmountDrink;
                        strBillDrink = "Alcoholic Drink: ";
                        strBillDrinkAmount = Helper.FormatPrice(subTotalDrink);
                        strBillBTWDrink = "BTW 19%: ";
                        strBillBTWDrinkAmount = Helper.FormatPrice(btwAmountDrink);

                        Rectangle rectFooterDrink = new Rectangle((int)leftMargin, topMarginFooterFinal, 200, intLineHeight);
                        Rectangle rectFooterDrinkAmount = new Rectangle(rectFooterDrink.Width + 5, topMarginFooterFinal, 60, intLineHeight);

                        topMarginFooterFinal = topMarginFooterFinal + intLineHeight + 5;
                        Rectangle rectFooterBTWDrink = new Rectangle((int)leftMargin, topMarginFooterFinal, 200, intLineHeight);
                        Rectangle rectFooterBTWDrinkAmount = new Rectangle(rectFooterBTW.Width + 5, topMarginFooterFinal, 60, intLineHeight);

                        ev.Graphics.DrawString(strBillDrink, printFont, Brushes.Black, rectFooterDrink, sf);
                        ev.Graphics.DrawString(strBillDrinkAmount, printFont, Brushes.Black, rectFooterDrinkAmount, sfFar);
                        ev.Graphics.DrawString(strBillBTWDrink, printFont, Brushes.Black, rectFooterBTWDrink, sf);
                        ev.Graphics.DrawString(strBillBTWDrinkAmount, printFont, Brushes.Black, rectFooterBTWDrinkAmount, sfFar);

                        ev.Graphics.DrawRectangle(p, rectFooterDrink);
                        ev.Graphics.DrawRectangle(p, rectFooterDrinkAmount);
                        ev.Graphics.DrawRectangle(p, rectFooterBTWDrink);
                        ev.Graphics.DrawRectangle(p, rectFooterBTWDrinkAmount);

                        topMarginFooterFinal = topMarginFooterFinal + intLineHeight + 5;
                    }
                }

                Rectangle rectFooterTotal = new Rectangle((int)leftMargin, topMarginFooterFinal, 200, intLineHeight);
                Rectangle rectFooterTotalAmount = new Rectangle(rectFooterTotal.Width + 5 - 40, topMarginFooterFinal, 100, intLineHeight);
                Rectangle rectFooterClose = new Rectangle((int)leftMargin, topMarginFooterFinal + intLineHeight + 30, 260, intLineHeight);

                ev.Graphics.DrawString(strBillTotal, printFont, Brushes.Black, rectFooterTotal, sf);
                ev.Graphics.DrawString(strBillTotalAmount, printFont, Brushes.Black, rectFooterTotalAmount, sfFar);
                ev.Graphics.DrawString(strBillFooter, printFont, Brushes.Black, rectFooterClose, sfHeader);

                ev.Graphics.DrawRectangle(p, rectFooterTotal);
                ev.Graphics.DrawRectangle(p, rectFooterTotalAmount);
                ev.Graphics.DrawRectangle(p, rectFooterClose);

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

        public static void PrintAllOrder(OrderKassa[] orders, DateTime selectedDate)
        {            
            string strSelectedDate = selectedDate.ToLongDateString();
            decimal totalIncomeSelectedDate = GetTotalIncomeSelectedDate(orders);

            float fontSize = Convert.ToSingle(ConfigurationManager.AppSettings["FontSize"]);
            Font printFont = new Font("Arial", fontSize);
            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = ConfigurationManager.AppSettings["PrinterName"];

            List<string> lines = new List<string>();

            lines.Add("Meat Compiler\n");
            lines.Add("\nAll order from date: " + strSelectedDate);
            lines.Add("\n-------------------------------------------");
            
            if (orders.Length <= 0)
            {
                MessageBox.Show("There is no order on the selected date.");
                return;
            }

            decimal grandTotal = 0;
            foreach (OrderKassa o in orders)
            {
                string tableName = o.TableSeat.TableName;
                decimal orderTotal = o.OrderTotal;
                grandTotal += orderTotal;
                lines.Add("\n(" + o.OrderNumber + ") " + tableName + ":   " + Helper.FormatPrice(orderTotal));
            }
            lines.Add("\n-------------------------------------------\n");
            lines.Add("\nGrand Total:   " + Helper.FormatPrice(grandTotal));

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

        public static void PrintAllOrderWithBTW(OrderKassa[] orders, DateTime selectedDate)
        {            
            string strSelectedDate = selectedDate.ToLongDateString();
            decimal totalIncomeSelectedDate = GetTotalIncomeSelectedDate(orders);

            float fontSize = Convert.ToSingle(ConfigurationManager.AppSettings["FontSize"]);
            Font printFont = new Font("Arial", fontSize);
            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = ConfigurationManager.AppSettings["PrinterName"];

            List<string> lines = new List<string>();

            lines.Add("Meat Compiler\n");
            lines.Add("\nAll order from date: " + strSelectedDate);
            lines.Add("\n-------------------------------------------");

            if (orders.Length <= 0)
            {
                MessageBox.Show("There is no order on the selected date.");
                return;
            }

            decimal grandTotal = 0;
            decimal grandSubtotal = 0;
            decimal grandSubtotal6 = 0;
            decimal grandTotalBTW6 = 0;
            decimal grandSubtotalDrink = 0;
            decimal grandTotalBTW19 = 0;
            decimal grandSubtotal19 = 0;
            foreach (OrderKassa o in orders)
            {
                string tableName = o.TableSeat.TableName;
                decimal orderTotal = o.OrderTotal;
                grandTotal += orderTotal;
                lines.Add("\n(" + o.OrderNumber + ") " + tableName + ":   " + Helper.FormatPrice(orderTotal));

                var orderDetails = o.OrderDetails.ToList();
                decimal orderTotal6 = 0;
                decimal orderTotal19 = 0;
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
            lines.Add("\nGrand Total:   " + Helper.FormatPrice(grandTotal));

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

        private static decimal GetTotalIncomeSelectedDate(OrderKassa[] orders)
        {
            decimal total = 0;
            foreach (OrderKassa order in orders)
                total += order.OrderTotal;

            return total;
        }

        public static string GetOrderName(POSDataContext ctx, int tableId)
        {
            var order = ctx.OrderKassas.Where(x => x.TableID == tableId && x.TableSeat.TableStatus.ToLower() == "occupied").OrderByDescending(x => x.OrderID).FirstOrDefault();
            if (order != null)
                return order.Name;
            return string.Empty;
        }

        public static MenuCard UpdateMenuStock(int menuId, int updateValue)
        {
            POSDataContext db = new POSDataContext();

            var menuToUpdate = db.MenuCards.FirstOrDefault(x => x.id == menuId);

            menuToUpdate.Stock = updateValue;

            db.SubmitChanges();

            return menuToUpdate;
        }

        public static MenuCard GetMenuCard(int menuId)
        {
            POSDataContext db = new POSDataContext();

            return db.MenuCards.FirstOrDefault(x => x.id == menuId);
        }
    }
}