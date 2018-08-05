using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using FrameWork.Database;

namespace FrameWork
{
    /// <summary>
    /// Represents the operation that will be performed on a given data object.
    /// </summary>
    public enum DatabaseOp : byte
    {
        DOO_None,
        DOO_Insert,
        DOO_Update,
        DOO_Delete
    }

    public abstract class ObjectDatabase : IObjectDatabase
    {
        protected static readonly NumberFormatInfo Nfi = new CultureInfo("en-US", false).NumberFormat;
        /// <summary>
        /// Holds the binding info array for each DataObject type, which describes that type's members.
        /// </summary>
        private readonly Dictionary<Type, BindingInfo[]> _bindingInfos = new Dictionary<Type, BindingInfo[]>();
        private readonly Dictionary<Type, ObjectPool<StaticMemberBindInfo>> _staticBindPools = new Dictionary<Type, ObjectPool<StaticMemberBindInfo>>();
        protected readonly DataConnection Connection;
        private readonly Dictionary<Type, ConstructorInfo> ConstructorByFieldType = new Dictionary<Type, ConstructorInfo>();
        private readonly Dictionary<Type, MemberInfo[]> MemberInfoCache = new Dictionary<Type, MemberInfo[]>();
        private readonly Dictionary<MemberInfo, Relation[]> RelationAttributes = new Dictionary<MemberInfo, Relation[]>();

        protected readonly Dictionary<string, DataTableHandler> TableDatasets;
        public virtual string SqlCommand_CharLength()
        {
            return "CHAR_LENGTH";
        }
        private readonly Thread _updater;

        private const int PROCESS_INTERVAL = 5000; // 5 second processing of ops queue.
        private const int SAVE_INTERVAL = 60000; // 1 minute saving of objects. 600000; // 10 minute saving of objects. was 200ms before... 

        private long _nextSaveTime;

        private readonly List<Action> _onProcessActions = new List<Action>(); 

        protected ObjectDatabase(DataConnection connection)
        {
            TableDatasets = new Dictionary<string, DataTableHandler>();
            Connection = connection;

            _nextSaveTime = TCPManager.GetTimeStampMS() + SAVE_INTERVAL; 

            ThreadStart start = Update;
            _updater = new Thread(start);
            _updater.Start();
        }

        #region Data tables

        protected DataSet GetDataSet(string tableName)
        {
            if (!TableDatasets.ContainsKey(tableName))
                return null;

            return TableDatasets[tableName].DataSet;
        }

        protected void FillObjectWithRow<TObject>(ref TObject dataObject, DataRow row, bool reload)
            where TObject : DataObject
        {
            bool relation = false;

            string tableName = dataObject.TableName;
            Type myType = dataObject.GetType();
            string id = row[tableName + "_ID"].ToString();

            MemberInfo[] myMembers = myType.GetMembers();
            dataObject.ObjectId = id;

            for (int i = 0; i < myMembers.Length; i++)
            {
                object[] myAttributes = GetRelationAttributes(myMembers[i]);

                if (myAttributes.Length > 0)
                {
                    //if(myAttributes[0] is Attributes.Relation)
                    //{
                    relation = true;
                    //}
                }
                else
                {
                    object[] keyAttrib = myMembers[i].GetCustomAttributes(typeof(PrimaryKey), true);
                    myAttributes = myMembers[i].GetCustomAttributes(typeof(DataElement), true);
                    if (myAttributes.Length > 0 || keyAttrib.Length > 0)
                    {
                        object val = row[myMembers[i].Name];
                        if (val != null && !val.GetType().IsInstanceOfType(DBNull.Value))
                        {
                            if (myMembers[i] is PropertyInfo)
                            {
                                ((PropertyInfo)myMembers[i]).SetValue(dataObject, val, null);
                            }
                            if (myMembers[i] is FieldInfo)
                            {
                                ((FieldInfo)myMembers[i]).SetValue(dataObject, val);
                            }
                        }
                    }
                }
            }

            dataObject.Dirty = false;


            if (relation)
            {
                FillLazyObjectRelations(dataObject, true);
            }

            dataObject.IsValid = true;
        }

