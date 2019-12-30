
using System.Threading.Tasks;
using System.Data;
using System.Drawing;
using Search_Invoice.Data.Domain;
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
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using DevExpress.XtraReports.Parameters;
using ICSharpCode.SharpZipLib.Zip;
using Search_Invoice.Data;

namespace Search_Invoice.Services
{
    public class TracuuService2 : ITracuuService2
    {
        private INopDbContext2 _nopDbContext2;
        private ICacheManager _cacheManager;
        private IWebHelper _webHelper;

        public TracuuService2(
                              INopDbContext2 nopDbContext2,
                              ICacheManager cacheManager,
                              IWebHelper webHelper
          )
        {
            this._nopDbContext2 = nopDbContext2;
            this._cacheManager = cacheManager;
            this._webHelper = webHelper;

        }
        public JObject GetInvoiceFromdateTodate(JObject model)
        {
            JObject json = new JObject();
            try
            {
                string mst = model["masothue"].ToString().Replace("-", "");

                DateTime tu_ngay = (DateTime)model["tu_ngay"];
                DateTime den_ngay = (DateTime)model["den_ngay"];
                string ma_dt = model["ma_dt"].ToString();
                _nopDbContext2.setConnect(mst);
                DataTable dt = this._nopDbContext2.ExecuteCmd("SELECT * FROM inv_InvoiceAuth WHERE inv_invoiceIssuedDate >= '" + tu_ngay + "' and inv_invoiceIssuedDate <= '" + den_ngay + "' AND ma_dt ='" + ma_dt + "'");
                //dt.Columns.Add("mst", typeof(string));

                //foreach (DataRow row in dt.Rows)
                //{
                //    row.BeginEdit();
                //    row["mst"] = mst;
                //    row.EndEdit();
                //}
                if (dt.Rows.Count > 0)
                {
                    JArray jar = JArray.FromObject(dt);
                    json.Add("data", jar);
                }
                else
                {
                    json.Add("error", "Không tìm thấy dữ liệu.");
                    return json;
                }
            }
            catch (Exception ex)
            {
                json.Add("error", ex.Message);
            }
            return json;
        }

        public JObject GetInfoInvoice(JObject model)
        {
            JObject json = new JObject();
            try
            {
                string mst = model["masothue"].ToString().Replace("-", "");

                string sobaomat = model["sobaomat"].ToString();
                _nopDbContext2.setConnect(mst);
                DataTable dt = this._nopDbContext2.ExecuteCmd("SELECT TOP 1 * FROM inv_InvoiceAuth WHERE sobaomat ='" + sobaomat + "'");
                dt.Columns.Add("mst", typeof(string));
                dt.Columns.Add("inv_auth_id", typeof(string));
                dt.Columns.Add("sum_tien", typeof(decimal));

                var sumTien = _nopDbContext2.ExecuteCmd($"SELECT SUM(ISNULL(inv_TotalAmount, 0)) AS sum_total_amount FROM dbo.inv_InvoiceAuthDetail WHERE inv_InvoiceAuth_id = '{dt.Rows[0]["inv_InvoiceAuth_id"].ToString()}'");


                var connectionString = _nopDbContext2.GetInvoiceDb().Database.Connection.ConnectionString;
                byte[] byt = System.Text.Encoding.UTF8.GetBytes(connectionString);
                var b = Convert.ToBase64String(byt);

                foreach (DataRow row in dt.Rows)
                {
                    row.BeginEdit();
                    row["mst"] = mst;
                    //row["a"] = connectionString;
                    row["inv_auth_id"] = b;
                    row["sum_tien"] = sumTien.Rows[0]["sum_total_amount"];
                    row.EndEdit();
                }
                if (dt.Rows.Count > 0)
                {
                    JArray jar = JArray.FromObject(dt);
                    json.Add("data", jar);
                }
                else
                {
                    json.Add("error", "Không tồn tại hóa đơn có số bảo mật: " + sobaomat);
                    return json;
                }
            }
            catch (Exception ex)
            {
                json.Add("error", ex.Message);
            }
            return json;
        }
        public byte[] PrintInvoiceFromSBM(string sobaomat, string masothue, string folder, string type)
        {
            byte[] results = PrintInvoiceFromSBM(sobaomat, masothue, folder, type, false);
            return results;
        }

