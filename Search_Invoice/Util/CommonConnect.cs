using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Common;
using Search_Invoice.Data;
using System.Threading.Tasks;

namespace Search_Invoice.Util
{
    public class CommonConnect
    {
        private InvoiceDbContext invoiceDbContext;
        public void setConnect(string mst)
        {
            mst = mst.Replace("-", "");
            TracuuHDDTContext tracuu = new TracuuHDDTContext();
            //tracuu.Inv_admin.Where
            var inv_admin = tracuu.Inv_admin.Where(c => c.MST.Replace("-", "") == mst).FirstOrDefault();

            if (inv_admin == null)
            {
                throw new Exception("Không tồn tại " + mst + " trên hệ thống của M-Invoice !");
            }
            else
            {
                if (inv_admin.ConnectString.StartsWith("Data Source"))
                {
                    invoiceDbContext = new InvoiceDbContext(inv_admin.ConnectString);
                }
                else
                {
                    invoiceDbContext = new InvoiceDbContext(EncodeXML.Decrypt(inv_admin.ConnectString, "NAMPV18081202"));
                }
            }
        }
        public InvoiceDbContext GetInvoiceDb()
        {
            return this.invoiceDbContext;
        }

        public DataTable GetStoreProcedureParameters(string storeProcedure)
        {
            DataTable tblParameters = this.ExecuteCmd("SELECT p.*,t.[name] AS [type] FROM sys.procedures sp " +
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
                var invoiceDb = this.invoiceDbContext;

                connection = invoiceDb.Database.Connection;
                var command = connection.CreateCommand();

                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = sql;

                DataTable tblParameters = this.ExecuteCmd("SELECT p.*,t.[name] AS [Type] FROM sys.procedures sp " +
                                    "JOIN sys.parameters p  ON sp.object_id = p.object_id " +
                                    "JOIN sys.types t  ON p.user_type_id = t.user_type_id " +
                                    "WHERE sp.name = '" + sql + "' and t.name<>'sysname'");

                for (int i = 0; i < tblParameters.Rows.Count; i++)
                {
                    DataRow row = tblParameters.Rows[i];
                    var para = parameters.Where(c => c.Key == row["name"].ToString().Substring(1)).FirstOrDefault();

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
                if (connection.State == ConnectionState.Open)
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
                var invoiceDb = this.invoiceDbContext;

                connection = invoiceDb.Database.Connection;
                var command = connection.CreateCommand();

                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = sql;

                DataTable tblParameters = this.ExecuteCmd("SELECT p.*,t.[name] AS [Type] FROM sys.procedures sp " +
                                    "JOIN sys.parameters p  ON sp.object_id = p.object_id " +
                                    "JOIN sys.types t  ON p.user_type_id = t.user_type_id " +
                                    "WHERE sp.name = '" + sql + "' and t.name<>'sysname'");

                for (int i = 0; i < tblParameters.Rows.Count; i++)
                {
                    DataRow row = tblParameters.Rows[i];
                    var para = parameters.Where(c => c.Key == row["name"].ToString().Substring(1)).FirstOrDefault();

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
                DataTable table = new DataTable();
                table.TableName = "Table";

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
                if (connection.State == ConnectionState.Open)
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
                var invoiceDb = this.invoiceDbContext;

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
                if (connection.State == ConnectionState.Open)
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
                var invoiceDb = this.invoiceDbContext;

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
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
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
                var invoiceDb = this.invoiceDbContext;

                connection = invoiceDb.Database.Connection;
                var command = connection.CreateCommand();

                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = sql;

                DataTable tblParameters = await this.ExecuteCmdAsync("SELECT p.*,t.[name] AS [Type] FROM sys.procedures sp " +
                                    "JOIN sys.parameters p  ON sp.object_id = p.object_id " +
                                    "JOIN sys.types t  ON p.user_type_id = t.user_type_id " +
                                    "WHERE sp.name = '" + sql + "' and t.name<>'sysname'");

                for (int i = 0; i < tblParameters.Rows.Count; i++)
                {
                    DataRow row = tblParameters.Rows[i];
                    var para = parameters.Where(c => c.Key == row["name"].ToString().Substring(1)).FirstOrDefault();

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
                if (connection.State == ConnectionState.Open)
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
                var invoiceDb = this.invoiceDbContext;

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
                if (connection.State == ConnectionState.Open)
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
                var invoiceDb = this.invoiceDbContext;

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
                if (connection.State == ConnectionState.Open)
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
                var invoiceDb = this.invoiceDbContext;

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
                if (connection.State == ConnectionState.Open)
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
                var invoiceDb = this.invoiceDbContext;

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
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return table;
        }
    }
}