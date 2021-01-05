using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    /// MS-SQL 접속/해제, 테이블 SELECT/INSERT/UPDATE/DELETE, Procedure/Query 실행 기능을 제공하는 Class
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
        /// DB 접속 상태
        /// </summary>
        public ConnectionState dbState
        {
            get
            {
                if (mssqlConn == null)
                {
                    return ConnectionState.Closed;
                }
                else
                {
                    return mssqlConn.State;
                }
            }
        }

        /// <summary>
        /// SqlConnection 객체
        /// </summary>
        private SqlConnection mssqlConn;

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
        /// 빈 값으로 Property 초기화
        /// </summary>
        public MssqlHandler()
        {
            this.mssqlConn = new SqlConnection();
            this.hostName = string.Empty;
            this.dbName = string.Empty;
            this.dbId = string.Empty;
            this.dbPw = string.Empty;

            this.stopWatch = new Stopwatch();
        }

        /// <summary>
        /// Parameter를 사용하여 Property 초기화
        /// </summary>
        /// <param name="hostName">DB 접속 주소</param>
        /// <param name="dbName">DB 명</param>
        /// <param name="dbId">DB 접속 ID</param>
        /// <param name="dbPw">DB 접속 PW</param>
        public MssqlHandler(string hostName, string dbName, string dbId, string dbPw)
        {
            this.mssqlConn = new SqlConnection();
            this.hostName = hostName;
            this.dbName = dbName;
            this.dbId = dbId;
            this.dbPw = dbPw;

            this.stopWatch = new Stopwatch();
        }
        #endregion

        #region Method
        /// <summary>
        /// MS-SQL 접속
        /// </summary>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult Connect()
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                if (hostName.Equals(string.Empty) || dbName.Equals(string.Empty) || dbId.Equals(string.Empty) || dbPw.Equals(string.Empty))
                {
                    //LogHandler.WriteLog(string.Empty, string.Format("{0} :: Connect() Fail :: Database Info Not Initialzed", this.ToString()));

                    result.isSuccess = false;
                }
                else
                {
                    connectionString = string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3};Pooling=true;Max Pool Size=10;Connection Timeout=5", hostName, dbName, dbId, dbPw);

                    if (dbState != ConnectionState.Open)
                    {
                        mssqlConn.ConnectionString = connectionString;
                        mssqlConn.Open();

                        if (dbState == ConnectionState.Open)
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
        /// MS-SQL 접속 해제
        /// </summary>
        /// <returns>함수 실행 결과 (FuncResult 객체)</returns>
        public CommonStruct.FuncResult DisConnect()
        {
            CommonStruct.FuncResult result = new CommonStruct.FuncResult();

            stopWatch.Start();

            try
            {
                if (dbState == System.Data.ConnectionState.Open)
                {
                    mssqlConn.Close();
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
        /// 지정 테이블의 데이터 SELECT
        /// </summary>
        /// <param name="tableName">SELECT 테이블명</param>
        /// <param name="condition">조건(컬럼명, 값)</param>
        /// <returns>함수 실행 결과 (DbmsResult 객체)</returns>
        public CommonStruct.DbmsResult Select(string tableName, string condition = null)
        {
            CommonStruct.DbmsResult result = new CommonStruct.DbmsResult();
            SqlCommand mssqlCommand = null;
            SqlDataReader mssqlReader = null;
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
                mssqlCommand = new SqlCommand();
                mssqlCommand.Connection = mssqlConn;
                mssqlCommand.CommandType = System.Data.CommandType.Text;
                mssqlCommand.CommandText = selectQuery;
                mssqlCommand.CommandTimeout = 1;

                mssqlReader = mssqlCommand.ExecuteReader();
                resultTable.Load(mssqlReader);

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

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            return result;
        }

        /// <summary>
        /// 지정 테이블의 데이터 INSERT
        /// </summary>
        /// <param name="tableName">테이블</param>
        /// <param name="data">데이터</param>
        /// <returns>함수 실행 결과 (DbmsResult 객체)</returns>
        public CommonStruct.DbmsResult Insert(string tableName, Dictionary<string, object> data)
        {
            CommonStruct.DbmsResult result = new CommonStruct.DbmsResult();
            SqlCommand mssqlCommand = null;

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

                mssqlCommand.ExecuteNonQuery();

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
                if (mssqlCommand != null)
                {
                    mssqlCommand.Dispose();
                    mssqlCommand = null;
                }
            }

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 지정 테이블의 데이터 UPDATE
        /// </summary>
        /// <param name="tableName">테이블</param>
        /// <param name="data">수정 데이터</param>
        /// <param name="condition">조건(컬럼명, 값)</param>
        /// <returns>함수 실행 결과 (DbmsResult 객체)</returns>
        public CommonStruct.DbmsResult Update(string tableName, Dictionary<string, object> data, string condition = null)
        {
            CommonStruct.DbmsResult result = new CommonStruct.DbmsResult();
            SqlCommand mssqlCommand = null;

            stopWatch.Start();

            try
            {
                if (dbState != System.Data.ConnectionState.Open)
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

                mssqlCommand.ExecuteNonQuery();

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
                if (mssqlCommand != null)
                {
                    mssqlCommand.Dispose();
                    mssqlCommand = null;
                }
            }

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 지정 테이블의 데이터 DELETE
        /// </summary>
        /// <param name="tableName">테이블</param>
        /// <param name="condition">조건(컬럼명, 값)</param>
        /// <returns>함수 실행 결과 (DbmsResult 객체)</returns>
        public CommonStruct.DbmsResult Delete(string tableName, string condition = null)
        {
            CommonStruct.DbmsResult result = new CommonStruct.DbmsResult();
            SqlCommand mssqlCommand = null;

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

                mssqlCommand = new SqlCommand();
                mssqlCommand.Connection = mssqlConn;
                mssqlCommand.CommandType = System.Data.CommandType.Text;
                mssqlCommand.CommandText = deleteQuery;
                mssqlCommand.CommandTimeout = 1;

                mssqlCommand.ExecuteNonQuery();

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
                if (mssqlCommand != null)
                {
                    mssqlCommand.Dispose();
                    mssqlCommand = null;
                }
            }

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 지정 Procedure 실행
        /// </summary>
        /// <param name="procName">Procedure</param>
        /// <param name="param">Procedure Parameter</param>
        /// <param name="type">Procedure의 실행 타입 (ExecuteType 변수)</param>
        /// <returns>함수 실행 결과 (DbmsResult 객체)</returns>
        public CommonStruct.DbmsResult Execute(string procName, Dictionary<string, object> param, CommonEnum.ExecuteType type)
        {
            CommonStruct.DbmsResult result = new CommonStruct.DbmsResult();
            SqlCommand mssqlCommand = null;
            SqlDataReader mssqlReader = null;
            DataTable resultTable = null;

            stopWatch.Start();

            try
            {
                if (dbState != System.Data.ConnectionState.Open)
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
                }
                else
                {
                    resultTable = null;
                    mssqlCommand.ExecuteNonQuery();
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
                if (mssqlCommand != null)
                {
                    mssqlCommand.Dispose();
                    mssqlCommand = null;
                }
            }

            stopWatch.Stop();

            result.funcResult.totalMilliseconds = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();

            return result;
        }

        /// <summary>
        /// 지정 Query 실행
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="type">Query의 실행 타입 (SELECT/NOSELECT)</param>
        /// <returns>함수 실행 결과 (DbmsResult 객체)</returns>
        public CommonStruct.DbmsResult Execute(string query, CommonEnum.ExecuteType type)
        {
            CommonStruct.DbmsResult result = new CommonStruct.DbmsResult();
            SqlCommand mssqlCommand = null;
            SqlDataReader mssqlReader = null;
            DataTable resultTable = null;

            stopWatch.Start();

            try
            {
                if (dbState != System.Data.ConnectionState.Open)
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
                }
                else
                {
                    resultTable = null;
                    mssqlCommand.ExecuteNonQuery();
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
                if (mssqlCommand != null)
                {
                    mssqlCommand.Dispose();
                    mssqlCommand = null;
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
