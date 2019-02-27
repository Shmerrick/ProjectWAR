using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.IO;
using System.Xml;

using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Threading;

namespace FrameWork
{
    public class MySQLObjectDatabase : ObjectDatabase
    {
        public MySQLObjectDatabase(MySqlDataConnection connection) : base(connection)
        {
        }

        #region Value conversion

        /// <summary>
        /// Converts an input value to the representation that will be stored in the DB.
        /// </summary>
        protected Object ConvertToDatabaseFormat(Object val, String dateFormat)
        {
            Object Obj = null;

            if (val is Boolean)
                Obj = ((Boolean)val) ? 1 : 0;
            else if (val is DateTime)
                Obj = ((DateTime)val).ToString(dateFormat);
            else if (val is Single)
                Obj = ((Single)val).ToString(Nfi);
            else if (val is Double)
                Obj = ((Double)val).ToString(Nfi);
            else if (val is String)
                Obj = Escape((String)val);

            else if (val is List<Byte>)
                Obj = Utils.ConvertArrayToString<Byte>(((List<Byte>)val).ToArray());
            else if (val is Byte[])
                Obj = Utils.ConvertArrayToString<Byte>(((Byte[])val));

            else if (val is List<Int16>)
                Obj = Utils.ConvertArrayToString<Int16>(((List<Int16>)val).ToArray());
            else if (val is Int16[])
                Obj = Utils.ConvertArrayToString<Int16>(((Int16[])val));

            else if (val is List<Int32>)
                Obj = Utils.ConvertArrayToString<Int32>(((List<Int32>)val).ToArray());
            else if (val is Int32[])
                Obj = Utils.ConvertArrayToString<Int32>(((Int32[])val));

            else if (val is List<UInt32>)
                Obj = Utils.ConvertArrayToString<UInt32>(((List<UInt32>)val).ToArray());
            else if (val is UInt32[])
                Obj = Utils.ConvertArrayToString<UInt32>(((UInt32[])val));

            else if (val is List<Single>)
                Obj = Utils.ConvertArrayToString<Single>(((List<Single>)val).ToArray());
            else if (val is Single[])
                Obj = Utils.ConvertArrayToString<Single>(((Single[])val));

            else if (val is List<UInt64>)
                Obj = Utils.ConvertArrayToString<UInt64>(((List<UInt64>)val).ToArray());
            else if (val is UInt64[])
                Obj = Utils.ConvertArrayToString<UInt64>(((UInt64[])val));

            else if (val is List<Int64>)
                Obj = Utils.ConvertArrayToString<Int64>(((List<Int64>)val).ToArray());
            else if (val is Int64[])
                Obj = Utils.ConvertArrayToString<Int64>(((Int64[])val));

            else if (val != null)
                Obj = Escape(val.ToString());

            return Obj;
        }

        protected Object ConvertFromDatabaseFormat(Type type, Object val)
        {
            if (type == typeof(Boolean))
                return Convert.ToBoolean(Convert.ToInt32(val));

            if (type == typeof(DateTime))
            {
                return val is MySqlDateTime ? ((MySqlDateTime)val).GetDateTime() : (Object)((IConvertible)val).ToDateTime(null);
            }

            if (!(val is String) || type == typeof(String))
                return val;

            if (type == typeof(Byte[]))
                return Utils.ConvertStringToArray<Byte>((String)val).ToArray();
            if (type == typeof(List<Byte>))
                return Utils.ConvertStringToArray<Byte>((String)val);

            if (type == typeof(Int16[]))
                return Utils.ConvertStringToArray<Int16>((String)val).ToArray();
            if (type == typeof(List<Int16>))
                return Utils.ConvertStringToArray<Int16>((String)val);

            if (type == typeof(UInt16[]))
                return Utils.ConvertStringToArray<UInt16>((String)val).ToArray();
            if (type == typeof(List<UInt16>))
                return Utils.ConvertStringToArray<UInt16>((String)val);

            if (type == typeof(Int32[]))
                return Utils.ConvertStringToArray<Int32>((String)val).ToArray();
            if (type == typeof(List<Int32>))
                return Utils.ConvertStringToArray<Int32>((String)val);

            if (type == typeof(UInt32[]))
                return Utils.ConvertStringToArray<UInt32>((String)val).ToArray();
            if (type == typeof(List<UInt32>))
                return Utils.ConvertStringToArray<UInt32>((String)val);

            if (type == typeof(Int64[]))
                return Utils.ConvertStringToArray<Int64>((String)val).ToArray();
            if (type == typeof(List<Int64>))
                return Utils.ConvertStringToArray<Int64>((String)val);

            if (type == typeof(UInt64[]))
                return Utils.ConvertStringToArray<UInt64>((String)val).ToArray();
            if (type == typeof(List<UInt64>))
                return Utils.ConvertStringToArray<UInt64>((String)val);

            if (type == typeof(Single[]))
                return Utils.ConvertStringToArray<Single>((String)val).ToArray();
            return type == typeof(List<Single>) ? Utils.ConvertStringToArray<Single>((String)val) : val;
        }

