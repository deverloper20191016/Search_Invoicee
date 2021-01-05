
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using HtmlAgilityPack;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using Search_Invoice.DAL;
using Search_Invoice.Data;
using Search_Invoice.Data.Domain;
using Search_Invoice.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
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
        public JObject GetInvoiceFromdateTodate(JObject model)
        {
            JObject json = new JObject();
            try
            {
                string mst = model["masothue"].ToString().Replace("-", "");
                DateTime tuNgay = (DateTime)model["tu_ngay"];
                DateTime denNgay = (DateTime)model["den_ngay"];
                string maDt = model["ma_dt"].ToString();
                _nopDbContext2.SetConnect(mst);
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"@ma_dt", maDt}
                };
                string sql = $"SELECT * FROM inv_InvoiceAuth WHERE inv_invoiceIssuedDate >= '{tuNgay}' and inv_invoiceIssuedDate <= '{denNgay}' AND ma_dt = @ma_dt";
                DataTable dt = _nopDbContext2.ExecuteCmd(sql, CommandType.Text, parameters);
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
                _nopDbContext2.SetConnect(mst);
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"@sobaomat", sobaomat}
                };
                string sql = "SELECT TOP 1 a.* FROM inv_InvoiceAuth AS a INNER JOIN dbo.InvoiceXmlData AS b ON b.inv_InvoiceAuth_id = a.inv_InvoiceAuth_id WHERE a.sobaomat = @sobaomat";
                DataTable dt = _nopDbContext2.ExecuteCmd(sql, CommandType.Text, parameters);
                TracuuHDDTContext tracuu = new TracuuHDDTContext();
                inv_customer_banned checkTraCuu = tracuu.inv_customer_banneds.FirstOrDefault(x =>
                    x.mst.Replace("-", "").Equals(mst.Replace("-", "")) && x.type.Equals("KHOATRACUU") && x.is_unblock == false);
                if (checkTraCuu != null && !string.IsNullOrEmpty(checkTraCuu.mst))
                {
                    json.Add("error", $"Quý khách đang bị khóa tra cứu. Vui lòng liên hệ admin để giải quyết");
                    return json;
                }
                if (dt.Rows.Count == 0)
                {
                    json.Add("error", $"Không tồn tại hóa đơn có số bảo mật: {sobaomat} tại mã số thuế: {mst}");
                    return json;
                }
                dt.Columns.Add("mst", typeof(string));
                dt.Columns.Add("inv_auth_id", typeof(string));
                dt.Columns.Add("sum_tien", typeof(decimal));
                DataTable sumTien = _nopDbContext2.ExecuteCmd($"SELECT SUM(ISNULL(inv_TotalAmount, 0)) AS sum_total_amount FROM dbo.inv_InvoiceAuthDetail WHERE inv_InvoiceAuth_id = '{dt.Rows[0]["inv_InvoiceAuth_id"].ToString()}'");
                string connectionString = EncodeXml.Encrypt(_nopDbContext2.GetInvoiceDb().Database.Connection.ConnectionString, "NAMPV18081202");
                byte[] byt = Encoding.UTF8.GetBytes(connectionString);
                string b = Convert.ToBase64String(byt);
                foreach (DataRow row in dt.Rows)
                {
                    row.BeginEdit();
                    row["mst"] = model["masothue"].ToString();
                    row["inv_auth_id"] = b;
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
        public byte[] PrintInvoiceFromSbm(string sobaomat, string masothue, string folder, string type, out string fileNamePrint)
        {
            byte[] results = PrintInvoiceFromSbm(sobaomat, masothue, folder, type, false, out fileNamePrint);
            return results;
        }

        public byte[] PrintInvoiceFromSbm(string sobaomat, string masothue, string folder, string type, bool inchuyendoi, out string fileNamePrint)
        {
            string xml;

            var bytes = PrintInvoice(sobaomat, masothue, folder, type, inchuyendoi, out xml, out fileNamePrint);
            return bytes;
        }

        public byte[] PrintInvoiceFromSbm(string sobaomat, string masothue, string folder, string type, bool inchuyendoi,  out string xml, out string fileNamePrint)
        {
            var bytes = PrintInvoice(sobaomat, masothue, folder, type, inchuyendoi, out xml, out fileNamePrint);
            return bytes;
        }

        public byte[] ExportZipFileXml(string sobaomat, string masothue, string pathReport, ref string fileName, ref string key)
        {
            _nopDbContext2.SetConnect(masothue);
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"@sobaomat", sobaomat}
            };
            string checkTable = "SELECT * FROM INFORMATION_SCHEMA.TABLES where table_name ='wb_setting'";
            string mahoa = "SELECT gia_tri FROM dbo.wb_setting WHERE ma = 'MA_HOA_XML' AND gia_tri ='C'";
            string sql = "SELECT TOP 1 a.* FROM inv_InvoiceAuth AS a INNER JOIN dbo.InvoiceXmlData AS b ON b.inv_InvoiceAuth_id = a.inv_InvoiceAuth_id WHERE a.sobaomat = @sobaomat";
            DataTable dt = _nopDbContext2.ExecuteCmd(sql, CommandType.Text, parameters);
            if (dt.Rows.Count <= 0)
            {
                return null;
            }
            string mauHd = dt.Rows[0]["mau_hd"].ToString();
            string soSerial = dt.Rows[0]["inv_invoiceSeries"].ToString();
            string soHd = dt.Rows[0]["inv_invoiceNumber"].ToString();
            string invInvoiceCodeId = dt.Rows[0]["inv_InvoiceCode_id"].ToString();
            fileName = $"{masothue}_invoice_{ mauHd.Trim().Replace("/", "")}_{soSerial.Trim().Replace("/", "")}_{soHd}";
            Guid invInvoiceAuthId = Guid.Parse(dt.Rows[0]["inv_InvoiceAuth_id"].ToString());
            DataTable tblInvoiceXmlData = _nopDbContext2.ExecuteCmd($"SELECT * FROM InvoiceXmlData WHERE inv_InvoiceAuth_id = '{invInvoiceAuthId}'");
            if (tblInvoiceXmlData.Rows.Count == 0)
            {
                return null;
            }
            DataTable tblCtthongbao = _nopDbContext2.ExecuteCmd($"SELECT * FROM ctthongbao WHERE ctthongbao_id = '{invInvoiceCodeId}'");
            DataTable tblMauHoaDon = _nopDbContext2.ExecuteCmd($"SELECT dmmauhoadon_id, report FROM dmmauhoadon WHERE dmmauhoadon_id = '{tblCtthongbao.Rows[0]["dmmauhoadon_id"].ToString()}'");

            Guid keyxml = Guid.NewGuid();
            string  xml = tblInvoiceXmlData.Rows[0]["data"].ToString();
            xml = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + xml;

            bool checkMaHoaXml = false;

            DataTable CheckTable = _nopDbContext2.ExecuteCmd(checkTable, CommandType.Text, parameters);
   
            if (CheckTable.Rows.Count > 0)
            {
                DataTable mahoaxml = _nopDbContext2.ExecuteCmd(mahoa, CommandType.Text, parameters);
                if(mahoaxml.Rows.Count > 0)
                {
                    xml = EncodeXml.Encrypt(xml.ToString(), keyxml.ToString());
                    checkMaHoaXml = true;
                }

            }
            MemoryStream outputMemStream = new MemoryStream();
            ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);
            zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
                                   // attack file xml
            ZipEntry newEntry = new ZipEntry(masothue + ".xml")
            {
                DateTime = DateTime.Now,
                IsUnicodeText = true
            };
            zipStream.PutNextEntry(newEntry);
            byte[] _keyxml = Encoding.UTF8.GetBytes(keyxml.ToString());
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

            if (CheckTable.Rows.Count > 0 && checkMaHoaXml)
            {
                newEntry = new ZipEntry("key.txt")
                {
                    DateTime = DateTime.Now,
                    IsUnicodeText = true
                };
                zipStream.PutNextEntry(newEntry);

                inStream = new MemoryStream(_keyxml);
                inStream.WriteTo(zipStream);
                inStream.Close();
                zipStream.CloseEntry();
            }
                


            zipStream.IsStreamOwner = false;    // False stops the Close also Closing the underlying stream.
            zipStream.Close();          // Must finish the ZipOutputStream before using outputMemStream.
            outputMemStream.Position = 0;
            var result = outputMemStream.ToArray();
            outputMemStream.Close();
            return result;
            //if(CheckTable.Rows.Count <= 0)
            //{


            //    string xml = tblInvoiceXmlData.Rows[0]["data"].ToString();
            //    xml = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + xml;
            //    MemoryStream outputMemStream = new MemoryStream();
            //    ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);
            //    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
            //                           // attack file xml
            //    ZipEntry newEntry = new ZipEntry(masothue + ".xml")
            //    {
            //        DateTime = DateTime.Now,
            //        IsUnicodeText = true
            //    };
            //    zipStream.PutNextEntry(newEntry);
            //    byte[] bytes = Encoding.UTF8.GetBytes(xml);
            //    MemoryStream inStream = new MemoryStream(bytes);
            //    inStream.WriteTo(zipStream);
            //    inStream.Close();
            //    zipStream.CloseEntry();
            //    inStream = new MemoryStream();
            //    using (StreamWriter sw = new StreamWriter(inStream))
            //    {
            //        sw.Write(tblMauHoaDon.Rows[0]["report"].ToString());
            //        sw.Flush();
            //        newEntry = new ZipEntry("invoice.repx")
            //        {
            //            DateTime = DateTime.Now,
            //            IsUnicodeText = true
            //        };
            //        zipStream.PutNextEntry(newEntry);
            //        inStream.WriteTo(zipStream);
            //        inStream.Close();
            //        zipStream.CloseEntry();
            //        sw.Close();
            //    }
            //    zipStream.IsStreamOwner = false;    // False stops the Close also Closing the underlying stream.
            //    zipStream.Close();          // Must finish the ZipOutputStream before using outputMemStream.
            //    outputMemStream.Position = 0;
            //    var result = outputMemStream.ToArray();
            //    outputMemStream.Close();
            //    return result;
            //} else
            //{
            //    string xml1 = "";
            //    string xml = "";
            //    Guid keyxml = Guid.NewGuid();
            //    DataTable mahoaxml = _nopDbContext2.ExecuteCmd(mahoa, CommandType.Text, parameters);

            //    if (mahoaxml.Rows.Count <= 0)
            //    {
            //        xml = tblInvoiceXmlData.Rows[0]["data"].ToString();
            //        xml = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + xml;
            //    }
            //    else
            //    {
            //        xml1 = tblInvoiceXmlData.Rows[0]["data"].ToString();
            //        xml = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + xml1;
            //        xml = EncodeXml.Encrypt(xml1.ToString(), keyxml.ToString());
            //    }

            //    MemoryStream outputMemStream = new MemoryStream();
            //    ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);
            //    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
            //                           // attack file xml
            //    ZipEntry newEntry = new ZipEntry(masothue + ".xml")
            //    {
            //        DateTime = DateTime.Now,
            //        IsUnicodeText = true
            //    };
            //    zipStream.PutNextEntry(newEntry);
            //    byte[] _keyxml = Encoding.UTF8.GetBytes(keyxml.ToString());
            //    byte[] bytes = Encoding.UTF8.GetBytes(xml);

            //    MemoryStream inStream = new MemoryStream(bytes);
            //    inStream.WriteTo(zipStream);
            //    inStream.Close();
            //    zipStream.CloseEntry();
            //    inStream = new MemoryStream();
            //    using (StreamWriter sw = new StreamWriter(inStream))
            //    {
            //        sw.Write(tblMauHoaDon.Rows[0]["report"].ToString());
            //        sw.Flush();
            //        newEntry = new ZipEntry("invoice.repx")
            //        {
            //            DateTime = DateTime.Now,
            //            IsUnicodeText = true
            //        };
            //        zipStream.PutNextEntry(newEntry);
            //        inStream.WriteTo(zipStream);
            //        inStream.Close();
            //        zipStream.CloseEntry();
            //        sw.Close();
            //    }


            //    newEntry = new ZipEntry("key.txt")
            //    {
            //        DateTime = DateTime.Now,
            //        IsUnicodeText = true
            //    };
            //    zipStream.PutNextEntry(newEntry);

            //    inStream = new MemoryStream(_keyxml);
            //    inStream.WriteTo(zipStream);
            //    inStream.Close();
            //    zipStream.CloseEntry();


            //    zipStream.IsStreamOwner = false;    // False stops the Close also Closing the underlying stream.
            //    zipStream.Close();          // Must finish the ZipOutputStream before using outputMemStream.
            //    outputMemStream.Position = 0;
            //    var result = outputMemStream.ToArray();
            //    outputMemStream.Close();
            //    return result;
            //}


        }

        public byte[] GetInvoiceXml(string soBaoMat, string maSoThue)
        {
            try
            {
                _nopDbContext2.SetConnect(maSoThue);
                InvoiceDbContext db = _nopDbContext2.GetInvoiceDb();
                byte[] bytes = null;
                DataTable tblInvInvoiceAuth = _nopDbContext2.ExecuteCmd($"SELECT * FROM inv_InvoiceAuth WHERE sobaomat = '{soBaoMat}' ");
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

        public JObject GetHtml(JObject model)
        {
            JObject json = new JObject();
            try
            {
                string sobaomat = model["sobaomat"].ToString();
                string masothue = model["masothue"].ToString();
                string folder = model["folder"].ToString();
                string fileName;
                byte[] result = PrintInvoiceFromSbm(sobaomat, masothue, folder, "Html", out fileName);
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
                TracuuHDDTContext tracuuHddtContext = new TracuuHDDTContext();
                inv_admin admin = tracuuHddtContext.Inv_admin.FirstOrDefault(c => c.MST == mst);
                if (admin == null)
                {
                    json.Add("error", "Không tồn tại MST : " + mst);
                    return json;
                }
                string json1 = Newtonsoft.Json.JsonConvert.SerializeObject(admin);
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
            JObject result = new JObject();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string mst = data["mst"].ToString();
                if (string.IsNullOrEmpty(mst))
                {
                    result.Add("status_code", 400);
                    result.Add("error", "Vui lòng nhập Mã số thuế đơn vị bán");
                    return result;
                }
                string userName = data.ContainsKey("user_name") ? data["user_name"].ToString() : "";
                if (string.IsNullOrEmpty(userName))
                {
                    result.Add("status_code", 400);
                    result.Add("error", "Không có thông tin đăng nhập");
                    return result;
                }
                string maDt = data.ContainsKey("ma_dt") ? data["ma_dt"].ToString() : "";
                if (string.IsNullOrEmpty(maDt))
                {
                    result.Add("status_code", 400);
                    result.Add("error", "Không có thông tin đăng nhập");
                    return result;
                }
                _nopDbContext2.SetConnect(mst);
                // type: all tất cả, date: Từ ngày - Đến ngày, number: Số hóa đơn, series: Mẫu số - Ký hiệu
                string type = data["type"].ToString();
                string soHd = "";
                DateTime now = DateTime.Now;
                int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                string a = $"{now.Year}-{now.Month}-{daysInMonth}";
                var tuNgay = data.ContainsKey("tu_ngay") ? data["tu_ngay"].ToString() : $"{now.Year}-{now.Month}-1";
                var denNgay = data.ContainsKey("den_ngay") ? data["den_ngay"].ToString() : a;
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
                parameters.Add("@ma_dt", maDt);
                string sqlBuilder = $"SELECT * FROM dbo.inv_InvoiceAuth WHERE trang_thai_hd != 13 AND ma_dt = @ma_dt AND inv_InvoiceAuth_id IN (SELECT inv_InvoiceAuth_id FROM InvoiceXmlData) ";
                string sql;
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
                        parameters.Add("@inv_invoiceNumber", soHd);
                        sql = $"{sqlBuilder} AND inv_invoiceNumber = @inv_invoiceNumber ";
                        break;
                    }
                    case "series":
                    {
                        string mauSo = data.ContainsKey("mau_so") ? data["mau_so"].ToString() : "";
                        string kyHieu = data.ContainsKey("ky_hieu") ? data["ky_hieu"].ToString() : "";
                        if (string.IsNullOrEmpty(mauSo) || string.IsNullOrEmpty(kyHieu))
                        {
                            result.Add("status_code", 400);
                            result.Add("error", "Vui lòng nhập mẫu số, ký hiệu");
                            return result;
                        }

                        parameters.Add("@mau_hd", mauSo.Trim());
                        parameters.Add("@inv_invoiceSeries", kyHieu.Trim());

                        sql = $"{sqlBuilder} AND mau_hd = @mau_hd AND inv_invoiceSeries = @inv_invoiceSeries ";

                        string invoiceType = data.ContainsKey("invoice_type") ? data["invoice_type"].ToString() : "";
                        if (!string.IsNullOrEmpty(invoiceType))
                        {
                            parameters.Add("@inv_invoiceType", invoiceType);
                            sql += $"AND inv_invoiceType = @inv_invoiceType ";
                        }

                        break;
                    }
                    case "id":
                    {
                        string id = data.ContainsKey("id") ? data["id"].ToString() : "";
                        if (string.IsNullOrEmpty(id))
                        {
                            result.Add("status_code", 400);
                            result.Add("error", "Vui lòng nhập id");
                            return result;
                        }
                        parameters.Add("@inv_InvoiceAuth_id", id);
                        sql = $"SELECT * FROM dbo.inv_InvoiceAuth WHERE inv_InvoiceAuth_id = @inv_InvoiceAuth_id ";
                        break;
                    }
                    default:
                    {
                        sql = sqlBuilder;
                        break;
                    }
                }

                sql += " ORDER BY inv_invoiceNumber, inv_invoiceSeries ";
                string connectionString = _nopDbContext2.GetInvoiceDb().Database.Connection.ConnectionString;
                byte[] byt = Encoding.UTF8.GetBytes(connectionString);
                string b = Convert.ToBase64String(byt);
                DataTable table = _nopDbContext2.ExecuteCmd(sql, CommandType.Text, parameters);
                table.Columns.Add("inv_auth_id", typeof(string));
                table.Columns.Add("masothue", typeof(string));
                table.Columns.Add("url_preview", typeof(string));
                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        row.BeginEdit();
                        row["inv_auth_id"] = b;
                        row["masothue"] = mst;
                        row["url_preview"] = $"http://{mst.Trim().Replace("-", "")}.minvoice.com.vn/api/Invoice/Preview?id={row["inv_invoiceAuth_id"].ToString()}";
                        row.EndEdit();
                    }
                    JArray arr = JArray.FromObject(table);
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
            TracuuHDDTContext traCuu = new TracuuHDDTContext();
            inv_user user = traCuu.inv_users.FirstOrDefault(x => x.username.Replace("-", "").Equals(userName.Replace("-", "")) && x.mst.Replace("-", "").Equals(mst.Replace("-", "")));
            _nopDbContext2.SetConnect(mst);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"@ma_dt", userName }
            };
            var sql = $"SELECT TOP 1 * FROM dmdt WHERE ma_dt = @ma_dt";

            DataTable infoTable = _nopDbContext2.ExecuteCmd(sql, CommandType.Text, parameters);
            string tenDoiTuong = "";
            if (infoTable.Rows.Count > 0)
            {
                tenDoiTuong = infoTable.Rows[0]["ten_dt"].ToString();
            }
            bool boolCheck = user != null && !string.IsNullOrEmpty(user.inv_user_id.ToString());
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
            JObject json = new JObject();
            try
            {
                string userName = data["user_name"].ToString();
                string sql = "SELECT * FROM inv_InvoiceAuth";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"@ma_dt", userName }
                };
                string where = $" WHERE trang_thai_hd != 13 AND ma_dt = @ma_dt ";
                string orderBy = " ORDER BY inv_invoiceNumber ";
                if (data.ContainsKey("filter"))
                {
                    JObject filterObject = (JObject)data["filter"];
                    if (filterObject.ContainsKey("trang_thai_hd"))
                    {
                        JToken trangThaiHoaDon = filterObject.ContainsKey("trang_thai_hd") ? filterObject["trang_thai_hd"] : 1;
                        parameters.Add("@trang_thai_hd", trangThaiHoaDon.ToString());
                        where += $" AND trang_thai_hd = @trang_thai_hd ";
                    }
                    if (filterObject.ContainsKey("ngay_hoa_don"))
                    {
                        JObject ngayHoaDonObject = (JObject)filterObject["ngay_hoa_don"];
                        string tuNgay = ngayHoaDonObject.ContainsKey("tu_ngay") ? ngayHoaDonObject["tu_ngay"].ToString() : "";
                        string denNgay = ngayHoaDonObject.ContainsKey("den_ngay") ? ngayHoaDonObject["den_ngay"].ToString() : "";
                        DateTime now = DateTime.Now;
                        int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                        string dayNow = $"{now.Year}-{now.Month}-{daysInMonth}";
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
                        JObject soHoaDonObject = (JObject)filterObject["so_hoa_don"];
                        JToken tuSo = soHoaDonObject.ContainsKey("tu_so") ? soHoaDonObject["tu_so"] : 0;
                        JToken denSo = soHoaDonObject.ContainsKey("den_so") ? soHoaDonObject["den_so"] : 1;
                        where += $" AND  CONVERT(INT, inv_invoiceNumber) BETWEEN {tuSo} AND {denSo}";
                    }
                    if (filterObject.ContainsKey("gia_tri_hoa_don"))
                    {
                        JObject soHoaDonObject = (JObject)filterObject["gia_tri_hoa_don"];
                        JToken min = soHoaDonObject.ContainsKey("min") ? soHoaDonObject["min"] : 0;
                        JToken max = soHoaDonObject.ContainsKey("max") ? soHoaDonObject["max"] : 10000;
                        where += $" AND inv_InvoiceAuth_id IN (SELECT inv_InvoiceAuth_id FROM dbo.inv_InvoiceAuthDetail GROUP BY inv_InvoiceAuth_id HAVING (SUM(inv_TotalAmount) >= {min} AND SUM(inv_TotalAmount) <= {max})) ";
                    }
                    if (data.ContainsKey("paging"))
                    {
                        JObject pagingObject = (JObject)data["paging"];
                        int index = pagingObject.ContainsKey("index") ? (int)pagingObject["index"] : 1;
                        int count = pagingObject.ContainsKey("count") ? (int)pagingObject["count"] : 50;
                        int start = index <= 1 ? 0 : (index - 1) * count;
                        var paging = $" OFFSET {start} ROWS FETCH NEXT {count} ROW ONLY ";
                        string sqlBuilder = $"{sql} {where} {orderBy} {paging}";
                        string mst = data["mst"].ToString();
                        _nopDbContext2.SetConnect(mst);
                        DataTable table = _nopDbContext2.ExecuteCmd(sqlBuilder, CommandType.Text, parameters);
                        if (table.Rows.Count > 0)
                        {
                            table.Columns.Add("masothue", typeof(string));
                            table.Columns.Add("url_preview", typeof(string));
                            foreach (DataRow row in table.Rows)
                            {
                                row.BeginEdit();
                                row["masothue"] = mst;
                                row["url_preview"] = $"http://{mst.Trim().Replace("-", "")}.minvoice.com.vn/api/Invoice/Preview?id={row["inv_invoiceAuth_id"].ToString()}";
                                row.EndEdit();
                            }
                            JArray arr = JArray.FromObject(table);
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
                string value = data.ContainsKey("value") ? data["value"].ToString() : "";
                if (string.IsNullOrEmpty(value))
                {
                    json.Add("status_code", 400);
                    json.Add("error", "Vui lòng nhập giá trị");
                    return json;
                }
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"@ma_loai", value }
                };
                var sql = value.Equals("all") ? "SELECT DISTINCT ma_loai, ten_loai FROM dbo.ctthongbao  " : $"SELECT ctthongbao_id, ma_loai, ten_loai, mau_so, ky_hieu FROM dbo.ctthongbao WHERE ma_loai = @ma_loai ";
                string mst = data["mst"].ToString();
                _nopDbContext2.SetConnect(mst);
                DataTable table = _nopDbContext2.ExecuteCmd(sql, CommandType.Text, parameters);
                if (table.Rows.Count > 0)
                {
                    JArray arr = JArray.FromObject(table);
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
            JObject json = new JObject();
            try
            {
                string userName = data["user_name"].ToString();
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"@ma_dt", userName }
                };
                string sqlSelect = "SELECT * FROM dbo.inv_InvoiceAuth ";
                string where = $" WHERE trang_thai_hd != 13 AND ma_dt = @ma_dt ";
                string orderBy = " ORDER BY inv_invoiceNumber ";
                int count = 50;
                int start = 0;
                string pagination = $" OFFSET {start} ROWS FETCH NEXT {count} ROW ONLY ";
                string mst = data.ContainsKey("mst") ? data["mst"].ToString() : "";

                if (string.IsNullOrEmpty(mst))
                {
                    json.Add("status_code", 400);
                    json.Add("error", "Vui lòng nhập mã số thuế");
                    return json;
                }
                string value = data.ContainsKey("value") ? data["value"].ToString() : "";
                if (string.IsNullOrEmpty(value))
                {
                    json.Add("status_code", 400);
                    json.Add("error", "Vui lòng nhập giá trị tìm kiếm");
                    return json;
                }
                _nopDbContext2.SetConnect(mst);
                DataTable tableColumn = _nopDbContext2.GetAllColumnsOfTable("inv_InvoiceAuth");
                if (tableColumn.Rows.Count > 0)
                {
                    where += " AND ( ";
                    int i = 0;
                    foreach (DataRow row in tableColumn.Rows)
                    {
                        string dataType = !string.IsNullOrEmpty(row["DATA_TYPE"].ToString())
                            ? row["DATA_TYPE"].ToString()
                            : "string";
                        string columnName = row["COLUMN_NAME"].ToString();
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
                    JObject pagingObject = (JObject)data["paging"];
                    var index = pagingObject.ContainsKey("index") ? (int)pagingObject["index"] : 1;
                    count = pagingObject.ContainsKey("count") ? (int)pagingObject["count"] : 50;
                    start = index <= 1 ? 0 : (index - 1) * count;
                    pagination = $" OFFSET {start} ROWS FETCH NEXT {count} ROW ONLY ";
                }
                string sqlBuilder = $"{sqlSelect} {where} {orderBy} {pagination}";
                DataTable table = _nopDbContext2.ExecuteCmd(sqlBuilder, CommandType.Text, parameters);
                if (table.Rows.Count > 0)
                {
                    table.Columns.Add("masothue", typeof(string));
                    table.Columns.Add("url_preview", typeof(string));
                    foreach (DataRow row in table.Rows)
                    {
                        row.BeginEdit();
                        row["masothue"] = mst;
                        row["url_preview"] = $"http://{mst.Trim().Replace("-", "")}.minvoice.com.vn/api/Invoice/Preview?id={row["inv_invoiceAuth_id"].ToString()}";
                        row.EndEdit();
                    }
                    JArray arr = JArray.FromObject(table);
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

        public JObject ShowCert(string idShow, string xml)
        {
            JObject result = new JObject();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlNodeList nodeList = doc.GetElementsByTagName("Signature");
                foreach (XmlElement element in nodeList)
                {
                    string id = element.Attributes["Id"].Value;
                    if (id == idShow)
                    {
                        SignedXml signedXml = new SignedXml(doc);
                        signedXml.LoadXml(element);
                        KeyInfoX509Data x509data = null;
                        IEnumerator enums = signedXml.Signature.KeyInfo.GetEnumerator();
                        while (enums.MoveNext())
                        {
                            if (enums.Current is KeyInfoX509Data)
                            {
                                x509data = (KeyInfoX509Data)enums.Current;
                            }
                        }
                        X509Certificate2 cert = x509data.Certificates[0] as X509Certificate2;
                        X509Certificate2UI.DisplayCertificate(cert);
                    }
                }
                result.Add("callback", "ShowCert");
                return result;
            }
            catch (Exception ex)
            {
                result.Add("error", ex.Message);
                return result;
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

        private byte[] PrintInvoice(string sobaomat, string masothue, string folder, string type, bool inchuyendoi, out string xml, out string fileNamePrint)
        {
            _nopDbContext2.SetConnect(masothue);
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
                var sql = "SELECT TOP 1 a.* FROM inv_InvoiceAuth AS a INNER JOIN dbo.InvoiceXmlData AS b ON b.inv_InvoiceAuth_id = a.inv_InvoiceAuth_id WHERE a.sobaomat = @sobaomat";
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
                string maDvcs = "VP";
                if (tblInvInvoiceAuth.Columns.Contains("ma_dvcs"))
                {
                    maDvcs = tblInvInvoiceAuth.Rows[0]["ma_dvcs"].ToString();
                }

                fileNamePrint = $"{masothue}_invoice_{mauHd.Trim().Replace("/", "")}_{soSerial.Trim().Replace("/", "")}_{soHd}";
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
                            report.Parameters["LINK_TRACUU"].Value = $"http://{masothue.Trim().Replace("-", "")}.minvoice.com.vn/api/Invoice/Preview?id={invInvoiceAuthId}";
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
                    string fileName = folder + $@"\{masothue}_BienBanXoaBo.repx";
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

                if (trangThaiHd == 19 || trangThaiHd == 21 || trangThaiHd == 5)
                {
                    string reportFile = trangThaiHd == 5 ? "INCT_BBDC_DD.repx" : "INCT_BBDC_GT.repx";
                    string sqlDieuChinh = trangThaiHd == 5 ? "sproc_inct_hd_dieuchinhdg" : "sproc_inct_hd_dieuchinhgt";
                    string fileName = folder + $@"\{masothue}_{reportFile}";

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
                        {"ma_dvcs", maDvcs},
                        {"document_id", invInvoiceAuthId }
                    };

                    DataSet dsDieuChinh = _nopDbContext2.GetDataSet(sqlDieuChinh, pars);

                    rpBienBan.DataSource = dsDieuChinh;
                    rpBienBan.DataMember = dsDieuChinh.Tables[0].TableName;
                    rpBienBan.CreateDocument();
                    rpBienBan.PrintingSystem.ContinuousPageNumbering = false;
                    report.PrintingSystem.ContinuousPageNumbering = false;
                    report.Pages.AddRange(rpBienBan.Pages);

                    int pageCount = report.Pages.Count;
                    report.Pages[pageCount - 1].AssignWatermark(new PageWatermark());

                }

                if (trangThaiHd == 3)
                {
                    string reportFileThayThe = "INCT_BBTT.repx";
                    string sqlThayThe = "sproc_inct_hd_thaythe";
                    string fileName = folder + $@"\{masothue}_{reportFileThayThe}";

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
                        {"ma_dvcs", maDvcs},
                        {"document_id", invInvoiceAuthId }
                    };

                    DataSet dsThayThe = _nopDbContext2.GetDataSet(sqlThayThe, pars);

                    rpBienBanThayThe.DataSource = dsThayThe;
                    rpBienBanThayThe.DataMember = dsThayThe.Tables[0].TableName;
                    rpBienBanThayThe.CreateDocument();
                    rpBienBanThayThe.PrintingSystem.ContinuousPageNumbering = false;
                    report.PrintingSystem.ContinuousPageNumbering = false;
                    report.Pages.AddRange(rpBienBanThayThe.Pages);

                    int pageCount = report.Pages.Count;
                    report.Pages[pageCount - 1].AssignWatermark(new PageWatermark());

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

        public async Task<JObject> GetThongBaoPH(JObject model)
        {

            var rs = new JObject();
            var urlGet = $@"http://admin.minvoice.vn/api/dmkh/getthongbaophathanhthue?model=";

            var mst = model.ContainsKey("mst") ? model["mst"].ToString() : null;
            
            var token = EncodeXml.Encrypt($"{mst}{DateTime.Now:yyyy-MM-dd}", CommonConstants.KeyMaHoa);
            var UriGet = urlGet + mst;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bear", token);
            HttpResponseMessage result = await client.GetAsync(UriGet);
            string response = await result.Content.ReadAsStringAsync();


            var responseJObject = JObject.Parse(response);
            if (responseJObject.ContainsKey("ok"))
            {
                var data = JArray.Parse(responseJObject["data"].ToString());
                if (data.Any())
                {
                    var mauSo = model.ContainsKey("mau_so") ? model["mau_so"].ToString().Trim() : "";
                    var kyHieu = model.ContainsKey("ky_hieu") ? model["ky_hieu"].ToString().Trim() : "";
                    var filter = data.Where(x => x["formNo"].ToString().Equals(mauSo) && x["symbol"].ToString().Contains(kyHieu)).ToList();
                    if (filter.Any())
                    {
                        rs.Add("ok", JArray.FromObject(filter));
                        return rs;
                    }
                    rs.Add("error", "Không tìm thấy");
                }
                rs.Add("error", "Không tìm thấy");

            }
            else
            {
                rs.Add("error", responseJObject["error"].ToString());
            }
            return rs;

          

            throw new NotImplementedException();
        }
    }
}
