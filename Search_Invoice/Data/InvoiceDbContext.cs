using Search_Invoice.Data.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;

namespace Search_Invoice.Data
{
    public class InvoiceDbContext : DbContext
    {
        public InvoiceDbContext()
         : base("DefaultConnection")
        {
            //this.Configuration.LazyLoadingEnabled = false;
            //this.Configuration.ProxyCreationEnabled = false;
        }

        public InvoiceDbContext(string connectionString)
            : base(connectionString)
        {
            //this.Configuration.LazyLoadingEnabled = false;
            //this.Configuration.ProxyCreationEnabled = false;
        }
        public DataTable ExecuteCmd(string sql)
        {
            DbConnection connection = null;
            DataTable table = new DataTable();
            try
            {
                //var invoiceDb = this.Database.Connection;
                connection = Database.Connection;
                DbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                DbDataReader reader = command.ExecuteReader();
                do
                {
                    table.Load(reader);
                } while (!reader.IsClosed);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (connection?.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return table;
        }

        public DataSet GetDataSet(string sql, Dictionary<string, string> parameters)
        {
            DbConnection connection = null;
            DataSet ds = new DataSet();
            ds.DataSetName = "dataSet1";
            try
            {
                connection = Database.Connection;
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = sql;
                DataTable tblParameters = ExecuteCmd("SELECT p.*,t.[name] AS [Type] FROM sys.procedures sp " +
                                    "JOIN sys.parameters p  ON sp.object_id = p.object_id " +
                                    "JOIN sys.types t  ON p.user_type_id = t.user_type_id " +
                                    "WHERE sp.name = '" + sql + "' and t.name<>'sysname'");
                for (int i = 0; i < tblParameters.Rows.Count; i++)
                {
                    DataRow row = tblParameters.Rows[i];
                    var para = parameters.FirstOrDefault(c => c.Key == row["name"].ToString().Substring(1));
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = row["name"].ToString();
                    if (para.Value == null)
                    {
                        parameter.Value = DBNull.Value;
                    }
                    else
                    {
                        parameter.Value = para.Value;
                    }
                    command.Parameters.Add(parameter);
                }
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                var reader = command.ExecuteReader();
                DataTable table = new DataTable { TableName = "Table" };
                do
                {
                    table.Load(reader);
                } while (!reader.IsClosed);
                ds.Tables.Add(table);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (connection?.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return ds;
        }
        public DbSet<wb_window> WbWindows { get; set; }
        public DbSet<wb_user> WbUsers { get; set; }
        public DbSet<wb_menu> WbMenus { get; set; }
        public DbSet<wb_tab> WbTabs { get; set; }
        public DbSet<wb_field> WbFields { get; set; }
        public DbSet<systemsetting> Systemsettings { get; set; }
        public DbSet<dmdvcs> Dmdvcss { get; set; }
        public DbSet<dvpermission> Dvpermissions { get; set; }
        public DbSet<wb_ctquyen> WbCtquyens { get; set; }
        public DbSet<wb_log> WbLogs { get; set; }
        public DbSet<wb_log_sms> wb_log_smss { get; set; }
        public DbSet<Inv_InvoiceAuth> Inv_InvoiceAuths { get; set; }
    }
}