using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Xml;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data.SqlTypes;
using FrameWork.Database.Connection;
using System.Diagnostics;

namespace FrameWork
{
    public class SQLObjectDatabase : ObjectDatabase
    {
        public SQLObjectDatabase(SqlDataConnection connection)
            : base(connection)
        {
        }

        #region Value conversion

        /// <summary>
        /// Unboxes an object to its appropriate type.
        /// </summary>
        protected object ConvertVal(object val, string dateFormat)
        {
            object Obj = null;

            if (val is bool)
                Obj = ((bool)val) ? 1 : 0;
            else if (val is DateTime)
                Obj = ((DateTime)val).ToString(dateFormat);
            else if (val is float)
                Obj = ((float)val).ToString(Nfi);
            else if (val is double)
                Obj = ((double)val).ToString(Nfi);
            else if (val is string)
                Obj = Escape((string)val);

            else if (val is List<byte>)
                Obj = Utils.ConvertArrayToString<byte>(((List<byte>)val).ToArray());
            else if (val is byte[])
                Obj = Utils.ConvertArrayToString<byte>(((byte[])val));

            else if (val is List<short>)
                Obj = Utils.ConvertArrayToString<short>(((List<short>)val).ToArray());
            else if (val is short[])
                Obj = Utils.ConvertArrayToString<short>(((short[])val));

            else if (val is List<int>)
                Obj = Utils.ConvertArrayToString<int>(((List<int>)val).ToArray());
            else if (val is int[])
                Obj = Utils.ConvertArrayToString<int>(((int[])val));

            else if (val is List<uint>)
                Obj = Utils.ConvertArrayToString<uint>(((List<uint>)val).ToArray());
            else if (val is uint[])
                Obj = Utils.ConvertArrayToString<uint>(((uint[])val));

            else if (val is List<float>)
                Obj = Utils.ConvertArrayToString<float>(((List<float>)val).ToArray());
            else if (val is float[])
                Obj = Utils.ConvertArrayToString<float>(((float[])val));

            else if (val is List<ulong>)
                Obj = Utils.ConvertArrayToString<ulong>(((List<ulong>)val).ToArray());
            else if (val is ulong[])
                Obj = Utils.ConvertArrayToString<ulong>(((ulong[])val));

            else if (val is List<long>)
                Obj = Utils.ConvertArrayToString<long>(((List<long>)val).ToArray());
            else if (val is long[])
                Obj = Utils.ConvertArrayToString<long>(((long[])val));

            else if (val != null)
                Obj = Escape(val.ToString());

            return Obj;
        }

