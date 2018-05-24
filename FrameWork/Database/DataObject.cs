using System;
using System.ComponentModel;
using System.Xml.Serialization;
using MySql.Data.MySqlClient;

namespace FrameWork
{
    /// <summary>
    /// The base class of all objects responsible for holding and receiving data involved in database queries and transactions.
    /// </summary>
    [Serializable]
    public abstract class DataObject
    {
        bool m_allowAdd = true;
        bool m_allowDelete = true;

        // Génération d'un objet unique pour chaque DataObject
        protected DataObject()
        {
            IsValid = false;
            AllowAdd = true;
            AllowDelete = true;
            IsDeleted = false;
        }

        // Nom de la table dont l'objet provient
        [Browsable(false)]
        public virtual string TableName
        {
            get
            {
                Type myType = GetType();
                return GetTableName(myType);
            }
        }

        // Chargement en cache ou non de l'objet
        [Browsable(false)]
        public virtual bool UsesPreCaching
        {
            get
            {
                Type myType = GetType();
                return GetPreCachedFlag(myType);
            }
        }

        // Objet Valide ?
        [XmlIgnore()]
        [Browsable(false)]
        public bool IsValid { get; set; }

        // Peut être ou non ajouté a la DB
        [XmlIgnore()]
        [Browsable(false)]
        public virtual bool AllowAdd
        {
            get { return m_allowAdd; }
            set { m_allowAdd = value; }
        }

        // Peut être ou non supprimé de la DB
        [XmlIgnore()]
        [Browsable(false)]
        public virtual bool AllowDelete
        {
            get { return m_allowDelete; }
            set { m_allowDelete = value; }
        }

        // Numéro de l'objet dans la table
        [XmlIgnore()]
        [Browsable(false)]
        public string ObjectId { get; set; }

        // Objet différent ke celui de la table ?
        [XmlIgnore()]
        [Browsable(false)]
        public virtual bool Dirty { get; set; }

        // Cette objet a été delete de la table ?
        [XmlIgnore()]
        [Browsable(false)]
        public virtual bool IsDeleted { get; set; }

        [XmlIgnore()]
        [Browsable(false)]
        public DatabaseOp pendingOp { get { return _pendingOperation; } set { _pendingOperation = value; } }

        private DatabaseOp _pendingOperation;

        public DataObject Clone()
        {
            DataObject obj = (DataObject)MemberwiseClone();
            return obj;
        }

        // Récupère la table name en lisant les attributs
        public static string GetTableName(Type myType)
        {
            object[] attri = myType.GetCustomAttributes(typeof(DataTable), true);

            if ((attri.Length >= 1) && (attri[0] is DataTable))
            {
                var tab = (DataTable) attri[0];
                string name = tab.TableName;
                if (name != null)
                    return name;
            }

            return myType.Name;
        }

        public static string GetViewName(Type myType)
        {
            object[] attri = myType.GetCustomAttributes(typeof(DataTable), true);

            if ((attri.Length >= 1) && (attri[0] is DataTable))
            {
                var tab = (DataTable) attri[0];
                string name = tab.ViewName;
                if (name != null)
                    return name;
            }

            return null;
        }

        // Précache au démarrage ?
        public static bool GetPreCachedFlag(Type myType)
        {
            object[] attri = myType.GetCustomAttributes(typeof(DataTable), true);
            if ((attri.Length >= 1) && (attri[0] is DataTable))
            {
                var tab = (DataTable) attri[0];
                return tab.PreCache;
            }

            return false;
        }

        // Récupère la table name en lisant les attributs
        public static EBindingMethod GetBindingMethod(Type myType)
        {
            object[] attri = myType.GetCustomAttributes(typeof(DataTable), true);

            if ((attri.Length >= 1) && (attri[0] is DataTable))
            {
                var tab = (DataTable)attri[0];
                EBindingMethod bindMethod = tab.BindMethod;
                return bindMethod;
            }

            return EBindingMethod.CompiledExpression;
        }

        public void UpdateDBStatus()
        {
            switch (pendingOp)
            {
                case DatabaseOp.DOO_Insert:
                    IsValid = true;
                    IsDeleted = false;
                    break;
                case DatabaseOp.DOO_Update:
                    IsValid = true;
                    break;
                case DatabaseOp.DOO_Delete:
                    IsValid = false;
                    IsDeleted = true;
                    break;
            }

            pendingOp = DatabaseOp.DOO_None;
        }

        public virtual void Load(MySqlDataReader reader, int field)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// The type of loading that should be performed when data related to this class is read from the database.
    /// </summary>
    public enum EBindingMethod
    {
        /// <summary>
        /// Uses compiled expressions to bind to properties. Faster than reflection.
        /// </summary>
        CompiledExpression,
        /// <summary>
        /// <para>Uses cached accessors which assign to a constant object, which is then cloned.</para>
        /// <para>Fast, but not thread safe and requires additional logic for classes which create instance members during their load phase.</para>
        /// </summary>
        StaticBound,
        /// <summary>
        /// Uses PropertyInfo.SetValue. The slowest, original method.
        /// </summary>
        Reflected,
        /// <summary>
        /// Causes the loader to invoke the constructor with the DataReader supplied as a parameter. Fast and thread safe but requires explicit binding code.
        /// </summary>
        Manual
    }
}