        protected void FillRowWithObject(DataObject dataObject, DataRow row)
        {
            bool relation = false;

            Type myType = dataObject.GetType();

            row[dataObject.TableName + "_ID"] = dataObject.ObjectId;

            MemberInfo[] myMembers = myType.GetMembers();

            for (int i = 0; i < myMembers.Length; i++)
            {
                object[] myAttributes = GetRelationAttributes(myMembers[i]);
                object val = null;

                if (myAttributes.Length > 0)
                {
                    relation = true;
                }
                else
                {
                    myAttributes = myMembers[i].GetCustomAttributes(typeof(DataElement), true);
                    object[] keyAttrib = myMembers[i].GetCustomAttributes(typeof(PrimaryKey), true);

                    if (myAttributes.Length > 0 || keyAttrib.Length > 0)
                    {
                        if (myMembers[i] is PropertyInfo)
                        {
                            val = ((PropertyInfo)myMembers[i]).GetValue(dataObject, null);
                        }
                        if (myMembers[i] is FieldInfo)
                        {
                            val = ((FieldInfo)myMembers[i]).GetValue(dataObject);
                        }
                        if (val != null)
                        {
                            row[myMembers[i].Name] = val;
                        }
                    }
                }
                //}
            }

            if (relation)
            {
                SaveObjectRelations(dataObject);
            }
        }

        protected DataRow FindRowByKey(DataObject dataObject)
        {
            DataRow row;

            string tableName = dataObject.TableName;

            System.Data.DataTable table = GetDataSet(tableName).Tables[tableName];

            Type myType = dataObject.GetType();

            string key = table.PrimaryKey[0].ColumnName;

            if (key.Equals(tableName + "_ID"))
                row = table.Rows.Find(dataObject.ObjectId);
            else
            {
                MemberInfo[] keymember = myType.GetMember(key);

                object val = null;

                if (keymember[0] is PropertyInfo)
                    val = ((PropertyInfo)keymember[0]).GetValue(dataObject, null);
                if (keymember[0] is FieldInfo)
                    val = ((FieldInfo)keymember[0]).GetValue(dataObject);

                if (val != null)
                    row = table.Rows.Find(val);
                else
                    return null;
            }

            return row;
        }

        #endregion

        #region Public API

        public string GetSchemaName()
        {
            return Connection.SchemaName;
        }

        public void RegisterAction(Action action)
        {
            _onProcessActions.Add(action);
        }

        /// <summary>A list of all objects that are pending some kind of database modification.</summary>
        private List<DataObject> _dirtyObjects = new List<DataObject>();

        /// <summary>A list of database operations to be performed on given objects, in order.</summary>
        private List<Tuple<DataObject, DatabaseOp>> _operations = new List<Tuple<DataObject, DatabaseOp>>();

        public bool AddObject(DataObject dataObject)
        {
            lock(_operations)
            {
                if (dataObject.AllowAdd)
                {
                    _operations.Add(new Tuple<DataObject, DatabaseOp>(dataObject, DatabaseOp.DOO_Insert));
                    dataObject.AllowAdd = false;
                    return true;
                }
            }


            Log.Notice("ObjectDatabase", "Can not save, AllowAdd = False " + dataObject.TableName + " : " + dataObject.ObjectId);
            return false;
        }

        public void SaveObject(DataObject dataObject)
        {
            if (dataObject.Dirty)
            {
                if (!dataObject.IsValid)
                {
                    /*
                    if (dataObject.AllowAdd)
                        Log.Error("ObjectDatabase", "SaveObject attempt on " + dataObject.GetType() +" which was never added to DB");
                    else if (!dataObject.AllowDelete)
                        Log.Error("ObjectDatabase", "SaveObject attempt on " + dataObject.GetType() +" pending deletion");
                    else if (dataObject.IsDeleted)
                        Log.Error("ObjectDatabase", "SaveObject attempt on deleted " + dataObject.GetType());
                    else
                        Log.Error("ObjectDatabase", "SaveObject attempt on otherwise invalid " + dataObject.GetType());
                    */

                    return;
                }

                lock(_operations)
                    _operations.Add(new Tuple<DataObject, DatabaseOp>(dataObject, DatabaseOp.DOO_Update));
            }
        }

        public virtual void ForceSave()
        {
            _nextSaveTime = TCPManager.GetTimeStampMS() + 10;
        }

        public void DeleteObject(DataObject dataObject)
        {
            if (dataObject.AllowDelete)
            {
                lock (_operations)
                {
                    _operations.Add(new Tuple<DataObject, DatabaseOp>(dataObject, DatabaseOp.DOO_Delete));
                    dataObject.AllowDelete = false;
                    dataObject.AllowAdd = true;
                }
            }
        }

