using System;

namespace FrameWork
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataTable : Attribute
    {
        public DataTable()
        {
            TableName = null;
            PreCache = false;
        }

        // Défini le nom de la table a charger
        public string TableName { get; set; }

        // Pour l'affichage de la table
        public string ViewName { get; set; }

        // Nom de la DB a utiliser
        public string DatabaseName { get; set; }

        // True si la table doit être préchargée pour optimiser les performances
        public bool PreCache { get; set; }

        // Method of loading items from this table
        public EBindingMethod BindMethod { get; set; } = EBindingMethod.CompiledExpression;
    }
}
