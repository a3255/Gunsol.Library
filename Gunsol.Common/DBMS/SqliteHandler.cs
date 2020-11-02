using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

using Gunsol.Common.File;
using Gunsol.Common.Protocol;


namespace Gunsol.Common.DBMS
{
    /// <summary>
    /// SQLite 관련 기능을 제공하는 Class
    /// </summary>
    public class SqliteHandler
    {
        #region Property (Enum/Struct)
        /// <summary>
        /// Procedure/Query 의 실행 타입
        /// </summary>
        public enum ExecuteType
        {
            /// <summary>
            /// SELECT Procedure/Query
            /// </summary>
            SELECT = 0,

            /// <summary>
            /// No Select Procedure/Query
            /// </summary>
            NOSELECT = 1
        }

        /// <summary>
        /// Method 실행 결과
        /// </summary>
        public struct CallResult
        {
            /// <summary>
            /// 성공 여부
            /// </summary>
            public bool isSuccess;

            /// <summary>
            /// 결과 테이블 (결과가 없을 경우 null)
            /// </summary>
            public DataTable resultTable;

            /// <summary>
            /// 결과 영향 받은 RowCount (Select일 경우 Select한 RowCount)
            /// </summary>
            public int resultRowCount;

            /// <summary>
            /// 실행 시간(ms)
            /// </summary>
            public double resultTime;
        }
        #endregion

        #region Property (Variable)
        /// <summary>
        /// SQLite 파일 경로
        /// </summary>
        public string localDbPath { get; set; }

        /// <summary>
        /// SQLite 객체
        /// </summary>
        private SQLiteConnection sqliteConn;

        /// <summary>
        /// 연결 문자열
        /// </summary>
        private string connectionString;
        #endregion