        /// <summary>
        /// Assigns data loaded from the DB to the DataObject field to which it is to be loaded.
        /// </summary>
        protected Exception GetVal(Object Object, MemberInfo info, Object val)
        {
            try
            {
                Type type;
                if (info is FieldInfo)
                    type = ((FieldInfo)info).FieldType;
                else if (info is PropertyInfo)
                    type = ((PropertyInfo)info).PropertyType;
                else
                    return null;

                Object obj = ConvertFromDatabaseFormat(type, val);

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

        /// <summary>
        /// Returns a MySQL statement which would add the given object to the database.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="hasRelations"></param>
        /// <returns></returns>
        private String FormulateInsert(DataObject dataObject, out Boolean hasRelations)
        {
            String tableName = dataObject.TableName;

            var columns = new StringBuilder();
            var values = new StringBuilder();

            BindingInfo[] bindInfos = GetBindingInfo(dataObject.GetType());

            hasRelations = false;
            Boolean first = true;
            String dateFormat = Connection.GetDBDateFormat();

            if (TableDatasets[tableName].RequiresObjectId)
            {
                first = false;

                if (dataObject.ObjectId == null)
                    dataObject.ObjectId = IDGenerator.GenerateID();

                columns.Append("`" + tableName + "_ID`");
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
                    Object val = null;
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

                    columns.Append("`" + bindInfo.Member.Name + "`");

                    val = ConvertToDatabaseFormat(val, dateFormat);

                    values.Append('\'');
                    values.Append(val);
                    values.Append('\'');
                }
            }

            return "INSERT INTO `" + tableName + "` (" + columns + ") VALUES (" + values + ")";
        }

        /// <summary>
        /// Returns a MySQL statement which would update the given object in the database.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="hasRelations"></param>
        /// <returns></returns>
        private String FormulateUpdate(DataObject dataObject, out Boolean hasRelations)
        {
            String tableName = dataObject.TableName;

            _opBuilder.Clear();
            _opBuilder.Append("UPDATE `" + tableName + "` SET ");

            _whereBuilder.Clear();
            _whereBuilder.Append(" WHERE ");

            BindingInfo[] bindInfos = GetBindingInfo(dataObject.GetType());
            hasRelations = false;
            Boolean first = true;
            Boolean firstPK = true;
            String dateFormat = Connection.GetDBDateFormat();

            foreach (BindingInfo bind in bindInfos)
            {
                if (bind.ReadOnly)
                    continue;

                if (!hasRelations)
                    hasRelations = bind.HasRelation;

                // Add PKs to the WHERE clause.
                if (bind.PrimaryKey)
                {
                    Object val;
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

                    val = ConvertToDatabaseFormat(val, dateFormat);

                    _whereBuilder.Append("`" + bind.Member.Name + "` = ");
                    _whereBuilder.Append('\'');
                    _whereBuilder.Append(val);
                    _whereBuilder.Append('\'');

                }

                // Add other elements to the SET clause.
                else if (!bind.HasRelation)
                {
                    Object val;
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

                    val = ConvertToDatabaseFormat(val, dateFormat);

                    _opBuilder.Append("`" + bind.Member.Name + "` = ");
                    _opBuilder.Append('\'');
                    _opBuilder.Append(val);
                    _opBuilder.Append('\'');
                }
            }

            // Object has no PKs within its elements, so use the ObjectId to target the save.
            if (firstPK)
                _whereBuilder.Append($"`{tableName}_ID` = '{Escape(dataObject.ObjectId)}'");

            _opBuilder.Append(_whereBuilder);

            return _opBuilder.ToString();
        }

        /// <summary>
        /// Returns a MySQL statement which would delete this object from the DB.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <returns></returns>
        private String FormulateDelete(DataObject dataObject)
        {
            _opBuilder.Clear();
            _opBuilder.Append("DELETE FROM `");
            _opBuilder.Append(dataObject.TableName);
            _opBuilder.Append("`");

            _whereBuilder.Clear();
            _whereBuilder.Append(" WHERE ");

            BindingInfo[] bindInfos = GetBindingInfo(dataObject.GetType());
            Boolean firstPK = true;
            String dateFormat = Connection.GetDBDateFormat();

            foreach (BindingInfo bind in bindInfos)
            {
                // Add PKs to the WHERE clause.
                if (bind.PrimaryKey)
                {
                    Object val;
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

                    val = ConvertToDatabaseFormat(val, dateFormat);

                    _whereBuilder.Append("`" + bind.Member.Name + "` = ");
                    _whereBuilder.Append('\'');
                    _whereBuilder.Append(val);
                    _whereBuilder.Append('\'');

                }
            }

            // Object has no PKs within its elements, so use the ObjectId to target the save.
            if (firstPK)
                _whereBuilder.Append($"`{dataObject.TableName}_ID` = '{Escape(dataObject.ObjectId)}'");

            return _opBuilder.Append(_whereBuilder).ToString();
        }

        /// <summary>
        /// Adds a new object to the database.
        /// </summary>
        protected override Boolean AddObjectImpl(DataObject dataObject)
        {
            try
            {
                Boolean hasRelations = false;

                String sql = FormulateInsert(dataObject, out hasRelations);

                Log.Debug("MysqlObject", sql);

                Int32 res = Connection.ExecuteNonQuery(sql);
                if (res == 0)
                {
                    Log.Error("MysqlObject", "Add Error : " + dataObject.TableName + " ID=" + dataObject.ObjectId + "Query = " + sql);
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
                Log.Error("MysqlObject", "Add Error : " + dataObject.TableName + " " + dataObject.ObjectId + e);
            }

            return false;
        }

        /// <summary>
        /// Updates an existing object in the database.
        /// </summary>
        protected override void SaveObjectImpl(DataObject dataObject)
        {
            try
            {
                Boolean hasRelations;

                String sql = FormulateUpdate(dataObject, out hasRelations);

                Log.Debug("MysqlObject", sql);

                Int32 res = Connection.ExecuteNonQuery(sql);

                if (res == 0)
                {
                    Log.Error("MysqlObject", "Modify error : " + dataObject.TableName + " ID=" + dataObject.ObjectId + " --- keyvalue changed? " + sql);
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
                Log.Error("MysqlObject", "Modify error : " + dataObject.TableName + " " + dataObject.ObjectId + e);
            }
        }

        /// <summary>
        /// Removes an object from the database.
        /// </summary>
        protected override void DeleteObjectImpl(DataObject dataObject)
        {
            try
            {
                String sql = FormulateDelete(dataObject);

                Log.Debug("MysqlObject", sql);

                Int32 result = Connection.ExecuteNonQuery(sql);
                if (result == 0)
                    Log.Error("MysqlObject", "Delete Object : " + dataObject.TableName + " failed! ID=" + dataObject.ObjectId + " " + Environment.StackTrace);

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

        protected override Boolean RunTransaction(List<DataObject> dataObjects)
        {
            var sqlConn = (MySqlConnection)Connection.GetConnection();
            MySqlCommand sqlCommand = sqlConn.CreateCommand();
            MySqlTransaction transaction = sqlConn.BeginTransaction();

            sqlCommand.Connection = sqlConn;
            sqlCommand.Transaction = transaction;
            
            Boolean rel;

            try {
                foreach (DataObject dataObject in dataObjects) {
                    Thread.Yield();

                    switch (dataObject.pendingOp) {

                        case DatabaseOp.DOO_Insert:
                            sqlCommand.CommandText = FormulateInsert(dataObject, out rel);
                            sqlCommand.ExecuteNonQuery();
                            ++_inserts;
                            break;

                        case DatabaseOp.DOO_Update:
                            sqlCommand.CommandText = FormulateUpdate(dataObject, out rel);
                            sqlCommand.ExecuteNonQuery();
                            ++_updates;
                            break;

                        case DatabaseOp.DOO_Delete:
                            sqlCommand.CommandText = FormulateDelete(dataObject);
                            sqlCommand.ExecuteNonQuery();
                            ++_deletes;
                            break;
                    }
                }
                transaction.Commit();

                Log.Debug(Connection.SchemaName, "Transaction committed: " + dataObjects.Count + " Inserts " + _inserts + ", Updates " + _updates + ", Deletes " + _deletes + ".");

                foreach (DataObject d in dataObjects){
                    Thread.Yield();

                    d.Dirty = false;
                    d.UpdateDBStatus();
                }

                return true;
            }
            catch (Exception e) {
                Log.Error(Connection.SchemaName, e.Message);
            }
            finally {
            }

            try {
                transaction.Rollback();
            }
            catch (Exception e) {
                sqlConn.Close();
                Log.Error(Connection.SchemaName, "Transaction rollback FAILED " + e.Message);
            }

            return false;
        }

        #endregion

        #region Locators

        protected override DataObject FindObjectByKeyImpl(Type objectType, Object key)
        {
            MemberInfo[] members = objectType.GetMembers();
            var ret = Activator.CreateInstance(objectType) as DataObject;

            String tableName = ret.TableName;
            DataTableHandler dth = TableDatasets[tableName];
            String whereClause = null;

            if (dth.UsesPreCaching)
            {
                DataObject obj = dth.GetPreCachedObject(key);
                if (obj != null)
                    return obj;
            }

            // Escape PK value
            key = Escape(key.ToString());

            for (Int32 i = 0; i < members.Length; i++)
            {
                Object[] keyAttrib = members[i].GetCustomAttributes(typeof(PrimaryKey), true);
                if (keyAttrib.Length > 0)
                {
                    whereClause = "`" + members[i].Name + "` = '" + key + "'";
                    break;
                }
            }

            if (whereClause == null)
            {
                whereClause = "`" + ret.TableName + "_ID` = '" + key + "'";
            }

            DataObject[] objs = SelectObjectsImpl(objectType, whereClause, IsolationLevel.DEFAULT);
            if (objs.Length > 0)
            {
                dth.SetPreCachedObject(key, objs[0]);
                return objs[0];
            }

            return null;
        }

        // Retourne l'objet a partir de sa primary key
        protected override TObject FindObjectByKeyImpl<TObject>(Object key)
        {
            MemberInfo[] members = typeof(TObject).GetMembers();
            var ret = (TObject)Activator.CreateInstance(typeof(TObject));

            String tableName = ret.TableName;
            DataTableHandler dth = TableDatasets[tableName];
            String whereClause = null;

            if (dth.UsesPreCaching)
            {
                DataObject obj = dth.GetPreCachedObject(key);
                if (obj != null)
                    return obj as TObject;
            }

            // Escape PK value
            key = Escape(key.ToString());

            for (Int32 i = 0; i < members.Length; i++)
            {
                Object[] keyAttrib = members[i].GetCustomAttributes(typeof(PrimaryKey), true);
                if (keyAttrib.Length > 0)
                {
                    whereClause = "`" + members[i].Name + "` = '" + key + "'";
                    break;
                }
            }

            if (whereClause == null)
            {
                whereClause = "`" + ret.TableName + "_ID` = '" + key + "'";
            }

            IList<TObject> objs = SelectObjectsImpl<TObject>(whereClause, IsolationLevel.DEFAULT);
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
        protected override DataObject[] SelectObjectsImpl(Type objectType, String whereClause, IsolationLevel isolation)
        {
            String tableName = GetTableOrViewName(objectType);
            var dataObjects = new List<DataObject>(64);
            Boolean useObjectID = TableDatasets[tableName].RequiresObjectId;

            // build sql command
            var sb = new StringBuilder("SELECT ");
            if (useObjectID)
                sb.Append("`" + tableName + "_ID`, ");

            Boolean first = true;

            BindingInfo[] bindingInfo = GetBindingInfo(objectType);
            foreach (BindingInfo bind in bindingInfo)
            {
                if (!bind.HasRelation)
                {
                    if (!first)
                        sb.Append(", ");
                    else
                        first = false;
                    sb.Append("`" + bind.Member.Name + "`");
                }
            }

            sb.Append(" FROM `" + tableName + "`");

            if (whereClause != null && whereClause.Trim().Length > 0)
            {
                sb.Append(" WHERE " + whereClause);
            }

            String sql = sb.ToString();

            Log.Debug("MysqlObject", "DataObject[] SelectObjectsImpl: " + sql);

            Int32 objCount = 0;

            // read data and fill objects
            Connection.ExecuteSelect(sql, (reader) =>
            {
                Object[] data = new Object[reader.FieldCount];
                while (reader.Read())
                {
                    objCount++;

                    reader.GetValues(data);

                    var obj = Activator.CreateInstance(objectType) as DataObject;

                    Int32 field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        if (data[0] != null)
                            obj.ObjectId = (String)data[0];
                    }
                    Boolean hasRelations = false;

                    // we can use hard index access because we iterate the same order here
                    foreach (BindingInfo bind in bindingInfo)
                    {
                        if (!hasRelations)
                        {
                            hasRelations = bind.HasRelation;
                        }

                        if (!bind.HasRelation)
                        {
                            Object val = data[field++];
                            if (val != null && !val.GetType().IsInstanceOfType(DBNull.Value))
                            {
                                Exception e = GetVal(obj, bind.Member, val);
                                if (e != null)
                                {
                                    Log.Error("MysqlObject",
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

        // Sélectionne tous les objets d'une table
        protected override IList<TObject> SelectObjectsImpl<TObject>(String whereClause, IsolationLevel isolation)
        {
            String tableName = GetTableOrViewName(typeof(TObject));
            Boolean useObjectID = TableDatasets[tableName].RequiresObjectId;

            // build sql command
            var sb = new StringBuilder("SELECT ");
            if (useObjectID)
                sb.Append("`" + tableName + "_ID`, ");

            Boolean first = true;

            BindingInfo[] bindingInfo = GetBindingInfo(typeof(TObject));
            for (Int32 i = 0; i < bindingInfo.Length; i++)
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
                    sb.Append("`" + bindingInfo[i].Member.Name + "`");
                }
            }

            sb.Append(" FROM `" + tableName + "`");

            if (whereClause != null && whereClause.Trim().Length > 0)
            {
                sb.Append(" WHERE " + whereClause);
            }

            String sql = sb.ToString();

            Log.Debug("MysqlObject", "IList<TObject> SelectObjectsImpl: " + sql);

            switch (TableDatasets[tableName].BindingMethod)
            {
                case EBindingMethod.Reflected:
                    return ReflectionSelect<TObject>(isolation, tableName, sql, bindingInfo, useObjectID);
                case EBindingMethod.StaticBound:
                    return StaticBindSelect<TObject>(isolation, tableName, sql, bindingInfo, useObjectID);
                case EBindingMethod.CompiledExpression:
                    return CompiledExpressionSelect<TObject>(isolation, tableName, sql, bindingInfo, useObjectID);
                case EBindingMethod.Manual:
                    return ManualSelect<TObject>(isolation, tableName, sql, bindingInfo, useObjectID);
                default:
                    throw new DatabaseException($"No valid binding method exists for {tableName}.");
            }
        }

        protected List<TObject> ReflectionSelect<TObject>(IsolationLevel isolation, String tableName, String sqlCommand, BindingInfo[] bindingInfo, Boolean useObjectID) where TObject : DataObject
        {
            var dataObjects = new List<TObject>(64);

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
                Object[] data = new Object[reader.FieldCount];

#if LOGTIME
                long objectLoad = 0;
                long count = 0;
                Stopwatch watch = new Stopwatch();
#endif
                while (reader.Read())
                {
#if LOGTIME
                    watch.Restart();
                    ++count;
#endif
                    reader.GetValues(data);

                    var currentObject = (TObject)Activator.CreateInstance(typeof(TObject));

                    Int32 field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        if (data[0] != null)
                            currentObject.ObjectId = (String)data[0];
                    }
                    Boolean hasRelations = false;

                    // we can use hard index access because we iterate the same order here
                    foreach (BindingInfo bind in bindingInfo)
                    {
                        if (!hasRelations)
                        {
                            hasRelations = bind.HasRelation;
                        }

                        if (!bind.HasRelation)
                        {
                            Object val = data[field++];
                            if (val != null && !val.GetType().IsInstanceOfType(DBNull.Value))
                            {
                                Exception e = GetVal(currentObject, bind.Member, val);
                                if (e != null)
                                {
                                    Log.Error("MysqlObject",
                                        tableName + ": " + bind.Member.Name + " = " + val.GetType().FullName +
                                        " doesnt fit to " + bind.Member.DeclaringType.FullName + e);
                                }
                            }
                        }
                    }

                    dataObjects.Add(currentObject);

                    if (hasRelations)
                    {
                        FillLazyObjectRelations(currentObject, true);
                    }

                    currentObject.IsValid = true;
                    currentObject.AllowAdd = false; // exists already
                    currentObject.Dirty = false;

#if LOGTIME
                    watch.Stop();
                    objectLoad += watch.ElapsedTicks;
#endif
                }

#if LOGTIME
                if (count > 0)
                    Log.Notice(tableName, $"ReflectionSelect object loading time: {objectLoad / (float)Stopwatch.Frequency} seconds, average ticks per object: {objectLoad / count}");
#endif
            }, isolation);

            return dataObjects;
        }

        protected List<TObject> StaticBindSelect<TObject>(IsolationLevel isolation, String tableName, String sqlCommand, BindingInfo[] bindingInfo, Boolean useObjectID) where TObject : DataObject
        {
            var dataObjects = new List<TObject>(64);

            ObjectPool<StaticMemberBindInfo> pool = GetStaticBindPool<TObject>();

            StaticMemberBindInfo staticBindInfo = pool.Dequeue();

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
                Object[] data = new Object[reader.FieldCount];

#if LOGTIME
                long objectLoad = 0;
                long count = 0;

                Stopwatch watch = new Stopwatch();
#endif

                while (reader.Read())
                {
#if LOGTIME
                    watch.Restart();
                    ++count;
#endif

                    reader.GetValues(data);

                    //TObject obj = Activator.CreateInstance(typeof(TObject)) as TObject;
                    var currentObject = (TObject)staticBindInfo.AssignObject;

                    Int32 field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        if (data[0] != null)
                            currentObject.ObjectId = (String)data[0];
                    }

                    Boolean hasRelations = false;
                    // we can use hard index access because we iterate the same order here
                    for (Int32 i = 0; i < bindingInfo.Length; i++)
                    {
                        BindingInfo bind = bindingInfo[i];

                        if (staticBindInfo.DataBinders[i] == null)
                        {
                            if (bind.Member is FieldInfo)
                                Log.Error($"StaticBindSelect on {tableName}", $"{((FieldInfo)bind.Member).Name} is a field. StaticBindSelect will only work with properties.");
                            else
                                Log.Error($"StaticBindSelect on {tableName}", $"Could not find a data binder for the property {((PropertyInfo)bind.Member).Name}.");

                            Environment.Exit(1);
                        }

                        if (!hasRelations)
                        {
                            hasRelations = bind.HasRelation;
                        }

                        if (!bind.HasRelation)
                        {
                            Object val = data[field++];
                            Object obj = null;

                            if (val != null && !Convert.IsDBNull(val))
                                obj = ConvertFromDatabaseFormat(((PropertyInfo)bind.Member).PropertyType, val);

                            try
                            {
                                staticBindInfo.DataBinders[i].Assign(obj);
                            }
                            catch (InvalidCastException)
                            {
                                Log.Error(tableName, "Failed to cast " + ((PropertyInfo)bind.Member).Name);
                            }
                        }
                    }

                    currentObject = (TObject)currentObject.Clone();

                    dataObjects.Add(currentObject);

                    if (hasRelations)
                        FillLazyObjectRelations(currentObject, true);

                    currentObject.IsValid = true;
                    currentObject.AllowAdd = false; // exists already
                    currentObject.Dirty = false;

#if LOGTIME
                    watch.Stop();
                    objectLoad += watch.ElapsedTicks;
#endif
                }

#if LOGTIME
                if (count > 0)
                    Log.Notice(tableName, $"StaticBindSelect object loading time: {objectLoad / (float)Stopwatch.Frequency} seconds, average ticks per object: {objectLoad / count}");
#endif
            }, isolation);

            pool.Enqueue(staticBindInfo);

            return dataObjects;
        }

        protected List<TObject> CompiledExpressionSelect<TObject>(IsolationLevel isolation, String tableName, String sqlCommand, BindingInfo[] bindingInfo, Boolean useObjectID) where TObject : DataObject
        {
            var dataObjects = new List<TObject>(64);

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
#if LOGTIME
                long objectLoad = 0;
                long count = 0;

                Stopwatch watch = new Stopwatch();
#endif

                var mySqlReader = (MySqlDataReader)reader;
                while (mySqlReader.Read())
                {
#if LOGTIME
                    watch.Restart();
                    ++count;
#endif

                    var currentObject = (TObject)Activator.CreateInstance(typeof(TObject));

                    Int32 field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        currentObject.ObjectId = reader.GetString(0);
                    }

                    Boolean hasRelations = false;
                    // we can use hard index access because we iterate the same order here
                    for (Int32 i = 0; i < bindingInfo.Length; i++)
                    {
                        BindingInfo bind = bindingInfo[i];
                        if (!hasRelations)
                            hasRelations = bind.HasRelation;

                        if (!bind.HasRelation && bind.MySqlBinder != null && !mySqlReader.IsDBNull(field))
                        {
                            // Value type
                            if (((PropertyInfo)bind.Member).PropertyType.IsValueType && !((PropertyInfo)bind.Member).PropertyType.IsEnum)
                            {
                                //try
                                //{
                                bind.MySqlBinder.Assign(currentObject, mySqlReader, field);
                                //}
                                //catch (Exception)
                                //{
                                //    Log.Error(tableName, $"Failed to assign value type {((PropertyInfo)bind.Member).PropertyType.FullName} (from DB column: {mySqlReader.GetName(field)}) to {((PropertyInfo)bind.Member).Name}");
                                //    Environment.Exit(0);
                                //}
                            }

                            else
                            {
                                //try
                                //{
                                Object obj = ConvertFromDatabaseFormat(((PropertyInfo)bind.Member).PropertyType, mySqlReader.GetValue(field));
                                bind.MySqlBinder.AssignObject(currentObject, obj);
                                //}
                                //catch (Exception)
                                //{
                                //    Log.Error(tableName, $"Failed to assign reference type {((PropertyInfo) bind.Member).PropertyType.FullName} to {((PropertyInfo) bind.Member).Name}");
                                //}
                            }
                        }

                        ++field;
                    }

                    dataObjects.Add(currentObject);

                    if (hasRelations)
                    {
                        FillLazyObjectRelations(currentObject, true);
                    }

                    currentObject.IsValid = true;
                    currentObject.AllowAdd = false; // exists already
                    currentObject.Dirty = false;

#if LOGTIME
                    watch.Stop();
                    objectLoad += watch.ElapsedTicks;
#endif
                }

#if LOGTIME
                if (count > 0)
                    Log.Notice(tableName, $"CompiledExpressionSelect object loading time: {objectLoad / (float)Stopwatch.Frequency} seconds, average ticks per object: {objectLoad / count}");
#endif
            }, isolation);

            return dataObjects;
        }

        protected List<TObject> ManualSelect<TObject>(IsolationLevel isolation, String tableName, String sqlCommand, BindingInfo[] bindingInfo, Boolean useObjectID) where TObject : DataObject
        {
            var dataObjects = new List<TObject>(64);

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
#if LOGTIME
                long objectLoad = 0;
                long count = 0;

                Stopwatch watch = new Stopwatch();

#endif

                var mySqlReader = (MySqlDataReader)reader;
                while (mySqlReader.Read())
                {
#if LOGTIME
                    watch.Restart();
                    ++count;
#endif

                    var currentObject = (TObject)Activator.CreateInstance(typeof(TObject));

                    Int32 field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        currentObject.ObjectId = reader.GetString(0);
                    }

                    currentObject.Load(mySqlReader, field);

                    dataObjects.Add(currentObject);

                    currentObject.IsValid = true;
                    currentObject.AllowAdd = false; // exists already
                    currentObject.Dirty = false;

#if LOGTIME
                    watch.Stop();
                    objectLoad += watch.ElapsedTicks;
#endif
                }

#if LOGTIME
                if (count > 0)
                    Log.Notice(tableName, $"Object loading time: {objectLoad / (float)Stopwatch.Frequency} seconds, average ticks per object: {objectLoad / count}");
#endif
            }, isolation);

            return dataObjects;
        }