        public byte[] PrintInvoiceFromSBM(string sobaomat, string masothue, string folder, string type, bool inchuyendoi)
        {
            _nopDbContext2.setConnect(masothue);
            var db = this._nopDbContext2.GetInvoiceDb();

            byte[] bytes = null;

            string xml = "";
            string msg_tb = "";

            try
            {
                // Guid inv_InvoiceAuth_id = Guid.Parse(id);

                DataTable tblInv_InvoiceAuth = this._nopDbContext2.ExecuteCmd("SELECT * FROM inv_InvoiceAuth WHERE sobaomat='" + sobaomat + "'");
                if (tblInv_InvoiceAuth.Rows.Count == 0)
                {
                    throw new Exception("Không tồn tại hóa đơn có số bảo mật " + sobaomat);
                }
                string inv_InvoiceAuth_id = tblInv_InvoiceAuth.Rows[0]["inv_InvoiceAuth_id"].ToString();
                DataTable tblInv_InvoiceAuthDetail = this._nopDbContext2.ExecuteCmd("SELECT * FROM inv_InvoiceAuthDetail WHERE inv_InvoiceAuth_id = '" + inv_InvoiceAuth_id + "'");
                DataTable tblInvoiceXmlData = this._nopDbContext2.ExecuteCmd("SELECT * FROM InvoiceXmlData WHERE inv_InvoiceAuth_id='" + inv_InvoiceAuth_id + "'");


                xml = tblInvoiceXmlData.Rows.Count > 0 ? tblInvoiceXmlData.Rows[0]["data"].ToString() : db.Database.SqlQuery<string>("EXECUTE sproc_export_XmlInvoice '" + inv_InvoiceAuth_id + "'").FirstOrDefault<string>();

                var invoiceDb = this._nopDbContext2.GetInvoiceDb();
                string inv_InvoiceCode_id = tblInv_InvoiceAuth.Rows[0]["inv_InvoiceCode_id"].ToString();
                int trang_thai_hd = Convert.ToInt32(tblInv_InvoiceAuth.Rows[0]["trang_thai_hd"]);
                string inv_originalId = tblInv_InvoiceAuth.Rows[0]["inv_originalId"].ToString();

                DataTable tblCtthongbao = this._nopDbContext2.ExecuteCmd("SELECT * FROM ctthongbao a INNER JOIN dpthongbao b ON a.dpthongbao_id=b.dpthongbao_id WHERE a.ctthongbao_id='" + inv_InvoiceCode_id + "'");
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
                if (hang_nghin == null || hang_nghin == "")
                {
                    hang_nghin = ".";
                }
                if (thap_phan == "" || thap_phan == null)
                {
                    thap_phan = ",";
                }

                string cacheReportKey = string.Format(CachePattern.INVOICE_REPORT_PATTERN_KEY + "{0}", tblCtthongbao.Rows[0]["dmmauhoadon_id"]);

                XtraReport report = new XtraReport();
                report = null;

                if (report == null)
                {

                    DataTable tblDmmauhd = this._nopDbContext2.ExecuteCmd("SELECT * FROM dmmauhoadon WHERE dmmauhoadon_id='" + tblCtthongbao.Rows[0]["dmmauhoadon_id"].ToString() + "'");
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

                DataTable tblXmlData = new DataTable();
                tblXmlData.TableName = "TblXmlData";
                tblXmlData.Columns.Add("data");

                DataRow r = tblXmlData.NewRow();
                r["data"] = xml;
                tblXmlData.Rows.Add(r);
                ds.Tables.Add(tblXmlData);

                string datamember = report.DataMember;

                if (datamember.Length > 0)
                {
                    if (ds.Tables.Contains(datamember))
                    {
                        DataTable tblChiTiet = ds.Tables[datamember];

                        int rowcount = ds.Tables[datamember].Rows.Count;


                    }
                }

                if (trang_thai_hd == 11 || trang_thai_hd == 13 || trang_thai_hd == 17)
                {
                    if (!string.IsNullOrEmpty(inv_originalId))
                    {
                        DataTable tblInv = this._nopDbContext2.ExecuteCmd("SELECT * FROM inv_InvoiceAuth WHERE inv_InvoiceAuth_id='" + inv_originalId + "'");
                        string inv_adjustmentType = tblInv.Rows[0]["inv_adjustmentType"].ToString();

                        string loai = inv_adjustmentType.ToString() == "5" || inv_adjustmentType.ToString() == "19" || inv_adjustmentType.ToString() == "21" ? "điều chỉnh" : inv_adjustmentType.ToString() == "3" ? "thay thế" : inv_adjustmentType.ToString() == "7" ? "xóa bỏ" : "";

                        if (inv_adjustmentType.ToString() == "5" || inv_adjustmentType.ToString() == "7" || inv_adjustmentType.ToString() == "3" || inv_adjustmentType.ToString() == "19" || inv_adjustmentType.ToString() == "21")
                        {
                            msg_tb = "Hóa đơn bị " + loai + " bởi hóa đơn số: " + tblInv.Rows[0]["inv_invoiceNumber"] + " ngày " + string.Format("{0:dd/MM/yyyy}", tblInv.Rows[0]["inv_invoiceIssuedDate"]) + ", mẫu số " + tblInv.Rows[0]["mau_hd"] + " ký hiệu " + tblInv.Rows[0]["inv_invoiceSeries"];

                        }
                    }
                }

                if (Convert.ToInt32(tblInv_InvoiceAuth.Rows[0]["inv_adjustmentType"]) == 7)
                {
                    msg_tb = "";
                }

                if (report.Parameters["MSG_TB"] != null)
                {
                    report.Parameters["MSG_TB"].Value = msg_tb;
                }

                var lblHoaDonMau = report.AllControls<XRLabel>().Where(c => c.Name == "lblHoaDonMau").FirstOrDefault<XRLabel>();

                if (lblHoaDonMau != null)
                {
                    lblHoaDonMau.Visible = false;
                }

                if (inchuyendoi)
                {
                    var tblInChuyenDoi = report.AllControls<XRTable>().Where(c => c.Name == "tblInChuyenDoi").FirstOrDefault<XRTable>();

                    if (tblInChuyenDoi != null)
                    {
                        tblInChuyenDoi.Visible = true;
                    }

                    if (report.Parameters["MSG_HD_TITLE"] != null)
                    {
                        report.Parameters["MSG_HD_TITLE"].Value = "Hóa đơn chuyển đổi từ hóa đơn điện tử";
                    }

                    if (report.Parameters["NGUOI_IN_CDOI"] != null)
                    {
                        report.Parameters["NGUOI_IN_CDOI"].Value = "";
                        report.Parameters["NGUOI_IN_CDOI"].Visible = true;
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
                    var tblQrCodeLink = _nopDbContext2.ExecuteCmd(sqlQrCodeLink);
                    if (tblQrCodeLink.Rows.Count > 0)
                    {
                        var giatri = tblQrCodeLink.Rows[0]["gia_tri"].ToString();
                        if (giatri.Equals("C"))
                        {
                            report.Parameters["LINK_TRACUU"].Value = $"http://{masothue.Trim().Replace("-", "")}.minvoice.com.vn/api/Invoice/Preview?id={inv_InvoiceAuth_id}";
                            report.Parameters["LINK_TRACUU"].Visible = true;
                        }
                    }
                }

                var inv_currencyCode = tblInv_InvoiceAuth.Rows[0]["inv_currencyCode"].ToString();

                var tbldmnt = _nopDbContext2.ExecuteCmd($"SELECT * FROM dbo.dmnt	WHERE ma_nt = '{inv_currencyCode}'");
                if (tbldmnt.Rows.Count > 0)
                {
                    var rowDmnt = tbldmnt.Rows[0];
                    var quantityDmnt = 0;
                    var unitPriceDmnt = 0;
                    var totalAmountWithoutVatDmnt = 0;
                    var totalAmountDmnt = 0;

                    var quantityFomart = "n0";
                    var unitPriceFomart = "n0";
                    var totalAmountWithoutVatFomart = "n0";
                    var totalAmountFomart = "n0";

                    if (tbldmnt.Columns.Contains("inv_quantity"))
                    {
                        quantityDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_quantity"].ToString())
                            ? rowDmnt["inv_quantity"].ToString()
                            : "0");
                        quantityFomart = GetFormatString(tblInv_InvoiceAuthDetail, quantityDmnt, "inv_quantity");

                    }

                    if (tbldmnt.Columns.Contains("inv_unitPrice"))
                    {
                        unitPriceDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_unitPrice"].ToString())
                            ? rowDmnt["inv_unitPrice"].ToString()
                            : "0");
                        unitPriceFomart = GetFormatString(tblInv_InvoiceAuthDetail, unitPriceDmnt, "inv_unitPrice");

                    }


                    if (tbldmnt.Columns.Contains("inv_TotalAmountWithoutVat"))
                    {
                        totalAmountWithoutVatDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_TotalAmountWithoutVat"].ToString())
                            ? rowDmnt["inv_TotalAmountWithoutVat"].ToString()
                            : "0");
                        totalAmountWithoutVatFomart = GetFormatString(tblInv_InvoiceAuthDetail, totalAmountWithoutVatDmnt, "inv_TotalAmountWithoutVat");

                    }

                    if (tbldmnt.Columns.Contains("inv_TotalAmount"))
                    {
                        totalAmountDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_TotalAmount"].ToString())
                            ? rowDmnt["inv_TotalAmount"].ToString()
                            : "0");
                        totalAmountFomart = GetFormatString(tblInv_InvoiceAuthDetail, totalAmountDmnt, "inv_TotalAmount");

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
                    newCulture.NumberFormat.NumberDecimalSeparator = thap_phan;
                    newCulture.NumberFormat.NumberGroupSeparator = hang_nghin;

                    System.Threading.Thread.CurrentThread.CurrentCulture = newCulture;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = newCulture;

                    report.CreateDocument();

                });

                t.Wait();

                //DataTable tblLicenseInfo = this._nopDbContext.ExecuteCmd("SELECT * FROM LicenseInfo WHERE ma_dvcs=N'" + tblInv_InvoiceAuth.Rows[0]["ma_dvcs"] + "' AND key_license IS NOT NULL AND LicenseXmlInfo IS NOT NULL");
                //if (tblLicenseInfo.Rows.Count == 0)
                //{
                //    Bitmap bmp = ReportUtil.DrawStringDemo(report);
                //    int pageCount = report.Pages.Count;

                //    for (int i = 0; i < pageCount; i++)
                //    {
                //        PageWatermark pmk = new PageWatermark();
                //        pmk.Image = bmp;
                //        report.Pages[i].AssignWatermark(pmk);
                //    }
                //}
                //if (masothue == "2700638514")
                //{
                //    if (tblInv_InvoiceAuthDetail.Rows.Count > 9)
                //    {
                //        if (tblInv_InvoiceAuth.Columns.Contains("inv_currencyCode"))
                //        {
                //            if (tblInv_InvoiceAuth.Rows[0]["inv_currencyCode"].ToString().Length > 0)
                //            {
                //                string currencyCode = tblInv_InvoiceAuth.Rows[0]["inv_currencyCode"].ToString();

                //                string fileName = currencyCode == "VND" ? folder + "\\INHD_BK_2700638514_VND.repx" : folder + "\\INHD_BK_2700638514_USD.repx";
                //                string rp_code = currencyCode == "VND" ? "sproc_inct_bangke_VND" : "sproc_inct_bangke_USD";
                //                //if (!File.Exists(fileName))
                //                //{
                //                //    fileName = folder + "\\BangKeDinhKem.repx";
                //                //}
                //                XtraReport rpBangKeTST = null;

                //                if (!File.Exists(fileName))
                //                {
                //                    rpBangKeTST = new XtraReport();
                //                    rpBangKeTST.SaveLayout(fileName);
                //                }
                //                else
                //                {
                //                    rpBangKeTST = XtraReport.FromFile(fileName, true);
                //                }

                //                //rpBangKeTST.ScriptReferencesString = "AccountSignature.dll";
                //                rpBangKeTST.Name = "rpBKTST";
                //                rpBangKeTST.DisplayName = "BangKeTST.repx";

                //                Dictionary<string, string> parameters = new Dictionary<string, string>();
                //                //parameters.Add("ma_dvcs", _webHelper.GetDvcs());
                //                parameters.Add("inv_InvoiceAuth_id", inv_InvoiceAuth_id);

                //                DataSet dataSource = this._nopDbContext2.GetDataSet(rp_code, parameters);

                //                rpBangKeTST.DataSource = dataSource;
                //                rpBangKeTST.DataMember = dataSource.Tables[0].TableName;

                //                rpBangKeTST.CreateDocument();
                //                report.Pages.AddRange(rpBangKeTST.Pages);
                //            }
                //        }
                //    }
                //}

                if (tblInv_InvoiceAuth.Columns.Contains("inv_sobangke"))
                {
                    if (tblInv_InvoiceAuth.Rows[0]["inv_sobangke"].ToString().Length > 0)
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
                }

                if (tblInv_InvoiceAuth.Rows[0]["trang_thai_hd"].ToString() == "7")
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

