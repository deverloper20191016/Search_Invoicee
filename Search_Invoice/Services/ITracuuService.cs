namespace Search_Invoice.Services
{
    public interface ITracuuService
    {
        byte[] PrintInvoiceFromSbm(string id, string folder, string type);
        byte[] PrintInvoiceFromSbm(string id, string mst, string folder, string type);
        byte[] PrintInvoiceFromSbm(string id,  string mst, string folder, string type, bool inchuyendoi);
    }
}