        /// <summary>
        /// Assigns data loaded from the DB to the DataObject field to which it is to be loaded.
        /// </summary>
        protected Exception GetVal(object Object, MemberInfo info, object val)
        {
            try
            {
                Type type = null;
                if (info is FieldInfo)
                    type = ((FieldInfo)info).FieldType;
                else if (info is PropertyInfo)
                    type = ((PropertyInfo)info).PropertyType;
                else
                    return null;

                object obj;

                if (type == typeof(bool))
                    obj = val.ToString() != "0";
                else if (type == typeof(DateTime))
                {
                    obj = (DateTime)val;
                }
                else if (type == typeof(byte[]))
                    obj = Utils.ConvertStringToArray<byte>(val as string).ToArray();
                else if (type == typeof(List<byte>))
                    obj = Utils.ConvertStringToArray<byte>(val as string);

                else if (type == typeof(short[]))
                    obj = Utils.ConvertStringToArray<short>(val as string).ToArray();
                else if (type == typeof(List<short>))
                    obj = Utils.ConvertStringToArray<short>(val as string);

                else if (type == typeof(ushort[]))
                    obj = Utils.ConvertStringToArray<ushort>(val as string).ToArray();
                else if (type == typeof(List<ushort>))
                    obj = Utils.ConvertStringToArray<ushort>(val as string);

                else if (type == typeof(int[]))
                    obj = Utils.ConvertStringToArray<int>(val as string).ToArray();
                else if (type == typeof(List<int>))
                    obj = Utils.ConvertStringToArray<int>(val as string);

                else if (type == typeof(uint[]))
                    obj = Utils.ConvertStringToArray<uint>(val as string).ToArray();
                else if (type == typeof(List<uint>))
                    obj = Utils.ConvertStringToArray<uint>(val as string);

                else if (type == typeof(long[]))
                    obj = Utils.ConvertStringToArray<long>(val as string).ToArray();
                else if (type == typeof(List<long>))
                    obj = Utils.ConvertStringToArray<long>(val as string);

                else if (type == typeof(ulong[]))
                    obj = Utils.ConvertStringToArray<ulong>(val as string).ToArray();
                else if (type == typeof(List<ulong>))
                    obj = Utils.ConvertStringToArray<ulong>(val as string);

                else if (type == typeof(float[]))
                    obj = Utils.ConvertStringToArray<float>(val as string).ToArray();
                else if (type == typeof(List<float>))
                    obj = Utils.ConvertStringToArray<float>(val as string);
                else if (val is long &&  type == typeof(uint))
                    obj = Convert.ToUInt32(val);
                else if (val is int && type == typeof(ushort))
                    obj = Convert.ToUInt16(val);
                else if (val is int && type == typeof(uint))
                    obj = Convert.ToUInt32(val);
                else if (val is long && type == typeof(ulong))
                    obj = Convert.ToUInt64(val);
                else if (val is ushort && type == typeof(sbyte))
                    obj = Convert.ToSByte(val);
                else if (val is short && type == typeof(sbyte))
                    obj = Convert.ToSByte(val);
                else
                    obj = val;

                if (info is PropertyInfo)
                {
                    ((PropertyInfo)info).SetValue(Object, obj, null);
                }
                else ((FieldInfo)info).SetValue(Object, obj);

            }
            catch (Exception e)
            {
                return e;
            }

            return null;
        }

        #endregion

        #region Insert/Update/Delete

        private readonly StringBuilder _opBuilder = new StringBuilder(2048);
        private readonly StringBuilder _whereBuilder = new StringBuilder(1024);

        private string FormulateInsert(DataObject dataObject, out bool hasRelations)
        {
            string tableName = dataObject.TableName;

            var columns = new StringBuilder();
            var values = new StringBuilder();

            BindingInfo[] bindInfos = GetBindingInfo(dataObject.GetType());

            hasRelations = false;
            bool first = true;
            string dateFormat = Connection.GetDBDateFormat();

            if (TableDatasets[tableName].RequiresObjectId)
            {
                first = false;

                if (dataObject.ObjectId == null)
                    dataObject.ObjectId = IDGenerator.GenerateID();

                columns.Append("[" + tableName + "_ID]");
                values.Append("'" + Escape(dataObject.ObjectId) + "'");
            }

            foreach (BindingInfo bindInfo in bindInfos)
            {
                if (bindInfo.HasRelation)
                    hasRelations = true;

                // Check whether or not this class member is a primary key or a data element
                // and is thus valid for saving
                if (bindInfo.PrimaryKey || bindInfo.DataElementAttribute != null)
                {
                    object val = null;
                    if (bindInfo.Member is PropertyInfo)
                        val = ((PropertyInfo)bindInfo.Member).GetValue(dataObject, null);
                    else if (bindInfo.Member is FieldInfo)
                        val = ((FieldInfo)bindInfo.Member).GetValue(dataObject);

                    if (!first)
                    {
                        columns.Append(", ");
                        values.Append(", ");
                    }
                    else
                        first = false;

                    columns.Append("[" + bindInfo.Member.Name + "]");

                    val = ConvertVal(val, dateFormat);

                    values.Append('\'');
                    values.Append(val);
                    values.Append('\'');
                }
            }

            return "INSERT INTO [" + Connection.SchemaName + "].[" + tableName + "] (" + columns + ") VALUES (" + values + ")";
        }