                //if (trang_thai_hd == 19 || trang_thai_hd == 21 || trang_thai_hd == 5)
                //{

                //    string rp_file = trang_thai_hd == 19 || trang_thai_hd == 21 ? "INCT_BBDC_GT.repx" : "INCT_BBDC_DD.repx";
                //    string rp_code = trang_thai_hd == 19 || trang_thai_hd == 21 ? "sproc_inct_hd_dieuchinhgt" : "sproc_inct_hd_dieuchinhdg";

                //    string fileName = folder + "\\" + rp_file;
                //    XtraReport rpBienBan = XtraReport.FromFile(fileName, true);

                //    rpBienBan.ScriptReferencesString = "AccountSignature.dll";
                //    rpBienBan.Name = "rpBienBanDC";
                //    rpBienBan.DisplayName = rp_file;

                //    Dictionary<string, string> parameters = new Dictionary<string, string>();
                //    parameters.Add("ma_dvcs", _webHelper.GetDvcs());
                //    parameters.Add("document_id", id);

                //    DataSet dataSource = this._nopDbContext.GetDataSet(rp_code, parameters);

                //    rpBienBan.DataSource = dataSource;
                //    rpBienBan.DataMember = dataSource.Tables[0].TableName;

                //    rpBienBan.CreateDocument();

                //    rpBienBan.PrintingSystem.ContinuousPageNumbering = false;
                //    report.PrintingSystem.ContinuousPageNumbering = false;

                //    report.Pages.AddRange(rpBienBan.Pages);

                //    Page page = report.Pages[report.Pages.Count - 1];
                //    page.AssignWatermark(new PageWatermark());

                //}

