using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using Search_Invoice.Data.Domain;
using ICSharpCode.SharpZipLib.Zip;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using ICSharpCode.SharpZipLib.Core;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Search_Invoice.Data;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using DevExpress.XtraReports.Parameters;

namespace Search_Invoice.Util
{
    public class ReportUtil
    {

        public static void ExtracInvoice(Stream zipStream, ref string xml, ref string repx, ref string key)
        {
            ZipInputStream stream = new ZipInputStream(zipStream);
            for (ZipEntry entry = stream.GetNextEntry(); entry != null; entry = stream.GetNextEntry())
            {
                string str = entry.Name;
                byte[] buffer = new byte[0x1000];
                MemoryStream stream2 = new MemoryStream();
                StreamUtils.Copy(stream, stream2, buffer);
                byte[] bytes = stream2.ToArray();
                if (str.ToLower().EndsWith("xml"))
                {
                    xml = Encoding.UTF8.GetString(bytes).Trim();
                }
                if (str.ToLower().EndsWith("repx"))
                {
                    repx = Encoding.UTF8.GetString(bytes);
                }
                if (str.ToLower().EndsWith("txt"))
                {
                    key = Encoding.UTF8.GetString(bytes);
                }
                bytes = null;
                stream2.Close();
            }
            stream.Close();
        }

        public static byte[] InvoiceReport(string xml, string repx, string folder, string type)
        {
            XmlReader reader;
            Bitmap bitmap;
            int num2;
            int num3;
            PageWatermark watermark;
            int? nullable;
            string msg_tb = "";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(repx));
            XtraReport report = XtraReport.FromStream(stream, true);
            report.Name = "XtraReport1";
            report.ScriptReferencesString = "AccountSignature.dll";
            DataSet set = new DataSet();
            using (reader = XmlReader.Create(new StringReader(report.DataSourceSchema)))
            {
                set.ReadXmlSchema(reader);
                reader.Close();
            }
            using (reader = XmlReader.Create(new StringReader(xml)))
            {
                set.ReadXml(reader);
                reader.Close();
            }
            if (set.Tables.Contains("TblXmlData"))
            {
                set.Tables.Remove("TblXmlData");
            }
            DataTable table = new DataTable
            {
                TableName = "TblXmlData"
            };
            table.Columns.Add("data");

            DataRow row = table.NewRow();
            row["data"] = xml;
            table.Rows.Add(row);
            set.Tables.Add(table);

            string name = report.DataMember;
            if ((name.Length > 0) && set.Tables.Contains(name))
            {
                DataTable table2 = set.Tables[name];
                int count = set.Tables[name].Rows.Count;
            }
            string mst = set.Tables["ThongTinHoaDon"].Rows[0]["MaSoThueNguoiBan"].ToString().Replace("-", "").Replace(" ", "");
            string input = set.Tables["ThongTinHoaDon"].Rows[0]["SellerAppRecordId"].ToString();

            TracuuHDDTContext tracuu = new TracuuHDDTContext();
            var inv_admin = tracuu.Inv_admin.Where(c => c.MST == mst || c.alias == mst).FirstOrDefault<inv_admin>();
            InvoiceDbContext invoiceContext = new InvoiceDbContext(EncodeXML.Decrypt(inv_admin.ConnectString, "NAMPV18081202"));

            Guid inv_InvoiceAuth_id = Guid.Parse(input);
            Inv_InvoiceAuth invoice = (from c in invoiceContext.Inv_InvoiceAuths
                                       where c.Inv_InvoiceAuth_id.ToString() == inv_InvoiceAuth_id.ToString()
                                       select c).FirstOrDefault<Inv_InvoiceAuth>();

            if (invoice == null)
            {
                throw new Exception("MST: " + mst + ". Không tồn tại hóa đơn");
            }
            Int32 trang_thai_hd = (Int32)invoice.Trang_thai_hd;
            string inv_originalId = invoice.Inv_originalId.ToString();
            string inv_InvoiceCode_id = invoice.Inv_InvoiceCode_id.ToString();


         

