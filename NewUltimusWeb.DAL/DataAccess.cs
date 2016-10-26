using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using System.Linq;
using Dapper;
using KendoUI.Model;
using System.Text;
using System.Threading.Tasks;

namespace NewUltimusWeb.DAL
{
    public enum DBs
    {
        bpmDB,
        ultOC,
        ultDB        
    }

    public class DataAccess : IDisposable
    {
        private TransactionScope scope { get; set; }
        // Connect to bpmDB
        private SqlConnection dbConnection { get; set; }
        // Connect to ultOC
        private SqlConnection ocConnection { get; set; }
        // Connect to ultDB
        private SqlConnection ultDbConnection { get; set; }

        public DataAccess()
        {
            dbConnection = new SqlConnection() { ConnectionString = ConfigurationManager.ConnectionStrings["BpmDB"].ConnectionString };
            ocConnection = new SqlConnection() { ConnectionString = ConfigurationManager.ConnectionStrings["ultOC"].ConnectionString };
            ultDbConnection = new SqlConnection() { ConnectionString = ConfigurationManager.ConnectionStrings["ultDB"].ConnectionString };
        }

        private SqlConnection GetConnection(DBs db)
        {
            SqlConnection connection;
            switch (db)
            {
                case DBs.ultDB:
                    connection = ultDbConnection;
                    break;
                case DBs.ultOC:
                    connection = ocConnection;
                    break;
                case DBs.bpmDB:
                default:
                    connection = dbConnection;
                    break;
            }
            return connection;
        }

        public object DBNullHelper(object obj)
        {
            return obj == null ? DBNull.Value : obj;
        }

        private KendoGridData<T> GetKendoGridData<T>(DBs db, string spName, bool isTransaction, KendoFilterCollection filter, List<KendoSort> sort, int page, int pageSize, int skip, int take, out int? total)
        {
            KendoGridData<T> returnData = new KendoGridData<T>();
            total = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            #region // 處理 where 條件
            string whereStr = string.Empty;
            if (filter != null)
            {
                whereStr = filter.GetSqlWhereString(false);
            }
            #endregion

            #region // 處理 orderby
            string orderbyStr = string.Empty;
            foreach (KendoSort s in sort)
            {
                orderbyStr += s.GetSqlOrderBy();
            }
            orderbyStr = orderbyStr.TrimEnd(',');
            #endregion

            #region // 設定 dapper，用來取資料
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("whereStr", whereStr);
            parameters.Add("orderbyStr", orderbyStr);
            parameters.Add("page", page);
            parameters.Add("pageSize", pageSize);
            parameters.Add("skip", skip);
            parameters.Add("take", take);
            parameters.Add("total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            #endregion

            try
            {
                var data = GetData<T>(db, spName, isTransaction, parameters);
                returnData.gridData = data;
                total = parameters.Get<int?>("total");
            }
            catch
            {
                throw;
            }

            return returnData;
        }

        /// <summary>
        /// 用來取得資料的執行方法
        /// </summary>
        /// <typeparam name="T">要回傳的資料型別</typeparam>
        /// <param name="spName">資料來源 Stored Procedure 名稱</param>
        /// <param name="parameters">資料來源 Stored Procedure 參數</param>
        /// <returns></returns>
        private IEnumerable<T> GetData<T>(DBs db, string spName, bool isTransaction, DynamicParameters parameters)
        {
            IEnumerable<T> returnData;
            SqlConnection conn = GetConnection(db);

            try
            {
                returnData = conn.Query<T>(
                    spName,
                    parameters,
                    commandType: CommandType.StoredProcedure).AsEnumerable();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (!isTransaction)
                {
                    conn.Close();
                }                
            }

            return returnData;
        }

        public int ExecuteNonQuery(DBs db, string spName, bool isTransaction, params SqlParameter[] parameters)
        {
            SqlConnection conn = GetConnection(db);
            SqlCommand cmd = new SqlCommand(spName, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                cmd.Parameters.Clear();
                if (!isTransaction)
                {
                    conn.Close();
                }
            }
        }

        public object ExecuteScalar(DBs db, string spName, bool isTransaction, params SqlParameter[] parameters)
        {
            SqlConnection conn = GetConnection(db);

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                DataTable resultDt = new DataTable();
                SqlCommand cmd = new SqlCommand(spName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                {
                    foreach (SqlParameter param in parameters)
                    {
                        cmd.Parameters.Add(param);
                    }
                }
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (!isTransaction)
                {
                    conn.Close();
                }
            }
        }

        public DataRow ExecuteSqlToDR(DBs db, string spName, bool isTransaction, params SqlParameter[] parameters)
        {
            DataRow dr = null;

            DataTable dt = ExecuteSqlToDT(db, spName, isTransaction, parameters);
            if (dt != null && dt.Rows.Count > 0)
            {
                dr = dt.Rows[0];
            }

            return dr;
        }

        public DataTable ExecuteSqlToDT(DBs db, string spName, bool isTransaction, params SqlParameter[] parameters)
        {
            SqlConnection conn = GetConnection(db);

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                DataTable resultDt = new DataTable();
                SqlCommand cmd = new SqlCommand(spName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                {
                    foreach (SqlParameter param in parameters)
                    {
                        cmd.Parameters.Add(param);
                    }
                }
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(resultDt);

                return resultDt;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (!isTransaction)
                {
                    conn.Close();
                }
            }
        }

        public void bulkCopyDt(DBs db, string tableName, DataTable resourceDt, Dictionary<string, string> mappingColumns, bool isTransaction)
        {
            SqlConnection con = GetConnection(db);

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(GetConnection(db)))
            {
                bulkCopy.DestinationTableName = tableName;

                try
                {
                    // Mapping
                    foreach (var dItem in mappingColumns)
                    {
                        bulkCopy.ColumnMappings.Add(dItem.Key, dItem.Value);
                    }

                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    // Write from the source to the destination.
                    bulkCopy.WriteToServer(resourceDt);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (!isTransaction)
                    {
                        con.Close();
                    }
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置 Managed 狀態 (Managed 物件)。
                    if (this.dbConnection != null && this.dbConnection.State == ConnectionState.Open)
                    {
                        this.dbConnection.Close();
                    }
                    this.dbConnection.Dispose();
                    this.dbConnection = null;

                    if (this.ultDbConnection != null && this.ultDbConnection.State == ConnectionState.Open)
                    {
                        this.ultDbConnection.Close();
                    }
                    this.ultDbConnection.Dispose();
                    this.ultDbConnection = null;

                    if (this.ocConnection != null && this.ocConnection.State == ConnectionState.Open)
                    {
                        this.ocConnection.Close();
                    }
                    this.ocConnection.Dispose();
                    this.ocConnection = null;

                    scope.Dispose();
                    scope = null;
                }

                // TODO: 釋放 Unmanaged 資源 (Unmanaged 物件) 並覆寫下方的完成項。
                // TODO: 將大型欄位設為 null。

                disposedValue = true;
            }
        }

        // TODO: 僅當上方的 Dispose(bool disposing) 具有會釋放 Unmanaged 資源的程式碼時，才覆寫完成項。
        //~DataAccess()
        //{
        //    // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
        //    Dispose(false);
        //}

        // 加入這個程式碼的目的在正確實作可處置的模式。
        public void Dispose()
        {
            // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果上方的完成項已被覆寫，即取消下行的註解狀態。
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