        // Sélectionne tous les objets d'une table
        protected override IList<TObject> SelectAllObjectsImpl<TObject>(IsolationLevel isolation) => SelectObjectsImpl<TObject>("", isolation);

        #endregion

        #region Map

        protected override Dictionary<TKey, TObject> MapAllObjectsImpl<TKey, TObject>(String keyName, String whereClause, Int32 expectedRowCount, IsolationLevel isolation)
        {
            String tableName = GetTableOrViewName(typeof(TObject));
            Boolean useObjectID = TableDatasets[tableName].RequiresObjectId;

            // build sql command
            var sb = new StringBuilder("SELECT ");
            if (useObjectID)
                sb.Append("`" + tableName + "_ID`, ");

            Boolean first = true;

            BindingInfo[] bindingInfo = GetBindingInfo(typeof(TObject));
            Int32 keyIndex = -1;
            for (Int32 i = 0; i < bindingInfo.Length; i++)
            {
                if (!bindingInfo[i].HasRelation)
                {
                    if (!first)
                        sb.Append(", ");
                    else
                        first = false;
                    String name = bindingInfo[i].Member.Name;
                    sb.Append("`" + name + "`");

                    if (name.Equals(keyName, StringComparison.InvariantCultureIgnoreCase))
                        keyIndex = i;
                }
            }
            if (keyIndex == -1)
                throw new ArgumentException("Could not find key column of name : " + keyName, "keyName");

            sb.Append(" FROM `" + tableName + "`");

            if (whereClause != null && whereClause.Trim().Length > 0)
            {
                sb.Append(" WHERE " + whereClause);
            }

            String sql = sb.ToString();

            Log.Debug("MysqlObject", "IList<TObject> SelectObjectsImpl: " + sql);

            switch (TableDatasets[tableName].BindingMethod)
            {
                case EBindingMethod.Reflected:
                    return ReflectionMap<TKey, TObject>(keyIndex, expectedRowCount, isolation, tableName, sql, bindingInfo, useObjectID);
                case EBindingMethod.StaticBound:
                    return StaticBindMap<TKey, TObject>(keyIndex, expectedRowCount, isolation, tableName, sql, bindingInfo, useObjectID);
                case EBindingMethod.CompiledExpression:
                    return CompiledExpressionMap<TKey, TObject>(keyIndex, expectedRowCount, isolation, tableName, sql, bindingInfo, useObjectID);
                case EBindingMethod.Manual:
                    return ManualMap<TKey, TObject>(keyIndex, expectedRowCount, isolation, tableName, sql, bindingInfo, useObjectID);
                default:
                    throw new DatabaseException($"No valid binding method exists for {tableName}.");
            }
        }

