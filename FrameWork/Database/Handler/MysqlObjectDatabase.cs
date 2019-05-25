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

namespace FrameWork
{
    public class MySQLObjectDatabase : ObjectDatabase
    {
        public MySQLObjectDatabase(MySqlDataConnection connection)
            : base(connection)
        {
        }

        #region Value conversion

        /// <summary>
        /// Converts an input value to the representation that will be stored in the DB.
        /// </summary>
        protected object ConvertToDatabaseFormat(object val, string dateFormat)
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
                Obj = Utils.ConvertArrayToString<byte>(((List<byte>) val).ToArray());
            else if (val is byte[])
                Obj = Utils.ConvertArrayToString<byte>(((byte[]) val));

            else if (val is List<short>)
                Obj = Utils.ConvertArrayToString<short>(((List<short>) val).ToArray());
            else if (val is short[])
                Obj = Utils.ConvertArrayToString<short>(((short[]) val));

            else if (val is List<int>)
                Obj = Utils.ConvertArrayToString<int>(((List<int>) val).ToArray());
            else if (val is int[])
                Obj = Utils.ConvertArrayToString<int>(((int[]) val));

            else if (val is List<uint>)
                Obj = Utils.ConvertArrayToString<uint>(((List<uint>) val).ToArray());
            else if (val is uint[])
                Obj = Utils.ConvertArrayToString<uint>(((uint[]) val));

            else if (val is List<float>)
                Obj = Utils.ConvertArrayToString<float>(((List<float>) val).ToArray());
            else if (val is float[])
                Obj = Utils.ConvertArrayToString<float>(((float[]) val));

            else if (val is List<ulong>)
                Obj = Utils.ConvertArrayToString<ulong>(((List<ulong>) val).ToArray());
            else if (val is ulong[])
                Obj = Utils.ConvertArrayToString<ulong>(((ulong[]) val));

            else if (val is List<long>)
                Obj = Utils.ConvertArrayToString<long>(((List<long>) val).ToArray());
            else if (val is long[])
                Obj = Utils.ConvertArrayToString<long>(((long[]) val));

            else if(val != null)
                Obj = Escape(val.ToString());

            return Obj;
        }

        protected object ConvertFromDatabaseFormat(Type type, object val)
        {
            if (type == typeof (bool))
                return Convert.ToBoolean(Convert.ToInt32(val));

            if (type == typeof(DateTime))
            {
                if (val is MySqlDateTime)
                    return ((MySqlDateTime)val).GetDateTime();
                return ((IConvertible)val).ToDateTime(null);
            }

            if (!(val is string) || type == typeof(string))
                return val;

            if (type == typeof (byte[]))
                return Utils.ConvertStringToArray<byte>((string) val).ToArray();
            if (type == typeof(List<byte>))
                return Utils.ConvertStringToArray<byte>((string) val);

            if (type == typeof(short[]))
                return Utils.ConvertStringToArray<short>((string) val).ToArray();
            if (type == typeof(List<short>))
                return Utils.ConvertStringToArray<short>((string) val);

            if (type == typeof(ushort[]))
                return Utils.ConvertStringToArray<ushort>((string) val).ToArray();
            if (type == typeof(List<ushort>))
                return Utils.ConvertStringToArray<ushort>((string) val);

            if (type == typeof(int[]))
                return Utils.ConvertStringToArray<int>((string) val).ToArray();
            if (type == typeof(List<int>))
                return Utils.ConvertStringToArray<int>((string) val);

            if (type == typeof(uint[]))
                return Utils.ConvertStringToArray<uint>((string) val).ToArray();
            if (type == typeof(List<uint>))
                return Utils.ConvertStringToArray<uint>((string) val);

            if (type == typeof(long[]))
                return Utils.ConvertStringToArray<long>((string) val).ToArray();
            if (type == typeof(List<long>))
                return Utils.ConvertStringToArray<long>((string) val);