        private string FormulateUpdate(DataObject dataObject, out bool hasRelations)
        {
            string tableName = dataObject.TableName;

            _opBuilder.Clear();
            _opBuilder.Append("UPDATE [" + Connection.SchemaName + "].[" + tableName + "] SET ");

            _whereBuilder.Clear();
            _whereBuilder.Append(" WHERE ");

            BindingInfo[] bindInfos = GetBindingInfo(dataObject.GetType());
            hasRelations = false;
            bool first = true;
            bool firstPK = true;
            string dateFormat = Connection.GetDBDateFormat();

            foreach (BindingInfo bind in bindInfos)
            {
                if (bind.ReadOnly)
                    continue;

                if (!hasRelations)
                    hasRelations = bind.HasRelation;

                // Add PKs to the WHERE clause.
                if (bind.PrimaryKey)
                {
                    object val;
                    if (bind.Member is PropertyInfo)
                        val = ((PropertyInfo)bind.Member).GetValue(dataObject, null);
                    else if (bind.Member is FieldInfo)
                        val = ((FieldInfo)bind.Member).GetValue(dataObject);
                    else
                        continue;

                    if (!firstPK)
                        _whereBuilder.Append(" AND ");
                    else
                        firstPK = false;

                    val = ConvertVal(val, dateFormat);

                    _whereBuilder.Append("[" + bind.Member.Name + "] = ");
                    _whereBuilder.Append('\'');
                    _whereBuilder.Append(val);
                    _whereBuilder.Append('\'');

                }

                // Add other elements to the SET clause.
                else if (!bind.HasRelation)
                {
                    object val;
                    if (bind.Member is PropertyInfo)
                        val = ((PropertyInfo)bind.Member).GetValue(dataObject, null);
                    else if (bind.Member is FieldInfo)
                        val = ((FieldInfo)bind.Member).GetValue(dataObject);
                    else
                        continue;

                    if (!first)
                        _opBuilder.Append(", ");
                    else
                        first = false;

                    val = ConvertVal(val, dateFormat);

                    _opBuilder.Append("[" + bind.Member.Name + "] = ");
                    _opBuilder.Append('\'');
                    _opBuilder.Append(val);
                    _opBuilder.Append('\'');
                }
            }

            // Object has no PKs within its elements, so use the ObjectId to target the save.
            if (firstPK)
                _whereBuilder.Append($"[{tableName}_ID] = '{Escape(dataObject.ObjectId)}'");

            _opBuilder.Append(_whereBuilder);

            return _opBuilder.ToString();
        }

        private string FormulateDelete(DataObject dataObject)
        {
            _opBuilder.Clear();
            _opBuilder.Append("DELETE FROM [");
            _opBuilder.Append(Connection.SchemaName + "].[" + dataObject.TableName);
            _opBuilder.Append("]");

            _whereBuilder.Clear();
            _whereBuilder.Append(" WHERE ");

            BindingInfo[] bindInfos = GetBindingInfo(dataObject.GetType());
            bool firstPK = true;
            string dateFormat = Connection.GetDBDateFormat();

            foreach (BindingInfo bind in bindInfos)
            {
                // Add PKs to the WHERE clause.
                if (bind.PrimaryKey)
                {
                    object val;
                    if (bind.Member is PropertyInfo)
                        val = ((PropertyInfo)bind.Member).GetValue(dataObject, null);
                    else if (bind.Member is FieldInfo)
                        val = ((FieldInfo)bind.Member).GetValue(dataObject);
                    else
                        continue;

                    if (!firstPK)
                        _whereBuilder.Append(" AND ");
                    else
                        firstPK = false;

                    val = ConvertVal(val, dateFormat);

                    _whereBuilder.Append("[" + bind.Member.Name + "] = ");
                    _whereBuilder.Append('\'');
                    _whereBuilder.Append(val);
                    _whereBuilder.Append('\'');

                }
            }

            // Object has no PKs within its elements, so use the ObjectId to target the save.
            if (firstPK)
                _whereBuilder.Append($"[{dataObject.TableName}_ID] = '{Escape(dataObject.ObjectId)}'");

            return _opBuilder.Append(_whereBuilder).ToString();
        }

