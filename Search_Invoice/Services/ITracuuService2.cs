using Newtonsoft.Json.Linq;

namespace Search_Invoice.Services
{
    public interface ITracuuService2
    {
        JObject GetInvoiceFromdateTodate(JObject model);
        JObject GetInfoInvoice(JObject model);
        byte[] PrintInvoiceFromSbm(string sobaomat, string masothue, string folder, string type);
        byte[] PrintInvoiceFromSbm(string authId, string sobaomat, string masothue, string folder, string type);
        byte[] PrintInvoiceFromSbm(string sobaomat, string masothue, string folder, string type, bool inchuyendoi);
        byte[] PrintInvoiceFromSbm(string authId, string sobaomat, string masothue, string folder, string type, bool inchuyendoi);
        byte[] PrintInvoiceFromSbm(string sobaomat, string masothue, string folder, string type, out string xml, out string fileName);
        byte[] PrintInvoiceFromSbm(string authId, string sobaomat, string masothue, string folder, string type, out string xml, out string fileName);
        byte[] ExportZipFileXml(string sobaomat, string masothue, string pathReport, ref string fileName, ref string key);
        byte[] ExportZipFileXml(string authId, string sobaomat, string masothue, string pathReport, ref string fileName, ref string key);
        byte[] GetInvoiceXml(string soBaoMat, string maSoThue);
        JObject GetHtml(JObject model);
        JObject Search_Tax(string mst);
        JObject GetInfoLogin(string userName, string mst);
        JObject ShowCert(string id, string xml);
    }
}