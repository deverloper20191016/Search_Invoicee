using Search_Invoice.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Data;

namespace Search_Invoice.Services
{
    public partial interface INopDbContext
    {
        InvoiceDbContext GetInvoiceDb();
        string ExecuteStoreProcedure(string sql, Dictionary<string, string> parameters);
        DataTable ExecuteCmd(string sql);
        DataTable ExecuteCmd(string sql, CommandType commandType, Dictionary<string, object> parameters);
        Task<DataTable> ExecuteCmdAsync(string sql);
        Task<string> ExecuteStoreProcedureAsync(string sql, Dictionary<string, object> parameters);
        DataTable GetStoreProcedureParameters(string storeProcedure);
        DataSet GetDataSet(string sql, Dictionary<string, string> parameters);

        void ExecuteNoneQuery(string sql);
        void ExecuteNoneQuery(string sql, Dictionary<string, object> parameters);
        Task<string> ExecuteNoneQueryAsync(string sql, CommandType commandType, Dictionary<string, object> parameters);
        void setConnect(string mst);
    }
}