            if (type == typeof(ulong[]))
                return Utils.ConvertStringToArray<ulong>((string) val).ToArray();
            if (type == typeof(List<ulong>))
                return Utils.ConvertStringToArray<ulong>((string) val);

            if (type == typeof(float[]))
                return Utils.ConvertStringToArray<float>((string) val).ToArray();
            if (type == typeof(List<float>))
                return Utils.ConvertStringToArray<float>((string) val);

            return val;
        }

        /// <summary>
        /// Assigns data loaded from the DB to the DataObject field to which it is to be loaded.
        /// </summary>
        protected Exception GetVal(object Object, MemberInfo info, object val)
        {
            try
            {
                Type type;
                if (info is FieldInfo)
                    type = ((FieldInfo) info).FieldType;
                else if (info is PropertyInfo)
                    type = ((PropertyInfo) info).PropertyType;
                else
                    return null;

                object obj = ConvertFromDatabaseFormat(type, val);

                if (info is PropertyInfo)
                {
                    ((PropertyInfo) info).SetValue(Object, obj, null);
                }
                else ((FieldInfo) info).SetValue(Object, obj);

            }
            catch(Exception e)
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
        private string FormulateUpdate(DataObject dataObject, out bool hasRelations)
        {
            string tableName = dataObject.TableName;

            _opBuilder.Clear();
            _opBuilder.Append("UPDATE `" + tableName + "` SET ");

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

                    val = ConvertToDatabaseFormat(val, dateFormat);

                    _whereBuilder.Append("`" + bind.Member.Name + "` = ");
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
        private string FormulateDelete(DataObject dataObject)
        {
            _opBuilder.Clear();
            _opBuilder.Append("DELETE FROM `");
            _opBuilder.Append(dataObject.TableName);
            _opBuilder.Append("`");

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
        protected override bool AddObjectImpl(DataObject dataObject)
        {
            try
            {
                bool hasRelations = false;

                string sql = FormulateInsert(dataObject, out hasRelations);

                Log.Debug("MysqlObject", sql);

                int res = Connection.ExecuteNonQuery(sql);
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
            bool hasRelations;

            string sql = FormulateUpdate(dataObject, out hasRelations);
            try
            {
               

                Log.Debug("MysqlObject", sql);

                int res = Connection.ExecuteNonQuery(sql);

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
                Log.Error("MysqlObject", "Modify error : " + dataObject.TableName + " " + dataObject.ObjectId + e + "SQL="+sql);
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

                Log.Debug("MysqlObject", sql);

                int result = Connection.ExecuteNonQuery(sql);
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

        protected override bool RunTransaction(List<DataObject> dataObjects)
        {
            long startTime = TCPManager.GetTimeStampMS();

            MySqlConnection sqlConn = (MySqlConnection)Connection.GetConnection();
            MySqlCommand sqlCommand = sqlConn.CreateCommand();
            MySqlTransaction transaction = sqlConn.BeginTransaction();

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
                    whereClause = "`" + members[i].Name + "` = '" + key + "'";
                    break;
                }
            }

            if (whereClause == null)
            {
                whereClause = "`" + ret.TableName + "_ID` = '" + key + "'";
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
                    whereClause = "`" + members[i].Name + "` = '" + key + "'";
                    break;
                }
            }

            if (whereClause == null)
            {
                whereClause = "`" + ret.TableName + "_ID` = '" + key + "'";
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
                sb.Append("`" + tableName + "_ID`, ");

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
                    sb.Append("`" + bind.Member.Name + "`");
                }
            }

            sb.Append(" FROM `" + tableName + "`");

            if (whereClause != null && whereClause.Trim().Length > 0)
            {
                sb.Append(" WHERE " + whereClause);
            }

            string sql = sb.ToString();

            Log.Debug("MysqlObject", "DataObject[] SelectObjectsImpl: " + sql);

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
        protected override IList<TObject> SelectObjectsImpl<TObject>(string whereClause, IsolationLevel isolation)
        {
            string tableName = GetTableOrViewName(typeof (TObject));
            bool useObjectID = TableDatasets[tableName].RequiresObjectId;

            // build sql command
            var sb = new StringBuilder("SELECT ");
            if (useObjectID)
                sb.Append("`" + tableName + "_ID`, ");

            bool first = true;

            BindingInfo[] bindingInfo = GetBindingInfo(typeof (TObject));
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
                    sb.Append("`" + bindingInfo[i].Member.Name + "`");
                }
            }

            sb.Append(" FROM `" + tableName + "`");

            if (whereClause != null && whereClause.Trim().Length > 0)
            {
                sb.Append(" WHERE " + whereClause);
            }

            string sql = sb.ToString();

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

        protected List<TObject> ReflectionSelect<TObject>(IsolationLevel isolation, string tableName, string sqlCommand, BindingInfo[] bindingInfo, bool useObjectID)
            where TObject: DataObject
        {
            List<TObject> dataObjects = new List<TObject>(64);

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
                var data = new object[reader.FieldCount];

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

                    TObject currentObject = (TObject) Activator.CreateInstance(typeof(TObject));

                    int field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        if (data[0] != null)
                            currentObject.ObjectId = (string)data[0];
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
            }
                , isolation);

            return dataObjects;
        }