                if (trang_thai_hd == 13 || trang_thai_hd == 17)
                {
                    Bitmap bmp = ReportUtil.DrawDiagonalLine(report);
                    int pageCount = report.Pages.Count;

                    for (int i = 0; i < pageCount; i++)
                    {
                        PageWatermark pmk = new PageWatermark();
                        pmk.Image = bmp;
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

                    string api = this._webHelper.GetRequest().ApplicationPath.StartsWith("/api") ? "/api" : "";
                    string serverApi = this._webHelper.GetRequest().Url.Scheme + "://" + this._webHelper.GetRequest().Url.Authority + api;

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
                    //head.AppendChild(xmlNode);

                    //xmlNode = doc.CreateElement("script");
                    //xmlNode.SetAttributeValue("src", serverApi + "/Content/Scripts/jquery-1.6.4.min.js");
                    //head.AppendChild(xmlNode);

                    //xmlNode = doc.CreateElement("script");
                    //xmlNode.SetAttributeValue("src", serverApi + "/Content/Scripts/jquery.signalR-2.2.3.min.js");
                    //head.AppendChild(xmlNode);

                    //xmlNode = doc.CreateElement("script");
                    //xmlNode.SetAttributeValue("type", "text/javascript");

                    //xmlNode.InnerHtml = "$(function () { "
                    //                   + "  var url = 'http://localhost:19898/signalr'; "
                    //                   + "  var connection = $.hubConnection(url, {  "
                    //                   + "     useDefaultPath: false "
                    //                   + "  });"
                    //                   + " var invoiceHubProxy = connection.createHubProxy('invoiceHub'); "
                    //                   + " invoiceHubProxy.on('resCommand', function (result) { "
                    //                   + " }); "
                    //                   + " connection.start().done(function () { "
                    //                   + "      console.log('Connect to the server successful');"
                    //                   + "      $('#certSeller').click(function () { "
                    //                   + "         var arg = { "
                    //                   + "              xml: document.getElementById('xmlData').innerHTML, "
                    //                   + "              id: 'seller' "
                    //                   + "         }; "
                    //                   + "         invoiceHubProxy.invoke('execCommand', 'ShowCert', JSON.stringify(arg)); "
                    //                   + "         }); "
                    //                   + "      $('#certVacom').click(function () { "
                    //                   + "         var arg = { "
                    //                   + "              xml: document.getElementById('xmlData').innerHTML, "
                    //                   + "              id: 'vacom' "
                    //                   + "         }; "
                    //                   + "         invoiceHubProxy.invoke('execCommand', 'ShowCert', JSON.stringify(arg)); "
                    //                   + "         }); "
                    //                   + "      $('#certBuyer').click(function () { "
                    //                   + "         var arg = { "
                    //                   + "              xml: document.getElementById('xmlData').innerHTML, "
                    //                   + "              id: 'buyer' "
                    //                   + "         }; "
                    //                   + "         invoiceHubProxy.invoke('execCommand', 'ShowCert', JSON.stringify(arg)); "
                    //                   + "         }); "
                    //                   + "      $('#certMinvoice').click(function () { "
                    //                   + "         var arg = { "
                    //                   + "              xml: document.getElementById('xmlData').innerHTML, "
                    //                   + "              id: 'minvoice' "
                    //                   + "         }; "
                    //                   + "         invoiceHubProxy.invoke('execCommand', 'ShowCert', JSON.stringify(arg)); "
                    //                   + "         }); "
                    //                   + "})"
                    //                   + ".fail(function () { "
                    //                   + "     alert('failed in connecting to the signalr server'); "
                    //                   + "});"
                    //                   + "});";

                    head.AppendChild(xmlNode);

                    if (report.Watermark != null)
                    {
                        if (report.Watermark.Image != null)
                        {
                            ImageConverter _imageConverter = new ImageConverter();
                            byte[] img = (byte[])_imageConverter.ConvertTo(report.Watermark.Image, typeof(byte[]));

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
                    }

                    ms.SetLength(0);
                    doc.Save(ms);

                    doc = null;
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

                if (bytes == null)
                {
                    throw new Exception("null");
                }

            }
            catch (Exception ex)
            {
                //_logService.Insert("PrintInvoiceFromId", ex.ToString());

                throw new Exception(ex.Message.ToString());

            }

            return bytes;
        }

        public byte[] PrintInvoiceFromSBM(string sobaomat, string masothue, string folder, string type, out string xml)
        {
            bool inchuyendoi = false;
            _nopDbContext2.setConnect(masothue);
            var db = this._nopDbContext2.GetInvoiceDb();

            byte[] bytes = null;

            xml = "";
            string msg_tb = "";

            try
            {
                // Guid inv_InvoiceAuth_id = Guid.Parse(id);

                DataTable tblInv_InvoiceAuth = this._nopDbContext2.ExecuteCmd("SELECT * FROM inv_InvoiceAuth WHERE sobaomat='" + sobaomat + "'");
                if (tblInv_InvoiceAuth.Rows.Count == 0)
                {
                    throw new Exception("Không tồn tại hóa đơn có số bảo mật " + sobaomat);
                }
                string inv_InvoiceAuth_id = tblInv_InvoiceAuth.Rows[0]["inv_InvoiceAuth_id"].ToString();
                DataTable tblInv_InvoiceAuthDetail = this._nopDbContext2.ExecuteCmd("SELECT * FROM inv_InvoiceAuthDetail WHERE inv_InvoiceAuth_id = '" + inv_InvoiceAuth_id + "'");
                DataTable tblInvoiceXmlData = this._nopDbContext2.ExecuteCmd("SELECT * FROM InvoiceXmlData WHERE inv_InvoiceAuth_id='" + inv_InvoiceAuth_id + "'");


                //if (masothue == "2700638514" && tblInv_InvoiceAuthDetail.Rows.Count > 9)
                //{
                //    xml = db.Database.SqlQuery<string>("EXECUTE sproc_export_XmlInvoice_BK '" + inv_InvoiceAuth_id + "'").FirstOrDefault<string>();
                //}
                //else
                //{
                if (tblInvoiceXmlData.Rows.Count > 0)
                {
                    xml = tblInvoiceXmlData.Rows[0]["data"].ToString();
                }
                else
                {
                    xml = db.Database.SqlQuery<string>("EXECUTE sproc_export_XmlInvoice '" + inv_InvoiceAuth_id + "'").FirstOrDefault<string>();
                }
                //}
                var invoiceDb = this._nopDbContext2.GetInvoiceDb();
                string inv_InvoiceCode_id = tblInv_InvoiceAuth.Rows[0]["inv_InvoiceCode_id"].ToString();
                int trang_thai_hd = Convert.ToInt32(tblInv_InvoiceAuth.Rows[0]["trang_thai_hd"]);
                string inv_originalId = tblInv_InvoiceAuth.Rows[0]["inv_originalId"].ToString();
                //string user_name = _webHelper.GetUser();
                // wb_user wbuser = invoiceDb.WbUsers.Where(c => c.username == user_name).FirstOrDefault<wb_user>();
                DataTable tblCtthongbao = this._nopDbContext2.ExecuteCmd("SELECT * FROM ctthongbao a INNER JOIN dpthongbao b ON a.dpthongbao_id=b.dpthongbao_id WHERE a.ctthongbao_id='" + inv_InvoiceCode_id + "'");
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
                if (hang_nghin == null || hang_nghin == "")
                {
                    hang_nghin = ".";
                }
                if (thap_phan == "" || thap_phan == null)
                {
                    thap_phan = ",";
                }
                //string hang_nghin = tblCtthongbao.Rows[0]["hang_nghin"].ToString();
                //string thap_phan = tblCtthongbao.Rows[0]["thap_phan"].ToString();

                string cacheReportKey = string.Format(CachePattern.INVOICE_REPORT_PATTERN_KEY + "{0}", tblCtthongbao.Rows[0]["dmmauhoadon_id"]);

                //XtraReport report = _cacheManager.Get<XtraReport>(cacheReportKey);
                XtraReport report = new XtraReport();
                report = null;

                if (report == null)
                {

                    DataTable tblDmmauhd = this._nopDbContext2.ExecuteCmd("SELECT * FROM dmmauhoadon WHERE dmmauhoadon_id='" + tblCtthongbao.Rows[0]["dmmauhoadon_id"].ToString() + "'");
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

                DataTable tblXmlData = new DataTable();
                tblXmlData.TableName = "TblXmlData";
                tblXmlData.Columns.Add("data");

                DataRow r = tblXmlData.NewRow();
                r["data"] = xml;
                tblXmlData.Rows.Add(r);
                ds.Tables.Add(tblXmlData);

                string datamember = report.DataMember;

                if (datamember.Length > 0)
                {
                    if (ds.Tables.Contains(datamember))
                    {
                        DataTable tblChiTiet = ds.Tables[datamember];

                        int rowcount = ds.Tables[datamember].Rows.Count;


                    }
                }

                if (trang_thai_hd == 11 || trang_thai_hd == 13 || trang_thai_hd == 17)
                {
                    if (!string.IsNullOrEmpty(inv_originalId))
                    {
                        DataTable tblInv = this._nopDbContext2.ExecuteCmd("SELECT * FROM inv_InvoiceAuth WHERE inv_InvoiceAuth_id='" + inv_originalId + "'");
                        string inv_adjustmentType = tblInv.Rows[0]["inv_adjustmentType"].ToString();

                        string loai = inv_adjustmentType.ToString() == "5" || inv_adjustmentType.ToString() == "19" || inv_adjustmentType.ToString() == "21" ? "điều chỉnh" : inv_adjustmentType.ToString() == "3" ? "thay thế" : inv_adjustmentType.ToString() == "7" ? "xóa bỏ" : "";

                        if (inv_adjustmentType.ToString() == "5" || inv_adjustmentType.ToString() == "7" || inv_adjustmentType.ToString() == "3" || inv_adjustmentType.ToString() == "19" || inv_adjustmentType.ToString() == "21")
                        {
                            msg_tb = "Hóa đơn bị " + loai + " bởi hóa đơn số: " + tblInv.Rows[0]["inv_invoiceNumber"] + " ngày " + string.Format("{0:dd/MM/yyyy}", tblInv.Rows[0]["inv_invoiceIssuedDate"]) + ", mẫu số " + tblInv.Rows[0]["mau_hd"] + " ký hiệu " + tblInv.Rows[0]["inv_invoiceSeries"];

                        }
                    }
                }

                if (Convert.ToInt32(tblInv_InvoiceAuth.Rows[0]["inv_adjustmentType"]) == 7)
                {
                    msg_tb = "";
                }

                if (report.Parameters["MSG_TB"] != null)
                {
                    report.Parameters["MSG_TB"].Value = msg_tb;
                }

                var lblHoaDonMau = report.AllControls<XRLabel>().Where(c => c.Name == "lblHoaDonMau").FirstOrDefault<XRLabel>();

                if (lblHoaDonMau != null)
                {
                    lblHoaDonMau.Visible = false;
                }

                if (inchuyendoi)
                {
                    var tblInChuyenDoi = report.AllControls<XRTable>().Where(c => c.Name == "tblInChuyenDoi").FirstOrDefault<XRTable>();

                    if (tblInChuyenDoi != null)
                    {
                        tblInChuyenDoi.Visible = true;
                    }

                    if (report.Parameters["MSG_HD_TITLE"] != null)
                    {
                        report.Parameters["MSG_HD_TITLE"].Value = "Hóa đơn chuyển đổi từ hóa đơn điện tử";
                    }

                    //if (report.Parameters["NGUOI_IN_CDOI"] != null)
                    //{
                    //    report.Parameters["NGUOI_IN_CDOI"].Value = wbuser.ten_nguoi_sd == null ? "" : wbuser.ten_nguoi_sd;
                    //    report.Parameters["NGUOI_IN_CDOI"].Visible = true;
                    //}

                    if (report.Parameters["NGAY_IN_CDOI"] != null)
                    {
                        report.Parameters["NGAY_IN_CDOI"].Value = DateTime.Now;
                        report.Parameters["NGAY_IN_CDOI"].Visible = true;
                    }
                }

                if (report.Parameters["LINK_TRACUU"] != null)
                {
                    var sqlQrCodeLink = "SELECT TOP 1 * FROM wb_setting WHERE ma = 'QR_CODE_LINK'";
                    var tblQrCodeLink = _nopDbContext2.ExecuteCmd(sqlQrCodeLink);
                    if (tblQrCodeLink.Rows.Count > 0)
                    {
                        var giatri = tblQrCodeLink.Rows[0]["gia_tri"].ToString();
                        if (giatri.Equals("C"))
                        {
                            report.Parameters["LINK_TRACUU"].Value = $"http://{masothue.Trim().Replace("-", "")}.minvoice.com.vn/api/Invoice/Preview?id={inv_InvoiceAuth_id}";
                            report.Parameters["LINK_TRACUU"].Visible = true;
                        }
                    }
                }

                var inv_currencyCode = tblInv_InvoiceAuth.Rows[0]["inv_currencyCode"].ToString();

                var tbldmnt = _nopDbContext2.ExecuteCmd($"SELECT * FROM dbo.dmnt	WHERE ma_nt = '{inv_currencyCode}'");
                if (tbldmnt.Rows.Count > 0)
                {
                    var rowDmnt = tbldmnt.Rows[0];
                    var quantityDmnt = 0;
                    var unitPriceDmnt = 0;
                    var totalAmountWithoutVatDmnt = 0;
                    var totalAmountDmnt = 0;

                    var quantityFomart = "n0";
                    var unitPriceFomart = "n0";
                    var totalAmountWithoutVatFomart = "n0";
                    var totalAmountFomart = "n0";

                    if (tbldmnt.Columns.Contains("inv_quantity"))
                    {
                        quantityDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_quantity"].ToString())
                            ? rowDmnt["inv_quantity"].ToString()
                            : "0");
                        quantityFomart = GetFormatString(tblInv_InvoiceAuthDetail, quantityDmnt, "inv_quantity");

                    }

                    if (tbldmnt.Columns.Contains("inv_unitPrice"))
                    {
                        unitPriceDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_unitPrice"].ToString())
                            ? rowDmnt["inv_unitPrice"].ToString()
                            : "0");
                        unitPriceFomart = GetFormatString(tblInv_InvoiceAuthDetail, unitPriceDmnt, "inv_unitPrice");

                    }


                    if (tbldmnt.Columns.Contains("inv_TotalAmountWithoutVat"))
                    {
                        totalAmountWithoutVatDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_TotalAmountWithoutVat"].ToString())
                            ? rowDmnt["inv_TotalAmountWithoutVat"].ToString()
                            : "0");
                        totalAmountWithoutVatFomart = GetFormatString(tblInv_InvoiceAuthDetail, totalAmountWithoutVatDmnt, "inv_TotalAmountWithoutVat");

                    }

                    if (tbldmnt.Columns.Contains("inv_TotalAmount"))
                    {
                        totalAmountDmnt = int.Parse(!string.IsNullOrEmpty(rowDmnt["inv_TotalAmount"].ToString())
                            ? rowDmnt["inv_TotalAmount"].ToString()
                            : "0");
                        totalAmountFomart = GetFormatString(tblInv_InvoiceAuthDetail, totalAmountDmnt, "inv_TotalAmount");

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
                    newCulture.NumberFormat.NumberDecimalSeparator = thap_phan;
                    newCulture.NumberFormat.NumberGroupSeparator = hang_nghin;

                    System.Threading.Thread.CurrentThread.CurrentCulture = newCulture;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = newCulture;



                    report.CreateDocument();

                });

                t.Wait();

                //DataTable tblLicenseInfo = this._nopDbContext.ExecuteCmd("SELECT * FROM LicenseInfo WHERE ma_dvcs=N'" + tblInv_InvoiceAuth.Rows[0]["ma_dvcs"] + "' AND key_license IS NOT NULL AND LicenseXmlInfo IS NOT NULL");
                //if (tblLicenseInfo.Rows.Count == 0)
                //{
                //    Bitmap bmp = ReportUtil.DrawStringDemo(report);
                //    int pageCount = report.Pages.Count;

                //    for (int i = 0; i < pageCount; i++)
                //    {
                //        PageWatermark pmk = new PageWatermark();
                //        pmk.Image = bmp;
                //        report.Pages[i].AssignWatermark(pmk);
                //    }
                //}
                //if (masothue == "2700638514")
                //{
                //    if (tblInv_InvoiceAuthDetail.Rows.Count > 9)
                //    {
                //        if (tblInv_InvoiceAuth.Columns.Contains("inv_currencyCode"))
                //        {
                //            if (tblInv_InvoiceAuth.Rows[0]["inv_currencyCode"].ToString().Length > 0)
                //            {
                //                string currencyCode = tblInv_InvoiceAuth.Rows[0]["inv_currencyCode"].ToString();

                //                string fileName = currencyCode == "VND" ? folder + "\\INHD_BK_2700638514_VND.repx" : folder + "\\INHD_BK_2700638514_USD.repx";
                //                string rp_code = currencyCode == "VND" ? "sproc_inct_bangke_VND" : "sproc_inct_bangke_USD";
                //                //if (!File.Exists(fileName))
                //                //{
                //                //    fileName = folder + "\\BangKeDinhKem.repx";
                //                //}
                //                XtraReport rpBangKeTST = null;

                //                if (!File.Exists(fileName))
                //                {
                //                    rpBangKeTST = new XtraReport();
                //                    rpBangKeTST.SaveLayout(fileName);
                //                }
                //                else
                //                {
                //                    rpBangKeTST = XtraReport.FromFile(fileName, true);
                //                }

                //                //rpBangKeTST.ScriptReferencesString = "AccountSignature.dll";
                //                rpBangKeTST.Name = "rpBKTST";
                //                rpBangKeTST.DisplayName = "BangKeTST.repx";

                //                Dictionary<string, string> parameters = new Dictionary<string, string>();
                //                //parameters.Add("ma_dvcs", _webHelper.GetDvcs());
                //                parameters.Add("inv_InvoiceAuth_id", inv_InvoiceAuth_id);

                //                DataSet dataSource = this._nopDbContext2.GetDataSet(rp_code, parameters);

                //                rpBangKeTST.DataSource = dataSource;
                //                rpBangKeTST.DataMember = dataSource.Tables[0].TableName;

                //                rpBangKeTST.CreateDocument();
                //                report.Pages.AddRange(rpBangKeTST.Pages);
                //            }
                //        }
                //    }
                //}

                if (tblInv_InvoiceAuth.Columns.Contains("inv_sobangke"))
                {
                    if (tblInv_InvoiceAuth.Rows[0]["inv_sobangke"].ToString().Length > 0)
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



                }

                if (tblInv_InvoiceAuth.Rows[0]["trang_thai_hd"].ToString() == "7")
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

                //if (trang_thai_hd == 19 || trang_thai_hd == 21 || trang_thai_hd == 5)
                //{

                //    string rp_file = trang_thai_hd == 19 || trang_thai_hd == 21 ? "INCT_BBDC_GT.repx" : "INCT_BBDC_DD.repx";
                //    string rp_code = trang_thai_hd == 19 || trang_thai_hd == 21 ? "sproc_inct_hd_dieuchinhgt" : "sproc_inct_hd_dieuchinhdg";

                //    string fileName = folder + "\\" + rp_file;
                //    XtraReport rpBienBan = XtraReport.FromFile(fileName, true);

                //    rpBienBan.ScriptReferencesString = "AccountSignature.dll";
                //    rpBienBan.Name = "rpBienBanDC";
                //    rpBienBan.DisplayName = rp_file;

                //    Dictionary<string, string> parameters = new Dictionary<string, string>();
                //    parameters.Add("ma_dvcs", _webHelper.GetDvcs());
                //    parameters.Add("document_id", id);

                //    DataSet dataSource = this._nopDbContext.GetDataSet(rp_code, parameters);

                //    rpBienBan.DataSource = dataSource;
                //    rpBienBan.DataMember = dataSource.Tables[0].TableName;

                //    rpBienBan.CreateDocument();

                //    rpBienBan.PrintingSystem.ContinuousPageNumbering = false;
                //    report.PrintingSystem.ContinuousPageNumbering = false;

                //    report.Pages.AddRange(rpBienBan.Pages);

                //    Page page = report.Pages[report.Pages.Count - 1];
                //    page.AssignWatermark(new PageWatermark());

                //}

                if (trang_thai_hd == 13 || trang_thai_hd == 17)
                {
                    Bitmap bmp = ReportUtil.DrawDiagonalLine(report);
                    int pageCount = report.Pages.Count;

                    for (int i = 0; i < pageCount; i++)
                    {
                        PageWatermark pmk = new PageWatermark();
                        pmk.Image = bmp;
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


                    string api = this._webHelper.GetRequest().ApplicationPath.StartsWith("/api") ? "/api" : "";
                    string serverApi = this._webHelper.GetRequest().Url.Scheme + "://" + this._webHelper.GetRequest().Url.Authority + api;

                    var nodes = doc.DocumentNode.SelectNodes("//td/@onmousedown");
                    //td[@onmousedown]

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
                    //head.AppendChild(xmlNode);

                    //xmlNode = doc.CreateElement("script");
                    //xmlNode.SetAttributeValue("src", serverApi + "/Content/Scripts/jquery-1.6.4.min.js");
                    //head.AppendChild(xmlNode);

                    //xmlNode = doc.CreateElement("script");
                    //xmlNode.SetAttributeValue("src", serverApi + "/Content/Scripts/jquery.signalR-2.2.3.min.js");
                    //head.AppendChild(xmlNode);

                    //xmlNode = doc.CreateElement("script");
                    //xmlNode.SetAttributeValue("type", "text/javascript");

                    //xmlNode.InnerHtml = "$(function () { "
                    //                   + "  var url = 'http://localhost:19898/signalr'; "
                    //                   + "  var connection = $.hubConnection(url, {  "
                    //                   + "     useDefaultPath: false "
                    //                   + "  });"
                    //                   + " var invoiceHubProxy = connection.createHubProxy('invoiceHub'); "
                    //                   + " invoiceHubProxy.on('resCommand', function (result) { "
                    //                   + " }); "
                    //                   + " connection.start().done(function () { "
                    //                   + "      console.log('Connect to the server successful');"
                    //                   + "      $('#certSeller').click(function () { "
                    //                   + "         var arg = { "
                    //                   + "              xml: document.getElementById('xmlData').innerHTML, "
                    //                   + "              id: 'seller' "
                    //                   + "         }; "
                    //                   + "         invoiceHubProxy.invoke('execCommand', 'ShowCert', JSON.stringify(arg)); "
                    //                   + "         }); "
                    //                   + "      $('#certVacom').click(function () { "
                    //                   + "         var arg = { "
                    //                   + "              xml: document.getElementById('xmlData').innerHTML, "
                    //                   + "              id: 'vacom' "
                    //                   + "         }; "
                    //                   + "         invoiceHubProxy.invoke('execCommand', 'ShowCert', JSON.stringify(arg)); "
                    //                   + "         }); "
                    //                   + "      $('#certBuyer').click(function () { "
                    //                   + "         var arg = { "
                    //                   + "              xml: document.getElementById('xmlData').innerHTML, "
                    //                   + "              id: 'buyer' "
                    //                   + "         }; "
                    //                   + "         invoiceHubProxy.invoke('execCommand', 'ShowCert', JSON.stringify(arg)); "
                    //                   + "         }); "
                    //                   + "      $('#certMinvoice').click(function () { "
                    //                   + "         var arg = { "
                    //                   + "              xml: document.getElementById('xmlData').innerHTML, "
                    //                   + "              id: 'minvoice' "
                    //                   + "         }; "
                    //                   + "         invoiceHubProxy.invoke('execCommand', 'ShowCert', JSON.stringify(arg)); "
                    //                   + "         }); "
                    //                   + "})"
                    //                   + ".fail(function () { "
                    //                   + "     alert('failed in connecting to the signalr server'); "
                    //                   + "});"
                    //                   + "});";

                    head.AppendChild(xmlNode);

                    if (report.Watermark != null)
                    {
                        if (report.Watermark.Image != null)
                        {
                            ImageConverter _imageConverter = new ImageConverter();
                            byte[] img = (byte[])_imageConverter.ConvertTo(report.Watermark.Image, typeof(byte[]));

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
                    }

                    ms.SetLength(0);
                    doc.Save(ms);

                    doc = null;
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

                if (bytes == null)
                {
                    throw new Exception("null");
                }

            }
            catch (Exception ex)
            {
                //_logService.Insert("PrintInvoiceFromId", ex.ToString());

                throw new Exception(ex.Message.ToString());

            }

            return bytes;
        }
        public byte[] ExportZipFileXML(string sobaomat, string masothue, string pathReport, ref string fileName, ref string key)
        {
            this._nopDbContext2.setConnect(masothue);
            var invoiceDb = this._nopDbContext2.GetInvoiceDb();
            byte[] result = null;

            //string ma_dvcs = this._webHelper.GetDvcs();
            //dmdvcs dvcs = invoiceDb.Dmdvcss.Where(c => c.ma_dvcs == "VP").FirstOrDefault<dmdvcs>();

            DataTable tblInv_InvoiceAuth = this._nopDbContext2.ExecuteCmd("SELECT * FROM inv_InvoiceAuth WHERE sobaomat='" + sobaomat + "'");
            if (tblInv_InvoiceAuth.Rows.Count <= 0)
            {
                return null;
            }
            string mau_hd = tblInv_InvoiceAuth.Rows[0]["mau_hd"].ToString();
            string so_serial = tblInv_InvoiceAuth.Rows[0]["inv_invoiceSeries"].ToString();
            string so_hd = tblInv_InvoiceAuth.Rows[0]["inv_invoiceNumber"].ToString();
            string inv_InvoiceCode_id = tblInv_InvoiceAuth.Rows[0]["inv_InvoiceCode_id"].ToString();

            fileName = masothue + "_invoice_" + mau_hd.Replace("/", "") + "_" + so_serial.Replace("/", "").Trim() + "_" + so_hd;
            Guid inv_InvoiceAuth_id = Guid.Parse(tblInv_InvoiceAuth.Rows[0]["inv_InvoiceAuth_id"].ToString());
            DataTable tblInvoiceXmlData = this._nopDbContext2.ExecuteCmd("SELECT * FROM InvoiceXmlData WHERE inv_InvoiceAuth_id='" + inv_InvoiceAuth_id + "'");

            if (tblInvoiceXmlData.Rows.Count == 0)
            {
                return null;
            }

            DataTable tblCtthongbao = this._nopDbContext2.ExecuteCmd("SELECT * FROM ctthongbao WHERE ctthongbao_id='" + inv_InvoiceCode_id + "'");
            DataTable tblMauHoaDon = this._nopDbContext2.ExecuteCmd("SELECT dmmauhoadon_id,report FROM dmmauhoadon WHERE dmmauhoadon_id='" + tblCtthongbao.Rows[0]["dmmauhoadon_id"].ToString() + "'");

            string xml = tblInvoiceXmlData.Rows[0]["data"].ToString();
            xml = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + xml;
            //key = Guid.NewGuid().ToString();
            //string encodeXML = EncodeXML.Encrypt(xml, key);

            //byte[] dataPdf = this.PrintInvoiceFromId(id, pathReport, "PDF");

            MemoryStream outputMemStream = new MemoryStream();
            ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

            zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

            // attack file xml
            ZipEntry newEntry = new ZipEntry(masothue + ".xml");
            newEntry.DateTime = DateTime.Now;
            newEntry.IsUnicodeText = true;

            zipStream.PutNextEntry(newEntry);

            byte[] bytes = Encoding.UTF8.GetBytes(xml);

            MemoryStream inStream = new MemoryStream(bytes);
            inStream.WriteTo(zipStream);
            inStream.Close();
            zipStream.CloseEntry();

            // attack file key
            //newEntry = new ZipEntry("key.txt");
            //newEntry.DateTime = DateTime.Now;
            //newEntry.IsUnicodeText = true;

            //zipStream.PutNextEntry(newEntry);
            //byte[] bytekey = Encoding.UTF8.GetBytes(key);
            //inStream = new MemoryStream(bytekey);
            //inStream.WriteTo(zipStream);
            //inStream.Close();
            //zipStream.CloseEntry();

            inStream = new MemoryStream();
            using (StreamWriter sw = new StreamWriter(inStream))
            {
                sw.Write(tblMauHoaDon.Rows[0]["report"].ToString());
                sw.Flush();

                newEntry = new ZipEntry("invoice.repx");
                newEntry.DateTime = DateTime.Now;
                newEntry.IsUnicodeText = true;
                zipStream.PutNextEntry(newEntry);

                inStream.WriteTo(zipStream);
                inStream.Close();
                zipStream.CloseEntry();

                sw.Close();
            }

            zipStream.IsStreamOwner = false;    // False stops the Close also Closing the underlying stream.
            zipStream.Close();          // Must finish the ZipOutputStream before using outputMemStream.

            outputMemStream.Position = 0;


            result = outputMemStream.ToArray();

            outputMemStream.Close();

            return result;
        }

        public JObject GetHtml(JObject model)
        {
            JObject json = new JObject();
            try
            {
                string sobaomat = model["sobaomat"].ToString();
                string masothue = model["masothue"].ToString();
                string folder = model["folder"].ToString();
                var result = PrintInvoiceFromSBM(sobaomat, masothue, folder, "Html");
                string html = Encoding.UTF8.GetString(result);
                json.Add("ok", html);
            }
            catch (Exception e)
            {
                json.Add("error", e.Message);
            }
            return json;
        }
        public JObject Search_Tax(string mst)
        {
            JObject json = new JObject();
            try
            {
                var _tracuuHDDTContext = new TracuuHDDTContext();
                inv_admin admin = _tracuuHDDTContext.Inv_admin.Where(c => c.MST == mst).FirstOrDefault<inv_admin>();
                if (admin == null)
                {
                    json.Add("error", "Không tồn tại MST : " + mst);
                    return json;
                }
                var json1 = Newtonsoft.Json.JsonConvert.SerializeObject(admin);
                json = JObject.Parse(json1);
            }
            catch (Exception ex)
            {
                json.Add("error", ex.Message);
            }
            return json;
        }

        public JObject SearchInvoice(JObject data)
        {
            var result = new JObject();
            try
            {

                var mst = data["mst"].ToString();
                if (string.IsNullOrEmpty(mst))
                {
                    result.Add("status_code", 400);
                    result.Add("error", "Vui lòng nhập mã số thuế");
                    return result;
                }

                _nopDbContext2.setConnect(mst);
                // type: all tất cả, date: Từ ngày - Đến ngày, number: Số hóa đơn, series: Mẫu số - Ký hiệu
                var type = data["type"].ToString();
                var tuNgay = "";
                var denNgay = "";
                var soHd = "";
                var now = DateTime.Now;

                var DaysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                var a = $"{now.Year}-{now.Month}-{DaysInMonth}";
                tuNgay = data.ContainsKey("tu_ngay") ? data["tu_ngay"].ToString() : $"{now.Year}-{now.Month}-1";
                denNgay = data.ContainsKey("den_ngay") ? data["den_ngay"].ToString() : a;

                if (string.IsNullOrEmpty(tuNgay))
                {
                    tuNgay = $"{now.Year}-{now.Month}-1";
                }

                if (string.IsNullOrEmpty(denNgay))
                {
                    denNgay = a;
                }

                if (data.ContainsKey("so_hd"))
                {
                    soHd = data["so_hd"].ToString();
                }

                var sqlBuilder = "SELECT * FROM dbo.inv_InvoiceAuth WHERE trang_thai_hd != 13 ";
                var sql = "";
                switch (type)
                {
                    case "all":
                        {
                            sql = sqlBuilder;
                            break;
                        }
                    case "date":
                        {
                            sql = $"{sqlBuilder} AND (inv_invoiceIssuedDate >= '{tuNgay}' AND inv_invoiceIssuedDate <= '{denNgay}') ";
                            break;
                        }
                    case "number":
                        {
                            if (string.IsNullOrEmpty(soHd))
                            {
                                result.Add("status_code", 400);
                                result.Add("error", "Vui lòng nhập số hóa đơn");
                                return result;
                            }
                            sql = $"{sqlBuilder} AND inv_invoiceNumber = '{soHd}'";
                            break;
                        }
                    case "series":
                        {
                            var mauSo = data.ContainsKey("mau_so") ? data["mau_so"].ToString() : "";
                            var kyHieu = data.ContainsKey("ky_hieu") ? data["ky_hieu"].ToString() : "";
                            if (string.IsNullOrEmpty(mauSo) || string.IsNullOrEmpty(kyHieu))
                            {
                                result.Add("status_code", 400);
                                result.Add("error", "Vui lòng nhập mẫu số, ký hiệu");
                                return result;
                            }
                            sql = $"{sqlBuilder} AND mau_hd = '{mauSo.Trim()}' AND inv_invoiceSeries = '{kyHieu.Trim()}' ";

                            var invoiceType = data.ContainsKey("invoice_type") ? data["invoice_type"].ToString() : "";
                            if (!string.IsNullOrEmpty(invoiceType))
                            {
                                sql += $"AND inv_invoiceType = '{invoiceType}' ";
                            }

                            break;
                        }
                    case "id":
                        {
                            var id = data.ContainsKey("id") ? data["id"].ToString() : "";
                            if (string.IsNullOrEmpty(id))
                            {
                                result.Add("status_code", 400);
                                result.Add("error", "Vui lòng nhập id");
                                return result;
                            }
                            sql = $"SELECT * FROM dbo.inv_InvoiceAuth WHERE inv_InvoiceAuth_id = '{id}' ";
                            break;
                        }
                    default:
                        {
                            sql = sqlBuilder;
                            break;
                        }
                }

                var connectionString = _nopDbContext2.GetInvoiceDb().Database.Connection.ConnectionString;
                byte[] byt = System.Text.Encoding.UTF8.GetBytes(connectionString);
                var b = Convert.ToBase64String(byt);
                var table = _nopDbContext2.ExecuteCmd(sql);

                table.Columns.Add("inv_auth_id", typeof(string));
                table.Columns.Add("masothue", typeof(string));
                table.Columns.Add("url_preview", typeof(string));
                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        row.BeginEdit();
                        //row["a"] = connectionString;
                        row["inv_auth_id"] = b;
                        row["masothue"] = mst;
                        row["url_preview"] = $"http://{mst.Trim().Replace("-", "")}.minvoice.com.vn/api/Invoice/Preview?id={row["inv_invoiceAuth_id"].ToString()}";
                        row.EndEdit();
                    }

                    var arr = JArray.FromObject(table);
                    result.Add("status_code", 200);
                    result.Add("ok", arr);
                    return result;
                }
                result.Add("status_code", 400);
                result.Add("error", "Không tìm thấy hóa đơn");
                return result;
            }
            catch (Exception ex)
            {
                result.Add("status_code", 400);
                result.Add("error", ex.Message);
                return result;
            }
        }

