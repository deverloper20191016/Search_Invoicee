
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using Search_Invoice.Data;
using Search_Invoice.Data.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
            inv_admin invAdmin = tracuu.Inv_admin.FirstOrDefault(c => c.MST == mst || c.alias == mst);
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
                    int trangThaiHd = (int)invoice.Trang_thai_hd;
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

                    string invCurrencyCode = invoice.Inv_currencyCode;
                    DataTable tbldmnt = invoiceContext.ExecuteCmd($"SELECT * FROM dbo.dmnt	WHERE ma_nt = '{invCurrencyCode}'");
                    if (tbldmnt.Rows.Count > 0)
                    {
                        DataRow rowDmnt = tbldmnt.Rows[0];
                        string quantityFomart = "n0";
                        string unitPriceFomart = "n0";
                        string totalAmountWithoutVatFomart = "n0";
                        string totalAmountFomart = "n0";
                        if (tbldmnt.Columns.Contains("inv_quantity"))
                        {
                            int quantityDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_quantity"].ToString())
                                ? rowDmnt["inv_quantity"].ToString()
                                : "0");
                            quantityFomart = GetFormatString(quantityDmnt);
                        }

                        if (tbldmnt.Columns.Contains("inv_unitPrice"))
                        {
                            int unitPriceDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_unitPrice"].ToString())
                                ? rowDmnt["inv_unitPrice"].ToString()
                                : "0");
                            unitPriceFomart = GetFormatString(unitPriceDmnt);
                        }
                        if (tbldmnt.Columns.Contains("inv_TotalAmountWithoutVat"))
                        {
                            int totalAmountWithoutVatDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_TotalAmountWithoutVat"].ToString())
                                ? rowDmnt["inv_TotalAmountWithoutVat"].ToString()
                                : "0");
                            totalAmountWithoutVatFomart = GetFormatString(totalAmountWithoutVatDmnt);
                        }
                        if (tbldmnt.Columns.Contains("inv_TotalAmount"))
                        {
                            int totalAmountDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_TotalAmount"].ToString())
                                ? rowDmnt["inv_TotalAmount"].ToString()
                                : "0");
                            totalAmountFomart = GetFormatString(totalAmountDmnt);
                        }
                        report.Parameters.Add(new Parameter
                        {
                            Name = "FM_inv_quantity",
                            Value = quantityFomart
                        });
                        report.Parameters.Add(new Parameter
                        {
                            Name = "FM_inv_unitPrice",
                            Value = unitPriceFomart
                        });
                        report.Parameters.Add(new Parameter
                        {
                            Name = "FM_inv_TotalAmountWithoutVat",
                            Value = totalAmountWithoutVatFomart
                        });
                        report.Parameters.Add(new Parameter
                        {
                            Name = "FM_inv_TotalAmount",
                            Value = totalAmountFomart
                        });
                    }
                    else
                    {
                        report.Parameters.Add(new Parameter
                        {
                            Name = "FM_inv_quantity",
                            Value = "n0"
                        });
                        report.Parameters.Add(new Parameter
                        {
                            Name = "FM_inv_unitPrice",
                            Value = "n0"
                        });
                        report.Parameters.Add(new Parameter
                        {
                            Name = "FM_inv_TotalAmountWithoutVat",
                            Value = "n0"
                        });
                        report.Parameters.Add(new Parameter
                        {
                            Name = "FM_inv_TotalAmount",
                            Value = "n0"
                        });
                    }

                    Task t = Task.Run(() =>
                    {
                        CultureInfo newCulture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
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
                        PageWatermark pmk = new PageWatermark { Image = bmp };
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
                        PageWatermark pmk = new PageWatermark { ShowBehind = false };
                        report.Pages[i].AssignWatermark(pmk);
                    }
                }


                string sqlCheck =
                    $@"IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'wb_setting') SELECT TOP 1 gia_tri FROM dbo.wb_setting WHERE ma = 'OTP_BIENBAN'";

                DataTable checkOtpBienBan = invoiceContext.ExecuteCmd(sqlCheck);

                string otpBienBan = "C";
                if (checkOtpBienBan.Rows.Count > 0)
                {
                    string checkOtpBienBanTable = checkOtpBienBan.Rows[0]["gia_tri"].ToString();
                    if (!string.IsNullOrEmpty(checkOtpBienBanTable))
                    {
                        otpBienBan = checkOtpBienBanTable;
                    }
                }


                if (invoice.Trang_thai_hd == 3)
                {
                    if (otpBienBan.Equals("C"))
                    {
                        string reportFileThayThe = "INCT_BBTT.repx";
                        string sqlThayThe = "sproc_inct_hd_thaythe";
                        string fileName = folder + $@"\{mst}_{reportFileThayThe}";

                        if (!File.Exists(fileName))
                        {
                            fileName = folder + $"\\{reportFileThayThe}";
                        }

                        XtraReport rpBienBanThayThe = XtraReport.FromFile(fileName, true);
                        rpBienBanThayThe.ScriptReferencesString = "AccountSignature.dll";
                        rpBienBanThayThe.Name = "rpBienBanThayThe";
                        rpBienBanThayThe.DisplayName = reportFileThayThe;
                        Dictionary<string, string> pars = new Dictionary<string, string>
                        {
                            {"ma_dvcs", invoice.Ma_dvcs},
                            {"document_id", invInvoiceAuthId.ToString() }
                        };

                        DataSet dsThayThe = invoiceContext.GetDataSet(sqlThayThe, pars);

                        rpBienBanThayThe.DataSource = dsThayThe;
                        rpBienBanThayThe.DataMember = dsThayThe.Tables[0].TableName;
                        rpBienBanThayThe.CreateDocument();
                        rpBienBanThayThe.PrintingSystem.ContinuousPageNumbering = false;
                        report.PrintingSystem.ContinuousPageNumbering = false;
                        report.Pages.AddRange(rpBienBanThayThe.Pages);

                        int pageCount = report.Pages.Count;
                        report.Pages[pageCount - 1].AssignWatermark(new PageWatermark());
                    }


                }


                if (invoice.Trang_thai_hd == 19 || invoice.Trang_thai_hd == 21 || invoice.Trang_thai_hd == 5)
                {
                    if (otpBienBan.Equals("C"))
                    {
                        string reportFile = invoice.Trang_thai_hd == 5 ? "INCT_BBDC_DD.repx" : "INCT_BBDC_GT.repx";
                        string sqlDieuChinh = invoice.Trang_thai_hd == 5 ? "sproc_inct_hd_dieuchinhdg" : "sproc_inct_hd_dieuchinhgt";
                        string fileName = folder + $@"\{mst}_{reportFile}";

                        if (!File.Exists(fileName))
                        {
                            fileName = folder + $"\\{reportFile}";
                        }

                        XtraReport rpBienBan = XtraReport.FromFile(fileName, true);
                        rpBienBan.ScriptReferencesString = "AccountSignature.dll";
                        rpBienBan.Name = "rpBienBanDieuChinh";
                        rpBienBan.DisplayName = reportFile;
                        Dictionary<string, string> pars = new Dictionary<string, string>
                        {
                            {"ma_dvcs", invoice.Ma_dvcs},
                            {"document_id", invInvoiceAuthId.ToString() }
                        };

                        DataSet dsDieuChinh = invoiceContext.GetDataSet(sqlDieuChinh, pars);

                        rpBienBan.DataSource = dsDieuChinh;
                        rpBienBan.DataMember = dsDieuChinh.Tables[0].TableName;
                        rpBienBan.CreateDocument();
                        rpBienBan.PrintingSystem.ContinuousPageNumbering = false;
                        report.PrintingSystem.ContinuousPageNumbering = false;
                        report.Pages.AddRange(rpBienBan.Pages);

                        int pageCount = report.Pages.Count;
                        report.Pages[pageCount - 1].AssignWatermark(new PageWatermark());
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
                XmlDocument xmlDoc = new XmlDocument { PreserveWhitespace = true };
                xmlDoc.LoadXml(xml);
                SignedXml signedXml = new SignedXml(xmlDoc);
                XmlNodeList nodeList = xmlDoc.GetElementsByTagName("Signature");
                XmlNodeList certificates = xmlDoc.GetElementsByTagName("X509Certificate");
                X509Certificate2 dcert2 = new X509Certificate2(Convert.FromBase64String(certificates[0].InnerText));
                foreach (XmlElement element in nodeList)
                {
                    string id = element.Attributes["Id"]?.InnerText;
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
            byte[] bytes = ms.ToArray();
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
            using (Graphics graphics = Graphics.FromImage(bmp))
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
            if (brightness < -255)
            {
                brightness = -255;
            }

            if (brightness > 255)
            {
                brightness = 255;
            }

            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    int cR = c.R + brightness;
                    int cG = c.G + brightness;
                    int cB = c.B + brightness;
                    if (cR < 0)
                    {
                        cR = 1;
                    }

                    if (cR > 255)
                    {
                        cR = 255;
                    }

                    if (cG < 0)
                    {
                        cG = 1;
                    }

                    if (cG > 255)
                    {
                        cG = 255;
                    }

                    if (cB < 0)
                    {
                        cB = 1;
                    }

                    if (cB > 255)
                    {
                        cB = 255;
                    }

                    bmap.SetPixel(i, j,
                    Color.FromArgb(c.A, (byte)cR, (byte)cG, (byte)cB));
                }
            }
            return bmap;
        }


        private static string GetFormatString(int formatDefault)
        {
            string format = "#,#0";
            string format2 = string.Empty;
            if (formatDefault == 0)
            {
                return format;
            }
            for (int i = 0; i < formatDefault; i++)
            {
                format2 += "#";
            }
            return $"{format}.{format2}";
        }
    }
}