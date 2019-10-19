using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search_Invoice.Services
{
    public partial interface ITracuuService
    {
        byte[] PrintInvoiceFromSBM(string id, string folder, string type);
        byte[] PrintInvoiceFromSBM(string id, string folder, string type, bool inchuyendoi);
    }
}