        protected List<TObject> StaticBindSelect<TObject>(IsolationLevel isolation, string tableName, string sqlCommand, BindingInfo[] bindingInfo, bool useObjectID)
        where TObject : DataObject
        {
            List<TObject> dataObjects = new List<TObject>(64);

            ObjectPool<StaticMemberBindInfo> pool = GetStaticBindPool<TObject>();

            StaticMemberBindInfo staticBindInfo = pool.Dequeue();

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
                var data = new object[reader.FieldCount];

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
                    TObject currentObject = (TObject)staticBindInfo.AssignObject;

                    int field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        if (data[0] != null)
                            currentObject.ObjectId = (string)data[0];
                    }

                    bool hasRelations = false;
                    // we can use hard index access because we iterate the same order here
                    for (int i = 0; i < bindingInfo.Length; i++)
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
                            object val = data[field++];
                            object obj = null;

                            if (val != null && !Convert.IsDBNull(val))
                                obj = ConvertFromDatabaseFormat(((PropertyInfo) bind.Member).PropertyType, val);
                            
                            try
                            {
                                staticBindInfo.DataBinders[i].Assign(obj);
                            }
                            catch (InvalidCastException)
                            {
                                Log.Error(tableName, "Failed to cast " + ((PropertyInfo) bind.Member).Name);
                            }
                        }
                    }

                    currentObject = (TObject)currentObject.Clone();

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
                    Log.Notice(tableName, $"StaticBindSelect object loading time: {objectLoad / (float)Stopwatch.Frequency} seconds, average ticks per object: {objectLoad / count}");
