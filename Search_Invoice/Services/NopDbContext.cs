using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using Search_Invoice.Data;
using Search_Invoice.Data.Domain;
using System.Threading.Tasks;
using System.Data.Common;

namespace Search_Invoice.Services
{
    public class NopDbContext : INopDbContext
    {
        private readonly IWebHelper _webHelper;
        private InvoiceDbContext invoiceDbContext;

        public NopDbContext(IWebHelper webHelper)
        {
            this._webHelper = webHelper;

            if (System.Configuration.ConfigurationManager.ConnectionStrings["InvoiceConnection"] != null)
            {
                string invoiceConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InvoiceConnection"].ConnectionString;
                invoiceDbContext = new InvoiceDbContext(invoiceConnectionString);
            }
            else
            {
                //string host = this._webHelper.GetRequest().Url.Host;
                string host = this._webHelper.GetRequest().Url.AbsoluteUri;
                //var host2 = this._webHelper.GetRequest();

                string[] paths = host.Split('=');

                string mst = paths[1].Substring(0, paths[1].Length - 3).Replace("-","");

                TracuuHDDTContext tracuuDB = new TracuuHDDTContext();

                if (mst == "localhost")
                {
                    mst = "0102236276";
                }

                var inv_admin = tracuuDB.Inv_admin.Where(c => c.MST.Replace("-","") == mst || c.alias == mst).FirstOrDefault<inv_admin>();

                if (inv_admin == null)
                {
                    throw new Exception("Không tồn tại mã số thuế " + mst + " trên hệ thống của M-Invoice");
                }
                else
                {
                    invoiceDbContext = new InvoiceDbContext(inv_admin.ConnectString);
                }
            }

        }

        //public string GetCurrentRequest()
        //{
        //    return this._context.Request.Url.OriginalString;
        //}

            /// <summary>
            /// đang đợi set connect  mai làm tiếp 
            /// </summary>
            /// <param name="mst"></param>
        public void setConnect(string mst)
        {
            TracuuHDDTContext tracuu = new TracuuHDDTContext();
            var inv_admin = tracuu.Inv_admin.Where(c => c.MST == mst || c.alias == mst).FirstOrDefault<inv_admin>();

            if(inv_admin == null)
            {
                throw new Exception("Không tồn tại " + mst + " trên hệ thống của M-Invoice !");

            }
            else
            {
                invoiceDbContext = new InvoiceDbContext(inv_admin.ConnectString);
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