        /// <summary>
        /// Adds a new object to the database.
        /// </summary>
        protected override bool AddObjectImpl(DataObject dataObject)
        {
            try
            {
                bool hasRelations = false;

                string sql = FormulateInsert(dataObject, out hasRelations);

                Log.Debug("mssqlObject", sql);

                int res = Connection.ExecuteNonQuery(sql);
                if (res == 0)
                {
                    Log.Error("mssqlObject", "Add Error : " + dataObject.TableName + " ID=" + dataObject.ObjectId + "Query = " + sql);
                    return false;
                }

                if (hasRelations)
                    SaveObjectRelations(dataObject);

                dataObject.Dirty = false;
                dataObject.IsValid = true;
                dataObject.IsDeleted = false;

                return true;
            }
            catch (Exception e)
            {
                Log.Error("mssqlObject", "Add Error : " + dataObject.TableName + " " + dataObject.ObjectId + e);
            }

            return false;
        }

        /// <summary>
        /// Updates an existing object in the database.
        /// </summary>
        protected override void SaveObjectImpl(DataObject dataObject)
        {
            bool hasRelations;

            string sql = FormulateUpdate(dataObject, out hasRelations);

            try
            {
                
                Log.Debug("mssqlObject", sql);

                int res = Connection.ExecuteNonQuery(sql);

                if (res == 0)
                {
                    Log.Error("mssqlObject", "Modify error : " + dataObject.TableName + " ID=" + dataObject.ObjectId + " --- keyvalue changed? " + sql);
                    dataObject.Dirty = false;
                    dataObject.IsValid = false;
                    dataObject.IsDeleted = true;
                    return;
                }

                if (hasRelations)
                    SaveObjectRelations(dataObject);

                dataObject.Dirty = false;
                dataObject.IsValid = true;
            }
            catch (Exception e)
            {
                Log.Error("mssqlObject", "Modify error : " + dataObject.TableName + " " + dataObject.ObjectId + e + " SQL:"+ sql);
            }
        }

        /// <summary>
        /// Removes an object from the database.
        /// </summary>
        protected override void DeleteObjectImpl(DataObject dataObject)
        {
            try
            {
                string sql = FormulateDelete(dataObject);

                Log.Debug("mssqlObject", sql);

                int result = Connection.ExecuteNonQuery(sql);
                if (result == 0)
                    Log.Error("mssqlObject", "Delete Object : " + dataObject.TableName + " failed! ID=" + dataObject.ObjectId + " " + Environment.StackTrace);

                dataObject.IsValid = false;

                //DeleteFromCache(dataObject.TableName, dataObject);
                DeleteObjectRelations(dataObject);

                dataObject.IsDeleted = true;
            }
            catch (Exception e)
            {
                throw new DatabaseException("Delete Databaseobject failed!", e);
            }
        }

        #endregion

        #region Transactions

        protected override bool RunTransaction(List<DataObject> dataObjects)
        {
            long startTime = TCPManager.GetTimeStampMS();

           SqlConnection sqlConn = (SqlConnection)Connection.GetConnection();
           SqlCommand sqlCommand = sqlConn.CreateCommand();
           SqlTransaction transaction = sqlConn.BeginTransaction();

            sqlCommand.Connection = sqlConn;
            sqlCommand.Transaction = transaction;

            try
            {
                uint addsThisPass = 0;
                uint savesThisPass = 0;
                uint deletesThisPass = 0;
                bool rel;

                foreach (var dataObject in dataObjects)
                {
                    switch (dataObject.pendingOp)
                    {
                        case DatabaseOp.DOO_Insert:
                            sqlCommand.CommandText = FormulateInsert(dataObject, out rel);
                            sqlCommand.ExecuteNonQuery();
                            ++addsThisPass; break;
                        case DatabaseOp.DOO_Update:
                            sqlCommand.CommandText = FormulateUpdate(dataObject, out rel);
                            sqlCommand.ExecuteNonQuery();
                            ++savesThisPass; break;
                        case DatabaseOp.DOO_Delete:
                            sqlCommand.CommandText = FormulateDelete(dataObject);
                            sqlCommand.ExecuteNonQuery();
                            ++deletesThisPass; break;
                    }
                }

                transaction.Commit();

                Log.Debug(Connection.SchemaName, "Transaction committed: " + dataObjects.Count + " objects in " + (TCPManager.GetTimeStampMS() - startTime) + "ms. Added " + addsThisPass + ", updated " + savesThisPass + ", deleted " + deletesThisPass + ".");

                foreach (DataObject d in dataObjects)
                {
                    d.Dirty = false;
                    d.UpdateDBStatus();
                }

                return true;
            }
            catch (Exception)
            {
                try
                {
                    transaction.Rollback();
                    return false;
                }
                catch (Exception)
                {
                    Log.Error(Connection.SchemaName, "Transaction rollback FAILED");
                    return false;
                }
            }
            finally
            {
                sqlConn.Close();
            }
        }