#endif
            }
                , isolation);

            pool.Enqueue(staticBindInfo);

            return dataObjects;
        }

        protected List<TObject> CompiledExpressionSelect<TObject>(IsolationLevel isolation, string tableName, string sqlCommand, BindingInfo[] bindingInfo, bool useObjectID)
        where TObject : DataObject
        {
            List<TObject> dataObjects = new List<TObject>(64);

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
#if LOGTIME
                long objectLoad = 0;
                long count = 0;

                Stopwatch watch = new Stopwatch();
#endif

                MySqlDataReader mySqlReader = (MySqlDataReader)reader;
                while (mySqlReader.Read())
                {
#if LOGTIME
                    watch.Restart();
                    ++count;
#endif

                    TObject currentObject = (TObject) Activator.CreateInstance(typeof(TObject));

                    int field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        currentObject.ObjectId = reader.GetString(0);
                    }

                    bool hasRelations = false;
                    // we can use hard index access because we iterate the same order here
                    for (int i = 0; i < bindingInfo.Length; i++)
                    {
                        BindingInfo bind = bindingInfo[i];
                        if (!hasRelations)
                            hasRelations = bind.HasRelation;

                        if (!bind.HasRelation && bind.MySqlBinder != null && !mySqlReader.IsDBNull(field))
                        {
                            // Value type
                            if (((PropertyInfo) bind.Member).PropertyType.IsValueType && !((PropertyInfo) bind.Member).PropertyType.IsEnum)
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
                                    object obj = ConvertFromDatabaseFormat(((PropertyInfo) bind.Member).PropertyType, mySqlReader.GetValue(field));
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
            }
                , isolation);

            return dataObjects;
        }

        protected List<TObject> ManualSelect<TObject>(IsolationLevel isolation, string tableName, string sqlCommand, BindingInfo[] bindingInfo, bool useObjectID)
        where TObject : DataObject
        {
            List<TObject> dataObjects = new List<TObject>(64);

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
#if LOGTIME
                long objectLoad = 0;
                long count = 0;

                Stopwatch watch = new Stopwatch();

#endif

                MySqlDataReader mySqlReader = (MySqlDataReader)reader;
                while (mySqlReader.Read())
                {
#if LOGTIME
                    watch.Restart();
                    ++count;
#endif

                    TObject currentObject = (TObject) Activator.CreateInstance(typeof(TObject));

                    int field = 0;

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

        #region Map
        protected override Dictionary<TKey, TObject> MapAllObjectsImpl<TKey, TObject>(string keyName, string whereClause, int expectedRowCount, IsolationLevel isolation)
        {
            string tableName = GetTableOrViewName(typeof(TObject));
            bool useObjectID = TableDatasets[tableName].RequiresObjectId;

            // build sql command
            var sb = new StringBuilder("SELECT ");
            if (useObjectID)
                sb.Append("`" + tableName + "_ID`, ");

            bool first = true;

            BindingInfo[] bindingInfo = GetBindingInfo(typeof(TObject));
            int keyIndex = -1;
            for (int i = 0; i < bindingInfo.Length; i++)
            {
                if (!bindingInfo[i].HasRelation)
                {
                    if (!first)
                        sb.Append(", ");
                    else
                        first = false;
                    string name = bindingInfo[i].Member.Name;
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

            string sql = sb.ToString();

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

        protected Dictionary<TKey, TObject> ReflectionMap<TKey, TObject>(int keyIndex, int expectedRowCount, IsolationLevel isolation, string tableName, string sqlCommand, BindingInfo[] bindingInfo, bool useObjectID)
            where TObject : DataObject
        {
            Dictionary<TKey, TObject> dataObjects = new Dictionary<TKey, TObject>(expectedRowCount);

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
                var data = new object[reader.FieldCount];

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

                    TObject currentObject = (TObject)Activator.CreateInstance(typeof(TObject));

                    int field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        if (data[0] != null)
                            currentObject.ObjectId = (string)data[0];
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
            }
                , isolation);

            return dataObjects;
        }

        protected Dictionary<TKey, TObject> StaticBindMap<TKey, TObject>(int keyIndex, int expectedRowCount, IsolationLevel isolation, string tableName, string sqlCommand, BindingInfo[] bindingInfo, bool useObjectID)
        where TObject : DataObject
        {
            Dictionary<TKey, TObject> dataObjects = new Dictionary<TKey, TObject>(expectedRowCount);

            ObjectPool<StaticMemberBindInfo> pool = GetStaticBindPool<TObject>();

            StaticMemberBindInfo staticBindInfo = pool.Dequeue();

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
                var data = new object[reader.FieldCount];

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
                    TObject currentObject = (TObject)staticBindInfo.AssignObject;

                    int field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        if (data[0] != null)
                            currentObject.ObjectId = (string)data[0];
                    }

                    bool hasRelations = false;
                    // we can use hard index access because we iterate the same order here
                    for (int i = 0; i < bindingInfo.Length; i++)
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
                            object val = data[field++];
                            object obj = null;

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
            }
                , isolation);

            pool.Enqueue(staticBindInfo);

            return dataObjects;
        }

        protected Dictionary<TKey, TObject> CompiledExpressionMap<TKey, TObject>(int keyIndex, int expectedRowCount, IsolationLevel isolation, string tableName, string sqlCommand, BindingInfo[] bindingInfo, bool useObjectID)
        where TObject : DataObject
        {
            Dictionary<TKey, TObject> dataObjects = new Dictionary<TKey, TObject>(expectedRowCount);

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
#if LOGTIME
                long objectLoad = 0;
                long count = 0;

                Stopwatch watch = new Stopwatch();
#endif

                MySqlDataReader mySqlReader = (MySqlDataReader)reader;
                while (mySqlReader.Read())
                {
#if LOGTIME
                    watch.Restart();
                    ++count;
#endif

                    TObject currentObject = (TObject)Activator.CreateInstance(typeof(TObject));

                    int field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        currentObject.ObjectId = reader.GetString(0);
                    }

                    bool hasRelations = false;
                    TKey key = default(TKey);
                    // we can use hard index access because we iterate the same order here
                    for (int i = 0; i < bindingInfo.Length; i++)
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
                                object obj = ConvertFromDatabaseFormat(((PropertyInfo)bind.Member).PropertyType, mySqlReader.GetValue(field));
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
            }
                , isolation);

            return dataObjects;
        }

        protected Dictionary<TKey, TObject> ManualMap<TKey, TObject>(int keyIndex, int expectedRowCount, IsolationLevel isolation, string tableName, string sqlCommand, BindingInfo[] bindingInfo, bool useObjectID)
        where TObject : DataObject
        {
            Dictionary<TKey, TObject> dataObjects = new Dictionary<TKey, TObject>(expectedRowCount);

            // read data and fill objects
            Connection.ExecuteSelect(sqlCommand, (reader) =>
            {
#if LOGTIME
                long objectLoad = 0;
                long count = 0;

                Stopwatch watch = new Stopwatch();

#endif

                MySqlDataReader mySqlReader = (MySqlDataReader)reader;
                while (mySqlReader.Read())
                {
#if LOGTIME
                    watch.Restart();
                    ++count;
#endif

                    TObject currentObject = (TObject)Activator.CreateInstance(typeof(TObject));

                    int field = 0;

                    // ObjectID field is in use, so we need to get rid of the first data value and start from index 1 when reading later.
                    if (useObjectID)
                    {
                        ++field;
                        currentObject.ObjectId = reader.GetString(0);
                    }

                    currentObject.Load(mySqlReader, field);

                    dataObjects[(TKey) reader.GetValue(keyIndex)] = currentObject;

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
            }
                , isolation);

            return dataObjects;
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
        protected override int GetObjectCountImpl<TObject>(string where)
        {
            string tableName = GetTableOrViewName(typeof(TObject));

            if (Connection.IsSQLConnection)
            {
                string query = "SELECT COUNT(*) FROM " + tableName;
                if (where != "")
                    query += " WHERE " + where;

                object count = Connection.ExecuteScalar(query);
                if (count is DBNull)
                    return 0;
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
                string query = "SELECT MAX(" + column+ ") FROM " + tableName;

                object count = Connection.ExecuteScalar(query);
                if (count is DBNull)
                    return 0;
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
                Log.Debug("MysqlObject", rawQuery);

                int res = Connection.ExecuteNonQuery(rawQuery);
                if (res == 0)
                {
                    Log.Info("MysqlObject", "Statement : " + rawQuery + " returned 0 results.");

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error("MysqlObject", "Execution error : " + rawQuery + e + e.Message);
            }

            return false;
        }

        /// <summary>
        /// Executes a non-blocking request. Looks for integer in first column, mostly used for select count(*) queries
        /// </summary>
        protected override long ExecuteQueryIntImpl(string rawQuery)
        {
            long result = 0;
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
