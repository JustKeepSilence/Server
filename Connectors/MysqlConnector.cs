using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using Server.Models;

namespace Server.Connectors
{

    public class MysqlConnectors : IDisposable
    {

        private MySqlConnection? Connection { get; set; }

        public MysqlConnectors(IConfiguration configuration, out string msg)
        {

            msg = string.Empty;

            try
            {

                string ip = configuration["MySqlSettings:Ip"].ToString();
                uint port = Convert.ToUInt16(configuration["MySqlSettings:Port"]);
                string userName = configuration["MySqlSettings:UserName"].ToString();
                string passWord = configuration["MySqlSettings:PassWord"].ToString();
                string dbName = configuration["MySqlSettings:DbName"].ToString();
                MySqlConnectionStringBuilder builder = new()
                {
                    Server = ip,
                    Port = port,
                    UserID = userName,
                    Password = passWord,
                    Database = dbName,
                    CharacterSet = "utf8",
                    Pooling = true,
                    MaximumPoolSize = 10000

                };
                Connection = new MySqlConnection(builder.ConnectionString);
                Connection.Open();

            }
            catch (Exception ex) { msg = ex.Message; }


        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <param name="dbName"></param>
        /// <param name="msg"></param>
        /// <param name="maxPoolSize"></param>
        public MysqlConnectors(string ip, uint port, string userName, string passWord, string dbName, out string msg,
        uint maxPoolSize = 100)
        {

            msg = string.Empty;
            MySqlConnectionStringBuilder builder = new()
            {
                Server = ip,
                Port = port,
                UserID = userName,
                Password = passWord,
                Database = dbName,
                CharacterSet = "utf8",
                Pooling = true,
                MaximumPoolSize = maxPoolSize

            };
            try
            {

                Connection = new MySqlConnection(builder.ConnectionString);
                Connection.Open();

            }
            catch (Exception ex)
            {

                msg = ex.Message;

            }

        }

        /// <summary>
        /// Insert Blob data to Mysql using Parameter binding
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="commands"></param>
        /// <param name="values"></param>
        /// <param name="blobIndex">blob column Index</param>
        /// <param name="maxTransactionSize"></param>
        /// <returns></returns>
        public async Task<Response<int>> Insert(List<string> parameters, List<string> commands, List<List<object>> values,
        List<int> blobIndex, int maxTransactionSize = 500)
        {

            try
            {

                var transaction = await Connection.BeginTransactionAsync();
                MySqlCommand sqlCommand = new()
                {
                    Connection = Connection,
                    Transaction = transaction
                };

                int rows = 0;
                for (int i = 0; i < values.Count; i++)
                {
                    for (int j = 0; j < parameters.Count; j++)
                    {
                        if (blobIndex.Contains(j))
                        {
                            // Blob Column
                            MySqlParameter parameter = new(parameters[j], MySqlDbType.LongBlob)
                            {
                                Value = (byte[])values[i][j]
                            };
                            sqlCommand.Parameters.Add(parameter);

                        }
                        else
                        {

                            // common index
                            sqlCommand.Parameters.AddWithValue(parameters[j], values[i][j].ToString());

                        }

                    }

                    sqlCommand.CommandText = commands[i];

                    rows += sqlCommand.ExecuteNonQuery();
                    sqlCommand.Parameters.Clear();
                    if ((i > 0 && (i % maxTransactionSize == 0)) || i == values.Count - 1)
                    {

                        // commit transaction
                        transaction.Commit();
                        transaction = await Connection.BeginTransactionAsync();
                        sqlCommand.Transaction = transaction;

                    }
                }

                transaction.Commit();

                return new Response<int>()
                {
                    Data = rows
                };



            }
            catch (Exception ex)
            {

                return new Response<int>()
                {

                    Message = ex.Message,
                    Code = 500

                };

            }

        }

        /// <summary>
        /// Insert Data to Mysql using Parameter binding
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="values"></param>
        /// <param name="maxTransactionSize"></param>
        /// <returns></returns>
        public async Task<Response<int>> Insert(List<string> parameters, List<string> commands, List<List<string>> values, int maxTransactionSize = 500)
        {

            try
            {

                var transaction = await Connection.BeginTransactionAsync();
                MySqlCommand sqlCommand = new()
                {
                    Connection = Connection,
                    Transaction = transaction
                };

                int rows = 0;
                for (int i = 0; i < values.Count; i++)
                {

                    sqlCommand.CommandText = commands[i];
                    for (int j = 0; j < parameters.Count; j++)
                    {
                        sqlCommand.Parameters.AddWithValue(parameters[j], values[i][j]);
                    }


                    rows += sqlCommand.ExecuteNonQuery();
                    sqlCommand.Parameters.Clear();
                    if ((i > 0 && (i % maxTransactionSize == 0)) || i == values.Count - 1)
                    {

                        // commit transaction
                        transaction.Commit();
                        transaction = await Connection.BeginTransactionAsync();
                        sqlCommand.Transaction = transaction;

                    }

                }

                transaction.Commit();   // Commit transaction

                return new Response<int>()
                {
                    Data = rows
                };

            }
            catch (Exception ex)
            {

                return new Response<int>()
                {

                    Message = ex.Message,
                    Code = 500

                };

            }

        }

        /// <summary>
        /// Insert Data to Mysql
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="maxTransactionSize"></param>
        /// <returns></returns>
        public async Task<Response<int>> Insert(List<string> commands, int maxTransactionSize = 500)
        {

            try
            {

                var transaction = await Connection.BeginTransactionAsync();
                MySqlCommand sqlCommand = new()
                {
                    Connection = Connection,
                    Transaction = transaction
                };

                int rows = 0;
                for (int i = 0; i < commands.Count; i++)
                {

                    sqlCommand.CommandText = commands[i];
                    rows += sqlCommand.ExecuteNonQuery();
                    if ((i > 0 && (i % maxTransactionSize == 0)) || i == commands.Count - 1)
                    {

                        // commit transaction
                        transaction.Commit();
                        transaction = await Connection.BeginTransactionAsync();
                        sqlCommand.Transaction = transaction;

                    }


                }

                transaction.Commit();

                return new Response<int>()
                {
                    Data = rows
                };

            }
            catch (Exception ex)
            {

                return new Response<int>()
                {

                    Message = ex.Message,
                    Code = 500

                };

            }

        }

        /// <summary>
        /// Insert Data to Mysql using MultiInsert
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="colunmNames"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public async Task<Response<int>> Insert(string tableName, List<string> colunmNames, List<List<string>> values)
        {

            try
            {

                // use parameter binding
                string cs = string.Join(',', colunmNames);
                StringBuilder sb = new();
                sb.Append($"insert into {tableName} ({cs}) values");
                List<string> parameterNames = new();
                List<string> temp = new();
                List<string> bindingValues = new();
                for (int i = 0; i < values.Count; i++)
                {
                    temp.Clear();
                    for (int j = 0; j < values[i].Count; j++)
                    {
                        temp.Add($"@{i}{j}Value");
                        bindingValues.Add(values[i][j]);
                    }
                    parameterNames.AddRange(temp);
                    sb.Append("(").Append(string.Join(',', temp)).Append("),");
                }
                string command = sb.ToString().TrimEnd(',');
                var result = await Insert(parameterNames, new List<string>() { command }, new List<List<string>>() { bindingValues });
                return result;
            }
            catch (Exception ex)
            {

                return new Response<int>()
                {

                    Message = ex.Message,
                    Code = 500

                };

            }

        }

        /// <summary>
        /// Query Bolb from mysql to byte[] using Parameter binding
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="command"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public async Task<Response<Dictionary<string, byte[]>>> QueryBolb(List<string> parameters, string command, List<string> values, List<string> fieldNames)
        {

            try
            {

                MySqlCommand sqlCommand = new()
                {
                    Connection = Connection,
                    CommandText = command
                };

                for (int i = 0; i < parameters.Count; i++)
                {
                    sqlCommand.Parameters.AddWithValue(parameters[i], values[i]);
                }

                var reader = await sqlCommand.ExecuteReaderAsync();
                await reader.ReadAsync();
                Dictionary<string, byte[]> results = new();
                foreach (var fieldName in fieldNames)
                {
                    var len = reader.GetBytes(reader.GetOrdinal(fieldName), 0, null, 0, 0);
                    var buffer = new byte[len];
                    reader.GetBytes(reader.GetOrdinal(fieldName), 0, buffer, 0, (int)len);
                    results[fieldName] = buffer;
                }
                return new Response<Dictionary<string, byte[]>>()
                {
                    Data = results
                };



            }
            catch (Exception ex)
            {

                return new Response<Dictionary<string, byte[]>>()
                {

                    Message = ex.Message,
                    Code = 500

                };
            }


        }

        /// <summary>
        /// Query Bolb from mysql to byte[]
        /// </summary>
        /// <param name="command"></param>
        /// <param name="fieldNames"></param>
        /// <returns></returns>
        public async Task<Response<Dictionary<string, byte[]>>> QueryBolb(string command, List<string> fieldNames)
        {

            try
            {

                MySqlCommand sqlCommand = new()
                {
                    Connection = Connection,
                    CommandText = command
                };

                var reader = await sqlCommand.ExecuteReaderAsync();
                await reader.ReadAsync();
                Dictionary<string, byte[]> results = new();
                foreach (var fieldName in fieldNames)
                {
                    var len = reader.GetBytes(reader.GetOrdinal(fieldName), 0, null, 0, 0);
                    var buffer = new byte[len];
                    reader.GetBytes(reader.GetOrdinal(fieldName), 0, buffer, 0, (int)len);
                    results[fieldName] = buffer;
                }
                return new Response<Dictionary<string, byte[]>>()
                {
                    Data = results
                };

            }
            catch (Exception ex)
            {

                return new Response<Dictionary<string, byte[]>>()
                {

                    Message = ex.Message,
                    Code = 500

                };
            }

        }

        /// <summary>
        /// Query Data from Mysql To DataTable using Paramete binding
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public async Task<Response<DataTable>> Query(List<string> parameters, string command, List<string> values)
        {

            try
            {

                MySqlCommand sqlCommand = new()
                {
                    Connection = Connection,
                    CommandText = command
                };

                for (int i = 0; i < parameters.Count; i++)
                {
                    sqlCommand.Parameters.AddWithValue(parameters[i], values[i]);
                }

                MySqlDataAdapter adapter = new(sqlCommand);
                DataTable dt = new();
                await adapter.FillAsync(dt);

                return new Response<DataTable>()
                {
                    Data = dt
                };

            }
            catch (Exception ex)
            {

                return new Response<DataTable>()
                {

                    Message = ex.Message,
                    Code = 500

                };
            }

        }

        /// <summary>
        /// Query Data from Mysql To DataTable
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<Response<DataTable>> Query(string command)
        {

            try
            {

                MySqlCommand sqlCommand = new()
                {

                    Connection = Connection,
                    CommandText = command

                };

                MySqlDataAdapter adapter = new MySqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                await adapter.FillAsync(dt);

                return new Response<DataTable>()
                {

                    Data = dt

                };

            }
            catch (Exception ex)
            {

                return new Response<DataTable>()
                {

                    Message = ex.Message,
                    Code = 500

                };
            }

        }

        public async Task<Response<int>> Update(List<string> parameters, string command, List<string> values)
        {

            try
            {

                MySqlCommand sqlCommand = new()
                {
                    Connection = Connection,
                    CommandText = command
                };

                for (int i = 0; i < parameters.Count; ++i)
                {

                    sqlCommand.Parameters.AddWithValue(parameters[i], values[i]);

                }

                int rows = await sqlCommand.ExecuteNonQueryAsync();

                return new Response<int>()
                {
                    Data = rows
                };

            }
            catch (Exception ex)
            {

                return new Response<int>()
                {

                    Message = ex.Message,
                    Code = 500

                };
            }

        }

        /// <summary>
        /// Update Blob Data in Mysql Using Parameter binding
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public async Task<Response<int>> UpdateBlob(List<string> parameters, string command, List<object> values, List<int> blobIndex)
        {

            try
            {

                MySqlCommand sqlCommand = new()
                {
                    Connection = Connection,
                    CommandText = command
                };


                for (int i = 0; i < parameters.Count; i++)
                {
                    if (blobIndex.Contains(i))
                    {


                        MySqlParameter parameter = new(parameters[i], MySqlDbType.LongBlob)
                        {
                            Value = (byte[])values[i]
                        };

                        sqlCommand.Parameters.Add(parameter);
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue(parameters[i], values[i].ToString());
                    }

                }

                int rows = await sqlCommand.ExecuteNonQueryAsync();

                return new Response<int>()
                {
                    Data = rows
                };

            }
            catch (Exception ex)
            {

                return new Response<int>()
                {

                    Message = ex.Message,
                    Code = 500

                };
            }

        }

        /// <summary>
        /// Update Data in Mysql(uodate and delete)
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<Response<int>> Update(string command)
        {

            try
            {

                MySqlCommand sqlCommand = new()
                {
                    CommandText = command,
                    Connection = Connection
                };

                int rows = await sqlCommand.ExecuteNonQueryAsync();

                return new Response<int>()
                {
                    Data = rows
                };

            }
            catch (Exception ex)
            {

                return new Response<int>()
                {

                    Message = ex.Message,
                    Code = 500

                };
            }

        }

        public void Dispose()
        {

            Connection?.Close();

        }


    }

}