            if (trang_thai_hd == 11 || trang_thai_hd == 13 || trang_thai_hd == 17)
            {
                if (!string.IsNullOrEmpty(inv_originalId))
                {
                    Inv_InvoiceAuth tblInv = invoiceContext.Inv_InvoiceAuths.SqlQuery("SELECT * FROM inv_InvoiceAuth WHERE inv_InvoiceAuth_id='" + inv_originalId + "'").FirstOrDefault<Inv_InvoiceAuth>();
                    string inv_adjustmentType = tblInv.Inv_adjustmentType.ToString();

                    string loai = inv_adjustmentType.ToString() == "5" || inv_adjustmentType.ToString() == "19" || inv_adjustmentType.ToString() == "21" ? "điều chỉnh" : inv_adjustmentType.ToString() == "3" ? "thay thế" : inv_adjustmentType.ToString() == "7" ? "xóa bỏ" : "";

                    if (inv_adjustmentType.ToString() == "5" || inv_adjustmentType.ToString() == "7" || inv_adjustmentType.ToString() == "3" || inv_adjustmentType.ToString() == "19" || inv_adjustmentType.ToString() == "21")
                    {
                        msg_tb = "Hóa đơn bị " + loai + " bởi hóa đơn số: " + tblInv.Inv_invoiceNumber + " ngày " + string.Format("{0:dd/MM/yyyy}", tblInv.Inv_invoiceIssuedDate) + ", mẫu số " + tblInv.Mau_hd + " ký hiệu " + tblInv.Inv_invoiceSeries;

                    }
                }
            }

            if (Convert.ToInt32(invoice.Inv_adjustmentType) == 7)
            {
                msg_tb = "";
            }

            if (report.Parameters["MSG_TB"] != null)
            {
                report.Parameters["MSG_TB"].Value = msg_tb;
            }
            XRLabel label = report.AllControls<XRLabel>().Where(c => c.Name == "lblHoaDonMau").FirstOrDefault<XRLabel>();
            //XRLabel label = (from c in report.AllControls<XRLabel>()
            //                 where c.Name == "lblHoaDonMau"
            //                 select c).FirstOrDefault<XRLabel>();
            if (label != null)
            {
                label.Visible = false;
            }
            report.DataSource = set;

            DataTable tblCtthongbao = invoiceContext.ExecuteCmd("SELECT * FROM ctthongbao a INNER JOIN dpthongbao b ON a.dpthongbao_id=b.dpthongbao_id WHERE a.ctthongbao_id='" + inv_InvoiceCode_id + "'");
            string hang_nghin = ".";
            string thap_phan = ",";

            DataColumnCollection columns = tblCtthongbao.Columns;
            if (columns.Contains("hang_nghin"))
            {
                hang_nghin = tblCtthongbao.Rows[0]["hang_nghin"].ToString();
            }
            if (columns.Contains("thap_phan"))
            {
                thap_phan = tblCtthongbao.Rows[0]["thap_phan"].ToString();
            }

            var t = Task.Run(() =>
            {
                var newCulture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                newCulture.NumberFormat.NumberDecimalSeparator = thap_phan;
                newCulture.NumberFormat.NumberGroupSeparator = hang_nghin;

                System.Threading.Thread.CurrentThread.CurrentCulture = newCulture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = newCulture;

                report.CreateDocument();

            });

            t.Wait();

            if (invoice.Inv_sobangke.ToString().Length > 0)
            {
                string fileName = folder + "\\BangKeDinhKem.repx";

                XtraReport rpBangKe = null;

                if (!File.Exists(fileName))
                {
                    rpBangKe = new XtraReport();
                    rpBangKe.SaveLayout(fileName);
                }
                else
                {
                    rpBangKe = XtraReport.FromFile(fileName, true);
                }

                rpBangKe.ScriptReferencesString = "AccountSignature.dll";
                rpBangKe.Name = "rpBangKe";
                rpBangKe.DisplayName = "BangKeDinhKem.repx";

                rpBangKe.DataSource = report.DataSource;

                rpBangKe.CreateDocument();
                report.Pages.AddRange(rpBangKe.Pages);

            }

