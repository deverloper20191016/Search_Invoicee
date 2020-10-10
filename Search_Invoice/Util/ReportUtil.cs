﻿using DevExpress.XtraPrinting;
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
using Search_Invoice.Data;
using System.Security.Cryptography.Xml;

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
                stream2.Close();
            }
            stream.Close();
        }

        public static byte[] InvoiceReport(string xml, string repx, string folder, string type)
        {
            XmlReader reader;
            string msgTb = "";
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
            string mst = set.Tables["ThongTinHoaDon"].Rows[0]["MaSoThueNguoiBan"].ToString().Replace("-", "").Replace(" ", "");
            string input = set.Tables["ThongTinHoaDon"].Rows[0]["SellerAppRecordId"].ToString();
            TracuuHDDTContext tracuu = new TracuuHDDTContext();
            var invAdmin = tracuu.Inv_admin.FirstOrDefault(c => c.MST == mst || c.alias == mst);
            if (invAdmin != null)
            {
                InvoiceDbContext invoiceContext = new InvoiceDbContext(EncodeXml.Decrypt(invAdmin.ConnectString, "NAMPV18081202"));
                Guid invInvoiceAuthId = Guid.Parse(input);
                Inv_InvoiceAuth invoice = (from c in invoiceContext.Inv_InvoiceAuths
                    where c.Inv_InvoiceAuth_id.ToString() == invInvoiceAuthId.ToString()
                    select c).FirstOrDefault();
                if (invoice == null)
                {
                    throw new Exception("MST: " + mst + ". Không tồn tại hóa đơn");
                }
                if (invoice.Trang_thai_hd != null)
                {
                    Int32 trangThaiHd = (Int32)invoice.Trang_thai_hd;
                    string invOriginalId = invoice.Inv_originalId.ToString();
                    string invInvoiceCodeId = invoice.Inv_InvoiceCode_id.ToString();
                    if (trangThaiHd == 11 || trangThaiHd == 13 || trangThaiHd == 17)
                    {
                        if (!string.IsNullOrEmpty(invOriginalId))
                        {
                            Inv_InvoiceAuth tblInv = invoiceContext.Inv_InvoiceAuths.SqlQuery($"SELECT * FROM inv_InvoiceAuth WHERE inv_InvoiceAuth_id ='{invOriginalId}'").FirstOrDefault();
                            if (tblInv != null)
                            {
                                string invAdjustmentType = tblInv.Inv_adjustmentType.ToString();

                                string loai = invAdjustmentType == "5" || invAdjustmentType == "19" || invAdjustmentType == "21" ? "điều chỉnh" : invAdjustmentType == "3" ? "thay thế" : invAdjustmentType == "7" ? "xóa bỏ" : "";

                                if (invAdjustmentType == "5" || invAdjustmentType == "7" || invAdjustmentType == "3" || invAdjustmentType == "19" || invAdjustmentType == "21")
                                {
                                    msgTb = $"Hóa đơn bị {loai} bởi hóa đơn số: {tblInv.Inv_invoiceNumber} ngày {tblInv.Inv_invoiceIssuedDate:dd/MM/yyyy} mẫu số {tblInv.Mau_hd} ký hiệu {tblInv.Inv_invoiceSeries}";
                                }
                            }
                        }
                    }
                    if (Convert.ToInt32(invoice.Inv_adjustmentType) == 7)
                    {
                        msgTb = "";
                    }
                    if (report.Parameters["MSG_TB"] != null)
                    {
                        report.Parameters["MSG_TB"].Value = msgTb;
                    }
                    XRLabel label = report.AllControls<XRLabel>().FirstOrDefault(c => c.Name == "lblHoaDonMau");
                    if (label != null)
                    {
                        label.Visible = false;
                    }
                    report.DataSource = set;
                    DataTable tblCtthongbao = invoiceContext.ExecuteCmd($"SELECT * FROM ctthongbao a INNER JOIN dpthongbao b ON a.dpthongbao_id=b.dpthongbao_id WHERE a.ctthongbao_id ='{invInvoiceCodeId}'");
                    string hangNghin = ".";
                    string thapPhan = ",";
                    DataColumnCollection columns = tblCtthongbao.Columns;
                    if (columns.Contains("hang_nghin"))
                    {
                        hangNghin = tblCtthongbao.Rows[0]["hang_nghin"].ToString();
                    }
                    if (columns.Contains("thap_phan"))
                    {
                        thapPhan = tblCtthongbao.Rows[0]["thap_phan"].ToString();
                    }

                    var t = Task.Run(() =>
                    {
                        var newCulture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                        newCulture.NumberFormat.NumberDecimalSeparator = thapPhan;
                        newCulture.NumberFormat.NumberGroupSeparator = hangNghin;
                        System.Threading.Thread.CurrentThread.CurrentCulture = newCulture;
                        System.Threading.Thread.CurrentThread.CurrentUICulture = newCulture;
                        report.CreateDocument();
                    });
                    t.Wait();
                }
                if (invoice.Inv_sobangke.ToString().Length > 0)
                {
                    string fileName = folder + "\\BangKeDinhKem.repx";
                    XtraReport rpBangKe;
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
                    Bitmap bmp = DrawDiagonalLine(report);
                    int pageCount = report.Pages.Count;
                    for (int i = 0; i < pageCount; i++)
                    {
                        Page page = report.Pages[i];
                        PageWatermark pmk = new PageWatermark {Image = bmp};
                        page.AssignWatermark(pmk);
                    }

                    string fileName = folder + $@"\{mst}_BienBanXoaBo.repx";
                    if (!File.Exists(fileName))
                    {
                        fileName = folder + $"\\BienBanXoaBo.repx";
                    }
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
                        PageWatermark pmk = new PageWatermark {ShowBehind = false};
                        report.Pages[i].AssignWatermark(pmk);
                    }
                }
            }
            stream.Close();
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
            JObject json = new JObject();
            try
            {
                XmlDocument xmlDoc = new XmlDocument {PreserveWhitespace = true};
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
                        json.Add(passes ? "ok" : "error", passes ? "Hóa đơn toàn vẹn và hợp lệ" : "Hóa đơn hợp lệ");
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
            var bytes = ms.ToArray();
            return bytes;
        }

        public static XtraReport LoadReportFromString(string s)
        {
            XtraReport report;
            using (StreamWriter sw = new StreamWriter(new MemoryStream()))
            {
                sw.Write(s);
                sw.Flush();
                report = XtraReport.FromStream(sw.BaseStream, true);
            }
            return report;
        }

        public static Bitmap DrawDiagonalLine(XtraReport report)
        {
            int pageWidth = report.PageWidth;
            int pageHeight = report.PageHeight;
            Bitmap bmp = new Bitmap(pageWidth, pageHeight);
            using (var graphics = Graphics.FromImage(bmp))
            {
                Pen blackPen = new Pen(Color.Red, 3);
                Point p1 = new Point(0, 0);
                Point p2 = new Point(pageWidth, pageHeight);
                Point p3 = new Point(pageWidth, 0);
                Point p4 = new Point(0, pageHeight);
                if (report.Watermark.Image != null)
                {
                    Image img = report.Watermark.Image;
                    Bitmap b = new Bitmap(img);
                    int transparentcy = report.Watermark.ImageTransparency;
                    if (transparentcy > 0)
                    {
                        b = SetBrightness(b, transparentcy);
                    }
                    Point p5 = new Point((pageWidth - b.Width) / 2, (pageHeight - b.Height) / 2);
                    graphics.DrawImage(b, p5);
                }
                graphics.DrawLine(blackPen, p1, p2);
                graphics.DrawLine(blackPen, p3, p4);
            }
            return bmp;
        }

        private static Bitmap SetBrightness(Bitmap currentBitmap, int brightness)
        {
            Bitmap bmap = currentBitmap;
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
    }
}