        #endregion

        #region Locators

        protected override DataObject FindObjectByKeyImpl(Type objectType, object key)
        {
            MemberInfo[] members = objectType.GetMembers();
            var ret = Activator.CreateInstance(objectType) as DataObject;

            string tableName = ret.TableName;
            DataTableHandler dth = TableDatasets[tableName];
            string whereClause = null;

            if (dth.UsesPreCaching)
            {
                DataObject obj = dth.GetPreCachedObject(key);
                if (obj != null)
                    return obj;
            }

            // Escape PK value
            key = Escape(key.ToString());

            for (int i = 0; i < members.Length; i++)
            {
                object[] keyAttrib = members[i].GetCustomAttributes(typeof(PrimaryKey), true);
                if (keyAttrib.Length > 0)
                {
                    whereClause = "[" + members[i].Name + "] = '" + key + "'";
                    break;
                }
            }

            if (whereClause == null)
            {
                whereClause = "[" + ret.TableName + "_ID] = '" + key + "'";
            }

            var objs = SelectObjectsImpl(objectType, whereClause, IsolationLevel.DEFAULT);
            if (objs.Length > 0)
            {
                dth.SetPreCachedObject(key, objs[0]);
                return objs[0];
            }

            return null;
        }

        // Retourne l'objet a partir de sa primary key
        protected override TObject FindObjectByKeyImpl<TObject>(object key)
        {
            MemberInfo[] members = typeof(TObject).GetMembers();
            var ret = (TObject)Activator.CreateInstance(typeof(TObject));

            string tableName = ret.TableName;
            DataTableHandler dth = TableDatasets[tableName];
            string whereClause = null;

            if (dth.UsesPreCaching)
            {
                DataObject obj = dth.GetPreCachedObject(key);
                if (obj != null)
                    return obj as TObject;
            }

            // Escape PK value
            key = Escape(key.ToString());

            for (int i = 0; i < members.Length; i++)
            {
                object[] keyAttrib = members[i].GetCustomAttributes(typeof(PrimaryKey), true);
                if (keyAttrib.Length > 0)
                {
                    whereClause = "[" + members[i].Name + "] = '" + key + "'";
                    break;
                }
            }

            if (whereClause == null)
            {
                whereClause = "[" + ret.TableName + "_ID] = '" + key + "'";
            }

            var objs = SelectObjectsImpl<TObject>(whereClause, IsolationLevel.DEFAULT);
            if (objs.Count > 0)
            {
                dth.SetPreCachedObject(key, objs[0]);
                return objs[0];
            }

            return null;
        }

        #endregion

        #region Select