        public void Update()
        {
            while (true)
            {
                long Start = TCPManager.GetTimeStampMS();

                if (_operations.Count > 0)
                {
                    List<Tuple<DataObject, DatabaseOp>> localOperations;

                    lock (_operations)
                    {
                        localOperations = new List<Tuple<DataObject, DatabaseOp>>(_operations);
                        _operations.Clear();
                    }
                    

                    foreach (var opInfo in localOperations)
                    {
                        if (opInfo.Item1.pendingOp == opInfo.Item2)
                            continue;

                        if (opInfo.Item1.pendingOp == DatabaseOp.DOO_None)
                        {
                            _dirtyObjects.Add(opInfo.Item1);
                            opInfo.Item1.pendingOp = opInfo.Item2;
                        }

                        switch (opInfo.Item2)
                        {
                            case DatabaseOp.DOO_Insert:
                                if (opInfo.Item1.pendingOp == DatabaseOp.DOO_Delete)
                                {
                                    Log.Info("ObjectDatabase", "Transforming Delete/Insert on "+opInfo.Item1.GetType()+" into Update.");
                                    opInfo.Item1.pendingOp = DatabaseOp.DOO_Update;
                                }
                                break;
                            case DatabaseOp.DOO_Delete:
                                if (opInfo.Item1.pendingOp == DatabaseOp.DOO_Insert)
                                    opInfo.Item1.pendingOp = DatabaseOp.DOO_None;
                                else
                                    opInfo.Item1.pendingOp = DatabaseOp.DOO_Delete;
                                break;
                        }
                    }
                }

                if (_onProcessActions.Count > 0)
                {
                    foreach (Action action in _onProcessActions)
                        action();
                }

                if (Start > _nextSaveTime)
                {
                    _nextSaveTime = Start + SAVE_INTERVAL;


                    if (_dirtyObjects.Count > 0)
                    {
                        // Fall back to inefficient handling if a transaction fails for whatever reason.
                        if (!RunTransaction(_dirtyObjects))
                        {
                            //Dictionary<string, uint> saveDistribution = new Dictionary<string, uint>();

                            uint addsThisPass = 0;
                            uint savesThisPass = 0;
                            uint deletesThisPass = 0;

                            foreach (var dataObject in _dirtyObjects)
                            {
                                /*if (saveDistribution.ContainsKey(dataObject.TableName))
                                    saveDistribution[dataObject.TableName]++;
                                else saveDistribution.Add(dataObject.TableName, 1);*/

                                switch (dataObject.pendingOp)
                                {
                                    case DatabaseOp.DOO_Insert:
                                        AddObjectImpl(dataObject);
                                        ++addsThisPass; break;
                                    case DatabaseOp.DOO_Update:
                                        SaveObjectImpl(dataObject);
                                        ++savesThisPass; break;
                                    case DatabaseOp.DOO_Delete:
                                        DeleteObjectImpl(dataObject);
                                        ++deletesThisPass; break;
                                }
                                dataObject.pendingOp = DatabaseOp.DOO_None;
                            }

                            Log.Notice(Connection.SchemaName, "Saved " + _dirtyObjects.Count + " objects in " + (TCPManager.GetTimeStampMS() - Start) + "ms. Added " + addsThisPass + ", updated " + savesThisPass + ", deleted " + deletesThisPass + ".");
                        }

                        _dirtyObjects.Clear();
                    }
                }

                long elapsed = TCPManager.GetTimeStampMS() - Start;
                if (elapsed < PROCESS_INTERVAL)
                    Thread.Sleep((int)(PROCESS_INTERVAL - elapsed));
            }
        }

        public int GetNextAutoIncrement<TObject>()
            where TObject : DataObject
        {
            return GetNextAutoIncrementImpl<TObject>();
        }

        public int GetObjectCount<TObject>()
            where TObject : DataObject
        {
            return GetObjectCount<TObject>("");
        }

        public int GetObjectCount<TObject>(string whereExpression)
            where TObject : DataObject
        {
            return GetObjectCountImpl<TObject>(whereExpression);
        }

        public long GetMaxColValue<TObject>(string column)
        where TObject : DataObject
        {
            return GetMaxColValueImpl<TObject>(column);
        }

        public TObject FindObjectByKey<TObject>(object key)
            where TObject : DataObject
        {
            var dataObject = FindObjectByKeyImpl<TObject>(key);

            return dataObject ?? default(TObject);
        }

        // Sélectionne un objet , si il y en a plusieurs , le premier et retourné
        public TObject SelectObject<TObject>(string whereExpression)
            where TObject : DataObject
        {
            return SelectObject<TObject>(whereExpression, IsolationLevel.DEFAULT);
        }

        // Sélectionne un objet , si il y en a plusieurs , le premier et retourné
        public TObject SelectObject<TObject>(string whereExpression, IsolationLevel isolation)
            where TObject : DataObject
        {
            var objs = SelectObjects<TObject>(whereExpression, isolation);

            if (objs.Count > 0)
                return objs[0];

            return default(TObject);
        }

        public IList<TObject> SelectObjects<TObject>(string whereExpression)
            where TObject : DataObject
        {
            return SelectObjects<TObject>(whereExpression, IsolationLevel.DEFAULT);
        }

        public IList<TObject> SelectObjects<TObject>(string whereExpression, IsolationLevel isolation)
            where TObject : DataObject
        {
            var dataObjects = SelectObjectsImpl<TObject>(whereExpression, isolation);

            return dataObjects ?? new List<TObject>();
        }