        public JObject GetInfoLogin(string userName, string mst)
        {
            var traCuu = new TracuuHDDTContext();
            var user = traCuu.inv_users.FirstOrDefault(x => x.username.Replace("-", "").Equals(userName.Replace("-", "")) && x.mst.Replace("-", "").Equals(mst.Replace("-", "")));
            _nopDbContext2.setConnect(mst);
            var infoTable = _nopDbContext2.ExecuteCmd($"SELECT TOP 1 * FROM dmdt WHERE ma_dt = '{userName}'");
            var tenDoiTuong = "";
            if (infoTable.Rows.Count > 0)
            {
                tenDoiTuong = infoTable.Rows[0]["ten_dt"].ToString();
            }
            var boolCheck = user != null && !string.IsNullOrEmpty(user.inv_user_id.ToString());
            if (boolCheck)
            {
                return new JObject
                {
                    {"status_code", 200 },
                    {"ok", new JObject
                    {
                        {"id", user.inv_user_id },
                        {"username", user.username },
                        {"mst", user.mst },
                        {"email", user.email },
                        {"ma_doi_tuong", user.ma_dt },
                        {"ten_doi_tuong", tenDoiTuong }
                    } }
                };
            }

            return new JObject
            {
                {"status_code", 400 },
                {"error",  $"Không tìm thấy thông tin tài khoản: {userName}, Mã số thuế: {mst}"}
            };

        }

