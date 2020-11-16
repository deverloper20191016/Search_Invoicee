using Newtonsoft.Json.Linq;

namespace Search_Invoice.Services
{
    public interface ITracuuService2
    {
       
        JObject GetInfoInvoice(JObject model);
        byte[] PrintInvoiceFromSbm(string sobaomat,  string folder, string type);
        byte[] PrintInvoiceFromSbm(string sobaomat,  string folder, string type, bool inchuyendoi);
        byte[] PrintInvoiceFromSbm(string sobaomat,  string folder, string type, bool inchuyendoi,  out string xml, out string fileName);
        byte[] ExportZipFileXml(string sobaomat, string pathReport, ref string fileName, ref string key);
        byte[] GetInvoiceXml(string soBaoMat);
        JObject SearchDataByDate(string tuNgay, string denNgay, string maDt);
    }
}