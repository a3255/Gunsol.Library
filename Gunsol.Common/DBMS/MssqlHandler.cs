using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using Gunsol.Common.File;
using Gunsol.Common.Model.Class;
using Gunsol.Common.Model.Enum;
using Gunsol.Common.Model.Struct;
using Gunsol.Common.Protocol;


namespace Gunsol.Common.DBMS
{
    /// <summary>
    /// MS-SQL 관련 기능을 제공하는 Class
    /// </summary>
    public class MssqlHandler
    {
        #region Property
        /// <summary>
        /// DB 접속 주소
        /// </summary>
        public string hostName { get; set; }

        /// <summary>
        /// DB명
        /// </summary>
        public string dbName { get; set; }

        /// <summary>
        /// DB 접속 ID
        /// </summary>
        public string dbId { get; set; }

        /// <summary>
        /// DB 접속 PW
        /// </summary>
        public string dbPw { get; set; }

        /// <summary>
        /// SqlConnection 객체
        /// </summary>
        private SqlConnection mssqlConn;

        /// <summary>
        /// 연결 문자열
        /// </summary>
        private string connectionString;
        #endregion

        #region Contructor
        /// <summary>
        /// 빈 값으로 Propery 초기화
        /// </summary>
        public MssqlHandler()
        {
            try
            {
                this.mssqlConn = new SqlConnection();
                this.hostName = string.Empty;
                this.dbName = string.Empty;
                this.dbId = string.Empty;
                this.dbPw = string.Empty;
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
        /// <param name="hostName">DB 접속 주소</param>
        /// <param name="dbName">DB 명</param>
        /// <param name="dbId">DB 접속 ID</param>
        /// <param name="dbPw">DB 접속 PW</param>
        public MssqlHandler(string hostName, string dbName, string dbId, string dbPw)
        {
            try
            {
                this.mssqlConn = new SqlConnection();
                this.hostName = hostName;
                this.dbName = dbName;
                this.dbId = dbId;
                this.dbPw = dbPw;
                this.connectionString = string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3};Pooling=true;Max Pool Size=10;Connection Timeout=1", hostName, dbName, dbId, dbPw);
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
        /// <returns>접속 성공 여부</returns>
        public bool Connect()
        {
            bool isConnect = false;

            try
            {
                if (hostName.Equals(string.Empty) || dbName.Equals(string.Empty) || dbId.Equals(string.Empty) || dbPw.Equals(string.Empty))
                {
                    LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Fail :: Database Info Not Initialzed", this.ToString()));
                }
                else
                {
                    if (connectionString.Equals(string.Empty))
                    {
                        connectionString = string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3};Pooling=true;Max Pool Size=10;Connection Timeout=1", hostName, dbName, dbId, dbPw);
                    }

                    if (mssqlConn.State != System.Data.ConnectionState.Open)
                    {
                        mssqlConn.ConnectionString = connectionString;
                        mssqlConn.Open();

                        if (mssqlConn.State == ConnectionState.Open)
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
        public void DisConnect()
        {
            try
            {
                if (mssqlConn.State == System.Data.ConnectionState.Open)
                {
                    mssqlConn.Close();
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
        public CommonStruct.CallResult Select(string tableName, string condition = null)
        {
            CommonStruct.CallResult result = new CommonStruct.CallResult();
            SqlCommand mssqlCommand = null;
            SqlDataReader mssqlReader = null;
            DataTable resultTable = null;

            try
            {
                DateTime callStart = DateTime.Now;
                TimeSpan callTime = new TimeSpan();
                string selectQuery = string.Empty;
                string tableQuery = string.Empty;
                string conditionQuery = string.Empty;

                if (mssqlConn.State != System.Data.ConnectionState.Open)
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
                mssqlCommand = new SqlCommand();
                mssqlCommand.Connection = mssqlConn;
                mssqlCommand.CommandType = System.Data.CommandType.Text;
                mssqlCommand.CommandText = selectQuery;
                mssqlCommand.CommandTimeout = 1;

                mssqlReader = mssqlCommand.ExecuteReader();
                resultTable.Load(mssqlReader);

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
                if (mssqlReader != null)
                {
                    mssqlReader.Close();
                    mssqlReader = null;
                }

                if (mssqlCommand != null)
                {
                    mssqlCommand.Dispose();
                    mssqlCommand = null;
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
        public CommonStruct.CallResult Insert(string tableName, Dictionary<string, object> data)
        {
            CommonStruct.CallResult result = new CommonStruct.CallResult();
            SqlCommand mssqlCommand = null;
            int insertRows = 0;

            try
            {
                DateTime callStart = DateTime.Now;
                TimeSpan callTime = new TimeSpan();
                string insertQuery = string.Empty;
                string columnQuery = string.Empty;
                string valueQuery = string.Empty;

                if (mssqlConn.State != System.Data.ConnectionState.Open)
                {
                    Connect();
                }

                foreach (KeyValuePair<string, object> d in data)
                {
                    columnQuery += string.Format("[{0}],", d.Key);
                    valueQuery += string.Format("'{0}',", d.Value);
                }

                columnQuery = columnQuery.Substring(0, columnQuery.Length - 1);
                valueQuery = valueQuery.Substring(0, valueQuery.Length - 1);
                insertQuery = string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", tableName, columnQuery, valueQuery);

                mssqlCommand = new SqlCommand();
                mssqlCommand.Connection = mssqlConn;
                mssqlCommand.CommandType = System.Data.CommandType.Text;
                mssqlCommand.CommandText = insertQuery;
                mssqlCommand.CommandTimeout = 1;

                insertRows = mssqlCommand.ExecuteNonQuery();

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
                if (mssqlCommand != null)
                {
                    mssqlCommand.Dispose();
                    mssqlCommand = null;
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
        public CommonStruct.CallResult Update(string tableName, Dictionary<string, object> data, string condition = null)
        {
            CommonStruct.CallResult result = new CommonStruct.CallResult();
            SqlCommand mssqlCommand = null;
            int updateRows = 0;

            try
            {
                DateTime callStart = DateTime.Now;
                TimeSpan callTime = new TimeSpan();

                if (mssqlConn.State != System.Data.ConnectionState.Open)
                {
                    Connect();
                }

                string updateQuery = string.Empty;
                string setQuery = string.Empty;
                string conditionQuery = string.Empty;

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

                mssqlCommand = new SqlCommand();
                mssqlCommand.Connection = mssqlConn;
                mssqlCommand.CommandType = System.Data.CommandType.Text;
                mssqlCommand.CommandText = updateQuery;
                mssqlCommand.CommandTimeout = 1;

                updateRows = mssqlCommand.ExecuteNonQuery();

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
                if (mssqlCommand != null)
                {
                    mssqlCommand.Dispose();
                    mssqlCommand = null;
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
        public CommonStruct.CallResult Delete(string tableName, string condition = null)
        {
            CommonStruct.CallResult result = new CommonStruct.CallResult();
            SqlCommand mssqlCommand = null;
            int deleteRows = 0;

            try
            {
                DateTime callStart = DateTime.Now;
                TimeSpan callTime = new TimeSpan();

                if (mssqlConn.State != System.Data.ConnectionState.Open)
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

                mssqlCommand = new SqlCommand();
                mssqlCommand.Connection = mssqlConn;
                mssqlCommand.CommandType = System.Data.CommandType.Text;
                mssqlCommand.CommandText = deleteQuery;
                mssqlCommand.CommandTimeout = 1;

                deleteRows = mssqlCommand.ExecuteNonQuery();

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
                if (mssqlCommand != null)
                {
                    mssqlCommand.Dispose();
                    mssqlCommand = null;
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
        public CommonStruct.CallResult Execute(string procName, Dictionary<string, object> param, CommonEnum.ExecuteType type)
        {
            CommonStruct.CallResult result = new CommonStruct.CallResult();
            SqlCommand mssqlCommand = null;
            SqlDataReader mssqlReader = null;
            DataTable resultTable = null;
            int executeRows = 0;

            try
            {
                DateTime callStart = DateTime.Now;
                TimeSpan callTime = new TimeSpan();

                if (mssqlConn.State != System.Data.ConnectionState.Open)
                {
                    Connect();
                }

                resultTable = new DataTable();
                mssqlCommand = new SqlCommand();
                mssqlCommand.Connection = mssqlConn;
                mssqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                mssqlCommand.CommandText = procName;
                mssqlCommand.CommandTimeout = 10;

                foreach (KeyValuePair<string, object> p in param)
                {
                    mssqlCommand.Parameters.AddWithValue(p.Key, p.Value.ToString());
                }

                if (type == CommonEnum.ExecuteType.SELECT)
                {
                    mssqlReader = mssqlCommand.ExecuteReader();
                    resultTable.Load(mssqlReader);
                    executeRows = resultTable.Rows.Count;
                }
                else
                {
                    resultTable = null;
                    executeRows = mssqlCommand.ExecuteNonQuery();
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
                if (mssqlCommand != null)
                {
                    mssqlCommand.Dispose();
                    mssqlCommand = null;
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
        public CommonStruct.CallResult Execute(string query, CommonEnum.ExecuteType type)
        {
            CommonStruct.CallResult result = new CommonStruct.CallResult();
            SqlCommand mssqlCommand = null;
            SqlDataReader mssqlReader = null;
            DataTable resultTable = null;
            int executeRows = 0;

            try
            {
                DateTime callStart = DateTime.Now;
                TimeSpan callTime = new TimeSpan();

                if (mssqlConn.State != System.Data.ConnectionState.Open)
                {
                    Connect();
                }

                resultTable = new DataTable();
                mssqlCommand = new SqlCommand();
                mssqlCommand.Connection = mssqlConn;
                mssqlCommand.CommandType = System.Data.CommandType.Text;
                mssqlCommand.CommandText = query;
                mssqlCommand.CommandTimeout = 10;

                if (type == CommonEnum.ExecuteType.SELECT)
                {
                    mssqlReader = mssqlCommand.ExecuteReader();
                    resultTable.Load(mssqlReader);
                    executeRows = resultTable.Rows.Count;
                }
                else
                {
                    resultTable = null;
                    executeRows = mssqlCommand.ExecuteNonQuery();
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
                if (mssqlCommand != null)
                {
                    mssqlCommand.Dispose();
                    mssqlCommand = null;
                }
            }

            return result;
        }
        #endregion
    }
}