        protected Dictionary<TKey, TObject> ReflectionMap<TKey, TObject>(Int32 keyIndex, Int32 expectedRowCount, IsolationLevel isolation, String tableName, String sqlCommand, BindingInfo[] bindingInfo, Boolean useObjectID) where TObject : DataObject
        {
            var dataObjects = new Dictionary<TKey, TObject>(expectedRowCount);

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
                Object[] data = new Object[reader.FieldCount];

#if LOGTIME
                long objectLoad = 0;
                long count = 0;
                Stopwatch watch = new Stopwatch();
#endif
                while (reader.Read())
                {
#if LOGTIME
                    watch.Restart();
                    ++count;
#endif
                    reader.GetValues(data);

                    var currentObject = (TObject)Activator.CreateInstance(typeof(TObject));

                    Int32 field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        if (data[0] != null)
                            currentObject.ObjectId = (String)data[0];
                    }
                    Boolean hasRelations = false;

                    // we can use hard index access because we iterate the same order here
                    foreach (BindingInfo bind in bindingInfo)
                    {
                        if (!hasRelations)
                        {
                            hasRelations = bind.HasRelation;
                        }

                        if (!bind.HasRelation)
                        {
                            Object val = data[field++];
                            if (val != null && !val.GetType().IsInstanceOfType(DBNull.Value))
                            {
                                Exception e = GetVal(currentObject, bind.Member, val);
                                if (e != null)
                                {
                                    Log.Error("MysqlObject",
                                        tableName + ": " + bind.Member.Name + " = " + val.GetType().FullName +
                                        " doesnt fit to " + bind.Member.DeclaringType.FullName + e);
                                }
                            }
                        }
                    }

                    dataObjects[(TKey)data[keyIndex]] = currentObject;

                    if (hasRelations)
                    {
                        FillLazyObjectRelations(currentObject, true);
                    }

                    currentObject.IsValid = true;
                    currentObject.AllowAdd = false; // exists already
                    currentObject.Dirty = false;

#if LOGTIME
                    watch.Stop();
                    objectLoad += watch.ElapsedTicks;
#endif
                }

#if LOGTIME
                if (count > 0)
                    Log.Notice(tableName, $"ReflectionSelect object loading time: {objectLoad / (float)Stopwatch.Frequency} seconds, average ticks per object: {objectLoad / count}");
#endif
            }, isolation);

            return dataObjects;
        }

        protected Dictionary<TKey, TObject> StaticBindMap<TKey, TObject>(Int32 keyIndex, Int32 expectedRowCount, IsolationLevel isolation, String tableName, String sqlCommand, BindingInfo[] bindingInfo, Boolean useObjectID) where TObject : DataObject
        {
            var dataObjects = new Dictionary<TKey, TObject>(expectedRowCount);

            ObjectPool<StaticMemberBindInfo> pool = GetStaticBindPool<TObject>();

            StaticMemberBindInfo staticBindInfo = pool.Dequeue();

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
                Object[] data = new Object[reader.FieldCount];

#if LOGTIME
                long objectLoad = 0;
                long count = 0;

                Stopwatch watch = new Stopwatch();
#endif

                while (reader.Read())
                {
#if LOGTIME
                    watch.Restart();
                    ++count;
#endif

                    reader.GetValues(data);

                    //TObject obj = Activator.CreateInstance(typeof(TObject)) as TObject;
                    var currentObject = (TObject)staticBindInfo.AssignObject;

                    Int32 field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        if (data[0] != null)
                            currentObject.ObjectId = (String)data[0];
                    }

                    Boolean hasRelations = false;
                    // we can use hard index access because we iterate the same order here
                    for (Int32 i = 0; i < bindingInfo.Length; i++)
                    {
                        BindingInfo bind = bindingInfo[i];

                        if (staticBindInfo.DataBinders[i] == null)
                        {
                            if (bind.Member is FieldInfo)
                                Log.Error($"StaticBindSelect on {tableName}", $"{((FieldInfo)bind.Member).Name} is a field. StaticBindSelect will only work with properties.");
                            else
                                Log.Error($"StaticBindSelect on {tableName}", $"Could not find a data binder for the property {((PropertyInfo)bind.Member).Name}.");

                            Environment.Exit(1);
                        }

                        if (!hasRelations)
                        {
                            hasRelations = bind.HasRelation;
                        }

                        if (!bind.HasRelation)
                        {
                            Object val = data[field++];
                            Object obj = null;

                            if (val != null && !Convert.IsDBNull(val))
                                obj = ConvertFromDatabaseFormat(((PropertyInfo)bind.Member).PropertyType, val);

                            try
                            {
                                staticBindInfo.DataBinders[i].Assign(obj);
                            }
                            catch (InvalidCastException)
                            {
                                Log.Error(tableName, "Failed to cast " + ((PropertyInfo)bind.Member).Name);
                            }
                        }
                    }

                    currentObject = (TObject)currentObject.Clone();

                    dataObjects[(TKey)data[keyIndex]] = currentObject;

                    if (hasRelations)
                    {
                        FillLazyObjectRelations(currentObject, true);
                    }

                    currentObject.IsValid = true;
                    currentObject.AllowAdd = false; // exists already
                    currentObject.Dirty = false;

#if LOGTIME
                    watch.Stop();
                    objectLoad += watch.ElapsedTicks;
#endif
                }

