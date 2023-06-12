using qDatabase.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace qDatabase
{
    public interface IDatabase
    {
        string ConnectionString { get; }
        bool IsOpen { get; }
        bool IsTransaction { get; }
        IEnumerable<object> UserProfiles { get; set; }

        void AddBatchStatement(string query, Dictionary<string, object> Parameters = null);
        IAsyncResult BeginExecuteProcedure(string procName, Dictionary<string, object> Parameters = null);
        void BeginTransaction();
        Task BeginTransactionAsync();
        List<ColumnDetails> ColumnDetails(string TableName, string Schema = "dbo", bool CheckDuplicates = false);
        void Commit();
        Task CommitAsync();
        void CommitBatch();
        SqlDataReader CreateReader(string query);
        Task<SqlDataReader> CreateReaderAsync(string query);
        void Dispose();
        object ExecuteInsertObject(object o, string tableName, string schema);
        object ExecuteInsertSQL(string query, Dictionary<string, object> Parameters = null);
        Task<object> ExecuteInsertSQLAsync(string query, Dictionary<string, object> Parameters = null);
        void ExecuteProcedure(string procName, Dictionary<string, object> Parameters = null);
        object ExecuteScalar(string query, Dictionary<string, object> Parameters = null);
        Task<object> ExecuteScalarAsync(string query, Dictionary<string, object> Parameters = null);
        void ExecuteSQL(object p);
        int ExecuteSQL(string query, Dictionary<string, object> Parameters = null);
        Task<int> ExecuteSQLAsync(string query, Dictionary<string, object> Parameters = null);
        bool ExistSchema(string schema);
        bool ExistTable(string schema, string tableName, string database = null);
        List<TableDetails> FindRelationships(string table, string schema);
        object FirstOrDefault();
        string GetDatabaseName(bool forceRefresh = false);
        string GetDatabasePath(string DatabaseName = null);
        DataSet GetDataSet(string query, Dictionary<string, object> Parameters = null);
        DataTable GetDataTable(string query, Dictionary<string, object> Parameters = null);
        List<T> GetRecords<T>(DataTable dt) where T : class, new();
        object GetRecords<T>(object p);
        List<T> GetRecords<T>(string query, Dictionary<string, object> parameters = null) where T : class, new();
        int GetRowCount(string tableName);
        void InsertDataTable(DataTable dt, string schema = null);
        Task InsertDataTableAsync(DataTable dt, string schema = null);
        void InsertDataTableSQL(DataTable dt, string schema = null);
        Task InsertDataTableSQLAsync(DataTable dt, string schema = null);
        object Query(string v);
        void RollBack();
        Task RollBackAsync();
        void RollBackBatch();
        void StartBatch();
        List<TableDetails> TableDetails(bool loadExtendedDetails = true);
        List<TableDetails> ViewDetails(bool count = true);
    }
}