        public IList<TObject> SelectAllObjects<TObject>()
            where TObject : DataObject
        {
            return SelectAllObjects<TObject>(IsolationLevel.DEFAULT);
        }

        public IList<TObject> SelectAllObjects<TObject>(IsolationLevel isolation)
            where TObject : DataObject
        {
            var dataObjects = SelectAllObjectsImpl<TObject>(isolation);

            return dataObjects ?? new List<TObject>();
        }

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName)
            where TObject : DataObject
        {
            return MapAllObjects<TKey, TObject>(keyName, 100, IsolationLevel.DEFAULT);
        }

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName, IsolationLevel isolation)
            where TObject : DataObject
        {
            return MapAllObjects<TKey, TObject>(keyName, 100, isolation);
        }

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName, string whereClause)
            where TObject : DataObject
        {
            return MapAllObjects<TKey, TObject>(keyName, whereClause, 100, IsolationLevel.DEFAULT);
        }

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName, string whereClause, IsolationLevel isolation)
            where TObject : DataObject
        {
            return MapAllObjects<TKey, TObject>(keyName, whereClause, 100, isolation);
        }

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName, int expectedRowCount)
            where TObject : DataObject
        {
            return MapAllObjects<TKey, TObject>(keyName, null, expectedRowCount, IsolationLevel.DEFAULT);
        }

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName, int expectedRowCount, IsolationLevel isolation)
            where TObject : DataObject
        {
            return MapAllObjects<TKey, TObject>(keyName, null, expectedRowCount, isolation);
        }

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName, string whereClause, int expectedRowCount)
            where TObject : DataObject
        {
            return MapAllObjects<TKey, TObject>(keyName, whereClause, expectedRowCount, IsolationLevel.DEFAULT);
        }

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName, string whereClause, int expectedRowCount, IsolationLevel isolation)
            where TObject : DataObject
        {
            Dictionary<TKey, TObject> dataObjects = MapAllObjectsImpl<TKey, TObject>(keyName, whereClause, expectedRowCount, isolation);

            return dataObjects ?? new Dictionary<TKey, TObject>();
        }
        

        public void RegisterDataObject(Type objType)
        {
            if (TableDatasets.ContainsKey(GetTableOrViewName(objType)))
                return;

            bool relations = false;

            string tableName = GetTableOrViewName(objType);
            var table = new System.Data.DataTable(tableName);

            List<string> primaryKeys = new List<string>();

            MemberInfo[] myMembers = objType.GetMembers();
            
            // Load the primary keys and data elements into the data table
            foreach (MemberInfo memberInfo in myMembers)
            {
                object[] myAttributes = memberInfo.GetCustomAttributes(typeof(PrimaryKey), true);

                if (myAttributes.Length > 0)
                {
                    if (memberInfo is PropertyInfo)
                        table.Columns.Add(memberInfo.Name, ((PropertyInfo)memberInfo).PropertyType);
                    else
                        table.Columns.Add(memberInfo.Name, ((FieldInfo)memberInfo).FieldType);

                    table.Columns[memberInfo.Name].AutoIncrement = ((PrimaryKey)myAttributes[0]).AutoIncrement;
                    table.Columns[memberInfo.Name].AutoIncrementSeed = ((PrimaryKey)myAttributes[0]).IncrementValue;

                    primaryKeys.Add(memberInfo.Name);
                    continue;
                }

                myAttributes = memberInfo.GetCustomAttributes(typeof(DataElement), true);

                if (myAttributes.Length > 0)
                {
                    if (memberInfo is PropertyInfo)
                        table.Columns.Add(memberInfo.Name, ((PropertyInfo)memberInfo).PropertyType);
                    else
                        table.Columns.Add(memberInfo.Name, ((FieldInfo)memberInfo).FieldType);

                    table.Columns[memberInfo.Name].AllowDBNull = ((DataElement)myAttributes[0]).AllowDbNull;

                    if (((DataElement)myAttributes[0]).Unique)
                        table.Constraints.Add(new UniqueConstraint("UNIQUE_" + memberInfo.Name, table.Columns[memberInfo.Name]));

                    if (((DataElement)myAttributes[0]).Index)
                        table.Columns[memberInfo.Name].ExtendedProperties.Add("INDEX", true);

                    if (((DataElement)myAttributes[0]).Varchar > 0)
                        table.Columns[memberInfo.Name].ExtendedProperties.Add("VARCHAR", ((DataElement)myAttributes[0]).Varchar);

                    if (!relations)
                        relations = GetRelationAttributes(memberInfo).Length > 0;
                }
            }

            if (primaryKeys.Count > 0)
            {
                var index = new DataColumn[primaryKeys.Count];
                for (int i=0; i < primaryKeys.Count; ++i)
                    index[i] = table.Columns[primaryKeys[i]];
                table.PrimaryKey = index;
            }

            else // Add a primary key column to use
            {
                table.Columns.Add(tableName + "_ID", typeof(string));

                DataColumn[] index = new DataColumn[1];
                index[0] = table.Columns[tableName + "_ID"];
                table.PrimaryKey = index;
            }

            if (Connection.IsSQLConnection)
                Connection.CheckOrCreateTable(table);

            DataSet dataSet = new DataSet
            {
                DataSetName = tableName,
                EnforceConstraints = true,
                CaseSensitive = false
            };

            dataSet.Tables.Add(table);

            DataTableHandler dataTableHandler = new DataTableHandler(dataSet)
            {
                HasRelations = relations,
                UsesPreCaching = DataObject.GetPreCachedFlag(objType),
                RequiresObjectId = primaryKeys.Count == 0,
                BindingMethod = DataObject.GetBindingMethod(objType)
            };

            TableDatasets.Add(tableName, dataTableHandler);
        }

        public string[] GetTableNameList()
        {
            return TableDatasets.Select(entry => entry.Key).ToArray();
        }

        public string Escape(string toEscape)
        {
            return Connection.Escape(toEscape);
        }

        public bool ExecuteNonQuery(string rawQuery)
        {
            return ExecuteNonQueryImpl(rawQuery);
        }

        public long ExecuteQueryInt(string rawQuery)
        {
            return ExecuteQueryIntImpl(rawQuery);
        }

        #endregion

        #region Implementation

        // Ajoute un objet a la database , true = Success
        protected abstract bool AddObjectImpl(DataObject dataObject);

        // Sauvegarde un Objet dans la Database
        protected abstract void SaveObjectImpl(DataObject dataObject);

        // Supprime un objet de la database
        protected abstract void DeleteObjectImpl(DataObject dataObject);

        protected abstract bool RunTransaction(List<DataObject> dataObjects);

        // Trouve un objet a partir de sa primaryKey
        protected abstract TObject FindObjectByKeyImpl<TObject>(object key)
            where TObject : DataObject;

        // Trouve un objet a partir de sa primaryKey
        protected abstract DataObject FindObjectByKeyImpl(Type objectType, object key);

        // Sélectionne un objet a partir d'une table et des paramètres
        protected abstract DataObject[] SelectObjectsImpl(Type objectType, string whereClause, IsolationLevel isolation);

        // Sélectionne un objet a partir d'une table et des paramètres
        protected abstract IList<TObject> SelectObjectsImpl<TObject>(string whereClause, IsolationLevel isolation)
            where TObject : DataObject;

        // Sélectionne tous les objets de la database
        protected abstract IList<TObject> SelectAllObjectsImpl<TObject>(IsolationLevel isolation)
            where TObject : DataObject;

        protected abstract Dictionary<TKey, TObject> MapAllObjectsImpl<TKey, TObject>(string keyName, string where, int expectedRowCount, IsolationLevel isolation)
            where TObject : DataObject;

        protected abstract int GetNextAutoIncrementImpl<TObject>()
            where TObject : DataObject;

        // Retourn le nombre d'objet dans la database
        protected abstract int GetObjectCountImpl<TObject>(string where)
            where TObject : DataObject;

        // Returns highest value within DB
        protected abstract long GetMaxColValueImpl<TObject>(string column)
            where TObject : DataObject;

        protected abstract bool ExecuteNonQueryImpl(string raqQuery);
        protected abstract long ExecuteQueryIntImpl(string raqQuery);
        #endregion

        #region Relations

        public void FillObjectRelations(DataObject dataObject)
        {
            FillLazyObjectRelations(dataObject, false);
        }

        protected void SaveObjectRelations(DataObject dataObject)
        {
            try
            {
                object val;

                Type myType = dataObject.GetType();

                MemberInfo[] myMembers = myType.GetMembers();

                for (int i = 0; i < myMembers.Length; i++)
                {
                    Relation[] myAttributes = GetRelationAttributes(myMembers[i]);
                    if (myAttributes.Length > 0)
                    {
                        bool array = false;

                        Type type;

                        if (myMembers[i] is PropertyInfo)
                            type = ((PropertyInfo)myMembers[i]).PropertyType;
                        else
                            type = ((FieldInfo)myMembers[i]).FieldType;

                        if (type.HasElementType)
                        {
                            //type = type.GetElementType();
                            array = true;
                        }

                        val = null;

                        if (array)
                        {
                            if (myMembers[i] is PropertyInfo)
                            {
                                val = ((PropertyInfo)myMembers[i]).GetValue(dataObject, null);
                            }
                            if (myMembers[i] is FieldInfo)
                            {
                                val = ((FieldInfo)myMembers[i]).GetValue(dataObject);
                            }
                            if (val is Array)
                            {
                                var a = val as Array;

                                foreach (object o in a)
                                {
                                    if (o is DataObject)
                                        SaveObject(o as DataObject);
                                }
                            }
                            else
                            {
                                if (val is DataObject)
                                    SaveObject(val as DataObject);
                            }
                        }
                        else
                        {
                            if (myMembers[i] is PropertyInfo)
                                val = ((PropertyInfo)myMembers[i]).GetValue(dataObject, null);
                            if (myMembers[i] is FieldInfo)
                                val = ((FieldInfo)myMembers[i]).GetValue(dataObject);
                            if (val is DataObject)
                                SaveObject(val as DataObject);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new DatabaseException("Relation save failed !", e);
            }
        }

        protected void DeleteObjectRelations(DataObject dataObject)
        {
            try
            {
                object val;

                Type myType = dataObject.GetType();

                MemberInfo[] myMembers = myType.GetMembers();

                for (int i = 0; i < myMembers.Length; i++)
                {
                    Relation[] myAttributes = GetRelationAttributes(myMembers[i]);
                    if (myAttributes.Length > 0)
                    {
                        if (myAttributes[0].AutoDelete == false)
                            continue;

                        bool array = false;

                        Type type;

                        if (myMembers[i] is PropertyInfo)
                            type = ((PropertyInfo)myMembers[i]).PropertyType;
                        else
                            type = ((FieldInfo)myMembers[i]).FieldType;

                        if (type.HasElementType)
                        {
                            type = type.GetElementType();
                            array = true;
                        }

                        val = null;

                        if (array)
                        {
                            if (myMembers[i] is PropertyInfo)
                            {
                                val = ((PropertyInfo)myMembers[i]).GetValue(dataObject, null);
                            }
                            if (myMembers[i] is FieldInfo)
                            {
                                val = ((FieldInfo)myMembers[i]).GetValue(dataObject);
                            }
                            if (val is Array)
                            {
                                var a = val as Array;

                                foreach (object o in a)
                                {
                                    if (o is DataObject)
                                        DeleteObject(o as DataObject);
                                }
                            }
                            else
                            {
                                if (val is DataObject)
                                    DeleteObject(val as DataObject);
                            }
                        }
                        else
                        {
                            if (myMembers[i] is PropertyInfo)
                                val = ((PropertyInfo)myMembers[i]).GetValue(dataObject, null);
                            if (myMembers[i] is FieldInfo)
                                val = ((FieldInfo)myMembers[i]).GetValue(dataObject);
                            if (val != null && val is DataObject)
                                DeleteObject(val as DataObject);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new DatabaseException("Relations delete failed !", e);
            }
        }

        protected void FillLazyObjectRelations(DataObject dataObject, bool autoload)
        {
            try
            {
                var dataObjectType = dataObject.GetType();

                MemberInfo[] myMembers;
                if (!MemberInfoCache.TryGetValue(dataObjectType, out myMembers))
                {
                    myMembers = dataObjectType.GetMembers();
                    MemberInfoCache[dataObjectType] = myMembers;
                }

                for (int i = 0; i < myMembers.Length; i++)
                {
                    Relation[] myAttributes = GetRelationAttributes(myMembers[i]);

                    if (myAttributes.Length > 0)
                    {
                        Relation rel = myAttributes[0];

                        if ((rel.AutoLoad == false) && autoload)
                            continue;

                        bool isArray = false;
                        Type remoteType;
                        DataObject[] elements;

                        string local = rel.LocalField;
                        string remote = rel.RemoteField;

                        if (myMembers[i] is PropertyInfo)
                        {
                            remoteType = ((PropertyInfo)myMembers[i]).PropertyType;
                        }
                        else
                        {
                            remoteType = ((FieldInfo)myMembers[i]).FieldType;
                        }

                        if (remoteType.HasElementType)
                        {
                            remoteType = remoteType.GetElementType();
                            isArray = true;
                        }

                        PropertyInfo prop = dataObjectType.GetProperty(local);
                        FieldInfo field = dataObjectType.GetField(local);

                        object val = 0;

                        if (prop != null)
                        {
                            val = prop.GetValue(dataObject, null);
                        }
                        if (field != null)
                        {
                            val = field.GetValue(dataObject);
                        }

                        if (val != null && val.ToString() != string.Empty)
                        {
                            if (DataObject.GetPreCachedFlag(remoteType))
                            {
                                elements = new DataObject[1];
                                elements[0] = FindObjectByKeyImpl(remoteType, val);
                            }
                            else
                            {
                                elements = SelectObjectsImpl(remoteType, remote + " = '" + Escape(val.ToString()) + "'", IsolationLevel.DEFAULT);
                            }

                            if ((elements != null) && (elements.Length > 0))
                            {
                                if (isArray)
                                {
                                    if (myMembers[i] is PropertyInfo)
                                    {
                                        ((PropertyInfo)myMembers[i]).SetValue(dataObject, elements, null);
                                    }
                                    if (myMembers[i] is FieldInfo)
                                    {
                                        var currentField = (FieldInfo)myMembers[i];
                                        ConstructorInfo constructor;
                                        if (!ConstructorByFieldType.TryGetValue(currentField.FieldType, out constructor))
                                        {
                                            constructor = currentField.FieldType.GetConstructor(new[] { typeof(int) });
                                            ConstructorByFieldType[currentField.FieldType] = constructor;
                                        }

                                        object elementHolder = constructor.Invoke(new object[] { elements.Length });
                                        var elementArray = (object[])elementHolder;

                                        for (int m = 0; m < elementArray.Length; m++)
                                        {
                                            elementArray[m] = elements[m];
                                        }

                                        currentField.SetValue(dataObject, elementArray);
                                    }
                                }
                                else
                                {
                                    if (myMembers[i] is PropertyInfo)
                                    {
                                        ((PropertyInfo)myMembers[i]).SetValue(dataObject, elements[0], null);
                                    }
                                    if (myMembers[i] is FieldInfo)
                                    {
                                        ((FieldInfo)myMembers[i]).SetValue(dataObject, elements[0]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new DatabaseException("Resolution of relations " + dataObject.TableName + " failed!", e);
            }
        }

        #endregion

        #region Cache - Methods within have no references.

        /// <summary>
        /// Referenced only from DeleteObject, no apparent function.
        /// </summary>
        protected void DeleteFromCache(string tableName, DataObject obj)
        {
            DataTableHandler handler = TableDatasets[tableName];
            handler.SetCacheObject(obj.ObjectId, null);
        }

        /// <summary>
        /// Unused method.
        /// </summary>
        public bool UpdateInCache<TObject>(object key)
            where TObject : DataObject
        {
            MemberInfo[] members = typeof(TObject).GetMembers();
            var ret = (TObject)Activator.CreateInstance(typeof(TObject));

            string tableName = ret.TableName;
            DataTableHandler dth = TableDatasets[tableName];
            string whereClause = null;

            if (!dth.UsesPreCaching || key == null)
                return false;

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

            var objs = SelectObjects<TObject>(whereClause);
            if (objs.Count > 0)
            {
                dth.SetPreCachedObject(key, objs[0]);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unused method.
        /// </summary>
        protected void ReloadCache(string tableName)
        {
            DataTableHandler handler = TableDatasets[tableName];

            ICache cache = handler.Cache;

            foreach (object o in cache.Keys)
            {
                ReloadObject(cache[o] as DataObject);
            }
        }

        #endregion

        #region Helpers

        protected Relation[] GetRelationAttributes(MemberInfo info)
        {
            Relation[] rel;
            if (RelationAttributes.TryGetValue(info, out rel))
                return rel;

            rel = (Relation[])info.GetCustomAttributes(typeof(Relation), true);
            RelationAttributes[info] = rel;

            return rel;
        }

        /// <summary>
        /// Returns an array of information about the database-relevant members of a given type, such as primary key status and presence of any relations.
        /// </summary>
        protected BindingInfo[] GetBindingInfo(Type objectType)
        {
            BindingInfo[] bindingInfos;

            if (!_bindingInfos.TryGetValue(objectType, out bindingInfos))
            {
                var list = new List<BindingInfo>();

                MemberInfo[] objMembers = objectType.GetMembers();

                DataObject assignObject = (DataObject)Activator.CreateInstance(objectType);

                for (int i = 0; i < objMembers.Length; i++)
                {
                    object[] keyAttrib = objMembers[i].GetCustomAttributes(typeof(PrimaryKey), true);
                    object[] readonlyAttrib = objMembers[i].GetCustomAttributes(typeof(ReadOnly), true);
                    object[] attrib = objMembers[i].GetCustomAttributes(typeof(DataElement), true);
                    Relation[] relAttrib = GetRelationAttributes(objMembers[i]);
                    

                    if (attrib.Length > 0 || keyAttrib.Length > 0 || relAttrib.Length > 0 || readonlyAttrib.Length > 0)
                    {
                        var info = new BindingInfo(assignObject, objMembers[i], keyAttrib.Length > 0, relAttrib.Length > 0, readonlyAttrib.Length > 0,
                                                   (attrib.Length > 0) ? (DataElement)attrib[0] : null);
                        list.Add(info);
                    }
                }

                bindingInfos = list.ToArray();
                _bindingInfos[objectType] = bindingInfos;
            }

            return bindingInfos;
        }

        /// <summary>
        /// Returns a list of binding functions for an instance of an object of a given type.
        /// </summary>
        protected ObjectPool<StaticMemberBindInfo> GetStaticBindPool<T>() where T : DataObject
        {
            ObjectPool<StaticMemberBindInfo> staticBindPool;

            Type objectType = typeof (T);

            if (!_staticBindPools.TryGetValue(objectType, out staticBindPool))
            {
                staticBindPool = new ObjectPool<StaticMemberBindInfo>(CreateStaticBindInfo<T>);
                _staticBindPools[objectType] = staticBindPool;
            }

            return staticBindPool;
        }

        public StaticMemberBindInfo CreateStaticBindInfo<T>() where T : DataObject
        {
            Type objectType = typeof(T);

            MemberInfo[] objMembers = objectType.GetMembers();

            DataObject assignObject = (DataObject)Activator.CreateInstance(objectType);

            List<MemberInfo> databaseMembers = new List<MemberInfo>();

            for (int i = 0; i < objMembers.Length; i++)
            {
                object[] keyAttrib = objMembers[i].GetCustomAttributes(typeof(PrimaryKey), true);
                object[] readonlyAttrib = objMembers[i].GetCustomAttributes(typeof(ReadOnly), true);
                object[] attrib = objMembers[i].GetCustomAttributes(typeof(DataElement), true);
                Relation[] relAttrib = GetRelationAttributes(objMembers[i]);

                if (attrib.Length > 0 || keyAttrib.Length > 0 || relAttrib.Length > 0 || readonlyAttrib.Length > 0)
                    databaseMembers.Add(objMembers[i]);
            }

            return new StaticMemberBindInfo(assignObject, databaseMembers);
        } 

        // Lecture de la clef primaire
        public static string GetTableOrViewName(Type objectType)
        {
            string name = DataObject.GetViewName(objectType);

            // if not a view, we use tablename, else viewname
            if (string.IsNullOrEmpty(name))
                return DataObject.GetTableName(objectType);

            return name;
        }

        /// <summary>
        /// Referenced only by unused caching methods.
        /// </summary>
        /// <param name="dataObject"></param>
        private void ReloadObject(DataObject dataObject)
        {
            try
            {
                if (dataObject == null)
                    return;

                DataRow row = FindRowByKey(dataObject);

                if (row == null)
                    throw new DatabaseException("Reload of Databaseobject failed (Keyvalue Changed ?)!");

                FillObjectWithRow(ref dataObject, row, true);

                dataObject.Dirty = false;
                dataObject.IsValid = true;
            }
            catch (Exception e)
            {
                throw new DatabaseException("Reload of Databaseobject failed !", e);
            }
        }

        #endregion

        #region Factory

        public static IObjectDatabase GetObjectDatabase(ConnectionType connectionType, string connectionString, string schemaName)
        {
            if (connectionType == ConnectionType.DATABASE_MYSQL)
                return new MySQLObjectDatabase(new MySqlDataConnection(connectionString, schemaName));
            else if(connectionType == ConnectionType.DATABASE_MSSQL)
               return new SQLObjectDatabase(new SqlDataConnection(connectionString,schemaName));

            return null;
        }

        #endregion

        #region Nested type: BindingInfo

        public class StaticMemberBindInfo
        {
            public DataObject AssignObject { get; }
            public DataBinder[] DataBinders;

            public StaticMemberBindInfo(DataObject assignObject, List<MemberInfo> members)
            {
                AssignObject = assignObject;

                DataBinders = new DataBinder[members.Count];

                for (int i=0; i < members.Count; ++i)
                {
                    // Used for the fast static binding method for assigning to properties.
                    PropertyInfo info = members[i] as PropertyInfo;
                    if (info == null)
                        continue;

                    DataBinders[i] = DataBinder.GetFor(AssignObject, info.PropertyType, info.GetSetMethod());
                }
            }
        }

        protected class BindingInfo
        {
            public readonly bool HasRelation;
            public readonly MemberInfo Member;
            public readonly bool ReadOnly;
            public DataElement DataElementAttribute;
            public MySqlExpressionDataBinder MySqlBinder;
            public bool PrimaryKey;

            public BindingInfo(DataObject assignObject, MemberInfo member, bool primaryKey, bool hasRelation, bool readOnly, DataElement attrib)
            {
                Member = member;

                // Used for the compiled expression method for assigning to properties and fields.
                MySqlBinder = (MySqlExpressionDataBinder)typeof(MySqlExpressionDataBinder).GetMethod("GetFor").MakeGenericMethod(assignObject.GetType()).Invoke(null, new object[] { member });

                PrimaryKey = primaryKey;
                HasRelation = hasRelation;
                DataElementAttribute = attrib;
                ReadOnly = readOnly;
            }
        }

        #endregion
    }
}