#if LOGTIME
                if (count > 0)
                    Log.Notice(tableName, $"StaticBindSelect object loading time: {objectLoad / (float)Stopwatch.Frequency} seconds, average ticks per object: {objectLoad / count}");
#endif
            }, isolation);

            pool.Enqueue(staticBindInfo);

            return dataObjects;
        }

        protected Dictionary<TKey, TObject> CompiledExpressionMap<TKey, TObject>(Int32 keyIndex, Int32 expectedRowCount, IsolationLevel isolation, String tableName, String sqlCommand, BindingInfo[] bindingInfo, Boolean useObjectID) where TObject : DataObject
        {
            var dataObjects = new Dictionary<TKey, TObject>(expectedRowCount);

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
#if LOGTIME
                long objectLoad = 0;
                long count = 0;

                Stopwatch watch = new Stopwatch();
#endif

                var mySqlReader = (MySqlDataReader)reader;
                while (mySqlReader.Read())
                {
#if LOGTIME
                    watch.Restart();
                    ++count;
#endif

                    var currentObject = (TObject)Activator.CreateInstance(typeof(TObject));

                    Int32 field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        currentObject.ObjectId = reader.GetString(0);
                    }

                    Boolean hasRelations = false;
                    var key = default(TKey);
                    // we can use hard index access because we iterate the same order here
                    for (Int32 i = 0; i < bindingInfo.Length; i++)
                    {
                        BindingInfo bind = bindingInfo[i];
                        if (!hasRelations)
                            hasRelations = bind.HasRelation;

                        if (!bind.HasRelation && bind.MySqlBinder != null && !mySqlReader.IsDBNull(field))
                        {
                            // Value type
                            if (((PropertyInfo)bind.Member).PropertyType.IsValueType && !((PropertyInfo)bind.Member).PropertyType.IsEnum)
                            {
                                bind.MySqlBinder.Assign(currentObject, mySqlReader, field);
                                if (i == keyIndex)
                                    key = (TKey)mySqlReader.GetValue(field);
                            }
                            else
                            {
                                Object obj = ConvertFromDatabaseFormat(((PropertyInfo)bind.Member).PropertyType, mySqlReader.GetValue(field));
                                bind.MySqlBinder.AssignObject(currentObject, obj);
                                if (i == keyIndex)
                                    key = (TKey)obj;
                            }
                        }

                        ++field;
                    }

                    dataObjects[key] = currentObject;

                    if (hasRelations)
                    {
                        FillLazyObjectRelations(currentObject, true);
                    }

                    currentObject.IsValid = true;
                    currentObject.AllowAdd = false; // exists already
                    currentObject.Dirty = false;

#if LOGTIME
                    watch.Stop();
                    objectLoad += watch.ElapsedTicks;
#endif
                }