        // Sélectionne tous les objets d'une table
        protected override DataObject[] SelectObjectsImpl(Type objectType, string whereClause, IsolationLevel isolation)
        {
            string tableName = GetTableOrViewName(objectType);
            var dataObjects = new List<DataObject>(64);
            bool useObjectID = TableDatasets[tableName].RequiresObjectId;

            // build sql command
            var sb = new StringBuilder("SELECT ");
            if (useObjectID)
                sb.Append("[" + tableName + "_ID], ");

            bool first = true;

            BindingInfo[] bindingInfo = GetBindingInfo(objectType);
            foreach (BindingInfo bind in bindingInfo)
            {
                if (!bind.HasRelation)
                {
                    if (!first)
                        sb.Append(", ");
                    else
                        first = false;
                    sb.Append("[" + bind.Member.Name + "]");
                }
            }

            sb.Append(" FROM [" + Connection.SchemaName + "].[" + tableName + "]");

            if (whereClause != null && whereClause.Trim().Length > 0)
            {
                sb.Append(" WHERE " + whereClause);
            }

            string sql = sb.ToString();

            Log.Debug("mssqlObject", "DataObject[] SelectObjectsImpl: " + sql);

            int objCount = 0;

            // read data and fill objects
            Connection.ExecuteSelect(sql, (reader) =>
            {
                var data = new object[reader.FieldCount];
                while (reader.Read())
                {
                    objCount++;

                    reader.GetValues(data);

                    DataObject obj = Activator.CreateInstance(objectType) as DataObject;

                    int field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        if (data[0] != null)
                            obj.ObjectId = (string)data[0];
                    }
                    bool hasRelations = false;

                    // we can use hard index access because we iterate the same order here
                    foreach (BindingInfo bind in bindingInfo)
                    {
                        if (!hasRelations)
                        {
                            hasRelations = bind.HasRelation;
                        }

                        if (!bind.HasRelation)
                        {
                            object val = data[field++];
                            if (val != null && !val.GetType().IsInstanceOfType(DBNull.Value))
                            {
                                Exception e = GetVal(obj, bind.Member, val);
                                if (e != null)
                                {
                                    Log.Error("mssqlObject",
                                        tableName + ": " + bind.Member.Name + " = " + val.GetType().FullName +
                                        " doesnt fit to " + bind.Member.DeclaringType.FullName + e);
                                }
                            }
                        }
                    }

                    dataObjects.Add(obj);
                    obj.Dirty = false;

                    if (hasRelations)
                    {
                        FillLazyObjectRelations(obj, true);
                    }

                    obj.IsValid = true;
                    obj.AllowAdd = false; // Exists already
                }
            }
            , isolation);

            return dataObjects.ToArray();
        }

        public override string SqlCommand_CharLength()
        {
            return "LEN";
        }


