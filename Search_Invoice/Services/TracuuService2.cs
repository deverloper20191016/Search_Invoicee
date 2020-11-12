
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using HtmlAgilityPack;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using Search_Invoice.Data;
using Search_Invoice.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Search_Invoice.Services
{
    public class TracuuService2 : ITracuuService2
    {
        private readonly INopDbContext2 _nopDbContext2;
        private readonly ICacheManager _cacheManager;
        private readonly IWebHelper _webHelper;

        public TracuuService2(INopDbContext2 nopDbContext2, ICacheManager cacheManager, IWebHelper webHelper)
        {
            _nopDbContext2 = nopDbContext2;
            _cacheManager = cacheManager;
            _webHelper = webHelper;

        }

        public JObject GetInfoInvoice(JObject model)
        {
            JObject json = new JObject();
            try
            {
                string sobaomat = model["sobaomat"].ToString();
                _nopDbContext2.SetConnect();
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"@sobaomat", sobaomat}
                };
                string sql = "SELECT TOP 1 * FROM inv_InvoiceAuth WHERE sobaomat = @sobaomat";
                DataTable dt = _nopDbContext2.ExecuteCmd(sql, CommandType.Text, parameters);

                if (dt.Rows.Count == 0)
                {
                    json.Add("error", $"Không tồn tại hóa đơn có số bảo mật: {sobaomat}");
                    return json;
                }
                dt.Columns.Add("mst", typeof(string));
                dt.Columns.Add("inv_auth_id", typeof(string));
                dt.Columns.Add("sum_tien", typeof(decimal));
                DataTable sumTien = _nopDbContext2.ExecuteCmd($"SELECT SUM(ISNULL(inv_TotalAmount, 0)) AS sum_total_amount FROM dbo.inv_InvoiceAuthDetail WHERE inv_InvoiceAuth_id = '{dt.Rows[0]["inv_InvoiceAuth_id"].ToString()}'");
              
                foreach (DataRow row in dt.Rows)
                {
                    row.BeginEdit();            
                    row["sum_tien"] = sumTien.Rows[0]["sum_total_amount"];
                    row.EndEdit();
                }
                JArray jar = JArray.FromObject(dt);
                json.Add("data", jar);
            }
            catch (Exception ex)
            {
                json.Add("error", ex.Message);
            }
            return json;
        }
        public byte[] PrintInvoiceFromSbm(string sobaomat, string folder, string type)
        {
            byte[] results = PrintInvoiceFromSbm(sobaomat,  folder, type, false);
            return results;
        }

        public byte[] PrintInvoiceFromSbm(string sobaomat, string folder, string type, bool inchuyendoi)
        {
            string xml;
            string fileNamePrint;
            var bytes = PrintInvoice(sobaomat,  folder, type, inchuyendoi, out xml, out fileNamePrint);
            return bytes;
        }

        public byte[] PrintInvoiceFromSbm(string sobaomat, string folder, string type, bool inchuyendoi,  out string xml, out string fileNamePrint)
        {
            var bytes = PrintInvoice(sobaomat,  folder, type, inchuyendoi, out xml, out fileNamePrint);
            return bytes;
        }

        public byte[] ExportZipFileXml(string sobaomat, string pathReport, ref string fileName, ref string key)
        {
            _nopDbContext2.SetConnect();
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"@sobaomat", sobaomat}
            };
            string sql = "SELECT TOP 1 * FROM inv_InvoiceAuth WHERE sobaomat = @sobaomat";
            DataTable dt = _nopDbContext2.ExecuteCmd(sql, CommandType.Text, parameters);
            if (dt.Rows.Count <= 0)
            {
                return null;
            }
            string mauHd = dt.Rows[0]["mau_hd"].ToString();
            string soSerial = dt.Rows[0]["inv_invoiceSeries"].ToString();
            string soHd = dt.Rows[0]["inv_invoiceNumber"].ToString();
            string invInvoiceCodeId = dt.Rows[0]["inv_InvoiceCode_id"].ToString();
            fileName = $"0100368686_invoice_{ mauHd.Trim().Replace("/", "")}_{soSerial.Trim().Replace("/", "")}_{soHd}";
            Guid invInvoiceAuthId = Guid.Parse(dt.Rows[0]["inv_InvoiceAuth_id"].ToString());
            DataTable tblInvoiceXmlData = _nopDbContext2.ExecuteCmd($"SELECT * FROM InvoiceXmlData WHERE inv_InvoiceAuth_id = '{invInvoiceAuthId}'");
            if (tblInvoiceXmlData.Rows.Count == 0)
            {
                return null;
            }
            DataTable tblCtthongbao = _nopDbContext2.ExecuteCmd($"SELECT * FROM ctthongbao WHERE ctthongbao_id = '{invInvoiceCodeId}'");
            DataTable tblMauHoaDon = _nopDbContext2.ExecuteCmd($"SELECT dmmauhoadon_id, report FROM dmmauhoadon WHERE dmmauhoadon_id = '{tblCtthongbao.Rows[0]["dmmauhoadon_id"].ToString()}'");
            string xml = tblInvoiceXmlData.Rows[0]["data"].ToString();
            xml = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + xml;
            MemoryStream outputMemStream = new MemoryStream();
            ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);
            zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
            // attack file xml
            ZipEntry newEntry = new ZipEntry("0100368686.xml")
            {
                DateTime = DateTime.Now,
                IsUnicodeText = true
            };
            zipStream.PutNextEntry(newEntry);
            byte[] bytes = Encoding.UTF8.GetBytes(xml);
            MemoryStream inStream = new MemoryStream(bytes);
            inStream.WriteTo(zipStream);
            inStream.Close();
            zipStream.CloseEntry();
            inStream = new MemoryStream();
            using (StreamWriter sw = new StreamWriter(inStream))
            {
                sw.Write(tblMauHoaDon.Rows[0]["report"].ToString());
                sw.Flush();
                newEntry = new ZipEntry("invoice.repx")
                {
                    DateTime = DateTime.Now,
                    IsUnicodeText = true
                };
                zipStream.PutNextEntry(newEntry);
                inStream.WriteTo(zipStream);
                inStream.Close();
                zipStream.CloseEntry();
                sw.Close();
            }
            zipStream.IsStreamOwner = false;    // False stops the Close also Closing the underlying stream.
            zipStream.Close();          // Must finish the ZipOutputStream before using outputMemStream.
            outputMemStream.Position = 0;
            var result = outputMemStream.ToArray();
            outputMemStream.Close();
            return result;
        }

        public byte[] GetInvoiceXml(string soBaoMat)
        {
            try
            {
                _nopDbContext2.SetConnect();
                InvoiceDbContext db = _nopDbContext2.GetInvoiceDb();
                byte[] bytes = null;
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"@sobaomat", soBaoMat}
                };
                string sql = "SELECT TOP 1 * FROM inv_InvoiceAuth WHERE sobaomat = @sobaomat";
                DataTable tblInvInvoiceAuth = _nopDbContext2.ExecuteCmd(sql, CommandType.Text, parameters);

                if (tblInvInvoiceAuth.Rows.Count == 0)
                {
                    throw new Exception("Không tồn tại hóa đơn có số bảo mật " + soBaoMat);
                }
                string invInvoiceAuthId = tblInvInvoiceAuth.Rows[0]["inv_InvoiceAuth_id"].ToString();
                DataTable tblInvoiceXmlData = _nopDbContext2.ExecuteCmd("SELECT * FROM InvoiceXmlData WHERE inv_InvoiceAuth_id='" + invInvoiceAuthId + "'");
                string xml = tblInvoiceXmlData.Rows.Count > 0 ? tblInvoiceXmlData.Rows[0]["data"].ToString() : db.Database.SqlQuery<string>("EXECUTE sproc_export_XmlInvoice '" + invInvoiceAuthId + "'").FirstOrDefault();
                if (xml != null)
                {
                    bytes = Encoding.UTF8.GetBytes(xml);
                }
                return bytes;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }

        private string GetFormatString(int formatDefault)
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

        private byte[] PrintInvoice(string sobaomat, string folder, string type, bool inchuyendoi, out string xml, out string fileNamePrint)
        {
            _nopDbContext2.SetConnect();
            InvoiceDbContext db = _nopDbContext2.GetInvoiceDb();
            byte[] bytes;
            xml = "";
            string msgTb = "";
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"@sobaomat", sobaomat}
                };
                var sql = "SELECT * FROM inv_InvoiceAuth WHERE sobaomat = @sobaomat";
                DataTable tblInvInvoiceAuth = _nopDbContext2.ExecuteCmd(sql, CommandType.Text, parameters);
                if (tblInvInvoiceAuth.Rows.Count == 0)
                {
                    throw new Exception("Không tồn tại hóa đơn có số bảo mật " + sobaomat);
                }
                string invInvoiceAuthId = tblInvInvoiceAuth.Rows[0]["inv_InvoiceAuth_id"].ToString();
                DataTable tblInvoiceXmlData = _nopDbContext2.ExecuteCmd($"SELECT * FROM InvoiceXmlData WHERE inv_InvoiceAuth_id = '{invInvoiceAuthId}'");
                string mauHd = tblInvInvoiceAuth.Rows[0]["mau_hd"].ToString();
                string soSerial = tblInvInvoiceAuth.Rows[0]["inv_invoiceSeries"].ToString();
                string soHd = tblInvInvoiceAuth.Rows[0]["inv_invoiceNumber"].ToString();
                fileNamePrint = $"0100368686_invoice_{mauHd.Trim().Replace("/", "")}_{soSerial.Trim().Replace("/", "")}_{soHd}";
                xml = tblInvoiceXmlData.Rows.Count > 0 ? tblInvoiceXmlData.Rows[0]["data"].ToString() : db.Database.SqlQuery<string>($"EXECUTE sproc_export_XmlInvoice '{invInvoiceAuthId}'").FirstOrDefault();
                string invInvoiceCodeId = tblInvInvoiceAuth.Rows[0]["inv_InvoiceCode_id"].ToString();
                int trangThaiHd = Convert.ToInt32(tblInvInvoiceAuth.Rows[0]["trang_thai_hd"]);
                string invOriginalId = tblInvInvoiceAuth.Rows[0]["inv_originalId"].ToString();
                DataTable tblCtthongbao = _nopDbContext2.ExecuteCmd($"SELECT * FROM ctthongbao a INNER JOIN dpthongbao b ON a.dpthongbao_id = b.dpthongbao_id WHERE a.ctthongbao_id = '{invInvoiceCodeId}'");
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
                if (string.IsNullOrEmpty(hangNghin))
                {
                    hangNghin = ".";
                }
                if (string.IsNullOrEmpty(thapPhan))
                {
                    thapPhan = ",";
                }
                string cacheReportKey = string.Format(CachePattern.InvoiceReportPatternKey + "{0}", tblCtthongbao.Rows[0]["dmmauhoadon_id"]);
                XtraReport report;
                DataTable tblDmmauhd = _nopDbContext2.ExecuteCmd($"SELECT * FROM dmmauhoadon WHERE dmmauhoadon_id= '{tblCtthongbao.Rows[0]["dmmauhoadon_id"].ToString()}'");
                string invReport = tblDmmauhd.Rows[0]["report"].ToString();

                if (invReport.Length > 0)
                {
                    report = ReportUtil.LoadReportFromString(invReport);
                    _cacheManager.Set(cacheReportKey, report, 30);
                }
                else
                {
                    throw new Exception("Không tải được mẫu hóa đơn");
                }
                report.Name = "XtraReport1";
                report.ScriptReferencesString = "AccountSignature.dll";
                DataSet ds = new DataSet();
                using (XmlReader xmlReader = XmlReader.Create(new StringReader(report.DataSourceSchema)))
                {
                    ds.ReadXmlSchema(xmlReader);
                    xmlReader.Close();
                }
                using (XmlReader xmlReader = XmlReader.Create(new StringReader(xml)))
                {
                    ds.ReadXml(xmlReader);
                    xmlReader.Close();
                }
                if (ds.Tables.Contains("TblXmlData"))
                {
                    ds.Tables.Remove("TblXmlData");
                }
                DataTable tblXmlData = new DataTable
                {
                    TableName = "TblXmlData"
                };
                tblXmlData.Columns.Add("data");
                DataRow r = tblXmlData.NewRow();
                r["data"] = xml;
                tblXmlData.Rows.Add(r);
                ds.Tables.Add(tblXmlData);
                if (trangThaiHd == 11 || trangThaiHd == 13 || trangThaiHd == 17)
                {
                    if (!string.IsNullOrEmpty(invOriginalId))
                    {
                        DataTable tblInv = _nopDbContext2.ExecuteCmd($"SELECT * FROM inv_InvoiceAuth WHERE inv_InvoiceAuth_id = '{invOriginalId}'");
                        string invAdjustmentType = tblInv.Rows[0]["inv_adjustmentType"].ToString();
                        string loai = invAdjustmentType == "5" || invAdjustmentType == "19" || invAdjustmentType == "21" || invAdjustmentType == "23" ? "điều chỉnh" : invAdjustmentType == "3" ? "thay thế" : invAdjustmentType == "7" ? "xóa bỏ" : "";
                        if (invAdjustmentType == "5" || invAdjustmentType == "7" || invAdjustmentType == "3" || invAdjustmentType == "19" || invAdjustmentType == "21" || invAdjustmentType == "23")
                        {
                            msgTb =
                                $"Hóa đơn bị {loai} bởi hóa đơn số: {tblInv.Rows[0]["inv_invoiceNumber"].ToString()} ngày {tblInv.Rows[0]["inv_invoiceIssuedDate"]:dd/MM/yyyy} mẫu số {tblInv.Rows[0]["mau_hd"].ToString()} ký hiệu {tblInv.Rows[0]["inv_invoiceSeries"].ToString()}";
                        }
                    }
                }
                if (Convert.ToInt32(tblInvInvoiceAuth.Rows[0]["inv_adjustmentType"]) == 7)
                {
                    msgTb = "";
                }

                if (report.Parameters["MSG_TB"] != null)
                {
                    report.Parameters["MSG_TB"].Value = msgTb;
                }
                XRLabel lblHoaDonMau = report.AllControls<XRLabel>().FirstOrDefault(c => c.Name == "lblHoaDonMau");
                if (lblHoaDonMau != null)
                {
                    lblHoaDonMau.Visible = false;
                }

                if (inchuyendoi)
                {
                    XRTable tblInChuyenDoi = report.AllControls<XRTable>().FirstOrDefault(c => c.Name == "tblInChuyenDoi");
                    if (tblInChuyenDoi != null)
                    {
                        tblInChuyenDoi.Visible = true;
                    }
                    if (report.Parameters["MSG_HD_TITLE"] != null)
                    {
                        report.Parameters["MSG_HD_TITLE"].Value = "Hóa đơn chuyển đổi từ hóa đơn điện tử";
                    }
                    if (report.Parameters["NGAY_IN_CDOI"] != null)
                    {
                        report.Parameters["NGAY_IN_CDOI"].Value = DateTime.Now;
                        report.Parameters["NGAY_IN_CDOI"].Visible = true;
                    }
                }
                if (report.Parameters["LINK_TRACUU"] != null)
                {
                    string sqlQrCodeLink = "SELECT TOP 1 * FROM wb_setting WHERE ma = 'QR_CODE_LINK'";
                    DataTable tblQrCodeLink = _nopDbContext2.ExecuteCmd(sqlQrCodeLink);
                    if (tblQrCodeLink.Rows.Count > 0)
                    {
                        string giatri = tblQrCodeLink.Rows[0]["gia_tri"].ToString();
                        if (giatri.Equals("C"))
                        {
                            report.Parameters["LINK_TRACUU"].Value = $"http://0100368686.minvoice.com.vn/api/Invoice/Preview?id={invInvoiceAuthId}";
                            report.Parameters["LINK_TRACUU"].Visible = true;
                        }
                    }
                }
                string invCurrencyCode = tblInvInvoiceAuth.Rows[0]["inv_currencyCode"].ToString();
                DataTable tbldmnt = _nopDbContext2.ExecuteCmd($"SELECT * FROM dbo.dmnt	WHERE ma_nt = '{invCurrencyCode}'");
                if (tbldmnt.Rows.Count > 0)
                {
                    DataRow rowDmnt = tbldmnt.Rows[0];
                    string quantityFomart = "n0";
                    string unitPriceFomart = "n0";
                    string totalAmountWithoutVatFomart = "n0";
                    string totalAmountFomart = "n0";
                    if (tbldmnt.Columns.Contains("inv_quantity"))
                    {
                        var quantityDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_quantity"].ToString())
                            ? rowDmnt["inv_quantity"].ToString()
                            : "0");
                        quantityFomart = GetFormatString(quantityDmnt);
                    }

                    if (tbldmnt.Columns.Contains("inv_unitPrice"))
                    {
                        var unitPriceDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_unitPrice"].ToString())
                            ? rowDmnt["inv_unitPrice"].ToString()
                            : "0");
                        unitPriceFomart = GetFormatString(unitPriceDmnt);
                    }
                    if (tbldmnt.Columns.Contains("inv_TotalAmountWithoutVat"))
                    {
                        var totalAmountWithoutVatDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_TotalAmountWithoutVat"].ToString())
                            ? rowDmnt["inv_TotalAmountWithoutVat"].ToString()
                            : "0");
                        totalAmountWithoutVatFomart = GetFormatString(totalAmountWithoutVatDmnt);
                    }
                    if (tbldmnt.Columns.Contains("inv_TotalAmount"))
                    {
                        var totalAmountDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_TotalAmount"].ToString())
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
                report.DataSource = ds;
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
                if (tblInvInvoiceAuth.Columns.Contains("inv_sobangke"))
                {
                    if (tblInvInvoiceAuth.Rows[0]["inv_sobangke"].ToString().Length > 0)
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
                }
                if (tblInvInvoiceAuth.Rows[0]["trang_thai_hd"].ToString() == "7")
                {
                    Bitmap bmp = ReportUtil.DrawDiagonalLine(report);
                    int pageCount = report.Pages.Count;
                    for (int i = 0; i < pageCount; i++)
                    {
                        Page page = report.Pages[i];
                        PageWatermark pmk = new PageWatermark
                        {
                            Image = bmp
                        };
                        page.AssignWatermark(pmk);
                    }
                    string fileName = folder + $@"\0100368686_BienBanXoaBo.repx";
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
                        PageWatermark pmk = new PageWatermark
                        {
                            ShowBehind = false
                        };
                        report.Pages[i].AssignWatermark(pmk);
                    }
                }
                if (trangThaiHd == 13 || trangThaiHd == 17)
                {
                    Bitmap bmp = ReportUtil.DrawDiagonalLine(report);
                    int pageCount = report.Pages.Count;
                    for (int i = 0; i < pageCount; i++)
                    {
                        PageWatermark pmk = new PageWatermark
                        {
                            Image = bmp
                        };
                        report.Pages[i].AssignWatermark(pmk);
                    }
                }
                MemoryStream ms = new MemoryStream();
                if (type == "Html")
                {
                    report.ExportOptions.Html.EmbedImagesInHTML = true;
                    report.ExportOptions.Html.ExportMode = HtmlExportMode.SingleFilePageByPage;
                    report.ExportOptions.Html.Title = "Hóa đơn điện tử M-Invoice";
                    report.ExportToHtml(ms);
                    string html = Encoding.UTF8.GetString(ms.ToArray());
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//td/@onmousedown");
                    if (nodes != null)
                    {
                        foreach (HtmlNode node in nodes)
                        {
                            string eventMouseDown = node.Attributes["onmousedown"].Value;
                            if (eventMouseDown.Contains("showCert('seller')"))
                            {
                                node.SetAttributeValue("id", "certSeller");
                            }
                            if (eventMouseDown.Contains("showCert('buyer')"))
                            {
                                node.SetAttributeValue("id", "certBuyer");
                            }
                            if (eventMouseDown.Contains("showCert('vacom')"))
                            {
                                node.SetAttributeValue("id", "certVacom");
                            }
                            if (eventMouseDown.Contains("showCert('minvoice')"))
                            {
                                node.SetAttributeValue("id", "certMinvoice");
                            }
                        }
                    }
                    HtmlNode head = doc.DocumentNode.SelectSingleNode("//head");
                    HtmlNode xmlNode = doc.CreateElement("script");
                    xmlNode.SetAttributeValue("id", "xmlData");
                    xmlNode.SetAttributeValue("type", "text/xmldata");
                    xmlNode.AppendChild(doc.CreateTextNode(xml));
                    head.AppendChild(xmlNode);
                    if (report.Watermark?.Image != null)
                    {
                        ImageConverter imageConverter = new ImageConverter();
                        byte[] img = (byte[])imageConverter.ConvertTo(report.Watermark.Image, typeof(byte[]));
                        string imgUrl = "data:image/png;base64," + Convert.ToBase64String(img);
                        HtmlNode style = doc.DocumentNode.SelectSingleNode("//style");
                        string strechMode = report.Watermark.ImageViewMode == ImageViewMode.Stretch ? "background-size: 100% 100%;" : "";
                        string waterMarkClass = ".waterMark { background-image:url(" + imgUrl + ");background-repeat:no-repeat;background-position:center;" + strechMode + " }";
                        HtmlTextNode textNode = doc.CreateTextNode(waterMarkClass);
                        style.AppendChild(textNode);
                        HtmlNode body = doc.DocumentNode.SelectSingleNode("//body");
                        HtmlNodeCollection pageNodes = body.SelectNodes("div");
                        foreach (HtmlNode pageNode in pageNodes)
                        {
                            pageNode.Attributes.Add("class", "waterMark");
                            string divStyle = pageNode.Attributes["style"].Value;
                            divStyle = divStyle + "margin-left:auto;margin-right:auto;";
                            pageNode.Attributes["style"].Value = divStyle;
                        }
                    }
                    ms.SetLength(0);
                    doc.Save(ms);
                }
                else if (type == "Image")
                {
                    ImageExportOptions options = new ImageExportOptions(ImageFormat.Png)
                    {
                        ExportMode = ImageExportMode.SingleFilePageByPage,
                    };
                    report.ExportToImage(ms, options);
                }
                else
                {
                    report.ExportToPdf(ms);
                }
                bytes = ms.ToArray();
                ms.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return bytes;
        }
    }
}