        public JObject GetListInvoice(JObject data)
        {
            var json = new JObject();
            try
            {
                var userName = data["user_name"].ToString();
                string sql = "SELECT * FROM inv_InvoiceAuth";
                var where = $"WHERE trang_thai_hd != 13 AND ma_dt = '{userName}'";
                var orderBy = "ORDER BY inv_invoiceNumber";
                var paging = "";



                if (data.ContainsKey("filter"))
                {
                    var filterObject = (JObject)data["filter"];

                    //if (filterObject.ContainsKey("invoice_type"))
                    //{
                    //    var invoiceTypeObject = (JObject)filterObject["invoice_type"];
                    //    var mauSo = invoiceTypeObject.ContainsKey("mau_so") ? invoiceTypeObject["mau_so"].ToString() : "";
                    //    var kyHieu = invoiceTypeObject.ContainsKey("ky_hieu") ? invoiceTypeObject["ky_hieu"].ToString() : "";
                    //    var invoiceTypeName = invoiceTypeObject.ContainsKey("type_name")
                    //        ? invoiceTypeObject["type_name"].ToString()
                    //        : "";
                    //    if (string.IsNullOrEmpty(mauSo) || string.IsNullOrEmpty(kyHieu))
                    //    {
                    //        json.Add("error", "Chưa có thông tin mẫu số, ký hiệu");
                    //        return json;
                    //    }

                    //    where += $" AND mau_hd = '{mauSo}' AND inv_invoiceSeries = '{kyHieu}' ";

                    //    if (!string.IsNullOrEmpty(invoiceTypeName))
                    //    {
                    //        where += $" AND inv_invoiceType = '{invoiceTypeName}' ";
                    //    }

                    //}
                    //else
                    //{
                    //    json.Add("error", "Chưa có thông tin mẫu số, ký hiệu");
                    //    return json;
                    //}

                    if (filterObject.ContainsKey("trang_thai_hd"))
                    {
                        var trangThaiHoaDon = filterObject.ContainsKey("trang_thai_hd") ? filterObject["trang_thai_hd"] : 1;
                        where += $" AND trang_thai_hd = {trangThaiHoaDon}";
                    }

                    if (filterObject.ContainsKey("ngay_hoa_don"))
                    {
                        var ngayHoaDonObject = (JObject)filterObject["ngay_hoa_don"];

                        var tuNgay = ngayHoaDonObject.ContainsKey("tu_ngay") ? ngayHoaDonObject["tu_ngay"].ToString() : "";
                        var denNgay = ngayHoaDonObject.ContainsKey("den_ngay") ? ngayHoaDonObject["den_ngay"].ToString() : "";

                        var now = DateTime.Now;

                        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                        var dayNow = $"{now.Year}-{now.Month}-{daysInMonth}";

                        if (string.IsNullOrEmpty(tuNgay))
                        {
                            tuNgay = $"{now.Year}-{now.Month}-1";
                        }

                        if (string.IsNullOrEmpty(denNgay))
                        {
                            denNgay = dayNow;
                        }

                        where += $" AND CONVERT(DATE, inv_invoiceIssuedDate) BETWEEN '{tuNgay}' AND '{denNgay}'";

                    }

                    if (filterObject.ContainsKey("so_hoa_don"))
                    {
                        var soHoaDonObject = (JObject)filterObject["so_hoa_don"];
                        var tuSo = soHoaDonObject.ContainsKey("tu_so") ? soHoaDonObject["tu_so"] : 0;
                        var denSo = soHoaDonObject.ContainsKey("den_so") ? soHoaDonObject["den_so"] : 1;
                        where += $" AND  CONVERT(INT, inv_invoiceNumber) BETWEEN {tuSo} AND {denSo}";
                    }

                    if (filterObject.ContainsKey("gia_tri_hoa_don"))
                    {
                        var soHoaDonObject = (JObject)filterObject["gia_tri_hoa_don"];
                        var min = soHoaDonObject.ContainsKey("min") ? soHoaDonObject["min"] : 0;
                        var max = soHoaDonObject.ContainsKey("max") ? soHoaDonObject["max"] : 10000;
                        where += $" AND inv_InvoiceAuth_id IN (SELECT inv_InvoiceAuth_id FROM dbo.inv_InvoiceAuthDetail GROUP BY inv_InvoiceAuth_id HAVING (SUM(inv_TotalAmount) >= {min} AND SUM(inv_TotalAmount) <= {max})) ";
                    }

                    if (data.ContainsKey("paging"))
                    {
                        var pagingObject = (JObject)data["paging"];
                        var index = pagingObject.ContainsKey("index") ? (int)pagingObject["index"] : 1;
                        var count = pagingObject.ContainsKey("count") ? (int)pagingObject["count"] : 50;
                        var start = index <= 1 ? 0 : (index - 1) * count;
                        paging = $" OFFSET {start} ROWS FETCH NEXT {count} ROW ONLY ";
                        var sqlBuilder = $"{sql} {where} {orderBy} {paging}";

                        var mst = data["mst"].ToString();

                        _nopDbContext2.setConnect(mst);

                        var table = _nopDbContext2.ExecuteCmd(sqlBuilder);



                        if (table.Rows.Count > 0)
                        {
                            table.Columns.Add("masothue", typeof(string));
                            table.Columns.Add("url_preview", typeof(string));

                            foreach (DataRow row in table.Rows)
                            {
                                row.BeginEdit();
                                //row["a"] = connectionString;
                                row["masothue"] = mst;
                                row["url_preview"] = $"http://{mst.Trim().Replace("-", "")}.minvoice.com.vn/api/Invoice/Preview?id={row["inv_invoiceAuth_id"].ToString()}";
                                row.EndEdit();
                            }

                            var arr = JArray.FromObject(table);
                            json.Add("status_code", 200);
                            json.Add("ok", arr);
                            return json;
                        }
                        json.Add("status_code", 404);
                        json.Add("error", "Không tìm thấy hóa đơn");
                        return json;
                    }
                    json.Add("status_code", 400);
                    json.Add("error", "Chưa có thông tin phân trang");
                    return json;
                }
                json.Add("status_code", 400);
                json.Add("error", "Chưa có thông tin tìm kiếm");
                return json;
            }
            catch (Exception ex)
            {
                json.Add("status_code", 400);
                json.Add("error", ex.Message);
                return json;
            }
        }

