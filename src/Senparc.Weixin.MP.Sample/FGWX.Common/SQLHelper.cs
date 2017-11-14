using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using System.Web;
using System.Collections;
using System.Data.SqlClient;

namespace FGWX.Common
{
    /// <summary>
    /// SQLHelper 的摘要说明
    /// </summary>
    public class SQLHelper
    {
        /// <summary>
        /// 读取数据库链接字符串只读不能修改
        /// </summary>
        //用于缓存参数的HASH表 Hashtable to store cached parameters
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

        public static DataTable ExecuteDataTable(SqlConnection conn, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter dap = new SqlDataAdapter();
            PrepareSelectCommand(dap, conn, null, cmdType, cmdText, commandParameters);
            dap.Fill(dt);
            dap.SelectCommand.Parameters.Clear();
            return dt;
        }
        public static SqlDataAdapter ExecuteAdapter(SqlTransaction tran, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlDataAdapter dap = new SqlDataAdapter();
            PrepareSelectCommand(dap, tran.Connection, tran, cmdType, cmdText, commandParameters);
            return dap;
        }
        public static bool ExecuteTransaction(SqlTransaction trans, CommandType cmdType, string[] cmdText, SqlParameter[][] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            for (int i = 0; i < cmdText.Length; i++)
            {
                if (commandParameters != null)
                    PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText[i], commandParameters[i]);
                else
                    PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText[i], null);
                int val = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.Parameters.Clear();
                if (val <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static DataSet ExecuteDataSet(SqlTransaction tran, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter dap = ExecuteAdapter(tran, cmdType, cmdText, commandParameters);
            dap.Fill(ds);
            dap.SelectCommand.Parameters.Clear();
            return ds;
        }
        public static DataTable ExecuteDataTable(SqlTransaction tran, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            SqlDataAdapter dap = ExecuteAdapter(tran, cmdType, cmdText, commandParameters);
            dap.Fill(dt);
            dap.SelectCommand.Parameters.Clear();
            return dt;
        }

        public static SqlDataReader ExecuteReader(SqlTransaction tran, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, tran.Connection, tran, cmdType, cmdText, commandParameters);
            SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return rdr;
        }