            if (invoice.Trang_thai_hd.ToString() == "7")
            {

                Bitmap bmp = ReportUtil.DrawDiagonalLine(report);
                int pageCount = report.Pages.Count;


                for (int i = 0; i < pageCount; i++)
                {
                    Page page = report.Pages[i];
                    PageWatermark pmk = new PageWatermark();
                    pmk.Image = bmp;
                    page.AssignWatermark(pmk);
                }

                string fileName = folder + "\\BienBanXoaBo.repx";
                XtraReport rpBienBan = XtraReport.FromFile(fileName, true);

                rpBienBan.ScriptReferencesString = "AccountSignature.dll";
                rpBienBan.Name = "rpBienBan";
                rpBienBan.DisplayName = "BienBanXoaBo.repx";

                rpBienBan.DataSource = report.DataSource;
                rpBienBan.DataMember = report.DataMember;

                rpBienBan.CreateDocument();

                rpBienBan.PrintingSystem.ContinuousPageNumbering = false;
                report.PrintingSystem.ContinuousPageNumbering = false;

                report.Pages.AddRange(rpBienBan.Pages);

                int idx = pageCount;
                pageCount = report.Pages.Count;

                for (int i = idx; i < pageCount; i++)
                {
                    PageWatermark pmk = new PageWatermark();
                    pmk.ShowBehind = false;
                    report.Pages[i].AssignWatermark(pmk);
                }

            }

            //systemsetting systemsetting = (from c in context.Systemsettings
            //                               where c.systemsetting_key == "DECIMAL_POINT"
            //                               select c).FirstOrDefault<systemsetting>();
            //if (systemsetting != null)
            //{
            //    Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = systemsetting.systemsetting_value;
            //}
            //systemsetting systemsetting2 = (from c in context.Systemsettings
            //                                where c.systemsetting_key == "GROUP_POINT"
            //                                select c).FirstOrDefault<systemsetting>();
            //if (systemsetting2 != null)
            //{
            //    Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator = systemsetting2.systemsetting_value;
            //}
            //report.CreateDocument();
            //if ((invoice != null) && invoice.Inv_sobangke.HasValue)
            //{
            //    string path = folder + @"\BangKeDinhKem.repx";
            //    XtraReport report2 = null;
            //    if (!File.Exists(path))
            //    {
            //        report2 = new XtraReport();
            //        report2.SaveLayout(path);
            //    }
            //    else
            //    {
            //        report2 = XtraReport.FromFile(path, true);
            //    }
            //    report2.ScriptReferencesString = "AccountSignature.dll";
            //    report2.Name = "rpBangKe";
            //    report2.DisplayName = "BangKeDinhKem.repx";
            //    report2.DataSource = report.DataSource;
            //    report2.CreateDocument();
            //    report.Pages.AddRange(report2.Pages);
            //}
            //if (set.Tables["ThongTinHoaDon"].Rows[0]["LoaiHoaDon"].ToString() == "7")
            //{
            //    bitmap = DrawDiagonalLine(report);
            //    num2 = report.Pages.Count();
            //    for (num3 = 0; num3 < num2; num3++)
            //    {
            //        Page page = report.Pages[num3];
            //        watermark = new PageWatermark();
            //        watermark.Image = bitmap;
            //        page.AssignWatermark(watermark);
            //    }
            //    XtraReport report3 = XtraReport.FromFile(folder + @"\BienBanXoaBo.repx", true);
            //    report3.ScriptReferencesString = "AccountSignature.dll";
            //    report3.Name="rpBienBan";
            //    report3.DisplayName = "BienBanXoaBo.repx";
            //    report3.DataSource = report.DataSource;
            //    report3.DataMember = report.DataMember;
            //    report3.CreateDocument();
            //    report3.PrintingSystem.ContinuousPageNumbering = false;
            //    report.PrintingSystem.ContinuousPageNumbering = false;
            //    report.Pages.AddRange(report3.Pages);
            //    int num4 = num2;
            //    num2 = report.Pages.Count();
            //    for (num3 = num4; num3 < num2; num3++)
            //    {
            //        watermark = new PageWatermark();
            //        watermark.ShowBehind = false;
            //        report.Pages[num3].AssignWatermark(watermark);
            //    }
            //}
            //if (invoice != null)
            //{
            //    nullable = invoice.Trang_thai_hd;
            //    if (((nullable.GetValueOrDefault() == 13) && nullable.HasValue) || (((nullable = invoice.Trang_thai_hd).GetValueOrDefault() == 0x11) && nullable.HasValue))
            //    {
            //        bitmap = DrawDiagonalLine(report);
            //        num2 = report.Pages.Count();
            //        for (num3 = 0; num3 < num2; num3++)
            //        {
            //            watermark = new PageWatermark();
            //            watermark.Image =bitmap;
            //            report.Pages[num3].AssignWatermark(watermark);
            //        }
            //    }
            //}

