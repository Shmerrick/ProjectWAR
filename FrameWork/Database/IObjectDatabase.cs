using System;
using System.Collections.Generic;
using System.Text;

namespace FrameWork
{
    public interface IObjectDatabase
    {
        bool AddObject(DataObject dataObject);
        void ForceSave();
        void SaveObject(DataObject dataObject);
        long GetMaxColValue<TObject>(string column) where TObject : DataObject;

        string GetSchemaName();
        void RegisterAction(Action action);

        void DeleteObject(DataObject dataObject);

        TObject FindObjectByKey<TObject>(object key)
            where TObject : DataObject;

        TObject SelectObject<TObject>(string whereExpression)
            where TObject : DataObject;

        string SqlCommand_CharLength();

        TObject SelectObject<TObject>(string whereExpression, IsolationLevel isolation)
            where TObject : DataObject;

        IList<TObject> SelectObjects<TObject>(string whereExpression)
            where TObject : DataObject;

        IList<TObject> SelectObjects<TObject>(string whereExpression, IsolationLevel isolation)
            where TObject : DataObject;

        IList<TObject> SelectAllObjects<TObject>()
            where TObject : DataObject;

        IList<TObject> SelectAllObjects<TObject>(IsolationLevel isolation)
            where TObject : DataObject;

        /// <summary>
        /// Selects all objects and map them in a dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of keys of dictionnary</typeparam>
        /// <typeparam name="TObject">Type of mapped entities</typeparam>
        /// <param name="keyName">Name of the key column (typically the primary key)</param>
        /// <returns>Dictionary of entities indexed by key valur, never null</returns>
        Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName)
            where TObject : DataObject;

        /// <summary>
        /// Selects all objects and map them in a dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of keys of dictionnary</typeparam>
        /// <typeparam name="TObject">Type of mapped entities</typeparam>
        /// <param name="keyName">Name of the key column (typically the primary key)</param>
        /// <param name="isolation">Transaction isolation level</param>
        /// <returns>Dictionary of entities indexed by key valur, never null</returns>
        Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName, IsolationLevel isolation)
            where TObject : DataObject;

        /// <summary>
        /// Selects all objects and map them in a dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of keys of dictionnary</typeparam>
        /// <typeparam name="TObject">Type of mapped entities</typeparam>
        /// <param name="keyName">Name of the key column (typically the primary key)</param>
        /// <param name="whereClause">Where clause, ignored if null</param>
        /// <returns>Dictionary of entities indexed by key valur, never null</returns>
        Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName, string whereClause)
            where TObject : DataObject;

        /// <summary>
        /// Selects all objects and map them in a dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of keys of dictionnary</typeparam>
        /// <typeparam name="TObject">Type of mapped entities</typeparam>
        /// <param name="keyName">Name of the key column (typically the primary key)</param>
        /// <param name="whereClause">Where clause, ignored if null</param>
        /// <param name="isolation">Transaction isolation level</param>
        /// <returns>Dictionary of entities indexed by key valur, never null</returns>
        Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName, string whereClause, IsolationLevel isolation)
            where TObject : DataObject;

        /// <summary>
        /// Selects all objects and map them in a dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of keys of dictionnary</typeparam>
        /// <typeparam name="TObject">Type of mapped entities</typeparam>
        /// <param name="keyName">Name of the key column (typically the primary key)</param>
        /// <param name="expectedRowCount">Excepted returned values count</param>
        /// <returns>Dictionary of entities indexed by key valur, never null</returns>
        Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName, int expectedRowCount)
            where TObject : DataObject;

        /// <summary>
        /// Selects all objects and map them in a dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of keys of dictionnary</typeparam>
        /// <typeparam name="TObject">Type of mapped entities</typeparam>
        /// <param name="keyName">Name of the key column (typically the primary key)</param>
        /// <param name="expectedRowCount">Excepted returned values count</param>
        /// <param name="isolation">Transaction isolation level</param>
        /// <returns>Dictionary of entities indexed by key valur, never null</returns>
        Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName, int expectedRowCount, IsolationLevel isolation)
            where TObject : DataObject;

        /// <summary>
        /// Selects all objects and map them in a dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of keys of dictionnary</typeparam>
        /// <typeparam name="TObject">Type of mapped entities</typeparam>
        /// <param name="keyName">Name of the key column (typically the primary key)</param>
        /// <param name="expectedRowCount">Excepted returned values count</param>
        /// <returns>Dictionary of entities indexed by key valur, never null</returns>
        Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName, string whereClause, int expectedRowCount)
            where TObject : DataObject;

        /// <summary>
        /// Selects all objects and map them in a dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of keys of dictionnary</typeparam>
        /// <typeparam name="TObject">Type of mapped entities</typeparam>
        /// <param name="keyName">Name of the key column (typically the primary key)</param>
        /// <param name="expectedRowCount">Excepted returned values count</param>
        /// <param name="isolation">Transaction isolation level</param>
        /// <returns>Dictionary of entities indexed by key valur, never null</returns>
        Dictionary<TKey, TObject> MapAllObjects<TKey, TObject>(string keyName, string whereClause, int expectedRowCount, IsolationLevel isolation)
            where TObject : DataObject;


        int GetNextAutoIncrement<TObject>()
            where TObject : DataObject;

        int GetObjectCount<TObject>()
            where TObject : DataObject;

        int GetObjectCount<TObject>(string whereExpression)
            where TObject : DataObject;

        void RegisterDataObject(Type dataObjectType);

        bool UpdateInCache<TObject>(object key)
            where TObject : DataObject;

        void FillObjectRelations(DataObject dataObject);

        string Escape(string rawInput);

        bool ExecuteNonQuery(string rawQuery);

        /// <summary>
        /// Returns integer value from first row, first column. Used for select count(*) queries
        /// </summary>
        /// <param name="rawQuery">Select query to execute</param>
        /// <returns></returns>
        long ExecuteQueryInt(string rawQuery);
    }
}