        #region Contructor
        /// <summary>
        /// 빈 값으로 Propery 초기화
        /// </summary>
        public SqliteHandler()
        {
            try
            {
                this.sqliteConn = new SQLiteConnection();
                this.localDbPath = string.Empty;
                this.connectionString = string.Empty;
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Constructor Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// Parameter를 사용하여 Propery 초기화
        /// </summary>
        /// <param name="localDbPath">// SQLite 파일 경로</param>
        public SqliteHandler(string localDbPath)
        {
            try
            {
                this.sqliteConn = new SQLiteConnection();
                this.localDbPath = localDbPath;
                this.connectionString = string.Format("Data source={0}; Version=3;", localDbPath);
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Constructor Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 데이터베이스에 접속
        /// </summary>
        /// <returns>IsSuccess</returns>
        private bool Connect()
        {
            bool isConnect = false;

            try
            {
                if (localDbPath.Equals(string.Empty))
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Fail :: Database Info Not Initialzed", this.ToString()));
                }
                else
                {
                    if (connectionString.Equals(string.Empty))
                    {
                        connectionString = string.Format("Data source={0}; Version=3;", localDbPath);
                    }

                    if (!System.IO.File.Exists(localDbPath))
                    {
                        SQLiteConnection.CreateFile(localDbPath);
                    }

                    if (sqliteConn.State == System.Data.ConnectionState.Closed)
                    {
                        sqliteConn.ConnectionString = connectionString;
                        sqliteConn.Open();

                        if(sqliteConn.State == ConnectionState.Open)
                        {
                            LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Success", this.ToString()));

                            isConnect = true;
                        }
                        else
                        {
                            LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Fail", this.ToString()));

                            isConnect = false;
                        }
                    }
                    else
                    {
                        LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Success :: Already Open", this.ToString()));

                        isConnect = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Exception :: Message = {1}", this.ToString(), ex.Message));

                isConnect = false;
            }

            return isConnect;
        }

        /// <summary>
        /// 데이터베이스 접속 해제
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (sqliteConn.State == System.Data.ConnectionState.Open)
                {
                    sqliteConn.Close();
                }

                LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Success", this.ToString()));
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Exception :: Message = {1}", this.ToString(), ex.Message));
            }
        }

        /// <summary>
        /// 테이블 SELECT
        /// </summary>
        /// <param name="tableName">테이블</param>
        /// <param name="condition">조건(컬럼명, 값)</param>
        /// <returns>Method 실행 결과</returns>
        public CallResult Select(string tableName, string condition = null)
        {
            CallResult result = new CallResult();
            SQLiteCommand sqliteCommand = null;
            SQLiteDataReader sqliteReader = null;
            DataTable resultTable = null;

            try
            {
                DateTime callStart = DateTime.Now;
                TimeSpan callTime = new TimeSpan();
                string selectQuery = string.Empty;
                string tableQuery = string.Empty;
                string conditionQuery = string.Empty;

                if (sqliteConn.State != System.Data.ConnectionState.Open)
                {
                    Connect();
                }

                tableQuery = string.Format("SELECT * FROM [{0}] WITH(NOLOCK)", tableName);

                if (condition != null)
                {
                    conditionQuery = string.Format("WHERE {0}", condition);
                }

                selectQuery = string.Format("{0} {1}", tableQuery, conditionQuery);

                resultTable = new DataTable();
                sqliteCommand = new SQLiteCommand();
                sqliteCommand.Connection = sqliteConn;
                sqliteCommand.CommandType = System.Data.CommandType.Text;
                sqliteCommand.CommandText = selectQuery;
                sqliteCommand.CommandTimeout = 1;

                sqliteReader = sqliteCommand.ExecuteReader();
                resultTable.Load(sqliteReader);

                callTime = DateTime.Now.Subtract(callStart);

                result.isSuccess = true;
                result.resultTable = resultTable;
                result.resultRowCount = resultTable.Rows.Count;
                result.resultTime = callTime.TotalMilliseconds;

                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Select(TableName = {1}) Success :: RowCount = {2}", this.ToString(), tableName, result.resultRowCount));
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Select(TableName = {1}) Exception :: Message = {2}", this.ToString(), tableName, ex.Message));

                result.isSuccess = false;
                result.resultTable = null;
                result.resultRowCount = 0;
                result.resultTime = 0;
            }
            finally
            {
                if (sqliteReader != null)
                {
                    sqliteReader.Close();
                    sqliteReader = null;
                }

                if (sqliteCommand != null)
                {
                    sqliteCommand.Dispose();
                    sqliteCommand = null;
                }
            }

            return result;
        }

        /// <summary>
        /// 테이블 INSERT
        /// </summary>
        /// <param name="tableName">테이블</param>
        /// <param name="data">데이터</param>
        /// <returns>Method 실행 결과</returns>
        public CallResult Insert(string tableName, Dictionary<string, object> data)
        {
            CallResult result = new CallResult();
            SQLiteCommand sqliteCommand = null;
            int insertRows = 0;

            try
            {
                DateTime callStart = DateTime.Now;
                TimeSpan callTime = new TimeSpan();
                string insertQuery = string.Empty;
                string columnQuery = string.Empty;
                string valueQuery = string.Empty;

                if (sqliteConn.State != System.Data.ConnectionState.Open)
                {
                    Connect();
                }

                foreach(KeyValuePair<string, object> d in data)
                {
                    columnQuery += string.Format("[{0}],", d.Key);
                    valueQuery += string.Format("'{0}',", d.Value);
                }

                columnQuery = columnQuery.Substring(0, columnQuery.Length - 1);
                valueQuery = valueQuery.Substring(0, valueQuery.Length - 1);
                insertQuery = string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", tableName, columnQuery, valueQuery);

                sqliteCommand = new SQLiteCommand();
                sqliteCommand.Connection = sqliteConn;
                sqliteCommand.CommandType = System.Data.CommandType.Text;
                sqliteCommand.CommandText = insertQuery;
                sqliteCommand.CommandTimeout = 1;

                insertRows = sqliteCommand.ExecuteNonQuery();

                callTime = DateTime.Now.Subtract(callStart);

                result.isSuccess = true;
                result.resultTable = null;
                result.resultRowCount = insertRows;
                result.resultTime = callTime.TotalMilliseconds;

                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Insert(TableName = {1}) Success", this.ToString(), tableName));
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Insert(TableName = {1}) Exception :: Message = {1}", this.ToString(), ex.Message));

                result.isSuccess = false;
                result.resultTable = null;
                result.resultRowCount = 0;
                result.resultTime = 0;
            }
            finally
            {
                if (sqliteCommand != null)
                {
                    sqliteCommand.Dispose();
                    sqliteCommand = null;
                }
            }

            return result;
        }

        /// <summary>
        /// 테이블 UPDATE
        /// </summary>
        /// <param name="tableName">테이블</param>
        /// <param name="data">수정 데이터</param>
        /// <param name="condition">조건(컬럼명, 값)</param>
        /// <returns>Method 실행 결과</returns>
        public CallResult Update(string tableName, Dictionary<string, object> data, string condition = null)
        {
            CallResult result = new CallResult();

            SQLiteCommand sqliteCommand = null;
            int updateRows = 0;

            try
            {
                DateTime callStart = DateTime.Now;
                TimeSpan callTime = new TimeSpan();
                string updateQuery = string.Empty;
                string setQuery = string.Empty;
                string conditionQuery = string.Empty;

                if (sqliteConn.State != System.Data.ConnectionState.Open)
                {
                    Connect();
                }

                for (int i = 0; i < data.Count; i++)
                {
                    if (i == 0)
                    {
                        setQuery = string.Format("SET [{0}] = '{1}',", data.Keys.ElementAt(i), data.Values.ElementAt(i));
                    }
                    else
                    {
                        setQuery += string.Format("[{0}] = '{1}',", data.Keys.ElementAt(i), data.Values.ElementAt(i));
                    }
                }

                if (condition != null)
                {
                    conditionQuery = string.Format("WHERE {0}", condition);
                }

                setQuery = setQuery.Substring(0, setQuery.Length - 1);
                updateQuery = string.Format("UPDATE [{0}] WITH(UPDLOCK) {1} {2}", tableName, setQuery, conditionQuery);

                sqliteCommand = new SQLiteCommand();
                sqliteCommand.Connection = sqliteConn;
                sqliteCommand.CommandType = System.Data.CommandType.Text;
                sqliteCommand.CommandText = updateQuery;
                sqliteCommand.CommandTimeout = 1;

                updateRows = sqliteCommand.ExecuteNonQuery();

                callTime = DateTime.Now.Subtract(callStart);

                result.isSuccess = true;
                result.resultTable = null;
                result.resultRowCount = updateRows;
                result.resultTime = callTime.TotalMilliseconds;

                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Update(TableName = {1}) Success :: RowCount = {2}", this.ToString(), tableName, result.resultRowCount));
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Update(TableName = {1}) Exception :: Message = {2}", this.ToString(), tableName, ex.Message));

                result.isSuccess = false;
                result.resultTable = null;
                result.resultRowCount = 0;
                result.resultTime = 0;
            }
            finally
            {
                if (sqliteCommand != null)
                {
                    sqliteCommand.Dispose();
                    sqliteCommand = null;
                }
            }

            return result;
        }

        /// <summary>
        /// 테이블 DELETE
        /// </summary>
        /// <param name="tableName">테이블</param>
        /// <param name="condition">조건(컬럼명, 값)</param>
        /// <returns>Method 실행 결과</returns>
        public CallResult Delete(string tableName, string condition = null)
        {
            CallResult result = new CallResult();

            SQLiteCommand sqliteCommand = null;
            int deleteRows = 0;

            try
            {
                DateTime callStart = DateTime.Now;
                TimeSpan callTime = new TimeSpan();

                if (sqliteConn.State != System.Data.ConnectionState.Open)
                {
                    Connect();
                }

                string deleteQuery = string.Empty;
                string tableQuery = string.Empty;
                string conditionQuery = string.Empty;

                tableQuery = string.Format("DELETE FROM [{0}]", tableName);

                if (condition != null)
                {
                    conditionQuery = string.Format("WHERE {0}", condition);
                }

                deleteQuery = string.Format("{0} {1}", tableQuery, conditionQuery);

                sqliteCommand = new SQLiteCommand();
                sqliteCommand.Connection = sqliteConn;
                sqliteCommand.CommandType = System.Data.CommandType.Text;
                sqliteCommand.CommandText = deleteQuery;
                sqliteCommand.CommandTimeout = 1;

                deleteRows = sqliteCommand.ExecuteNonQuery();

                callTime = DateTime.Now.Subtract(callStart);

                result.isSuccess = true;
                result.resultTable = null;
                result.resultRowCount = deleteRows;
                result.resultTime = callTime.TotalMilliseconds;

                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Delete(TableName = {1}) Success :: RowCount = {2}", this.ToString(), tableName, result.resultRowCount));
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Delete(TableName = {1}) Exception :: Message = {2}", this.ToString(), tableName, ex.Message));

                result.isSuccess = false;
                result.resultTable = null;
                result.resultRowCount = 0;
                result.resultTime = 0;
            }
            finally
            {
                if (sqliteCommand != null)
                {
                    sqliteCommand.Dispose();
                    sqliteCommand = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Procedure Execute
        /// </summary>
        /// <param name="procName">Procedure</param>
        /// <param name="param">Procedure Parameter</param>
        /// <param name="type">Procedure의 실행 타입(SELECT/NOSELECT)</param>
        /// <returns>Method 실행 결과</returns>
        public CallResult Execute(string procName, Dictionary<string, object> param, ExecuteType type)
        {
            CallResult result = new CallResult();
            SQLiteCommand sqliteCommand = null;
            SQLiteDataReader sqliteReader = null;
            DataTable resultTable = null;
            int executeRows = 0;

            try
            {
                DateTime callStart = DateTime.Now;
                TimeSpan callTime = new TimeSpan();

                if (sqliteConn.State != System.Data.ConnectionState.Open)
                {
                    Connect();
                }

                resultTable = new DataTable();
                sqliteCommand = new SQLiteCommand();
                sqliteCommand.Connection = sqliteConn;
                sqliteCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqliteCommand.CommandText = procName;
                sqliteCommand.CommandTimeout = 10;

                foreach (KeyValuePair<string, object> p in param)
                {
                    sqliteCommand.Parameters.AddWithValue(p.Key, p.Value.ToString());
                }

                if (type == ExecuteType.SELECT)
                {
                    sqliteReader = sqliteCommand.ExecuteReader();
                    resultTable.Load(sqliteReader);
                    executeRows = resultTable.Rows.Count;
                }
                else
                {
                    resultTable = null;
                    executeRows = sqliteCommand.ExecuteNonQuery();
                }

                callTime = DateTime.Now.Subtract(callStart);
                result.isSuccess = true;
                result.resultTable = resultTable;
                result.resultRowCount = executeRows;
                result.resultTime = callTime.TotalMilliseconds;

                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Execute(ProcName = {1}) Success :: Type = {2} RowCount = {3}", this.ToString(), procName, type, executeRows));
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Execute(ProcName = {1}) Exception :: Message = {2}", this.ToString(), procName, ex.Message));

                result.isSuccess = false;
                result.resultTable = null;
                result.resultRowCount = 0;
                result.resultTime = 0;
            }
            finally
            {
                if (sqliteCommand != null)
                {
                    sqliteCommand.Dispose();
                    sqliteCommand = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Query Execute
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="type">Query의 실행 타입(SELECT/NOSELECT)</param>
        /// <returns>Method 실행 결과</returns>
        public CallResult Execute(string query, ExecuteType type)
        {
            CallResult result = new CallResult();

            SQLiteCommand sqliteCommand = null;
            SQLiteDataReader sqliteReader = null;
            DataTable resultTable = null;
            int executeRows = 0;

            try
            {
                DateTime callStart = DateTime.Now;
                TimeSpan callTime = new TimeSpan();

                if (sqliteConn.State != System.Data.ConnectionState.Open)
                {
                    Connect();
                }

                resultTable = new DataTable();
                sqliteCommand = new SQLiteCommand();
                sqliteCommand.Connection = sqliteConn;
                sqliteCommand.CommandType = System.Data.CommandType.Text;
                sqliteCommand.CommandText = query;
                sqliteCommand.CommandTimeout = 10;

                if (type == ExecuteType.SELECT)
                {
                    sqliteReader = sqliteCommand.ExecuteReader();
                    resultTable.Load(sqliteReader);
                    executeRows = resultTable.Rows.Count;
                }
                else
                {
                    resultTable = null;
                    executeRows = sqliteCommand.ExecuteNonQuery();
                }

                callTime = DateTime.Now.Subtract(callStart);

                result.isSuccess = true;
                result.resultTable = resultTable;
                result.resultRowCount = executeRows;
                result.resultTime = callTime.TotalMilliseconds;

                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Execute(Query = {1}) Success :: Type = {2} RowCount = {3}", this.ToString(), query, type, executeRows));
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(string.Empty, string.Format("{0} :: Execute(Query = {1}) Exception :: Message = {2}", this.ToString(), query, ex.Message));

                result.isSuccess = false;
                result.resultTable = null;
                result.resultRowCount = 0;
                result.resultTime = 0;
            }
            finally
            {
                if (sqliteCommand != null)
                {
                    sqliteCommand.Dispose();
                    sqliteCommand = null;
                }
            }

            return result;
        }
        #endregion
    }
}