            //Bitmap bmp = ReportUtil.DrawDiagonalLine(report);
            //int pageCount = report.Pages.Count;

            //for (int i = 0; i < pageCount; i++)
            //{
            //    PageWatermark pmk = new PageWatermark();
            //    pmk.Image = bmp;
            //    report.Pages[i].AssignWatermark(pmk);
            //}

            stream.Close();
            stream = null;
            stream = new MemoryStream();
            if (type == "Html")
            {
                report.ExportOptions.Html.EmbedImagesInHTML = true;
                report.ExportToHtml(stream);
            }
            else
            {
                report.ExportToPdf(stream);
            }
            return stream.ToArray();
        }

        public static JObject VeryfyXml(string xml)
        {
            //JObject json = new JObject();
            //try
            //{
            //    XmlDocument document = new XmlDocument();
            //    document.LoadXml(xml);
            //    var invInvoiceAuthId = document.SelectSingleNode("HoaDonDienTu/HoaDon/ThongTinHoaDon/SellerAppRecordId")?.InnerText;
            //    var mst = document.SelectSingleNode("HoaDonDienTu/HoaDon/ThongTinHoaDon/MaSoThueNguoiBan")?.InnerText.Replace("-", "");



            //    TracuuHDDTContext tracuu = new TracuuHDDTContext();
            //    var inv_admin = tracuu.Inv_admin.Where(c => c.MST == mst || c.alias == mst).FirstOrDefault<inv_admin>();
            //    InvoiceDbContext invoiceContext = new InvoiceDbContext(inv_admin.ConnectString);

            //    DataTable table =
            //        invoiceContext.ExecuteCmd(
            //            $"SELECT * FROM dbo.InvoiceXmlData WHERE inv_InvoiceAuth_id = '{invInvoiceAuthId}'");

            //    var xmlGoc = table.Rows[0]["data"].ToString();

            //    if (xml.Contains(xmlGoc))
            //    {
            //        json.Add("ok", "Hóa đơn chưa bị chỉnh sửa");
            //    }
            //    else
            //    {
            //        json.Add("error", "Hóa đơn đã bị chỉnh sửa");
            //    }


            //    return json;

            //}
            //catch (Exception ex)
            //{
            //    json.Add("error", ex.Message);
            //    return json;

            //}
            JObject json = new JObject();

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.LoadXml(xml);
                SignedXml signedXml = new SignedXml(xmlDoc);
                XmlNodeList nodeList = xmlDoc.GetElementsByTagName("Signature");
                XmlNodeList certificates = xmlDoc.GetElementsByTagName("X509Certificate");
                X509Certificate2 dcert2 = new X509Certificate2(Convert.FromBase64String(certificates[0].InnerText));
                foreach (XmlElement element in nodeList)
                {
                    var id = element.Attributes["Id"]?.InnerText;
                    if (id.Equals("seller"))
                    {
                        signedXml.LoadXml(element);
                        bool passes = signedXml.CheckSignature(dcert2, true);
                        json.Add(passes ? "ok" : "error", passes ? "Hóa đơn chưa bị chính sửa" : "Hóa đơn đã bị chỉnh sửa");
                    }

                }
                return json;
            }
            catch (Exception e)
            {
                json.Add("error", $"Có lỗi xảy ra: {e.Message}");
                return json;
            }
        }


        public static byte[] PrintReport(object datasource, string repx, string type)
        {
            byte[] bytes = null;

            XtraReport report = XtraReport.FromFile(repx, true);

            if (datasource != null)
            {
                report.DataSource = datasource;
            }

            if (datasource is DataSet)
            {
                DataSet ds = datasource as DataSet;
                if (ds.Tables.Count > 0)
                {
                    report.DataMember = ds.Tables[0].TableName;
                }
            }

            report.CreateDocument();


            MemoryStream ms = new MemoryStream();

            if (type == "Html")
            {
                report.ExportToHtml(ms);
            }
            else if (type == "Excel" || type == "xlsx")
            {
                report.ExportToXlsx(ms);
            }
            else
            {
                report.ExportToPdf(ms);
            }

            bytes = ms.ToArray();
            return bytes;

        }

        public static XtraReport LoadReportFromString(string s)
        {
            XtraReport report = null;

            using (StreamWriter sw = new StreamWriter(new MemoryStream()))
            {
                sw.Write(s.ToString());
                sw.Flush();
                report = XtraReport.FromStream(sw.BaseStream, true);
            }


            return report;
        }

        public static Bitmap DrawDiagonalLine(XtraReport report)
        {
            int PageWidth = report.PageWidth;
            int PageHeight = report.PageHeight;

            Bitmap bmp = new Bitmap(PageWidth, PageHeight);

            using (var graphics = Graphics.FromImage(bmp))
            {
                Pen blackPen = new Pen(Color.Red, 3);
                Point p1 = new Point(0, 0);
                Point p2 = new Point(PageWidth, PageHeight);
                Point p3 = new Point(PageWidth, 0);
                Point p4 = new Point(0, PageHeight);

                if (report.Watermark.Image != null)
                {
                    Image img = report.Watermark.Image;
                    Bitmap b = new Bitmap(img);

                    int transparentcy = report.Watermark.ImageTransparency;

                    if (transparentcy > 0)
                    {

                        b = SetBrightness(b, transparentcy);

                    }


                    Point p5 = new Point((PageWidth - b.Width) / 2, (PageHeight - b.Height) / 2);
                    graphics.DrawImage(b, p5);
                }

                graphics.DrawLine(blackPen, p1, p2);
                graphics.DrawLine(blackPen, p3, p4);
            }

            return bmp;

        }

        public static Bitmap DrawStringDemo(XtraReport report)
        {
            int PageWidth = report.PageWidth;
            int PageHeight = report.PageHeight;

            Bitmap bmp = new Bitmap(PageWidth, PageHeight);

            using (var graphics = Graphics.FromImage(bmp))
            {
                Pen blackPen = new Pen(Color.Red, 3);
                Point p1 = new Point(0, 0);
                Point p2 = new Point(PageWidth, PageHeight);
                Point p3 = new Point(PageWidth, 0);
                Point p4 = new Point(0, PageHeight);

                if (report.Watermark.Image != null)
                {
                    Image img = report.Watermark.Image;
                    Bitmap b = new Bitmap(img);

                    int transparentcy = report.Watermark.ImageTransparency;

                    if (transparentcy > 0)
                    {

                        b = SetBrightness(b, transparentcy);

                    }


                    Point p5 = new Point((PageWidth - b.Width) / 2, (PageHeight - b.Height) / 2);
                    graphics.DrawImage(b, p5);
                }

                RectangleF rectf = new RectangleF(0, 0, bmp.Width, bmp.Height);

                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                string demo = "HÓA ĐƠN DÙNG THỬ";

                Font font = new Font("Arial", 42);

                StringFormat format = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                SolidBrush brush = new SolidBrush(Color.FromArgb(255, 230, 230));

                graphics.DrawString(demo, font, brush, rectf, format);
                graphics.Flush();

                //bmp = SetBrightness(bmp, 120);
            }

            return bmp;

        }

        private static Bitmap SetBrightness(Bitmap _currentBitmap, int brightness)
        {
            //Bitmap temp = (Bitmap)_currentBitmap;
            Bitmap bmap = _currentBitmap;
            if (brightness < -255) brightness = -255;
            if (brightness > 255) brightness = 255;
            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    int cR = c.R + brightness;
                    int cG = c.G + brightness;
                    int cB = c.B + brightness;

                    if (cR < 0) cR = 1;
                    if (cR > 255) cR = 255;

                    if (cG < 0) cG = 1;
                    if (cG > 255) cG = 255;

                    if (cB < 0) cB = 1;
                    if (cB > 255) cB = 255;

                    bmap.SetPixel(i, j,
                    Color.FromArgb(c.A, (byte)cR, (byte)cG, (byte)cB));
                }
            }


            return bmap;
        }


        public static string FormatCurrency(string formatValue)
        {
            var format = "#,#";
            var format2 = string.Empty;
            int num = string.IsNullOrEmpty(formatValue) ? 0 : Convert.ToInt32(formatValue);
            if (Convert.ToDouble(formatValue) < 0.0)
            {
                return "0:0,0";
            }

            if (num == 0)
            {
                return format;
            }

            for (int i = 0; i < num; i++)
            {
                format2 += "#";
            }

            return $"{format}.{format2}";
        }
    }
}