        // Sélectionne tous les objets d'une table
        protected override IList<TObject> SelectObjectsImpl<TObject>(string whereClause, IsolationLevel isolation)
        {
            string tableName = GetTableOrViewName(typeof(TObject));
            var dataObjects = new List<TObject>(64);
            bool useObjectID = TableDatasets[tableName].RequiresObjectId;

            // build sql command
            var sb = new StringBuilder("SELECT ");
            if (useObjectID)
                sb.Append("[" + tableName + "_ID], ");

            bool first = true;

            BindingInfo[] bindingInfo = GetBindingInfo(typeof(TObject));
            for (int i = 0; i < bindingInfo.Length; i++)
            {
                if (!bindingInfo[i].HasRelation)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }
                    sb.Append("[" + bindingInfo[i].Member.Name + "]");
                }
            }

            sb.Append(" FROM [" + Connection.SchemaName + "].[" + tableName + "]");

            if (whereClause != null && whereClause.Trim().Length > 0)
            {
                sb.Append(" WHERE " + whereClause);
            }

            string sql = sb.ToString();

            Log.Debug("mssqlObject", "IList<TObject> SelectObjectsImpl: " + sql);

            // read data and fill objects
            Connection.ExecuteSelect(sql, (reader) =>
            {
                var data = new object[reader.FieldCount];
                while (reader.Read())
                {
                    reader.GetValues(data);

                    TObject obj = Activator.CreateInstance(typeof(TObject)) as TObject;

                    int field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        if (data[0] != null)
                            obj.ObjectId = (string)data[0];
                    }

                    bool hasRelations = false;
                    // we can use hard index access because we iterate the same order here
                    for (int i = 0; i < bindingInfo.Length; i++)
                    {
                        BindingInfo bind = bindingInfo[i];
                        if (!hasRelations)
                        {
                            hasRelations = bind.HasRelation;
                        }

                        if (!bind.HasRelation)
                        {
                            object val = data[field++];
                            if (val != null && !val.GetType().IsInstanceOfType(DBNull.Value))
                            {
                                Exception e = GetVal(obj, bind.Member, val);
                                if (e != null)
                                {
                                    Log.Error("mssqlObject",
                                tableName + ": " + bind.Member.Name + " = " + val.GetType().FullName +
                                " doesnt fit to " + bind.Member.DeclaringType.FullName + e);
                                    continue;
                                }
                            }
                        }
                    }

                    dataObjects.Add(obj);
                    obj.Dirty = false;

                    if (hasRelations)
                    {
                        FillLazyObjectRelations(obj, true);
                    }

                    obj.IsValid = true;
                    obj.AllowAdd = false; // exists already
                }
            }
                , isolation);

            return dataObjects;
        }

        // Sélectionne tous les objets d'une table
        protected override IList<TObject> SelectAllObjectsImpl<TObject>(IsolationLevel isolation)
        {
            return SelectObjectsImpl<TObject>("", isolation);
        }

        #endregion

        #region General DB accessor

        ///<summary>Returns the next auto-increment for the supplied object.</summary> 
        protected override int GetNextAutoIncrementImpl<TObject>()
        {
            string sqlQuery = "SELECT * FROM information_schema.TABLES WHERE TABLE_NAME = '" + GetTableOrViewName(typeof(TObject)) + "'";
            int nextAutoIncrement = 0;

            if (Connection.IsSQLConnection)
            {
                Connection.ExecuteSelect(sqlQuery, (r) =>
                {
                    var reader = (SqlDataReader)r;
                    if (!reader.HasRows)
                        return;

                    while (reader.Read())
                        nextAutoIncrement = Convert.ToInt32(reader.GetInt64(0));//"AUTO_INCREMENT"));

                }, IsolationLevel.DEFAULT);
            }
            return nextAutoIncrement;
        }

        /// <summary>
        /// Returns the number of this object type in the DB, optionally matching the WHERE condition.
        /// </summary>
        protected override int GetObjectCountImpl<TObject>(string where)
        {
            string tableName = GetTableOrViewName(typeof(TObject));

            if (Connection.IsSQLConnection)
            {
                string query = "SELECT COUNT(*) FROM [" + Connection.SchemaName + "].[" + tableName + "]";
                if (where != "")
                    query += " WHERE " + where;

                object count = Connection.ExecuteScalar(query);
                if (count is long)
                    return (int)((long)count);

                return (int)count;
            }

            return 0;
        }

        /// <summary>
        /// Gets the highest value for the supplied column.
        /// </summary>
        protected override long GetMaxColValueImpl<TObject>(string column)
        {
            string tableName = GetTableOrViewName(typeof(TObject));

            if (Connection.IsSQLConnection)
            {
                string query = "SELECT MAX(" + column + ") FROM " + Connection.SchemaName + "].[" + tableName;

                object count = Connection.ExecuteScalar(query);
                return Convert.ToInt64(count);
            }

            return 0;
        }

        #endregion

        /// <summary>
        /// Executes a non-blocking request.
        /// </summary>
        protected override bool ExecuteNonQueryImpl(string rawQuery)
        {
            try
            {
                Log.Debug("mssqlObject", rawQuery);

                int res = Connection.ExecuteNonQuery(rawQuery);
                if (res == 0)
                {
                    Log.Error("mssqlObject", "Execution error : " + rawQuery);

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error("mssqlObject", "Execution error : " + rawQuery + e);
            }

            return false;
        }

        protected override long ExecuteQueryIntImpl(string raqQuery)
        {
            throw new NotImplementedException();
        }

        protected override Dictionary<TKey, TObject> MapAllObjectsImpl<TKey, TObject>(string keyName, string where, int expectedRowCount, IsolationLevel isolation)
        {
            throw new NotImplementedException();
        }
    }
}
