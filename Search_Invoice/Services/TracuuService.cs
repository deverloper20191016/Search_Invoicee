using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using HtmlAgilityPack;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraPrinting;
using Search_Invoice.Util;
using DevExpress.XtraReports.UI;
using System.Globalization;
using System.Xml;
using System.Drawing.Imaging;
using DevExpress.XtraReports.Parameters;

namespace Search_Invoice.Services
{
    public class TracuuService : ITracuuService
    {
        private readonly INopDbContext _nopDbContext;
        private readonly ICacheManager _cacheManager;
        private readonly IWebHelper _webHelper;

        public TracuuService(INopDbContext nopDbContext,ICacheManager cacheManager,IWebHelper webHelper)
        {
            _nopDbContext = nopDbContext;
            _cacheManager = cacheManager;
            _webHelper = webHelper;
        }
        public byte[] PrintInvoiceFromSbm(string id, string folder, string type)
        {
            byte[] results = PrintInvoiceFromSbm(id, "", folder, type, false);
            return results;
        }

        public byte[] PrintInvoiceFromSbm(string id, string mst, string folder, string type)
        {
            byte[] results = PrintInvoiceFromSbm(id, mst, folder, type, false);
            return results;
        }

        public byte[] PrintInvoiceFromSbm(string id, string mst, string folder, string type, bool inchuyendoi)
        {
            var db = _nopDbContext.GetInvoiceDb();
            byte[] bytes;
            string msgTb = "";
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"@sobaomat", id}
                };
                string sql = "SELECT TOP 1 * FROM inv_InvoiceAuth WHERE sobaomat = @sobaomat";
                DataTable tblInvInvoiceAuth = _nopDbContext.ExecuteCmd(sql, CommandType.Text, parameters);
                if (tblInvInvoiceAuth.Rows.Count == 0)
                {
                    throw new Exception("Không tồn tại số bảo mật " + id);
                }
                string invInvoiceAuthId = tblInvInvoiceAuth.Rows[0]["inv_InvoiceAuth_id"].ToString();
                DataTable tblInvoiceXmlData = _nopDbContext.ExecuteCmd($"SELECT * FROM InvoiceXmlData WHERE inv_InvoiceAuth_id = '{invInvoiceAuthId}'");
                var xml = tblInvoiceXmlData.Rows.Count > 0 ? tblInvoiceXmlData.Rows[0]["data"].ToString() : db.Database.SqlQuery<string>($"EXECUTE sproc_export_XmlInvoice '{invInvoiceAuthId}'").FirstOrDefault();
                string invInvoiceCodeId = tblInvInvoiceAuth.Rows[0]["inv_InvoiceCode_id"].ToString();
                int trangThaiHd = Convert.ToInt32(tblInvInvoiceAuth.Rows[0]["trang_thai_hd"]);
                string invOriginalId = tblInvInvoiceAuth.Rows[0]["inv_originalId"].ToString();
                DataTable tblCtthongbao = _nopDbContext.ExecuteCmd($"SELECT * FROM ctthongbao a INNER JOIN dpthongbao b ON a.dpthongbao_id = b.dpthongbao_id WHERE a.ctthongbao_id = '{invInvoiceCodeId}'");
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
                DataTable tblDmmauhd = _nopDbContext.ExecuteCmd($"SELECT * FROM dmmauhoadon WHERE dmmauhoadon_id = '{tblCtthongbao.Rows[0]["dmmauhoadon_id"].ToString()}'");
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
                DataTable tblXmlData = new DataTable {TableName = "TblXmlData"};
                tblXmlData.Columns.Add("data");
                DataRow r = tblXmlData.NewRow();
                r["data"] = xml;
                tblXmlData.Rows.Add(r);
                ds.Tables.Add(tblXmlData);
                if (trangThaiHd == 11 || trangThaiHd == 13 || trangThaiHd == 17)
                {
                    if (!string.IsNullOrEmpty(invOriginalId))
                    {
                        DataTable tblInv = _nopDbContext.ExecuteCmd($"SELECT * FROM inv_InvoiceAuth WHERE inv_InvoiceAuth_id = '{invOriginalId}'");
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
                var lblHoaDonMau = report.AllControls<XRLabel>().FirstOrDefault(c => c.Name == "lblHoaDonMau");
                if (lblHoaDonMau != null)
                {
                    lblHoaDonMau.Visible = false;
                }
                if (inchuyendoi)
                {
                    var tblInChuyenDoi = report.AllControls<XRTable>().FirstOrDefault(c => c.Name == "tblInChuyenDoi");
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
                    var sqlQrCodeLink = "SELECT TOP 1 * FROM wb_setting WHERE ma = 'QR_CODE_LINK'";
                    var tblQrCodeLink = _nopDbContext.ExecuteCmd(sqlQrCodeLink);
                    if (tblQrCodeLink.Rows.Count > 0)
                    {
                        var giatri = tblQrCodeLink.Rows[0]["gia_tri"].ToString();
                        if (giatri.Equals("C"))
                        {
                            report.Parameters["LINK_TRACUU"].Value = $"http://{mst.Trim().Replace("-", "")}.minvoice.com.vn/api/Invoice/Preview?id={invInvoiceAuthId}";
                            report.Parameters["LINK_TRACUU"].Visible = true;
                        }
                    }
                }
                var invCurrencyCode = tblInvInvoiceAuth.Rows[0]["inv_currencyCode"].ToString();
                var tbldmnt = _nopDbContext.ExecuteCmd($"SELECT * FROM dbo.dmnt	WHERE ma_nt = '{invCurrencyCode}'");
                if (tbldmnt.Rows.Count > 0)
                {
                    var rowDmnt = tbldmnt.Rows[0];
                    var quantityFomart = "n0";
                    var unitPriceFomart = "n0";
                    var totalAmountWithoutVatFomart = "n0";
                    var totalAmountFomart = "n0";
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
                    var bmp = ReportUtil.DrawDiagonalLine(report);
                    int pageCount = report.Pages.Count;
                    for (int i = 0; i < pageCount; i++)
                    {
                        Page page = report.Pages[i];
                        PageWatermark pmk = new PageWatermark {Image = bmp};
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
                if (trangThaiHd == 13 || trangThaiHd == 17)
                {
                    var bmp = ReportUtil.DrawDiagonalLine(report);
                    int pageCount = report.Pages.Count;
                    for (int i = 0; i < pageCount; i++)
                    {
                        PageWatermark pmk = new PageWatermark {Image = bmp};
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
                    string api = _webHelper.GetRequest().ApplicationPath.StartsWith("/api") ? "/api" : "";
                    string serverApi = _webHelper.GetRequest().Url.Scheme + "://" + _webHelper.GetRequest().Url.Authority + api;
                    var nodes = doc.DocumentNode.SelectNodes("//td/@onmousedown");
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
                    xmlNode = doc.CreateElement("script");
                    xmlNode.SetAttributeValue("src", serverApi + "/Content/Scripts/jquery-1.6.4.min.js");
                    head.AppendChild(xmlNode);
                    xmlNode = doc.CreateElement("script");
                    xmlNode.SetAttributeValue("src", serverApi + "/Content/Scripts/jquery.signalR-2.2.3.min.js");
                    head.AppendChild(xmlNode);
                    xmlNode = doc.CreateElement("script");
                    xmlNode.SetAttributeValue("type", "text/javascript");
                    xmlNode.InnerHtml = "$(function () { "
                                       + "  var url = 'http://localhost:19898/signalr'; "
                                       + "  var connection = $.hubConnection(url, {  "
                                       + "     useDefaultPath: false "
                                       + "  });"
                                       + " var invoiceHubProxy = connection.createHubProxy('invoiceHub'); "
                                       + " invoiceHubProxy.on('resCommand', function (result) { "
                                       + " }); "
                                       + " connection.start().done(function () { "
                                       + "      console.log('Connect to the server successful');"
                                       + "      $('#certSeller').click(function () { "
                                       + "         var arg = { "
                                       + "              xml: document.getElementById('xmlData').innerHTML, "
                                       + "              id: 'seller' "
                                       + "         }; "
                                       + "         invoiceHubProxy.invoke('execCommand', 'ShowCert', JSON.stringify(arg)); "
                                       + "         }); "
                                       + "      $('#certVacom').click(function () { "
                                       + "         var arg = { "
                                       + "              xml: document.getElementById('xmlData').innerHTML, "
                                       + "              id: 'vacom' "
                                       + "         }; "
                                       + "         invoiceHubProxy.invoke('execCommand', 'ShowCert', JSON.stringify(arg)); "
                                       + "         }); "
                                       + "      $('#certBuyer').click(function () { "
                                       + "         var arg = { "
                                       + "              xml: document.getElementById('xmlData').innerHTML, "
                                       + "              id: 'buyer' "
                                       + "         }; "
                                       + "         invoiceHubProxy.invoke('execCommand', 'ShowCert', JSON.stringify(arg)); "
                                       + "         }); "
                                       + "      $('#certMinvoice').click(function () { "
                                       + "         var arg = { "
                                       + "              xml: document.getElementById('xmlData').innerHTML, "
                                       + "              id: 'minvoice' "
                                       + "         }; "
                                       + "         invoiceHubProxy.invoke('execCommand', 'ShowCert', JSON.stringify(arg)); "
                                       + "         }); "
                                       + "})"
                                       + ".fail(function () { "
                                       + "     alert('failed in connecting to the signalr server'); "
                                       + "});"
                                       + "});";
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
                        foreach (var pageNode in pageNodes)
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
                    var options = new ImageExportOptions(ImageFormat.Png)
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

        private string GetFormatString(int formatDefault)
        {
            var format = "#,#0";
            var format2 = string.Empty;
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