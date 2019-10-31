using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search_Invoice.Services
{
    public partial interface ITracuuService2
    {
        JObject GetInvoiceFromdateTodate(JObject model);
        JObject GetInfoInvoice(JObject model);
        byte[] PrintInvoiceFromSBM(string sobaomat, string masothue, string folder, string type);
        byte[] PrintInvoiceFromSBM(string sobaomat, string masothue, string folder, string type, bool inchuyendoi);
        byte[] PrintInvoiceFromSBM(string sobaomat, string masothue, string folder, string type, out string xml);
        byte[] ExportZipFileXML(string sobaomat, string masothue, string pathReport, ref string fileName, ref string key);

        JObject GetHtml(JObject model);


        JObject Search_Tax(string mst);

        JObject SearchInvoice(JObject data);

        JObject GetInfoLogin(string userName, string mst);
    }
}