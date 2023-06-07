using qDatabase.Attributes;
using qDatabase.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace qDatabase
{
    public class Database : IDisposable
    {
        private readonly SqlConnection _conn;
        private SqlTransaction _tran;
        private readonly String _connectionString;
        private string _databaseName = null;
        private protected Dictionary<string, DataTable> _cachedQueries = new Dictionary<string, DataTable>();
        private List<BatchStatement> _batchStatements = null;

        public string ConnectionString => _connectionString;
        public Boolean IsOpen => _conn != null && _conn.State == ConnectionState.Open;
        public Boolean IsTransaction => _tran != null;

        public IEnumerable<object> UserProfiles { get; set; }

        public Database(string connectionString)
        {
            _connectionString = connectionString;

            _conn = new SqlConnection(connectionString + ";MultipleActiveResultSets=true");
            _conn.Open();
        }

        public object Query(string v)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (_conn.State != ConnectionState.Closed)
            {
                // cancel any pending transaction;
                this.RollBack();
            }
            _conn.Dispose();

            _cachedQueries.Clear();

        }

        public void BeginTransaction()
        {
            _tran = _conn.BeginTransaction(IsolationLevel.Serializable);
        }
        public async Task BeginTransactionAsync()
        {
            _tran = (SqlTransaction)await _conn.BeginTransactionAsync(IsolationLevel.Serializable);
        }

        public void Commit()
        {
            if (_tran != null)
            {
                _tran.Commit();
                _tran.Dispose();
                _tran = null;
            }
        }
        public async Task CommitAsync()
        {
            if (_tran != null)
            {
                await _tran.CommitAsync();
                _tran.Dispose();
                _tran = null;
            }
        }

        public void RollBack()
        {
            if (_tran != null)
            {
                _tran.Rollback();
                _tran.Dispose();
                _tran = null;
            }
        }

        public async Task RollBackAsync()
        {
            if (_tran != null)
            {
                await _tran.RollbackAsync();
                _tran.Dispose();
                _tran = null;
            }
        }

        private SqlCommand CreateCommand(string query, Dictionary<string, object> Parameters = null)
        {
            SqlCommand comm;

            if (_tran != null)
                comm = new SqlCommand(query, _conn, _tran);
            else
                comm = new SqlCommand(query, _conn);

            if (Parameters != null)
            {
                foreach (var pair in Parameters)
                {
                    if (pair.Value == null)
                    {
                        comm.Parameters.AddWithValue(pair.Key, DBNull.Value);
                    }
                    else
                    {
                        comm.Parameters.AddWithValue(pair.Key, pair.Value);
                    }
                    if (pair.Key.ToLower().StartsWith("@output_"))
                    {
                        comm.Parameters[pair.Key].Direction = ParameterDirection.Output;
                    }
                }
            }
            comm.CommandTimeout = 0; // seconds timeout
            return comm;
        }

        public SqlDataReader CreateReader(string query)
        {
            using (SqlCommand comm = CreateCommand(query))
            {
                return comm.ExecuteReader();
            }
        }

        public async Task<SqlDataReader> CreateReaderAsync(string query)
        {
            using (SqlCommand comm = CreateCommand(query))
            {
                return await comm.ExecuteReaderAsync();
            }
        }

        /// <summary>
        /// Get the Database name
        /// </summary>
        /// <param name="forceRefresh">Name is cached, set it to true to query again</param>
        public string GetDatabaseName(Boolean forceRefresh = false)
        {
            if (string.IsNullOrEmpty(_databaseName) || forceRefresh)
            {
                _databaseName = ExecuteScalar("select DB_NAME()").ToString();
            }
            return _databaseName;
        }

        /// <summary>
        /// Get the database path in the server
        /// </summary>
        /// <param name="DatabaseName">Name of the database. If null use current database</param>
        /// <returns></returns>
        public string GetDatabasePath(string DatabaseName = null)
        {
            if (string.IsNullOrEmpty(DatabaseName)) DatabaseName = GetDatabaseName();

            string path = ExecuteScalar("SELECT top 1 F.physical_name AS current_file_location FROM sys.master_files  F inner join sys.databases D on D.database_id = F.database_id where D.name = '" + DatabaseName + "' and F.Type_DESC = 'ROWS'").ToString();
            return System.IO.Path.GetDirectoryName(path);
        }

        public bool ExistSchema(string schema)
        {
            return Convert.ToInt32(ExecuteScalar("select count(*) from information_schema.schemata where schema_name = '" + schema + "'")) > 0;
        }

        public bool ExistTable(string schema, string tableName, string database = null)
        {
            database = string.IsNullOrEmpty(database) ? "" : "[" + database + "].";
            return 1 == Convert.ToInt32(ExecuteScalar("select count(*) from " + database + "information_schema.tables where table_schema = '" + schema + "' and table_name = '" + tableName + "'"));
        }

        public object FirstOrDefault()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executed a Stored Procedure
        /// </summary>
        /// <param name="procName">Name of the Stored Procedure</param>
        /// <param name="Parameters">Parameters of the Stored Procedure</param>
        public void ExecuteProcedure(string procName, Dictionary<string, object> Parameters = null)
        {
            using (SqlCommand comm = CreateCommand(procName, Parameters))
            {
                comm.CommandTimeout = 60 * 10; // 10 minutes
                comm.CommandType = CommandType.StoredProcedure;

                comm.ExecuteNonQuery();
            }
        }

        public IAsyncResult BeginExecuteProcedure(string procName, Dictionary<string, object> Parameters = null)
        {
            using (SqlCommand comm = CreateCommand(procName, Parameters))
            {
                comm.CommandTimeout = 60 * 10; // 10 minutes
                comm.CommandType = CommandType.StoredProcedure;

                return comm.BeginExecuteNonQuery(delegate (IAsyncResult ar) { try { comm.EndExecuteNonQuery(ar); } catch { } }, null);
            }
        }

        public int ExecuteSQL(string query, Dictionary<string, object> Parameters = null)
        {
            using (SqlCommand comm = CreateCommand(query, Parameters))
            {
                return comm.ExecuteNonQuery();
            }
        }

        public async Task<int> ExecuteSQLAsync(string query, Dictionary<string, object> Parameters = null)
        {
            using (SqlCommand comm = CreateCommand(query, Parameters))
            {
                return await comm.ExecuteNonQueryAsync();
            }
        }

        public object ExecuteInsertSQL(string query, Dictionary<string, object> Parameters = null)
        {
            using (SqlCommand comm = CreateCommand(query + ";select scope_identity();", Parameters))
            {
                return comm.ExecuteScalar();
            }
        }

        public async Task<object> ExecuteInsertSQLAsync(string query, Dictionary<string, object> Parameters = null)
        {
            using (SqlCommand comm = CreateCommand(query + ";select scope_identity();", Parameters))
            {
                return await comm.ExecuteScalarAsync();
            }
        }

        public object ExecuteInsertObject(object o, string tableName, string schema)
        {
            //obtain table columns, will use only the properties that match
            var table = GetDataTable("SELECT TOP 1 * FROM [" + schema + "].[" + tableName + "] WHERE 1 = 2");

            Dictionary<string, object> Parameters = new Dictionary<string, object>();
            string query = "INSERT INTO [" + schema + "].[" + tableName + "] (";
            string queryValues = ") VALUES (";
            foreach(var prop in o.GetType().GetProperties())
            {
                if (table.Columns.Contains(prop.Name))
                {
                    query += "[" + prop.Name + "],";
                    queryValues += "@" + prop.Name + ",";
                    Parameters.Add(prop.Name, prop.GetValue(o));
                }
            }

            query = query.Substring(0, query.Length - 1) + queryValues.Substring(0, queryValues.Length-1) + ")";


            using (SqlCommand comm = CreateCommand(query, Parameters))
            {
                return comm.ExecuteScalar();
            }
        }

        public void ExecuteSQL(object p)
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar(string query, Dictionary<string, object> Parameters = null)
        {
            using (SqlCommand comm = CreateCommand(query, Parameters))
            {
                return comm.ExecuteScalar();
            }
        }

        public async Task<object> ExecuteScalarAsync(string query, Dictionary<string, object> Parameters = null)
        {
            using (SqlCommand comm = CreateCommand(query, Parameters))
            {
                return await comm.ExecuteScalarAsync();
            }
        }

        public object GetRecords<T>(object p)
        {
            throw new NotImplementedException();
        }

        public DataSet GetDataSet(string query, Dictionary<string, object> Parameters = null)
        {
            DataSet oDataTable = new DataSet();

            using (SqlCommand comm = CreateCommand(query, Parameters))
            {
                SqlDataAdapter oDataAdapter = new SqlDataAdapter(comm);
                oDataAdapter.Fill(oDataTable);
                oDataAdapter.Dispose();

                return oDataTable;
            }
        }


        public DataTable GetDataTable(string query, Dictionary<string, object> Parameters = null)
        {
            DataTable oDataTable = new DataTable();
            string cacheHash = null;
            bool useCache = query.StartsWith("#");

            if (useCache)
            {
                query = query.Substring(1);

                cacheHash = query;

                if (Parameters != null && Parameters.Count > 0)
                {
                    foreach (var p in Parameters)
                    {
                        cacheHash = cacheHash.Replace(("@" + p.Key).Replace("@@", "@"), p.Value.ToString());
                    }
                }


                if (_cachedQueries.ContainsKey(cacheHash)) return _cachedQueries[cacheHash];
            }

            using (SqlCommand comm = CreateCommand(query, Parameters))
            {
                SqlDataAdapter oDataAdapter = new SqlDataAdapter(comm);
                oDataAdapter.Fill(oDataTable);
                oDataAdapter.Dispose();
                if (useCache)
                {
                    _cachedQueries.Add(cacheHash, oDataTable);
                }
                return oDataTable;
            }
        }

        public List<T> GetRecords<T>(DataTable dt) where T : class, new()
        {
            List<T> result = new List<T>();

            T newObject = new T();

            Dictionary<string, Type> Properties = GetObjectProperties<T>();
            //use only the properties that match the columns in the table
            for (int i = 0; i < Properties.Count; i++)
            {
                if (!dt.Columns.Contains(Properties.Keys.ElementAt(i)))
                {
                    Properties.Remove(Properties.Keys.ElementAt(i));
                    i--;
                }
            }

            var nType = newObject.GetType();

            foreach (DataRow row in dt.Rows)
            {
                newObject = new T();

                foreach (var prop in Properties)
                {
                    var nProp = nType.GetProperty(prop.Key);
                    if (((!object.ReferenceEquals(row[prop.Key], DBNull.Value))))
                    {
                        if (nProp.PropertyType.IsValueType || nProp.PropertyType.Namespace == "System")
                        {
                            nProp.SetValue(newObject, row[prop.Key], null);
                        }
                        else
                        {
                            TypeConverter conv = TypeDescriptor.GetConverter(nProp.PropertyType);

                            if (conv.CanConvertFrom(row[prop.Key].GetType()))
                            {
                                nProp.SetValue(newObject, conv.ConvertFrom(row[prop.Key]), null);
                            }
                        }
                    }
                    else
                    {
                        TypeConverter conv = TypeDescriptor.GetConverter(nProp.PropertyType);

                        if (conv.CanConvertFrom(row[prop.Key].GetType()))
                        {
                            nProp.SetValue(newObject, conv.ConvertFrom(row[prop.Key]), null);
                        }
                    }
                }

                result.Add(newObject);
            }


            return result;
        }

        public List<T> GetRecords<T>(string query, Dictionary<string, object> parameters = null) where T : class, new()
        {
            DataTable dt = GetDataTable(query, parameters);

            return GetRecords<T>(dt);
        }

        /// <summary>
        /// Return the number of records of a table
        /// </summary>
        public int GetRowCount(string tableName)
        {
            string sSQL = @"SELECT SUM(PART.rows) AS rows
                            FROM sys.tables TBL
                            INNER JOIN sys.partitions PART ON TBL.object_id = PART.object_id
                            INNER JOIN sys.indexes IDX ON PART.object_id = IDX.object_id
                            AND PART.index_id = IDX.index_id
                            WHERE TBL.name = '" + tableName + "' AND IDX.index_id < 2 GROUP BY TBL.object_id, TBL.name";

            return Convert.ToInt32(ExecuteScalar(sSQL));
        }

        private void assignValues(ref object newObject, ref int joinCount, ref DataColumnCollection columns, DataRow row)
        {
            var type = newObject.GetType();
            Dictionary<string, Type> Properties = GetObjectProperties(type);

            foreach (var prop in Properties)
            {
                if (prop.Value == typeof(LookupField))
                {

                    string columnName = "J" + joinCount + "-" + prop.Key;
                    if (columns.Contains(columnName))
                    {
                        if (!object.ReferenceEquals(row[columnName], DBNull.Value))
                        {
                            type.GetProperty(prop.Key).SetValue(newObject, row[columnName], null);
                        }
                    }
                    joinCount++;
                }
                else if (prop.Value == typeof(LookupObject))
                {
                    // instanciate the object
                    var newProperty = Activator.CreateInstance(type.GetProperty(prop.Key).PropertyType);

                    // add the values
                    var currentProperty = newObject.GetType().GetProperty(prop.Key);
                    foreach (var cProp in GetObjectProperties(currentProperty.PropertyType))
                    {
                        string columnName = "J" + joinCount + "-" + cProp.Key;
                        if (columns.Contains(columnName))
                        {
                            if (!object.ReferenceEquals(row[columnName], DBNull.Value))
                            {
                                newProperty.GetType().GetProperty(cProp.Key).SetValue(
                                    newProperty,
                                    row[columnName], null
                                );
                            }
                        }
                    }

                    // assign the lookup for the current lookup?

                    joinCount++;

                    assignValues(ref newProperty, ref joinCount, ref columns, row);

                    // set the value in the parent object
                    type.GetProperty(prop.Key).SetValue(
                        newObject,
                        newProperty
                    );


                }
            }
        }

        

        private Dictionary<string, Type> GetObjectProperties<T>() where T : class
        {
            Dictionary<string, Type> properties = new Dictionary<string, Type>();
            foreach (PropertyInfo info in typeof(T).GetProperties())
            {
                if (info.GetCustomAttribute(typeof(RelatedField), false) != null)
                {
                    properties.Add(info.Name, typeof(RelatedField));
                }
                else if (info.GetCustomAttribute(typeof(LookupField), false) != null)
                {
                    properties.Add(info.Name, typeof(LookupField));
                }
                else if (info.GetCustomAttribute(typeof(LookupObject), false) != null)
                {
                    properties.Add(info.Name, typeof(LookupObject));
                }
                else if (info.GetCustomAttribute(typeof(PrimaryKeyDefinition), false) != null)
                {
                    properties.Add(info.Name, typeof(PrimaryKeyDefinition));
                }
                else
                {
                    properties.Add(info.Name, null);
                }
            }

            return properties;
        }

        private Dictionary<string, Type> GetObjectProperties(Type who)
        {
            Dictionary<string, Type> properties = new Dictionary<string, Type>();
            foreach (PropertyInfo info in who.GetProperties())
            {
                if (info.GetCustomAttribute(typeof(RelatedField), false) != null)
                {
                    properties.Add(info.Name, typeof(RelatedField));
                }
                else if (info.GetCustomAttribute(typeof(LookupField), false) != null)
                {
                    properties.Add(info.Name, typeof(LookupField));
                }
                else if (info.GetCustomAttribute(typeof(LookupObject), false) != null)
                {
                    properties.Add(info.Name, typeof(LookupObject));
                }
                else if (info.GetCustomAttribute(typeof(PrimaryKeyDefinition), false) != null)
                {
                    properties.Add(info.Name, typeof(PrimaryKeyDefinition));
                }
                else
                {
                    properties.Add(info.Name, null);
                }
            }

            return properties;
        }

        //Finds the Primary Key definition in the ITable object
        private Dictionary<string, bool> GetITablePK<T>() where T : class
        {
            Dictionary<string, bool> properties = new Dictionary<string, bool>();
            foreach (PropertyInfo info in typeof(T).GetProperties())
            {
                var attr = info.GetCustomAttribute(typeof(PrimaryKeyDefinition), false);

                if (attr != null)
                {
                    properties.Add(info.Name, ((PrimaryKeyDefinition)attr).AutoNumeric);
                }
            }

            return properties;
        }

        /// <summary>
        /// Convert the string to a vaild SQL Server table name
        /// </summary>
        public static string FormatTableName(string name)
        {
            name = name.Trim();
            if (!name.StartsWith("[")) name = "[" + name;
            if (!name.EndsWith("]")) name = name + "]";
            name = name.Replace(".", "].[").Replace("]]", "]").Replace("[[", "[");
            return name;
        }

        /// <summary>
        /// Inserts the datatable using separate Insert statements, one for each row
        /// </summary>
        public void InsertDataTableSQL(DataTable dt, string schema = null)
        {
            string sSQL = "";
            string values = "";
            foreach (DataColumn column in dt.Columns)
            {

                sSQL += ",[" + column.ColumnName + "]";
                values += ",@param" + column.Ordinal;
            }
            sSQL = "insert into " + (string.IsNullOrEmpty(schema) ? "" : schema + ".") + "[" + dt.TableName + "] (" + sSQL.Substring(1) + ") values (" + values.Substring(1) + ")";

            foreach (DataRow row in dt.Rows)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                foreach (DataColumn column in dt.Columns)
                {
                    parameters.Add("@param" + column.Ordinal, row[column]);

                }

                ExecuteScalar(sSQL, parameters);
            }
        }

        public async Task InsertDataTableSQLAsync(DataTable dt, string schema = null)
        {
            string sSQL = "";
            string values = "";
            foreach (DataColumn column in dt.Columns)
            {

                sSQL += ",[" + column.ColumnName + "]";
                values += ",@param" + column.Ordinal;
            }
            sSQL = "insert into " + (string.IsNullOrEmpty(schema) ? "" : schema + ".") + "[" + dt.TableName + "] (" + sSQL.Substring(1) + ") values (" + values.Substring(1) + ")";

            foreach (DataRow row in dt.Rows)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                foreach (DataColumn column in dt.Columns)
                {
                    parameters.Add("@param" + column.Ordinal, row[column]);

                }

                await ExecuteScalarAsync(sSQL, parameters);
            }
        }

        /// <summary>
        /// Inserts the datatable into the specified table, using Bulk Insert
        /// </summary>
        public void InsertDataTable(DataTable dt, string schema = null)
        {

            using (SqlBulkCopy s = new SqlBulkCopy(_conn))
            {
                s.DestinationTableName = (string.IsNullOrEmpty(schema) ? (string.IsNullOrEmpty(dt.Namespace) ? "" : "[" + dt.Namespace + "].") : "[" + schema + "].") + "[" + dt.TableName + "]";


                foreach (var column in dt.Columns) s.ColumnMappings.Add(column.ToString(), column.ToString());

                s.BulkCopyTimeout = 60; // 1 minute
                s.BatchSize = 5000;

                s.WriteToServer(dt);
            }
        }

        public async Task InsertDataTableAsync(DataTable dt, string schema = null)
        {

            using (SqlBulkCopy s = new SqlBulkCopy(_conn))
            {
                s.DestinationTableName = (string.IsNullOrEmpty(schema) ? (string.IsNullOrEmpty(dt.Namespace) ? "" : "[" + dt.Namespace + "].") : "[" + schema + "].") + "[" + dt.TableName + "]";


                foreach (var column in dt.Columns) s.ColumnMappings.Add(column.ToString(), column.ToString());

                s.BulkCopyTimeout = 60; // 1 minute
                s.BatchSize = 5000;

                await s.WriteToServerAsync(dt);
            }
        }

        /// <summary>
        /// Retrieves the Foreign Keys related to the table
        /// </summary>
        public List<TableDetails> FindRelationships(string table, string schema)
        {
            string sSQL = @"inner join (SELECT
FK.TABLE_NAME as FK_Table,
CU.COLUMN_NAME as FK_Column,
FK.TABLE_SCHEMA as FK_Schema,
PK.TABLE_NAME as PK_Table,
PT.COLUMN_NAME as PK_Column,
Constraint_Name = C.CONSTRAINT_NAME
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
INNER JOIN (
SELECT i1.TABLE_NAME, i2.COLUMN_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
) PT ON PT.TABLE_NAME = PK.TABLE_NAME
WHERE FK.TABLE_NAME = '" + table.Replace("'", "''") + "'";
            if (!string.IsNullOrEmpty(schema))
            {
                sSQL += " and FK.TABLE_SCHEMA = '" + schema.Replace("'", "''") + "'";
            }
            sSQL += ") E on E.PK_Table = t.name and E.FK_Schema = OBJECT_SCHEMA_NAME(t.object_id)";

            List<TableDetails> details = new List<TableDetails>();

            // list all tables:
            foreach (DataRow r in GetDataTable("SELECT distinct OBJECT_SCHEMA_NAME(t.object_id) AS schema_name, E.FK_COlumn ,t.name AS table_name ,p.rows AS rows, E.FK_Column  AS rows FROM sys.tables t INNER JOIN sys.indexes i ON t.object_id = i.object_id INNER JOIN sys.partitions p ON i.object_id=p.object_id AND i.index_id=p.index_id " + sSQL).Rows)
            {
                var det = new TableDetails()
                {
                    Name = r["TABLE_NAME"].ToString()
                    ,
                    Schema = r["schema_name"].ToString()
                    ,
                    Type = "TABLE"
                    ,
                    RowCount = long.Parse("0" + r["rows"].ToString().Replace(",", "").Replace(".", ""))
                    ,
                    FileGroup = r["FK_Column"].ToString()

                };
                if (details.Any(D => D.Name == det.Name && D.Schema == det.Schema)) continue;

                details.Add(det);
            }

            return details;
        }

        public List<TableDetails> TableDetails(bool loadExtendedDetails = true)
        {
            List<TableDetails> details = new List<TableDetails>();

            string sSQL = @"SELECT case when Z.TABLE_NAME is null then 0 else 1 end as [HasPK], OBJECT_SCHEMA_NAME(t.object_id) AS schema_name ,t.name AS table_name ,i.index_id ,i.name AS index_name ,p.partition_number ,fg.name AS filegroup_name ,p.rows AS rows 
FROM sys.tables t INNER JOIN sys.indexes i ON t.object_id = i.object_id INNER JOIN sys.partitions p ON i.object_id=p.object_id AND i.index_id=p.index_id LEFT OUTER JOIN sys.partition_schemes ps ON i.data_space_id=ps.data_space_id LEFT OUTER JOIN sys.destination_data_spaces dds ON ps.data_space_id=dds.partition_scheme_id AND p.partition_number=dds.destination_id INNER JOIN sys.filegroups fg ON COALESCE(dds.data_space_id, i.data_space_id)=fg.data_space_id
outer apply (
select top 1 C.Table_Name, C.Table_Schema
from INFORMATION_SCHEMA.COLUMNS C
left join information_schema.key_column_usage K on K.Table_name = C.Table_Name and K.Table_Schema = C.Table_Schema and K.Column_Name = C.Column_Name 
where  C.TABLE_NAME = t.name and C.TABLE_SCHEMA = OBJECT_SCHEMA_NAME(t.object_id) and not  OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') is null
) Z";


            // list all tables:
            foreach (DataRow r in GetDataTable(sSQL).Rows)
            {
                var det = new TableDetails()
                {
                    Name = r["TABLE_NAME"].ToString()
                    ,
                    Schema = r["schema_name"].ToString()
                    ,
                    Type = "TABLE"
                    ,
                    FileGroup = r["filegroup_name"].ToString()
                    ,
                    RowCount = long.Parse("0" + r["rows"].ToString().Replace(",", "").Replace(".", ""))
                    ,
                    HasPk = Convert.ToBoolean(r["HasPk"])
                };
                if (details.Any(D => D.Name == det.Name && D.Schema == det.Schema)) continue;

                if (loadExtendedDetails)
                {
                    DataRow d = GetDataTable("sp_spaceused '" + det.Schema + "." + det.Name + "'").Rows[0];

                    det.IndexSpace = double.Parse(d["index_size"].ToString().Replace(" KB", "").Replace(",", ""));
                    det.DiskSpace = double.Parse(d["data"].ToString().Replace(" KB", "").Replace(",", ""));
                    det.UnusedSpace = double.Parse(d["unused"].ToString().Replace(" KB", "").Replace(",", ""));
                    det.Reserved = double.Parse(d["Reserved"].ToString().Replace(" KB", "").Replace(",", ""));

                }
                details.Add(det);
            }

            return details;
        }

        public List<ColumnDetails> ColumnDetails(string TableName, string Schema = "dbo", bool CheckDuplicates = false)
        {

            if (TableName.Contains("."))
            {
                Schema = TableName.Split('.')[0];
                TableName = TableName.Split('.')[1];
            }

            List<ColumnDetails> details = new List<ColumnDetails>();

            foreach (DataRow r in GetDataTable("select K.ORDINAL_POSITION as [Key], COLUMNPROPERTY(object_id(C.TABLE_SCHEMA + '.' + C.TABLE_NAME), C.COLUMN_NAME, 'IsIdentity') as [IDENTITY], C.COLUMN_NAME, C.DATA_TYPE, C.IS_NULLABLE, OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') as [IS_PK] " +
                " from INFORMATION_SCHEMA.COLUMNS C " +
                " left join information_schema.key_column_usage K on K.Table_name = C.Table_Name and K.Table_Schema = C.Table_Schema and K.Column_Name = C.Column_Name " +
                " where C.TABLE_NAME = @tableName and C.TABLE_SCHEMA = @schema ",
                new Dictionary<string, object>()
                {
                    {"@tableName", TableName},
                    {"@schema", Schema}
                }
                ).Rows)
            {
                var column = new ColumnDetails()
                {
                    Key = (r["KEY"] != DBNull.Value && (int)r["key"] > 0),
                    DataType = r["DATA_TYPE"].ToString(),
                    Name = r["COLUMN_NAME"].ToString(),
                    isNullable = r["IS_NULLABLE"].ToString() == "YES",
                    autoincrement = (r["IDENTITY"] != DBNull.Value && r["IDENTITY"].ToString() == "1"),
                    hasDuplicates = false,
                    IsPk = Convert.ToBoolean(r["IS_PK"] == DBNull.Value ? false : r["IS_PK"])
                };

                if (CheckDuplicates)
                {
                    // case sensitive grouping for text fields
                    object dups;
                    if (column.DataType.ToLower().Contains("char") || column.DataType.ToLower().Contains("text"))
                    {
                        dups = ExecuteScalar("select top 1 [" + column.Name +
                        "] COLLATE SQL_Latin1_General_CP1_CS_AS from [" + Schema + "].[" + TableName + "]  group by [" + column.Name +
                        "] COLLATE SQL_Latin1_General_CP1_CS_AS having count([" + column.Name + "]) > 1");
                    }
                    else
                    {
                        dups = ExecuteScalar("select top 1 [" + column.Name +
                        "] from [" + Schema + "].[" + TableName + "]  group by [" + column.Name +
                        "] having count([" + column.Name + "]) > 1");
                    }
                    column.hasDuplicates = dups != null && dups != DBNull.Value;
                }

                details.Add(column);
            }

            return details;
        }

        public List<TableDetails> ViewDetails(bool count = true)
        {
            List<TableDetails> details = new List<TableDetails>();

            foreach (DataRow r in GetDataTable("SELECT SCHEMA_NAME(schema_id) AS schema_name,name AS view_name FROM sys.views where is_ms_shipped = 0").Rows)
            {
                var det = new TableDetails()
                {
                    Name = r["view_name"].ToString()
                    ,
                    Schema = r["schema_name"].ToString()
                    ,
                    Type = "VIEW"
                    ,
                    FileGroup = string.Empty
                    ,
                    RowCount = 0

                };
                if (count)
                {
                    try
                    {
                        det.RowCount = Convert.ToInt64(ExecuteScalar("select count(1) from [" + r["schema_name"].ToString() + "].[" + r["view_name"].ToString() + "]"));
                    }
                    catch
                    {
                        //there is an issue with the view, so skipt it...
                    }
                }
                if (details.Any(D => D.Name == det.Name && D.Schema == det.Schema)) continue;


                det.IndexSpace = 0;
                det.DiskSpace = 0;
                det.UnusedSpace = 0;
                det.Reserved = 0;

                details.Add(det);
            }


            return details;
        }

        #region Batch Statements / Batch Queries
        /// <summary>
        /// Starts a new Batch. A batch is a series of SQL Queries that will be executed together in a transaction in certain order.
        /// </summary>
        public void StartBatch()
        {
            _batchStatements = new List<BatchStatement>();
        }
        public void AddBatchStatement(string query, Dictionary<string, object> Parameters = null)
        {
            _batchStatements.Add(new BatchStatement() { Query = query, Parameters = Parameters });
        }
        public void CommitBatch()
        {
            string query = string.Empty;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            int auxCount = 0;
            foreach (var entry in _batchStatements)
            {
                if (entry.Parameters != null)
                {
                    foreach (var p in entry.Parameters)
                    {
                        // if there is a duplicate parameter...
                        if (parameters.ContainsKey(p.Key))
                        {
                            entry.Query = entry.Query.Replace(p.Key + ",", p.Key + auxCount + ",")
                                .Replace(p.Key + " ", p.Key + auxCount + " ")
                                .Replace(p.Key + ")", p.Key + auxCount + ")");
                            parameters.Add(p.Key + auxCount, p.Value);
                        }
                        else
                        {
                            parameters.Add(p.Key, p.Value);
                        }
                    }
                }
                query += entry.Query + ";" + Environment.NewLine;
                auxCount++;
            }
            if (auxCount > 0)
            {
                ExecuteSQL(query, parameters);
            }
            RollBackBatch();
        }
        public void RollBackBatch()
        {
            _batchStatements.Clear();
            _batchStatements = null;
        }
        #endregion
    }
}