#if LOGTIME
                if (count > 0)
                    Log.Notice(tableName, $"CompiledExpressionSelect object loading time: {objectLoad / (float)Stopwatch.Frequency} seconds, average ticks per object: {objectLoad / count}");
#endif
            }, isolation);

            return dataObjects;
        }

        protected Dictionary<TKey, TObject> ManualMap<TKey, TObject>(Int32 keyIndex, Int32 expectedRowCount, IsolationLevel isolation, String tableName, String sqlCommand, BindingInfo[] bindingInfo, Boolean useObjectID) where TObject : DataObject
        {
            var dataObjects = new Dictionary<TKey, TObject>(expectedRowCount);

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
#if LOGTIME
                long objectLoad = 0;
                long count = 0;

                Stopwatch watch = new Stopwatch();

#endif

                var mySqlReader = (MySqlDataReader)reader;
                while (mySqlReader.Read())
                {
#if LOGTIME
                    watch.Restart();
                    ++count;
#endif

                    var currentObject = (TObject)Activator.CreateInstance(typeof(TObject));

                    Int32 field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        currentObject.ObjectId = reader.GetString(0);
                    }

                    currentObject.Load(mySqlReader, field);

                    dataObjects[(TKey)reader.GetValue(keyIndex)] = currentObject;

                    currentObject.IsValid = true;
                    currentObject.AllowAdd = false; // exists already
                    currentObject.Dirty = false;

#if LOGTIME
                    watch.Stop();
                    objectLoad += watch.ElapsedTicks;
#endif
                }

