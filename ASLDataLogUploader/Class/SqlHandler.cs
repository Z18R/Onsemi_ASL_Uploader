using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class SQLHandler
{
    private string err_msg;
    private string server, database, username, password;
    private SqlConnection sql_conn;
    private SqlParameter[] cmd_param;

    private List<string> cmdOutputParams;
    private List<string> cmdOutputValues;

    public SQLHandler()
    {
        server = "DESKTOP-6E9LU1F\\SQLEXPRESS";
        database = "MES_ATEC";
        username = "sa";
        password = "18Bz23efBd0J";
        sql_conn = new SqlConnection();
        err_msg = "";
    }

    public bool OpenConnection()
    {
        try
        {
            if (sql_conn.State == ConnectionState.Open)
            {
                sql_conn.Close();
            }

            string connection = BuildConnectionString();
            sql_conn.ConnectionString = connection;
            sql_conn.Open();

            return true;
        }
        catch (Exception ex)
        {
            err_msg = "Open Connection: " + ex.Message;
            return false;
        }
    }

    private string BuildConnectionString()
    {
        if (string.IsNullOrEmpty(username))
        {
            return $"server={server}; database={database}; connection timeout=30; Trusted_Connection=Yes;";
        }
        else
        {
            return $"server={server}; database={database}; user id={username}; password={password}; connection timeout=30";
        }
    }

    public bool CloseConnection()
    {
        try
        {
            sql_conn.Close();
            return true;
        }
        catch (Exception ex)
        {
            err_msg = ex.Message;
            return false;
        }
    }

    public bool CreateParameter(int size, bool _redim = false)
    {
        try
        {
            if (size == 0)
            {
                err_msg = "Create Parameter: Invalid size of parameters";
                return false;
            }
            cmdOutputParams = new List<string>();
            cmdOutputValues = new List<string>();
            if (_redim)
            {
                Array.Resize(ref cmd_param, size);
            }
            else
            {
                cmd_param = new SqlParameter[size];
            }

            return true;
        }
        catch (Exception ex)
        {
            err_msg = "Create Parameter: " + ex.Message;
            return false;
        }
    }

    public bool SetParameterValues(int position, string paramName, SqlDbType type, object value, ParameterDirection direction = ParameterDirection.Input)
    {
        try
        {
            if (cmd_param == null)
            {
                err_msg = "Set Parameter Values: Invalid size of parameters";
                return false;
            }

            cmd_param[position] = new SqlParameter(paramName, type)
            {
                Direction = direction
            };

            if (direction == ParameterDirection.Output)
            {
                cmdOutputParams.Add(paramName);
                if (type == SqlDbType.NVarChar || type == SqlDbType.VarChar)
                {
                    cmd_param[position].Size = 4000;
                }
            }
            else
            {
                cmd_param[position].Value = value;
            }

            return true;
        }
        catch (Exception ex)
        {
            err_msg = "Set Parameter Values: " + ex.Message;
            return false;
        }
    }

    private bool AttachParameter(SqlCommand cmd)
    {
        try
        {
            if (cmd_param != null && cmd_param.Length > 0)
            {
                foreach (var p in cmd_param)
                {
                    cmd.Parameters.Add(p);
                }
            }
            else
            {
                err_msg = "Attach Parameter: Invalid size of parameters";
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            err_msg = "Attach Parameter: " + ex.Message;
            return false;
        }
    }

    private void SetOutputParamValues(SqlCommand cmd)
    {
        if (cmdOutputParams != null)
        {
            foreach (var output in cmdOutputParams)
            {
                cmdOutputValues.Add(cmd.Parameters[output].Value.ToString());
            }
        }
    }

    public string GetOutputParamValue(string paramName)
    {
        for (int i = 0; i < cmdOutputParams.Count; i++)
        {
            if (cmdOutputParams[i] == paramName)
            {
                return cmdOutputValues[i];
            }
        }
        return "";
    }

    public bool ExecuteNonQuery(string sql_string, CommandType command_type)
    {
        try
        {
            if (OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(sql_string, sql_conn))
                {
                    command.CommandTimeout = 99999;
                    command.CommandType = command_type;

                    if (cmd_param != null)
                    {
                        AttachParameter(command);
                    }

                    command.ExecuteNonQuery();
                    SetOutputParamValues(command);
                }

                CloseConnection();
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            err_msg = "Execute Non Query: " + ex.Message;
            return false;
        }
    }

    public bool FillDataReader(string sql_string, out SqlDataReader dr, CommandType command_type)
    {
        try
        {
            dr = null;
            if (OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(sql_string, sql_conn))
                {
                    command.CommandTimeout = 999999;
                    command.CommandType = command_type;

                    if (cmd_param != null)
                    {
                        AttachParameter(command);
                    }

                    dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                    SetOutputParamValues(command);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            err_msg = "Fill DataReader: " + ex.Message;
            dr = null;
            return false;
        }
    }

    public bool FillDataSet(string sql_string, out DataSet ds, CommandType command_type)
    {
        ds = new DataSet();
        try
        {
            if (OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(sql_string, sql_conn))
                {
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    command.CommandTimeout = 999999;
                    command.CommandType = command_type;

                    if (cmd_param != null)
                    {
                        AttachParameter(command);
                    }

                    da.Fill(ds);
                }
                CloseConnection();
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            err_msg = "Fill DataSet: " + ex.Message;
            return false;
        }
    }

    public SqlDbType GetSQLDbType(string dataType)
    {
        return dataType.ToLower() switch
        {
            "binary" => SqlDbType.VarBinary,
            "bit" => SqlDbType.Bit,
            "char" => SqlDbType.Char,
            "date" => SqlDbType.Date,
            "datetime" => SqlDbType.DateTime,
            "datetime2" => SqlDbType.DateTime2,
            "datetimeoffset" => SqlDbType.DateTimeOffset,
            "decimal" => SqlDbType.Decimal,
            "float" => SqlDbType.Float,
            "image" => SqlDbType.Binary,
            "int" => SqlDbType.Int,
            "money" => SqlDbType.Money,
            "nchar" => SqlDbType.NChar,
            "ntext" => SqlDbType.NText,
            "numeric" => SqlDbType.Decimal,
            "nvarchar" => SqlDbType.NVarChar,
            "real" => SqlDbType.Real,
            "rowversion" => SqlDbType.Timestamp,
            "smalldatetime" => SqlDbType.DateTime,
            "smallint" => SqlDbType.SmallInt,
            "smallmoney" => SqlDbType.SmallMoney,
            "sql_variant" => SqlDbType.Variant,
            "text" => SqlDbType.Text,
            "time" => SqlDbType.Time,
            "timestamp" => SqlDbType.Timestamp,
            "tinyint" => SqlDbType.TinyInt,
            "uniqueidentifier" => SqlDbType.UniqueIdentifier,
            "varbinary" => SqlDbType.VarBinary,
            "varchar" => SqlDbType.VarChar,
            "xml" => SqlDbType.Xml,
            _ => SqlDbType.VarChar,
        };
    }
}
