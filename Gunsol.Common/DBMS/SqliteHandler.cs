using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;

//using Gunsol.Common.File;
using Gunsol.Common.Model.Class;
using Gunsol.Common.Model.Enum;
using Gunsol.Common.Model.Struct;
//using Gunsol.Common.Protocol;


namespace Gunsol.Common.DBMS
{
    /// <summary>
    /// SQLite 관련 기능을 제공하는 Class
    /// </summary>
    public class SqliteHandler
    {
        #region Property
        /// <summary>
        /// SQLite 파일 경로
        /// </summary>
        public string localDbPath { get; set; }

        /// <summary>
        /// DB 접속 상태
        /// </summary>
        public ConnectionState dbState
        {
            get
            {
                if (sqliteConn == null)
                    return ConnectionState.Closed;
                else
                    return sqliteConn.State;
            }
        }

        /// <summary>
        /// SQLite 객체
        /// </summary>
        private SQLiteConnection sqliteConn;

        /// <summary>
        /// 연결 문자열
        /// </summary>
        private string connectionString;

        /// <summary>
        /// StopWatch 객체
        /// </summary>
        private Stopwatch stopWatch;
        #endregion

        #region Contructor
        /// <summary>
        /// Parameter를 사용하여 Propery 초기화 (Parameter를 설정하지 않을 경우, 빈 값으로 초기화)
        /// </summary>
        /// <param name="localDbPath">SQLite 파일 경로</param>
        public SqliteHandler(string localDbPath = null)
        {
            this.sqliteConn = new SQLiteConnection();

            if (localDbPath == null)
            {
                this.localDbPath = string.Empty;
            }
            else
            {
                this.localDbPath = localDbPath;
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 데이터베이스에 접속
        /// </summary>
        /// <returns>IsSuccess</returns>
        private CommonStruct.FuncResult Connect()
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                if (localDbPath.Equals(string.Empty))
                {
                    //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Fail :: Database Info Not Initialzed", this.ToString()));

                    result.isSuccess = false;
                }
                else
                {
                    connectionString = string.Format("Data source={0}; Version=3;", localDbPath);

                    if (!System.IO.File.Exists(localDbPath))
                    {
                        SQLiteConnection.CreateFile(localDbPath);
                    }

                    if (dbState == ConnectionState.Closed)
                    {
                        sqliteConn.ConnectionString = connectionString;
                        sqliteConn.Open();

                        if(dbState == ConnectionState.Open)
                        {
                            //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Success", this.ToString()));

                            result.isSuccess = true;
                        }
                        else
                        {
                            //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Fail", this.ToString()));

                            result.isSuccess = false;
                        }
                    }
                    else
                    {
                        //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Success :: Already Open", this.ToString()));

                        result.isSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Exception :: Message = {1}", this.ToString(), ex.Message));

                result.isSuccess = false;
                result.funcException = ex;
            }

            stopWatch.Stop();

            result.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 데이터베이스 접속 해제
        /// </summary>
        public CommonStruct.FuncResult Disconnect()
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                if (dbState == System.Data.ConnectionState.Open)
                {
                    sqliteConn.Close();
                }

                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Success", this.ToString()));

                result.isSuccess = true;
            }
            catch (Exception ex)
            {
                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: DisConnect() Exception :: Message = {1}", this.ToString(), ex.Message));

                result.isSuccess = false;
                result.funcException = ex;
            }

            stopWatch.Stop();

            result.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 테이블 SELECT
        /// </summary>
        /// <param name="tableName">테이블</param>
        /// <param name="condition">조건(컬럼명, 값)</param>
        /// <returns>Method 실행 결과</returns>
        public CommonStruct.SqlResult Select(string tableName, string condition = null)
        {
            CommonStruct.SqlResult result = new CommonStruct.SqlResult();
            SQLiteCommand sqliteCommand = null;
            SQLiteDataReader sqliteReader = null;
            DataTable resultTable = null;

            stopWatch.Start();

            try
            {
                string selectQuery = string.Empty;
                string tableQuery = string.Empty;
                string conditionQuery = string.Empty;

                if (dbState != System.Data.ConnectionState.Open)
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

                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Select(TableName = {1}) Success :: RowCount = {2}", this.ToString(), tableName, result.resultRowCount));

                result.funcResult.isSuccess = true;
                result.funcResult.funcException = null;
                result.resultTable = resultTable;
            }
            catch (Exception ex)
            {
                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Select(TableName = {1}) Exception :: Message = {2}", this.ToString(), tableName, ex.Message));

                result.funcResult.isSuccess = false;
                result.funcResult.funcException = ex;
                result.resultTable = null;
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

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 테이블 INSERT
        /// </summary>
        /// <param name="tableName">테이블</param>
        /// <param name="data">데이터</param>
        /// <returns>Method 실행 결과</returns>
        public CommonStruct.SqlResult Insert(string tableName, Dictionary<string, object> data)
        {
            CommonStruct.SqlResult result = new CommonStruct.SqlResult();
            SQLiteCommand sqliteCommand = null;

            stopWatch.Start();

            try
            {
                string insertQuery = string.Empty;
                string columnQuery = string.Empty;
                string valueQuery = string.Empty;

                if (dbState != System.Data.ConnectionState.Open)
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

                sqliteCommand.ExecuteNonQuery();

                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Insert(TableName = {1}) Success", this.ToString(), tableName));

                result.funcResult.isSuccess = true;
                result.funcResult.funcException = null;
                result.resultTable = null;
            }
            catch (Exception ex)
            {
                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Insert(TableName = {1}) Exception :: Message = {1}", this.ToString(), ex.Message));

                result.funcResult.isSuccess = false;
                result.funcResult.funcException = ex;
                result.resultTable = null;
            }
            finally
            {
                if (sqliteCommand != null)
                {
                    sqliteCommand.Dispose();
                    sqliteCommand = null;
                }
            }

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 테이블 UPDATE
        /// </summary>
        /// <param name="tableName">테이블</param>
        /// <param name="data">수정 데이터</param>
        /// <param name="condition">조건(컬럼명, 값)</param>
        /// <returns>Method 실행 결과</returns>
        public CommonStruct.SqlResult Update(string tableName, Dictionary<string, object> data, string condition = null)
        {
            CommonStruct.SqlResult result = new CommonStruct.SqlResult();
            SQLiteCommand sqliteCommand = null;

            stopWatch.Start();

            try
            {
                string updateQuery = string.Empty;
                string setQuery = string.Empty;
                string conditionQuery = string.Empty;

                if (dbState != System.Data.ConnectionState.Open)
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

                sqliteCommand.ExecuteNonQuery();

                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Update(TableName = {1}) Success :: RowCount = {2}", this.ToString(), tableName, result.resultRowCount));

                result.funcResult.isSuccess = true;
                result.funcResult.funcException = null;
                result.resultTable = null;
            }
            catch (Exception ex)
            {
                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Update(TableName = {1}) Exception :: Message = {2}", this.ToString(), tableName, ex.Message));

                result.funcResult.isSuccess = false;
                result.funcResult.funcException = ex;
                result.resultTable = null;
            }
            finally
            {
                if (sqliteCommand != null)
                {
                    sqliteCommand.Dispose();
                    sqliteCommand = null;
                }
            }

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 테이블 DELETE
        /// </summary>
        /// <param name="tableName">테이블</param>
        /// <param name="condition">조건(컬럼명, 값)</param>
        /// <returns>Method 실행 결과</returns>
        public CommonStruct.SqlResult Delete(string tableName, string condition = null)
        {
            CommonStruct.SqlResult result = new CommonStruct.SqlResult();
            SQLiteCommand sqliteCommand = null;

            stopWatch.Start();

            try
            {
                if (dbState != System.Data.ConnectionState.Open)
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

                sqliteCommand.ExecuteNonQuery();

                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Delete(TableName = {1}) Success :: RowCount = {2}", this.ToString(), tableName, result.resultRowCount));

                result.funcResult.isSuccess = true;
                result.funcResult.funcException = null;
                result.resultTable = null;
            }
            catch (Exception ex)
            {
                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Delete(TableName = {1}) Exception :: Message = {2}", this.ToString(), tableName, ex.Message));

                result.funcResult.isSuccess = false;
                result.funcResult.funcException = ex;
                result.resultTable = null;
            }
            finally
            {
                if (sqliteCommand != null)
                {
                    sqliteCommand.Dispose();
                    sqliteCommand = null;
                }
            }

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// Procedure Execute
        /// </summary>
        /// <param name="procName">Procedure</param>
        /// <param name="param">Procedure Parameter</param>
        /// <param name="type">Procedure의 실행 타입(SELECT/NOSELECT)</param>
        /// <returns>Method 실행 결과</returns>
        public CommonStruct.SqlResult Execute(string procName, Dictionary<string, object> param, CommonEnum.ExecuteType type)
        {
            CommonStruct.SqlResult result = new CommonStruct.SqlResult();
            SQLiteCommand sqliteCommand = null;
            SQLiteDataReader sqliteReader = null;
            DataTable resultTable = null;

            stopWatch.Start();

            try
            {
                if (dbState != System.Data.ConnectionState.Open)
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

                if (type == CommonEnum.ExecuteType.SELECT)
                {
                    sqliteReader = sqliteCommand.ExecuteReader();
                    resultTable.Load(sqliteReader);
                }
                else
                {
                    resultTable = null;
                    sqliteCommand.ExecuteNonQuery();
                }

                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Execute(ProcName = {1}) Success :: Type = {2} RowCount = {3}", this.ToString(), procName, type, executeRows));

                result.funcResult.isSuccess = true;
                result.funcResult.funcException = null;
                result.resultTable = resultTable;
            }
            catch (Exception ex)
            {
                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Execute(ProcName = {1}) Exception :: Message = {2}", this.ToString(), procName, ex.Message));

                result.funcResult.isSuccess = false;
                result.funcResult.funcException = ex;
                result.resultTable = null;
            }
            finally
            {
                if (sqliteCommand != null)
                {
                    sqliteCommand.Dispose();
                    sqliteCommand = null;
                }
            }

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// Query Execute
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="type">Query의 실행 타입(SELECT/NOSELECT)</param>
        /// <returns>Method 실행 결과</returns>
        public CommonStruct.SqlResult Execute(string query, CommonEnum.ExecuteType type)
        {
            CommonStruct.SqlResult result = new CommonStruct.SqlResult();
            SQLiteCommand sqliteCommand = null;
            SQLiteDataReader sqliteReader = null;
            DataTable resultTable = null;

            try
            {
                if (dbState != System.Data.ConnectionState.Open)
                {
                    Connect();
                }

                resultTable = new DataTable();
                sqliteCommand = new SQLiteCommand();
                sqliteCommand.Connection = sqliteConn;
                sqliteCommand.CommandType = System.Data.CommandType.Text;
                sqliteCommand.CommandText = query;
                sqliteCommand.CommandTimeout = 10;

                if (type == CommonEnum.ExecuteType.SELECT)
                {
                    sqliteReader = sqliteCommand.ExecuteReader();
                    resultTable.Load(sqliteReader);
                }
                else
                {
                    resultTable = null;
                    sqliteCommand.ExecuteNonQuery();
                }

                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Execute(Query = {1}) Success :: Type = {2} RowCount = {3}", this.ToString(), query, type, executeRows));

                result.funcResult.isSuccess = true;
                result.funcResult.funcException = null;
                result.resultTable = resultTable;
            }
            catch (Exception ex)
            {
                //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Execute(Query = {1}) Exception :: Message = {2}", this.ToString(), query, ex.Message));

                result.funcResult.isSuccess = false;
                result.funcResult.funcException = ex;
                result.resultTable = null;
            }
            finally
            {
                if (sqliteCommand != null)
                {
                    sqliteCommand.Dispose();
                    sqliteCommand = null;
                }
            }

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }
        #endregion
    }
}
