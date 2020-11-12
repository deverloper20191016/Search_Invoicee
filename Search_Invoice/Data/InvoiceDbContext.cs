using Search_Invoice.Data.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;

namespace Search_Invoice.Data
{
    public class InvoiceDbContext : DbContext
    {
        public InvoiceDbContext()
         : base("InvoiceDbContext")
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

        public int ExecuteNoneQuery(string sql, Dictionary<string, object> parameters = null)
        {
            DbConnection connection = null;

            try
            {
                connection = this.Database.Connection;
                var command = connection.CreateCommand();

                command.CommandText = sql;

                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> entry in parameters)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = entry.Key;
                        parameter.Value = entry.Value;

                        command.Parameters.Add(parameter);
                    }
                }

                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
                throw new Exception(ex.Message);
            }
            finally
            {
                if (connection?.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
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
        public DbSet<NguoiSuDung> NguoiSuDungs { get; set; }
        public DbSet<QuyenHan> QuyenHans { get; set; }
    }
}