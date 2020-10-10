using Search_Invoice.Data;
using Search_Invoice.Data.Domain;
using Search_Invoice.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Search_Invoice.Services
{
    public class NopDbContext : INopDbContext
    {
        private readonly IWebHelper _webHelper;
        private InvoiceDbContext _invoiceDbContext;

        public NopDbContext(IWebHelper webHelper)
        {
            _webHelper = webHelper;
            if (System.Configuration.ConfigurationManager.ConnectionStrings["InvoiceConnection"] != null)
            {
                string invoiceConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InvoiceConnection"].ConnectionString;
                _invoiceDbContext = new InvoiceDbContext(invoiceConnectionString);
            }
            else
            {
                string host = _webHelper.GetRequest().Url.AbsoluteUri;
                string[] paths = host.Split('=');
                string mst = paths[1].Substring(0, paths[1].Length - 3).Replace("-", "");
                TracuuHDDTContext tracuuDb = new TracuuHDDTContext();
                if (mst == "localhost")
                {
                    mst = "0102236276";
                }
                inv_admin invAdmin = tracuuDb.Inv_admin.FirstOrDefault(c => c.MST.Replace("-", "") == mst || c.alias == mst);
                if (invAdmin == null)
                {
                    throw new Exception("Không tồn tại mã số thuế " + mst + " trên hệ thống của M-Invoice");
                }
                else
                {
                    _invoiceDbContext = invAdmin.ConnectString.StartsWith("Data Source") ? new InvoiceDbContext(invAdmin.ConnectString) : new InvoiceDbContext(EncodeXml.Decrypt(invAdmin.ConnectString, "NAMPV18081202"));
                }
            }
        }

        /// <summary>
        /// đang đợi set connect  mai làm tiếp 
        /// </summary>
        /// <param name="mst"></param>
        public void SetConnect(string mst)
        {
            TracuuHDDTContext tracuu = new TracuuHDDTContext();
            inv_admin invAdmin = tracuu.Inv_admin.FirstOrDefault(c => c.MST == mst || c.alias == mst);
            if (invAdmin == null)
            {
                throw new Exception("Không tồn tại " + mst + " trên hệ thống của M-Invoice !");
            }
            else
            {
                _invoiceDbContext = invAdmin.ConnectString.StartsWith("Data Source") ? new InvoiceDbContext(invAdmin.ConnectString) : new InvoiceDbContext(EncodeXml.Decrypt(invAdmin.ConnectString, "NAMPV18081202"));
            }
        }
        public InvoiceDbContext GetInvoiceDb()
        {
            return _invoiceDbContext;
        }

        public DataTable GetStoreProcedureParameters(string storeProcedure)
        {
            DataTable tblParameters = ExecuteCmd("SELECT p.*,t.[name] AS [type] FROM sys.procedures sp " +
                                    "JOIN sys.parameters p  ON sp.object_id = p.object_id " +
                                    "JOIN sys.types t  ON p.user_type_id = t.user_type_id " +
                                    "WHERE sp.name = '" + storeProcedure + "' and t.name<>'sysname'");
            return tblParameters;
        }

        public string ExecuteStoreProcedure(string sql, Dictionary<string, string> parameters)
        {
            DbConnection connection = null;
            try
            {
                InvoiceDbContext invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
                DbCommand command = connection.CreateCommand();
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = sql;
                DataTable tblParameters = ExecuteCmd("SELECT p.*,t.[name] AS [Type] FROM sys.procedures sp " +
                                    "JOIN sys.parameters p  ON sp.object_id = p.object_id " +
                                    "JOIN sys.types t  ON p.user_type_id = t.user_type_id " +
                                    "WHERE sp.name = '" + sql + "' and t.name<>'sysname'");
                for (int i = 0; i < tblParameters.Rows.Count; i++)
                {
                    DataRow row = tblParameters.Rows[i];
                    KeyValuePair<string, string> para = parameters.Where(c => c.Key == row["name"].ToString().Substring(1)).FirstOrDefault();
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = row["name"].ToString();
                    parameter.Value = para.Value;
                    command.Parameters.Add(parameter);
                }
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (connection?.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return null;
        }

        public DataSet GetDataSet(string sql, Dictionary<string, string> parameters)
        {
            DbConnection connection = null;
            DataSet ds = new DataSet
            {
                DataSetName = "dataSet1"
            };
            try
            {
                InvoiceDbContext invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
                DbCommand command = connection.CreateCommand();
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = sql;
                DataTable tblParameters = ExecuteCmd("SELECT p.*,t.[name] AS [Type] FROM sys.procedures sp " +
                                    "JOIN sys.parameters p  ON sp.object_id = p.object_id " +
                                    "JOIN sys.types t  ON p.user_type_id = t.user_type_id " +
                                    "WHERE sp.name = '" + sql + "' and t.name<>'sysname'");
                for (int i = 0; i < tblParameters.Rows.Count; i++)
                {
                    DataRow row = tblParameters.Rows[i];
                    KeyValuePair<string, string> para = parameters.FirstOrDefault(c => c.Key == row["name"].ToString().Substring(1));
                    DbParameter parameter = command.CreateParameter();
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
                DbDataReader reader = command.ExecuteReader();
                DataTable table = new DataTable
                {
                    TableName = "Table"
                };
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

        public DataTable ExecuteCmd(string sql)
        {
            DbConnection connection = null;
            DataTable table = new DataTable();
            try
            {
                InvoiceDbContext invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
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

        public async Task<DataTable> ExecuteCmdAsync(string sql)
        {
            DbConnection connection = null;
            DataTable table = new DataTable();
            try
            {
                InvoiceDbContext invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
                DbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                if (connection.State == ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }
                DbDataReader reader = command.ExecuteReader();
                do
                {
                    await Task.Run(() => { table.Load(reader); });

                } while (!reader.IsClosed);
                return table;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (connection?.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }


        public async Task<string> ExecuteStoreProcedureAsync(string sql, Dictionary<string, object> parameters)
        {
            DbConnection connection = null;
            try
            {
                InvoiceDbContext invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
                DbCommand command = connection.CreateCommand();
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = sql;
                DataTable tblParameters = await ExecuteCmdAsync("SELECT p.*,t.[name] AS [Type] FROM sys.procedures sp " +
                                    "JOIN sys.parameters p  ON sp.object_id = p.object_id " +
                                    "JOIN sys.types t  ON p.user_type_id = t.user_type_id " +
                                    "WHERE sp.name = '" + sql + "' and t.name<>'sysname'");
                for (int i = 0; i < tblParameters.Rows.Count; i++)
                {
                    DataRow row = tblParameters.Rows[i];
                    KeyValuePair<string, object> para = parameters.FirstOrDefault(c => c.Key == row["name"].ToString().Substring(1));
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = row["name"].ToString();
                    parameter.Value = para.Value;
                    command.Parameters.Add(parameter);
                }

                if (connection.State == ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }
                await command.ExecuteNonQueryAsync();
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
            return null;
        }

        public void ExecuteNoneQuery(string sql)
        {
            DbConnection connection = null;
            try
            {
                InvoiceDbContext invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
                DbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                command.ExecuteNonQuery();
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
        }

        public void ExecuteNoneQuery(string sql, Dictionary<string, object> parameters)
        {
            DbConnection connection = null;
            try
            {
                InvoiceDbContext invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
                DbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> entry in parameters)
                    {
                        DbParameter parameter = command.CreateParameter();
                        parameter.ParameterName = entry.Key;
                        parameter.Value = entry.Value;
                        command.Parameters.Add(parameter);
                    }
                }
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                command.ExecuteNonQuery();
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
        }

        public async Task<string> ExecuteNoneQueryAsync(string sql, CommandType commandType, Dictionary<string, object> parameters)
        {
            DbConnection connection = null;
            try
            {
                InvoiceDbContext invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
                DbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = commandType;
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> entry in parameters)
                    {
                        DbParameter parameter = command.CreateParameter();
                        parameter.ParameterName = entry.Key;
                        parameter.Value = entry.Value;
                        command.Parameters.Add(parameter);
                    }
                }
                if (connection.State == ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }
                await command.ExecuteNonQueryAsync();
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
            return "";
        }


        public DataTable ExecuteCmd(string sql, CommandType commandType, Dictionary<string, object> parameters)
        {
            DbConnection connection = null;
            DataTable table = new DataTable();
            try
            {
                InvoiceDbContext invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
                DbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = commandType;
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> entry in parameters)
                    {
                        DbParameter parameter = command.CreateParameter();
                        parameter.ParameterName = entry.Key;
                        parameter.Value = entry.Value;
                        command.Parameters.Add(parameter);
                    }
                }
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
    }
}