        /// <summary>
        /// 给定连接的数据库用假设参数执行一个sql命令（不返回数据集）
        /// Execute a SqlCommand (that returns no resultset) against the database specified in the connection string
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">一个有效的连接字符串 a valid connection string for a SqlConnection</param>
        /// <param name="commandType">命令类型(存储过程, 文本, 等等) the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或者sql命令语句 the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">执行命令所用参数的集合 an array of SqlParamters used to execute the command</param>
        /// <returns>执行命令所影响的行数 an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, SqlParameter[] commandParameters, int timeout = 120)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                    cmd.CommandTimeout = timeout;
                    int val = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    return val;
                }
                catch { throw; }
                finally
                {
                    Dispose(conn);
                }
            }
        }

        /// <summary>
        /// 用现有的数据库连接执行一个sql命令（不返回数据集）
        /// Execute a SqlCommand (that returns no resultset) against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="conn">一个现有的数据库连接 an existing database connection</param>
        /// <param name="commandType">命令类型(存储过程, 文本, 等等) the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或者sql命令语句 the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">执行命令所用参数的集合 an array of SqlParamters used to execute the command</param>
        /// <returns>执行命令所影响的行数 an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// 使用现有的SQL事务执行一个sql命令（不返回数据集）
        /// Execute a SqlCommand (that returns no resultset) using an existing SQL Transaction 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// 举例:  e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="trans">一个现有的事务 an existing sql transaction</param>
        /// <param name="commandType">命令类型(存储过程, 文本, 等等) the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或者sql命令语句 the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">执行命令所用参数的集合 an array of SqlParamters used to execute the command</param>
        /// <returns>执行命令所影响的行数 an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// 用事务执行SQL语句
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
        public static bool ExecuteTransaction(SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[][] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            if (commandParameters != null)
            {
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters[i]);
                    int val = Convert.ToInt32(cmd.ExecuteScalar());
                    cmd.Parameters.Clear();
                    if (val <= 0)
                    {
                        return false;
                    }
                }
            }
            else
            {
                PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, null);
                int val = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.Parameters.Clear();
                if (val <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 用事务执行SQL语句
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
        public static bool ExecuteTransaction(string connectionString, CommandType cmdType, string cmdText, SqlParameter[][] commandParameters)
        {
            using (SqlConnection Conn = new SqlConnection(connectionString))
            {
                try
                {
                    if (Conn.State != ConnectionState.Open)
                        Conn.Open();
                    using (SqlTransaction trans = Conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand();
                            for (int i = 0; i < commandParameters.Length; i++)
                            {
                                PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters[i]);
                                int val = Convert.ToInt32(cmd.ExecuteScalar());
                                cmd.Parameters.Clear();
                                if (val <= 0)
                                {
                                    trans.Rollback();
                                    return false;
                                }
                            }
                            trans.Commit();
                            return true;
                        }
                        catch
                        {
                            trans.Rollback();
                            //return false;
                            throw;
                        }
                        finally
                        {
                            trans.Dispose();
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    Dispose(Conn);
                }
            }
        }
        /// <summary>
        /// 用事务执行SQL语句
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
        public static bool ExecuteTransaction(string connectionString, string[] cmdTexts, SqlParameter[][] commandParameters)
        {
            return ExecuteTransaction(connectionString, CommandType.Text, cmdTexts, commandParameters);
        }

        /// <summary>
        /// 用事务执行SQL语句
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
        public static bool ExecuteTransaction(string connectionString, CommandType cmdType, string[] cmdTexts, SqlParameter[][] commandParameters)
        {
            using (SqlConnection Conn = new SqlConnection(connectionString))
            {
                try
                {
                    if (Conn.State != ConnectionState.Open)
                        Conn.Open();
                    using (SqlTransaction trans = Conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand();
                            for (int i = 0; i < cmdTexts.Length; i++)
                            {
                                if (commandParameters != null)
                                {
                                    PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdTexts[i], commandParameters[i]);
                                }
                                else
                                    PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdTexts[i], null);
                                int val = Convert.ToInt32(cmd.ExecuteScalar());
                                cmd.Parameters.Clear();
                                if (val <= 0)
                                {
                                    trans.Rollback();
                                    return false;
                                }
                            }
                            trans.Commit();
                            return true;
                        }
                        catch (Exception exc)
                        {
                            trans.Rollback();
                            throw exc;
                        }
                        finally
                        {
                            trans.Dispose();
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    Dispose(Conn);
                }
            }
        }


        /// <summary>
        /// 用事务执行SQL语句
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
        //public static bool ExecuteTransaction(string connectionString, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        //{
        //    //SqlConnection Conn;
        //    //Conn = new SqlConnection(ConnectionStringLocalTransaction);
        //    //创建一个SqlConnection对象
        //    //SqlConnection Conn = new SqlConnection(ConnectionStringLocalTransaction);
        //    try
        //    {
        //        SqlCommand cmd = new SqlCommand();
        //        //开始一个事务
        //        using (TransactionScope trans = new TransactionScope())
        //        {
        //            //创建一个SqlConnection对象
        //            using (SqlConnection conn = new SqlConnection(connectionString))
        //            {
        //                if (conn.State != ConnectionState.Open)
        //                    conn.Open();

        //                cmd.Connection = conn;
        //                cmd.CommandText = cmdText;

        //                //if (trans != null)
        //                //    cmd.Transaction = trans;

        //                cmd.CommandType = cmdType;

        //                if (cmdParms != null)
        //                {
        //                    foreach (SqlParameter parm in cmdParms)
        //                        cmd.Parameters.Add(parm);
        //                }

        //                cmd = CreateCmd(cmdText, conn);
        //                cmd.ExecuteNonQuery();
        //                trans.Complete();
        //            }



        //            //Conn.Open();
        //            //SqlCommand Cmd;
        //            //Cmd = CreateCmd(SQL, Conn);
        //            //Cmd.ExecuteNonQuery();
        //            //ts.Complete();
        //        }
        //        cmd.Parameters.Clear();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //    finally
        //    {
        //        //Dispose(conn);
        //    }
        //}
        /// <summary>
        /// 用事务执行SQL语句
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
        //public static bool ExecuteTransaction(string SQL)
        //{
        //    SqlConnection Conn;
        //    Conn = new SqlConnection(ConnectionStringLocalTransaction);
        //    //创建一个SqlConnection对象
        //    //SqlConnection Conn = new SqlConnection(ConnectionStringLocalTransaction);
        //    try
        //    {
        //        //开始一个事务
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            Conn.Open();
        //            SqlCommand Cmd;
        //            Cmd = CreateCmd(SQL, Conn);
        //            Cmd.ExecuteNonQuery();
        //            ts.Complete();
        //        }
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //    finally
        //    {
        //        Dispose(Conn);
        //    }
        //}

        public static void Dispose(SqlConnection Conn)
        {
            if (Conn != null)
            {
                Conn.Close();
                Conn.Dispose();
            }
            GC.Collect();
        }

        /// <summary>
        /// 用执行的数据库连接执行一个返回数据集的sql命令
        /// </summary>
        /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
        /// using the provided parameters.
        /// <remarks>
        /// 举例:  e.g.:  
        ///  SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">一个有效的连接字符串 a valid connection string for a SqlConnection</param>
        /// <param name="commandType">命令类型(存储过程, 文本, 等等) the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或者sql命令语句 the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">执行命令所用参数的集合 an array of SqlParamters used to execute the command</param>
        /// <returns>包含结果的读取器 A SqlDataReader containing the results</returns>
        public static SqlDataReader ExecuteReader(SqlConnection conn, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            //在这里我们用一个try/catch结构执行sql文本命令/存储过程，因为如果这个方法产生一个异常我们要关闭连接，因为没有读取器存在，
            //因此commandBehaviour.CloseConnection 就不会执行
            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return rdr;
        }
        /// <summary>
        /// 用执行的数据库连接执行一个返回数据集的sql命令
        /// </summary>
        /// <param name="connectionString">一个有效的连接字符串</param>
        /// <param name="cmdType">命令类型(存储过程, 文本, 等等)</param>
        /// <param name="cmdText">存储过程名称或者sql命令语句</param>
        /// <param name="commandParameters">执行命令所用参数的集合</param>
        /// <returns></returns>
        public static SqlDataAdapter ExecuteAdapter(SqlConnection conn, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlDataAdapter dap = new SqlDataAdapter();
            PrepareSelectCommand(dap, conn, null, cmdType, cmdText, commandParameters);
            return dap;
        }

        /// <summary>
        /// 用执行的数据库连接执行一个返回返回DataSet对象
        /// </summary>
        /// <param name="connectionString">一个有效的连接字符串</param>
        /// <param name="cmdType">命令类型(存储过程, 文本, 等等)</param>
        /// <param name="cmdText">存储过程名称或者sql命令语句</param>
        /// <param name="commandParameters">执行命令所用参数的集合</param>
        /// <returns>返回DataSet对象</returns>
        public static DataSet ExecuteDataSet(string connectionString, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    DataSet ds = new DataSet();
                    SqlDataAdapter dap = ExecuteAdapter(conn, cmdType, cmdText, commandParameters);
                    dap.Fill(ds);
                    dap.SelectCommand.Parameters.Clear();
                    return ds;
                }
                catch { throw; }
                finally
                {
                    Dispose(conn);
                }
            }
        }
        public static DataTable ExecuteDataTable(string connectionString, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    DataTable dt = new DataTable();
                    SqlDataAdapter dap = ExecuteAdapter(conn, cmdType, cmdText, commandParameters);
                    dap.Fill(dt);
                    dap.SelectCommand.Parameters.Clear();
                    return dt;
                }
                catch { throw; }
                finally
                {
                    Dispose(conn);
                }
            }
        }

        /// <summary>
        /// 用指定的数据库连接字符串执行一个命令并返回一个数据集的第一列
        /// Execute a SqlCommand that returns the first column of the first record against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// 例如:  e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        ///<param name="connectionString">一个有效的连接字符串 a valid connection string for a SqlConnection</param>
        /// <param name="commandType">命令类型(存储过程, 文本, 等等) the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或者sql命令语句 the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">执行命令所用参数的集合 an array of SqlParamters used to execute the command</param>
        /// <returns>用 Convert.To{Type}把类型转换为想要的 An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, SqlParameter[] commandParameters, int timeout = 120)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                    cmd.CommandTimeout = timeout;
                    object val = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    return val;
                }
                catch { throw; }
                finally
                {
                    Dispose(connection);
                }
            }
        }

        /// <summary>
        /// 用指定的数据库连接执行一个命令并返回一个数据集的第一列
        /// Execute a SqlCommand that returns the first column of the first record against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// 例如:  e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="conn">一个存在的数据库连接 an existing database connection</param>
        /// <param name="commandType">命令类型(存储过程, 文本, 等等) the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或者sql命令语句 the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">执行命令所用参数的集合 an array of SqlParamters used to execute the command</param>
        /// <returns>用 Convert.To{Type}把类型转换为想要的 An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }


        /*****************************/
        /// <summary>
        /// 使用现有的SQL事务执行一个命令并返回一个数据集的第一列
        /// Execute a SqlCommand that returns the first column of the first record against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// 例如:  e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="trans">一个现有的事务 an existing sql transaction</param>
        /// <param name="commandType">命令类型(存储过程, 文本, 等等) the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或者sql命令语句 the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">执行命令所用参数的集合 an array of SqlParamters used to execute the command</param>
        /// <returns>用 Convert.To{Type}把类型转换为想要的 An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }


        /***************************************************/
        /// <summary>
        /// 将参数集合添加到缓存 add parameter array to the cache
        /// </summary>
        /// <param name="cacheKey">添加到缓存的变量 Key to the parameter cache</param>
        /// <param name="cmdParms">一个将要添加到缓存的sql参数集合 an array of SqlParamters to be cached</param>
        public static void CacheParameters(string cacheKey, SqlParameter[] commandParameters)
        {
            parmCache[cacheKey] = commandParameters;
        }

        /// <summary>
        /// 找回缓存参数集合 Retrieve cached parameters
        /// </summary>
        /// <param name="cacheKey">用于找回参数的关键字 key used to lookup parameters</param>
        /// <returns>缓存的参数集合 Cached SqlParamters array</returns>
        public static SqlParameter[] GetCachedParameters(string cacheKey)
        {
            SqlParameter[] cachedParms = (SqlParameter[])parmCache[cacheKey];

            if (cachedParms == null)
                return null;

            SqlParameter[] clonedParms = new SqlParameter[cachedParms.Length];

            for (int i = 0, j = cachedParms.Length; i < j; i++)
                clonedParms[i] = (SqlParameter)((ICloneable)cachedParms[i]).Clone();

            return clonedParms;
        }

        /// <summary>
        /// 准备执行一个命令 Prepare a command for execution
        /// </summary>
        /// <param name="cmd">sql命令 SqlCommand object</param>
        /// <param name="conn">Sql连接 SqlConnection object</param>
        /// <param name="trans">Sql事务 SqlTransaction object</param>
        /// <param name="cmdType">命令类型例如 存储过程或者文本 Cmd type e.g. stored procedure or text</param>
        /// <param name="cmdText">命令文本,例如：Select * from Products Command text, e.g. Select * from Products</param>
        /// <param name="cmdParms">执行命令的参数 SqlParameters to use in the command</param>
        public static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }
        private static void PrepareSelectCommand(SqlDataAdapter dap, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] selcmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            dap.SelectCommand = new SqlCommand(cmdText, conn);

            if (trans != null)
                dap.SelectCommand.Transaction = trans;

            dap.SelectCommand.CommandType = cmdType;

            if (selcmdParms != null)
            {
                foreach (SqlParameter parm in selcmdParms)
                    dap.SelectCommand.Parameters.Add(parm);
            }
        }
    }
}