#if LOGTIME
                if (count > 0)
                    Log.Notice(tableName, $"Object loading time: {objectLoad / (float)Stopwatch.Frequency} seconds, average ticks per object: {objectLoad / count}");
#endif
            }, isolation);

            return dataObjects;
        }
        #endregion

        #region General DB accessor

        ///<summary>Returns the next auto-increment for the supplied object.</summary> 
        protected override Int32 GetNextAutoIncrementImpl<TObject>()
        {
            String sqlQuery = "SELECT * FROM information_schema.TABLES WHERE TABLE_NAME = '" + GetTableOrViewName(typeof(TObject)) + "'";
            Int32 nextAutoIncrement = 0;

            if (Connection.IsSQLConnection)
            {
                Connection.ExecuteSelect(sqlQuery, (r) =>
                {
                    var reader = (MySqlDataReader)r;
                    if (!reader.HasRows)
                        return;

                    while (reader.Read())
                        nextAutoIncrement = Convert.ToInt32(reader.GetInt64("AUTO_INCREMENT"));

                }, IsolationLevel.DEFAULT);
            }
            return nextAutoIncrement;
        }

        /// <summary>
        /// Returns the number of this object type in the DB, optionally matching the WHERE condition.
        /// </summary>
        protected override Int32 GetObjectCountImpl<TObject>(String where)
        {
            String tableName = GetTableOrViewName(typeof(TObject));

            if (Connection.IsSQLConnection)
            {
                String query = "SELECT COUNT(*) FROM " + tableName;
                if (where != "")
                    query += " WHERE " + where;

                Object count = Connection.ExecuteScalar(query);
                if (count is DBNull)
                    return 0;
                return count is Int64 ? (Int32)((Int64)count) : (Int32)count;
            }

            return 0;
        }

        /// <summary>
        /// Gets the highest value for the supplied column.
        /// </summary>
        protected override Int64 GetMaxColValueImpl<TObject>(String column)
        {
            String tableName = GetTableOrViewName(typeof(TObject));

            if (Connection.IsSQLConnection)
            {
                String query = "SELECT MAX(" + column + ") FROM " + tableName;

                Object count = Connection.ExecuteScalar(query);
                return count is DBNull ? 0 : Convert.ToInt64(count);
            }

            return 0;
        }

        #endregion

        /// <summary>
        /// Executes a non-blocking request.
        /// </summary>
        protected override Boolean ExecuteNonQueryImpl(String rawQuery)
        {
            try
            {
                Log.Debug("MysqlObject", rawQuery);

                Int32 res = Connection.ExecuteNonQuery(rawQuery);
                if (res == 0)
                {
                    Log.Error("MysqlObject", "Execution error : " + rawQuery);

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error("MysqlObject", "Execution error : " + rawQuery + e);
            }

            return false;
        }

        /// <summary>
        /// Executes a non-blocking request. Looks for integer in first column, mostly used for select count(*) queries
        /// </summary>
        protected override Int64 ExecuteQueryIntImpl(String rawQuery)
        {
            Int64 result = 0;
            try
            {
                Log.Debug("Mysql count query", rawQuery);
                Connection.ExecuteSelect(rawQuery, (reader) =>
                {
                    if (reader.HasRows && reader.FieldCount > 0 && reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                            result = reader.GetInt64(0);
                    }
                }, IsolationLevel.DEFAULT);

            }
            catch (Exception e)
            {
                Log.Error("Mysql count query", "Execution error : " + rawQuery + e);
            }
            return result;
        }
    }
}
