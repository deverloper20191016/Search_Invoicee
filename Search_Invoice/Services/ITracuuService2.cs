using Newtonsoft.Json.Linq;

namespace Search_Invoice.Services
{
    public interface ITracuuService2
    {
        JObject GetInvoiceFromdateTodate(JObject model);
        JObject GetInfoInvoice(JObject model);
        byte[] PrintInvoiceFromSbm(string sobaomat, string masothue, string folder, string type);
        byte[] PrintInvoiceFromSbm(string sobaomat, string masothue, string folder, string type, bool inchuyendoi);
        byte[] PrintInvoiceFromSbm(string sobaomat, string masothue, string folder, string type, bool inchuyendoi,  out string xml, out string fileName);
        byte[] ExportZipFileXml(string sobaomat, string masothue, string pathReport, ref string fileName, ref string key);
        byte[] GetInvoiceXml(string soBaoMat, string maSoThue);
        JObject GetHtml(JObject model);
        JObject Search_Tax(string mst);
        JObject SearchInvoice(JObject data);
        JObject GetInfoLogin(string userName, string mst);
        JObject GetListInvoice(JObject data);
        JObject GetListInvoiceType(JObject data);
        JObject Search(JObject data);
        JObject ShowCert(string id, string xml);
    }
}