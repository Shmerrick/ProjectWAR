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
    public enum DatabaseOp : Byte
    {
        DOO_None,
        DOO_Insert,
        DOO_Update,
        DOO_Delete
    }

    public abstract class ObjectDatabase : IObjectDatabase
    {
        protected static readonly NumberFormatInfo Nfi = new CultureInfo("en-US", false).NumberFormat;

        public ThreadLocal<List<Tuple<DataObject, DatabaseOp>>> ThreadLocalOperationsQueue = new ThreadLocal<List<Tuple<DataObject, DatabaseOp>>>();

        protected readonly DataConnection Connection;
        protected readonly Dictionary<String, DataTableHandler> TableDatasets;

        private readonly Dictionary<Type, BindingInfo[]> _bindingInfos = new Dictionary<Type, BindingInfo[]>();
        private readonly Dictionary<Type, ObjectPool<StaticMemberBindInfo>> _staticBindPools = new Dictionary<Type, ObjectPool<StaticMemberBindInfo>>();
        private readonly Dictionary<Type, ConstructorInfo> ConstructorByFieldType = new Dictionary<Type, ConstructorInfo>();
        private readonly Dictionary<Type, MemberInfo[]> MemberInfoCache = new Dictionary<Type, MemberInfo[]>();
        private readonly Dictionary<MemberInfo, Relation[]> RelationAttributes = new Dictionary<MemberInfo, Relation[]>();

        private volatile Boolean _updateLoopRunning = false;
        private volatile Boolean _updateTriggerLoopRunning = false;
        private readonly Thread _updateTriggerLoop;
        private readonly Thread _updateLoop;
        private const Int32 _updateInterval = 50000; // every minute (60000ms) process operations, actions and objects.
        private static ManualResetEvent _updateSleep = new ManualResetEvent(false);

        private volatile List<Action> _actions = new List<Action>();

        protected UInt32 _inserts = 0;
        protected UInt32 _updates = 0;
        protected UInt32 _deletes = 0;

        public virtual String SqlCommand_CharLength() => "CHAR_LENGTH";

        protected ObjectDatabase(DataConnection connection)
        {
            TableDatasets = new Dictionary<String, DataTableHandler>();
            Connection = connection;

            _updateSleep.Reset();

            ThreadStart updateTriggerStart = UpdateTrigger;
            _updateTriggerLoop = new Thread(updateTriggerStart);
            _updateTriggerLoop.Name = "ObjectDatabase.UpdateTrigger";
            _updateTriggerLoopRunning = true;
            _updateTriggerLoop.Start();

            ThreadStart updateLoopStart = UpdateLoop;
            _updateLoop = new Thread(updateLoopStart);
            _updateLoop.Name = "ObjectDatabase.Update";
            _updateLoopRunning = true;
            _updateLoop.Start();

        }

        #region Data tables

        protected DataSet GetDataSet(String tableName)
        {
            return !TableDatasets.ContainsKey(tableName) ? null : TableDatasets[tableName].DataSet;
        }

        protected void FillObjectWithRow<TObject>(ref TObject dataObject, DataRow row, Boolean reload) where TObject : DataObject {
            Boolean relation = false;

            String tableName = dataObject.TableName;
            Type myType = dataObject.GetType();
            String id = row[tableName + "_ID"].ToString();

            MemberInfo[] myMembers = myType.GetMembers();
            dataObject.ObjectId = id;

            for (Int32 i = 0; i < myMembers.Length; i++)
            {
                Object[] myAttributes = GetRelationAttributes(myMembers[i]);

                if (myAttributes.Length > 0)
                {
                    relation = true;
                }
                else
                {
                    Object[] keyAttrib = myMembers[i].GetCustomAttributes(typeof(PrimaryKey), true);
                    myAttributes = myMembers[i].GetCustomAttributes(typeof(DataElement), true);
                    if (myAttributes.Length > 0 || keyAttrib.Length > 0)
                    {
                        Object val = row[myMembers[i].Name];
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
            Boolean relation = false;

            Type myType = dataObject.GetType();

            row[dataObject.TableName + "_ID"] = dataObject.ObjectId;

            MemberInfo[] myMembers = myType.GetMembers();

            for (Int32 i = 0; i < myMembers.Length; i++)
            {
                Object[] myAttributes = GetRelationAttributes(myMembers[i]);
                Object val = null;

                if (myAttributes.Length > 0)
                {
                    relation = true;
                }
                else
                {
                    myAttributes = myMembers[i].GetCustomAttributes(typeof(DataElement), true);
                    Object[] keyAttrib = myMembers[i].GetCustomAttributes(typeof(PrimaryKey), true);

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
            }

            if (relation)
            {
                SaveObjectRelations(dataObject);
            }
        }

        protected DataRow FindRowByKey(DataObject dataObject)
        {
            DataRow row;

            String tableName = dataObject.TableName;

            System.Data.DataTable table = GetDataSet(tableName).Tables[tableName];

            Type myType = dataObject.GetType();

            String key = table.PrimaryKey[0].ColumnName;

            if (key.Equals(tableName + "_ID"))
                row = table.Rows.Find(dataObject.ObjectId);
            else
            {
                MemberInfo[] keymember = myType.GetMember(key);

                Object val = null;

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

        public String GetSchemaName() => Connection.SchemaName;

        public void RegisterAction(Action action) => _actions.Add(action);

        /// <summary>A list of all objects that are pending some kind of database modification.</summary>
        private volatile List<DataObject> _objects = new List<DataObject>();

        /// <summary>A list of database operations to be performed on given objects, in order.</summary>
        private volatile List<Tuple<DataObject, DatabaseOp>> _operations = new List<Tuple<DataObject, DatabaseOp>>();

        public Boolean AddObject(DataObject dataObject)
        {
            lock (_operations) {
                if (dataObject.AllowAdd) {
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
            if (dataObject.Dirty) {
                if (!dataObject.IsValid) {
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

                lock (_operations)
                    _operations.Add(new Tuple<DataObject, DatabaseOp>(dataObject, DatabaseOp.DOO_Update));
            }
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

        ~ObjectDatabase()
        {
            Stop();
        }

        public virtual void Stop()
        {
            _updateTriggerLoopRunning = false;
            _updateLoopRunning = false;
            _updateSleep.Set();
        }

        protected virtual void UpdateTrigger()
        {
            while (_updateTriggerLoopRunning) {
                Thread.Sleep(_updateInterval);
                _updateSleep.Set();
                Thread.Yield();
                Log.Notice(Thread.CurrentThread.Name, $"A Thread Necromancer awakens.");
            }
        }

        public virtual void ForceSave()
        {
            Log.Notice(Thread.CurrentThread.Name, $"A Thread Necromancer cries {_updateLoop.ThreadState} into your lap.");
            if (_updateLoopRunning && _updateLoop.ThreadState == ThreadState.WaitSleepJoin) {
                Log.Notice(Thread.CurrentThread.Name, $"{new System.Diagnostics.StackFrame(1, true).GetMethod().Name} triggered a forced Save");
                _updateSleep.Set();
                Log.Notice(Thread.CurrentThread.Name, $"A Thread Necromancer screams into your nipples.");
            }
        }

        protected void UpdateLoop()
        {
            Int64 previousTick;
            Int64 thisTick = TCPManager.GetTimeStampMS();
            Int64 deltaTick;

            while (_updateLoopRunning) {
                Thread.Yield();

                previousTick = thisTick;
                thisTick = TCPManager.GetTimeStampMS();
                deltaTick = thisTick - previousTick;

                if (0 != _operations.Count) {

                    lock (_operations) {

                        ThreadLocalOperationsQueue.Value = _operations;
                        _operations.Clear();
                    }

                    foreach (Tuple<DataObject, DatabaseOp> opInfo in ThreadLocalOperationsQueue.Value) {
                        Thread.Yield();

                        if (opInfo.Item1.pendingOp == opInfo.Item2)
                            continue;

                        if (opInfo.Item1.pendingOp == DatabaseOp.DOO_None) {
                            _objects.Add(opInfo.Item1);
                            opInfo.Item1.pendingOp = opInfo.Item2;
                        }

                        switch (opInfo.Item2) {

                            case DatabaseOp.DOO_Insert:
                                if (opInfo.Item1.pendingOp == DatabaseOp.DOO_Delete) {
                                    opInfo.Item1.pendingOp = DatabaseOp.DOO_Update;
                                    Log.Info("ObjectDatabase", "Transforming Update & Delete on " + opInfo.Item1.GetType() + " into an Update.");
                                }
                                break;

                            case DatabaseOp.DOO_Delete:
                                opInfo.Item1.pendingOp = opInfo.Item1.pendingOp == DatabaseOp.DOO_Insert ? DatabaseOp.DOO_None : DatabaseOp.DOO_Delete;
                                break;
                        }
                    }
                }

                if (0 != _actions.Count) {
                    foreach (Action action in _actions) {
                        Thread.Yield();

                        action();
                    }
                }

                if (0 != _objects.Count) {
                    if (!RunTransaction(_objects)) {

                        _inserts = 0;
                        _updates = 0;
                        _deletes = 0;

                        foreach (DataObject dataObject in _objects) {
                            Thread.Yield();

                            switch (dataObject.pendingOp) {

                                case DatabaseOp.DOO_Insert:
                                    AddObjectImpl(dataObject);
                                    ++_inserts; break;

                                case DatabaseOp.DOO_Update:
                                    SaveObjectImpl(dataObject);
                                    ++_updates; break;

                                case DatabaseOp.DOO_Delete:
                                    DeleteObjectImpl(dataObject);
                                    ++_deletes; break;
                            }
                            dataObject.pendingOp = DatabaseOp.DOO_None;

                        }

                        Log.Notice(Connection.SchemaName, "Saved " + _objects.Count + " Added " + _inserts + ", updated " + _updates + ", deleted " + _deletes + ".");
                    }

                    _objects.Clear();
                }

                _updateSleep.WaitOne();
                _updateSleep.Reset();
                Log.Notice(Thread.CurrentThread.Name, $"Thread Necromancer hit you for {deltaTick}ms.");
            }
        }

        public Int32 GetNextAutoIncrement<TObject>() where TObject : DataObject => GetNextAutoIncrementImpl<TObject>();

        public Int32 GetObjectCount<TObject>() where TObject : DataObject => GetObjectCount<TObject>("");

        public Int32 GetObjectCount<TObject>(String whereExpression) where TObject : DataObject => GetObjectCountImpl<TObject>(whereExpression);

        public Int64 GetMaxColValue<TObject>(String column) where TObject : DataObject => GetMaxColValueImpl<TObject>(column);

        public TObject FindObjectByKey<TObject>(Object key) where TObject : DataObject
        {
            TObject dataObject = FindObjectByKeyImpl<TObject>(key);
            return dataObject ?? default(TObject);
        }

        // Sélectionne un objet , si il y en a plusieurs , le premier et retourné
        public TObject SelectObject<TObject>(String whereExpression) where TObject : DataObject => SelectObject<TObject>(whereExpression, IsolationLevel.DEFAULT);

        // Sélectionne un objet , si il y en a plusieurs , le premier et retourné
        public TObject SelectObject<TObject>(String whereExpression, IsolationLevel isolation) where TObject : DataObject
        {
            IList<TObject> objs = SelectObjects<TObject>(whereExpression, isolation);

            return objs.Count > 0 ? objs[0] : default(TObject);
        }

        public IList<TObject> SelectObjects<TObject>(String whereExpression) where TObject : DataObject => SelectObjects<TObject>(whereExpression, IsolationLevel.DEFAULT);

        public IList<TObject> SelectObjects<TObject>(String whereExpression, IsolationLevel isolation) where TObject : DataObject
        {
            IList<TObject> dataObjects = SelectObjectsImpl<TObject>(whereExpression, isolation);
            return dataObjects ?? new List<TObject>();
        }

        public IList<TObject> SelectAllObjects<TObject>() where TObject : DataObject => SelectAllObjects<TObject>(IsolationLevel.DEFAULT);

        public IList<TObject> SelectAllObjects<TObject>(IsolationLevel isolation) where TObject : DataObject
        {
            IList<TObject> dataObjects = SelectAllObjectsImpl<TObject>(isolation);
            return dataObjects ?? new List<TObject>();
        }

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(String keyName) where TObject : DataObject => MapAllObjects<TKey, TObject>(keyName, 100, IsolationLevel.DEFAULT);

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(String keyName, IsolationLevel isolation) where TObject : DataObject => MapAllObjects<TKey, TObject>(keyName, 100, isolation);

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(String keyName, String whereClause) where TObject : DataObject => MapAllObjects<TKey, TObject>(keyName, whereClause, 100, IsolationLevel.DEFAULT);

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(String keyName, String whereClause, IsolationLevel isolation) where TObject : DataObject => MapAllObjects<TKey, TObject>(keyName, whereClause, 100, isolation);

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(String keyName, Int32 expectedRowCount) where TObject : DataObject => MapAllObjects<TKey, TObject>(keyName, null, expectedRowCount, IsolationLevel.DEFAULT);

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(String keyName, Int32 expectedRowCount, IsolationLevel isolation) where TObject : DataObject => MapAllObjects<TKey, TObject>(keyName, null, expectedRowCount, isolation);

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(String keyName, String whereClause, Int32 expectedRowCount) where TObject : DataObject => MapAllObjects<TKey, TObject>(keyName, whereClause, expectedRowCount, IsolationLevel.DEFAULT);

        public Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(String keyName, String whereClause, Int32 expectedRowCount, IsolationLevel isolation) where TObject : DataObject
        {
            Dictionary<TKey, TObject> dataObjects = MapAllObjectsImpl<TKey, TObject>(keyName, whereClause, expectedRowCount, isolation);
            return dataObjects ?? new Dictionary<TKey, TObject>();
        }

        public void RegisterDataObject(Type objType)
        {
            if (TableDatasets.ContainsKey(GetTableOrViewName(objType)))
                return;

            Boolean relations = false;

            String tableName = GetTableOrViewName(objType);
            var table = new System.Data.DataTable(tableName);

            var primaryKeys = new List<String>();

            MemberInfo[] myMembers = objType.GetMembers();

            // Load the primary keys and data elements into the data table
            foreach (MemberInfo memberInfo in myMembers)
            {
                Object[] myAttributes = memberInfo.GetCustomAttributes(typeof(PrimaryKey), true);

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
                for (Int32 i = 0; i < primaryKeys.Count; ++i)
                    index[i] = table.Columns[primaryKeys[i]];
                table.PrimaryKey = index;
            }

            else // Add a primary key column to use
            {
                table.Columns.Add(tableName + "_ID", typeof(String));

                var index = new DataColumn[1];
                index[0] = table.Columns[tableName + "_ID"];
                table.PrimaryKey = index;
            }

            if (Connection.IsSQLConnection)
                Connection.CheckOrCreateTable(table);

            var dataSet = new DataSet
            {
                DataSetName = tableName,
                EnforceConstraints = true,
                CaseSensitive = false
            };

            dataSet.Tables.Add(table);

            var dataTableHandler = new DataTableHandler(dataSet)
            {
                HasRelations = relations,
                UsesPreCaching = DataObject.GetPreCachedFlag(objType),
                RequiresObjectId = primaryKeys.Count == 0,
                BindingMethod = DataObject.GetBindingMethod(objType)
            };

            TableDatasets.Add(tableName, dataTableHandler);
        }

        public String[] GetTableNameList() => TableDatasets.Select(entry => entry.Key).ToArray();

        public String Escape(String toEscape) => Connection.Escape(toEscape);

        public Boolean ExecuteNonQuery(String rawQuery) => ExecuteNonQueryImpl(rawQuery);

        public Int64 ExecuteQueryInt(String rawQuery) => ExecuteQueryIntImpl(rawQuery);

        #endregion

        #region Implementation

        // Ajoute un objet a la database , true = Success
        protected abstract Boolean AddObjectImpl(DataObject dataObject);

        // Sauvegarde un Objet dans la Database
        protected abstract void SaveObjectImpl(DataObject dataObject);

        // Supprime un objet de la database
        protected abstract void DeleteObjectImpl(DataObject dataObject);

        protected abstract Boolean RunTransaction(List<DataObject> dataObjects);

        // Trouve un objet a partir de sa primaryKey
        protected abstract TObject FindObjectByKeyImpl<TObject>(Object key) where TObject : DataObject;

        // Trouve un objet a partir de sa primaryKey
        protected abstract DataObject FindObjectByKeyImpl(Type objectType, Object key);

        // Sélectionne un objet a partir d'une table et des paramètres
        protected abstract DataObject[] SelectObjectsImpl(Type objectType, String whereClause, IsolationLevel isolation);

        // Sélectionne un objet a partir d'une table et des paramètres
        protected abstract IList<TObject> SelectObjectsImpl<TObject>(String whereClause, IsolationLevel isolation) where TObject : DataObject;

        // Sélectionne tous les objets de la database
        protected abstract IList<TObject> SelectAllObjectsImpl<TObject>(IsolationLevel isolation) where TObject : DataObject;

        protected abstract Dictionary<TKey, TObject> MapAllObjectsImpl<TKey, TObject>(String keyName, String where, Int32 expectedRowCount, IsolationLevel isolation) where TObject : DataObject;

        protected abstract Int32 GetNextAutoIncrementImpl<TObject>() where TObject : DataObject;

        // Retourn le nombre d'objet dans la database
        protected abstract Int32 GetObjectCountImpl<TObject>(String where) where TObject : DataObject;

        // Returns highest value within DB
        protected abstract Int64 GetMaxColValueImpl<TObject>(String column) where TObject : DataObject;

        protected abstract Boolean ExecuteNonQueryImpl(String raqQuery);

        protected abstract Int64 ExecuteQueryIntImpl(String raqQuery);
       
        #endregion

        #region Relations

        public void FillObjectRelations(DataObject dataObject) => FillLazyObjectRelations(dataObject, false);

        protected void SaveObjectRelations(DataObject dataObject)
        {
            try
            {
                Object val;

                Type myType = dataObject.GetType();

                MemberInfo[] myMembers = myType.GetMembers();

                for (Int32 i = 0; i < myMembers.Length; i++)
                {
                    Relation[] myAttributes = GetRelationAttributes(myMembers[i]);
                    if (myAttributes.Length > 0)
                    {
                        Boolean array = false;

                        Type type = myMembers[i] is PropertyInfo ? ((PropertyInfo)myMembers[i]).PropertyType : ((FieldInfo)myMembers[i]).FieldType;
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

                                foreach (Object o in a)
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
                Object val;

                Type myType = dataObject.GetType();

                MemberInfo[] myMembers = myType.GetMembers();

                for (Int32 i = 0; i < myMembers.Length; i++)
                {
                    Relation[] myAttributes = GetRelationAttributes(myMembers[i]);
                    if (myAttributes.Length > 0)
                    {
                        if (myAttributes[0].AutoDelete == false)
                            continue;

                        Boolean array = false;

                        Type type = myMembers[i] is PropertyInfo ? ((PropertyInfo)myMembers[i]).PropertyType : ((FieldInfo)myMembers[i]).FieldType;
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

                                foreach (Object o in a)
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

        protected void FillLazyObjectRelations(DataObject dataObject, Boolean autoload)
        {
            try
            {
                Type dataObjectType = dataObject.GetType();

                if (!MemberInfoCache.TryGetValue(dataObjectType, out MemberInfo[] myMembers))
                {
                    myMembers = dataObjectType.GetMembers();
                    MemberInfoCache[dataObjectType] = myMembers;
                }

                for (Int32 i = 0; i < myMembers.Length; i++)
                {
                    Relation[] myAttributes = GetRelationAttributes(myMembers[i]);

                    if (myAttributes.Length > 0)
                    {
                        Relation rel = myAttributes[0];

                        if ((rel.AutoLoad == false) && autoload)
                            continue;

                        Boolean isArray = false;
                        Type remoteType;
                        DataObject[] elements;

                        String local = rel.LocalField;
                        String remote = rel.RemoteField;

                        remoteType = myMembers[i] is PropertyInfo ? ((PropertyInfo)myMembers[i]).PropertyType : ((FieldInfo)myMembers[i]).FieldType;

                        if (remoteType.HasElementType)
                        {
                            remoteType = remoteType.GetElementType();
                            isArray = true;
                        }

                        PropertyInfo prop = dataObjectType.GetProperty(local);
                        FieldInfo field = dataObjectType.GetField(local);

                        Object val = 0;

                        if (prop != null)
                        {
                            val = prop.GetValue(dataObject, null);
                        }
                        if (field != null)
                        {
                            val = field.GetValue(dataObject);
                        }

                        if (val != null && val.ToString() != String.Empty)
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
                                        if (!ConstructorByFieldType.TryGetValue(currentField.FieldType, out ConstructorInfo constructor))
                                        {
                                            constructor = currentField.FieldType.GetConstructor(new[] { typeof(Int32) });
                                            ConstructorByFieldType[currentField.FieldType] = constructor;
                                        }

                                        Object elementHolder = constructor.Invoke(new Object[] { elements.Length });
                                        Object[] elementArray = (Object[])elementHolder;

                                        for (Int32 m = 0; m < elementArray.Length; m++)
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
        protected void DeleteFromCache(String tableName, DataObject obj)
        {
            DataTableHandler handler = TableDatasets[tableName];
            handler.SetCacheObject(obj.ObjectId, null);
        }

        /// <summary>
        /// Unused method.
        /// </summary>
        public Boolean UpdateInCache<TObject>(Object key) where TObject : DataObject
        {
            MemberInfo[] members = typeof(TObject).GetMembers();
            var ret = (TObject)Activator.CreateInstance(typeof(TObject));

            String tableName = ret.TableName;
            DataTableHandler dth = TableDatasets[tableName];
            String whereClause = null;

            if (!dth.UsesPreCaching || key == null)
                return false;

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

            IList<TObject> objs = SelectObjects<TObject>(whereClause);
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
        protected void ReloadCache(String tableName)
        {
            DataTableHandler handler = TableDatasets[tableName];

            ICache cache = handler.Cache;

            foreach (Object o in cache.Keys)
            {
                ReloadObject(cache[o] as DataObject);
            }
        }

        #endregion

        #region Helpers

        protected Relation[] GetRelationAttributes(MemberInfo info)
        {
            if (RelationAttributes.TryGetValue(info, out Relation[] rel))
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
            if (!_bindingInfos.TryGetValue(objectType, out BindingInfo[] bindingInfos))
            {
                var list = new List<BindingInfo>();

                MemberInfo[] objMembers = objectType.GetMembers();

                var assignObject = (DataObject)Activator.CreateInstance(objectType);

                for (Int32 i = 0; i < objMembers.Length; i++)
                {
                    Object[] keyAttrib = objMembers[i].GetCustomAttributes(typeof(PrimaryKey), true);
                    Object[] readonlyAttrib = objMembers[i].GetCustomAttributes(typeof(ReadOnly), true);
                    Object[] attrib = objMembers[i].GetCustomAttributes(typeof(DataElement), true);
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
            Type objectType = typeof(T);

            if (!_staticBindPools.TryGetValue(objectType, out ObjectPool<StaticMemberBindInfo> staticBindPool))
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

            var assignObject = (DataObject)Activator.CreateInstance(objectType);

            var databaseMembers = new List<MemberInfo>();

            for (Int32 i = 0; i < objMembers.Length; i++)
            {
                Object[] keyAttrib = objMembers[i].GetCustomAttributes(typeof(PrimaryKey), true);
                Object[] readonlyAttrib = objMembers[i].GetCustomAttributes(typeof(ReadOnly), true);
                Object[] attrib = objMembers[i].GetCustomAttributes(typeof(DataElement), true);
                Relation[] relAttrib = GetRelationAttributes(objMembers[i]);

                if (attrib.Length > 0 || keyAttrib.Length > 0 || relAttrib.Length > 0 || readonlyAttrib.Length > 0)
                    databaseMembers.Add(objMembers[i]);
            }

            return new StaticMemberBindInfo(assignObject, databaseMembers);
        }

        // Lecture de la clef primaire
        public static String GetTableOrViewName(Type objectType)
        {
            String name = DataObject.GetViewName(objectType);

            // if not a view, we use tablename, else viewname
            return String.IsNullOrEmpty(name) ? DataObject.GetTableName(objectType) : name;
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

        public static IObjectDatabase GetObjectDatabase(ConnectionType connectionType, String connectionString, String schemaName)
        {
            if (connectionType == ConnectionType.DATABASE_MYSQL)
                return new MySQLObjectDatabase(new MySqlDataConnection(connectionString, schemaName));
            else if (connectionType == ConnectionType.DATABASE_MSSQL)
                return new SQLObjectDatabase(new SqlDataConnection(connectionString, schemaName));

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

                for (Int32 i = 0; i < members.Count; ++i)
                {
                    // Used for the fast static binding method for assigning to properties.
                    var info = members[i] as PropertyInfo;
                    if (info == null)
                        continue;

                    DataBinders[i] = DataBinder.GetFor(AssignObject, info.PropertyType, info.GetSetMethod());
                }
            }
        }

        protected class BindingInfo
        {
            public readonly Boolean HasRelation;
            public readonly MemberInfo Member;
            public readonly Boolean ReadOnly;
            public DataElement DataElementAttribute;
            public MySqlExpressionDataBinder MySqlBinder;
            public Boolean PrimaryKey;

            public BindingInfo(DataObject assignObject, MemberInfo member, Boolean primaryKey, Boolean hasRelation, Boolean readOnly, DataElement attrib)
            {
                Member = member;

                // Used for the compiled expression method for assigning to properties and fields.
                MySqlBinder = (MySqlExpressionDataBinder)typeof(MySqlExpressionDataBinder).GetMethod("GetFor").MakeGenericMethod(assignObject.GetType()).Invoke(null, new Object[] { member });

                PrimaryKey = primaryKey;
                HasRelation = hasRelation;
                DataElementAttribute = attrib;
                ReadOnly = readOnly;
            }
        }

        #endregion
    }
}