        public JObject GetListInvoiceType(JObject data)
        {
            JObject json = new JObject();
            try
            {
                var value = data.ContainsKey("value") ? data["value"].ToString() : "";
                if (string.IsNullOrEmpty(value))
                {
                    json.Add("status_code", 400);
                    json.Add("error", "Vui lòng nhập giá trị");
                    return json;
                }

                var sql = "";
                sql = value.Equals("all") ? "SELECT DISTINCT ma_loai, ten_loai FROM dbo.ctthongbao  " : $"SELECT ctthongbao_id, ma_loai, ten_loai, mau_so, ky_hieu FROM dbo.ctthongbao WHERE ma_loai = '{value}'";


                var mst = data["mst"].ToString();
                _nopDbContext2.setConnect(mst);

                var table = _nopDbContext2.ExecuteCmd(sql);
                if (table.Rows.Count > 0)
                {
                    var arr = JArray.FromObject(table);
                    json.Add("status_code", 200);
                    json.Add("ok", arr);
                    return json;
                }
                json.Add("status_code", 400);
                json.Add("error", "Không tìm thấy dữ liệu");
                return json;
            }
            catch (Exception e)
            {
                json.Add("status_code", 400);
                json.Add("error", e.Message);
                return json;
            }
        }

        public JObject Search(JObject data)
        {
            var json = new JObject();
            try
            {
                var userName = data["user_name"].ToString();
                var sqlSelect = "SELECT * FROM dbo.inv_InvoiceAuth ";
                var where = $"WHERE trang_thai_hd != 13 AND ma_dt = '{userName}'";
                var orderBy = "ORDER BY inv_invoiceNumber";

                var index = 0;
                var count = 50;
                var start = 0;

                var pagination = $" OFFSET {start} ROWS FETCH NEXT {count} ROW ONLY ";

                var mst = data.ContainsKey("mst") ? data["mst"].ToString() : "";
                if (string.IsNullOrEmpty(mst))
                {
                    json.Add("status_code", 400);
                    json.Add("error", "Vui lòng nhập mã số thuế");
                    return json;

                }


                //if (data.ContainsKey("invoice_type"))
                //{
                //    var invoiceTypeObject = (JObject)data["invoice_type"];
                //    var mauSo = invoiceTypeObject.ContainsKey("mau_so") ? invoiceTypeObject["mau_so"].ToString() : "";
                //    var kyHieu = invoiceTypeObject.ContainsKey("ky_hieu") ? invoiceTypeObject["ky_hieu"].ToString() : "";

                //    if (string.IsNullOrEmpty(mauSo) || string.IsNullOrEmpty(kyHieu))
                //    {
                //        json.Add("error", "Chưa có thông tin mẫu số, ký hiệu");
                //        return json;
                //    }

                //    where += $" AND mau_hd = '{mauSo.Trim()}' AND inv_invoiceSeries = '{kyHieu.Trim()}' ";

                //    var invoiceTypeName = invoiceTypeObject.ContainsKey("type_name")
                //        ? invoiceTypeObject["type_name"].ToString()
                //        : "";

                //    if (!string.IsNullOrEmpty(invoiceTypeName))
                //    {
                //        where += $" AND inv_invoiceType = '{invoiceTypeName}' ";
                //    }
                //}
                //else
                //{
                //    json.Add("error", "Chưa có thông tin mẫu số, ký hiệu");
                //    return json;
                //}


                var value = data.ContainsKey("value") ? data["value"].ToString() : "";
                if (string.IsNullOrEmpty(value))
                {
                    json.Add("status_code", 400);
                    json.Add("error", "Vui lòng nhập giá trị tìm kiếm");
                    return json;

                }
                _nopDbContext2.setConnect(mst);


                var tableColumn = _nopDbContext2.GetAllColumnsOfTable("inv_InvoiceAuth");
                if (tableColumn.Rows.Count > 0)
                {

                    where += " AND ( ";
                    var i = 0;
                    foreach (DataRow row in tableColumn.Rows)
                    {


                        var dataType = !string.IsNullOrEmpty(row["DATA_TYPE"].ToString())
                            ? row["DATA_TYPE"].ToString()
                            : "string";
                        var columnName = row["COLUMN_NAME"].ToString();
                        if (columnName == "ma_dt")
                        {
                            where += "";
                        }

                        if (i == 0)
                        {
                            if (dataType.Equals("datetime") || dataType.Equals("date"))
                            {
                                where += $" CONVERT(DATE ,{columnName}) LIKE N'%{value}%' ";
                            }
                            else
                            {
                                where += $" {columnName} LIKE N'%{value}%' ";
                            }
                        }
                        else
                        {
                            if (dataType.Equals("datetime") || dataType.Equals("date"))
                            {
                                where += $" OR CONVERT(DATE ,{columnName}) LIKE N'%{value}%' ";
                            }
                            else
                            {
                                where += $" OR {columnName} LIKE N'%{value}%' ";
                            }
                        }

                        i++;

                    }
                    where += " ) ";

                }
                else
                {
                    json.Add("status_code", 404);
                    json.Add("error", "Không tìm thấy dữ liệu");
                    return json;
                }



                if (data.ContainsKey("paging"))
                {
                    var pagingObject = (JObject)data["paging"];
                    index = pagingObject.ContainsKey("index") ? (int)pagingObject["index"] : 1;
                    count = pagingObject.ContainsKey("count") ? (int)pagingObject["count"] : 50;
                    start = index <= 1 ? 0 : (index - 1) * count;
                    pagination = $" OFFSET {start} ROWS FETCH NEXT {count} ROW ONLY ";

                }

                var sqlBuilder = $"{sqlSelect} {where} {orderBy} {pagination}";



                var table = _nopDbContext2.ExecuteCmd(sqlBuilder);
                if (table.Rows.Count > 0)
                {
                    table.Columns.Add("masothue", typeof(string));
                    table.Columns.Add("url_preview", typeof(string));

                    foreach (DataRow row in table.Rows)
                    {
                        row.BeginEdit();
                        //row["a"] = connectionString;
                        row["masothue"] = mst;
                        row["url_preview"] = $"http://{mst.Trim().Replace("-", "")}.minvoice.com.vn/api/Invoice/Preview?id={row["inv_invoiceAuth_id"].ToString()}";
                        row.EndEdit();
                    }

                    var arr = JArray.FromObject(table);
                    json.Add("status_code", 200);
                    json.Add("ok", arr);
                    return json;
                }
                json.Add("status_code", 404);
                json.Add("error", "Không tìm thấy hóa đơn");
                return json;

            }
            catch (Exception ex)
            {
                json.Add("status_code", 400);
                json.Add("error", ex.Message);
                return json;
            }
        }

        private string GetFormatString(DataTable table, int formatDefault, string columnName)
        {
            string result = $"n{formatDefault}";
            foreach (DataRow row in table.Rows)
            {
                var value = row[columnName].ToString();
                var a = value.Split('.');
                if (a.Length > 1)
                {
                    var resultGetMin = GetMinOfNumber(int.Parse(a[1]));
                    if (resultGetMin == 0)
                    {
                        return $"n0";
                    }
                    var resultString = resultGetMin.ToString();


                    if (resultString.Length < formatDefault)
                    {
                        result = $"n{resultString.Length}";
                    }
                }
            }

            return result;
        }

        private int GetMinOfNumber(int number)
        {
            if (number <= 0) return 0;
            while (number % 10 == 0)
            {
                number = number / 10;

            }

            return number;
        }


    }
}
