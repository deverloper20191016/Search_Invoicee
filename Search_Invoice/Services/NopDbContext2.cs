using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Search_Invoice.Data;
using System.Threading.Tasks;
using System.Data.Common;
using Search_Invoice.Util;

namespace Search_Invoice.Services
{
    public class NopDbContext2 : INopDbContext2
    {
        private InvoiceDbContext _invoiceDbContext;

        public void SetConnect()
        {
            _invoiceDbContext = new InvoiceDbContext();
        }
        /// <summary>
        /// đang đợi set connect  mai làm tiếp 
        /// </summary>
        /// <param name="mst"></param>
        public void SetConnect(string mst)
        {
            mst = mst.Replace("-", "");
            TracuuHDDTContext tracuu = new TracuuHDDTContext();
            var invAdmin = tracuu.Inv_admin.FirstOrDefault(c => c.MST.Replace("-", "") == mst || c.alias == mst);
            if (invAdmin == null)
            {
                throw new Exception("Không tồn tại " + mst + " trên hệ thống của M-Invoice !");
            }
            else
            {
                _invoiceDbContext = invAdmin.ConnectString.StartsWith("Data Source") ? new InvoiceDbContext(invAdmin.ConnectString) : new InvoiceDbContext(EncodeXml.Decrypt(invAdmin.ConnectString, "NAMPV18081202"));
            }
        }
        public DataTable GetAllColumnsOfTable(string tableName)
        {
            var data = ExecuteCmd($"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{tableName}'");
            return data;
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
                var invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
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
            DataSet ds = new DataSet();
            ds.DataSetName = "dataSet1";
            try
            {
                var invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
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
                DataTable table = new DataTable {TableName = "Table"};
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
            var table = new DataTable();
            try
            {
                var invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
                var command = connection.CreateCommand();
                command.CommandText = sql;
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                var reader = command.ExecuteReader();
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
            var table = new DataTable();
            try
            {
                var invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
                var command = connection.CreateCommand();
                command.CommandText = sql;
                if (connection.State == ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }
                var reader = command.ExecuteReader();
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
                var invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = sql;
                DataTable tblParameters = await ExecuteCmdAsync("SELECT p.*,t.[name] AS [Type] FROM sys.procedures sp " +
                                    "JOIN sys.parameters p  ON sp.object_id = p.object_id " +
                                    "JOIN sys.types t  ON p.user_type_id = t.user_type_id " +
                                    "WHERE sp.name = '" + sql + "' and t.name<>'sysname'");
                for (int i = 0; i < tblParameters.Rows.Count; i++)
                {
                    DataRow row = tblParameters.Rows[i];
                    var para = parameters.FirstOrDefault(c => c.Key == row["name"].ToString().Substring(1));
                    var parameter = command.CreateParameter();
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
                var invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
                var command = connection.CreateCommand();
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
                var invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
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
                var invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = commandType;
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
            var table = new DataTable();
            try
            {
                var invoiceDb = _invoiceDbContext;
                connection = invoiceDb.Database.Connection;
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = commandType;
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
                var reader = command.ExecuteReader();
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