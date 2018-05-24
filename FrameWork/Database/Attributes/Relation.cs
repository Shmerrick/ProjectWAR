using System;

namespace FrameWork
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class Relation : Attribute
    {
        public Relation()
        {
            LocalField = null;
            RemoteField = null;
            AutoLoad = true;
            AutoDelete = false;
        }
        
        // Relation entre plusieurs champs de table différente
        public string LocalField { get; set; }

        // Supprime la relation
        public string RemoteField { get; set; }

        // Chargement automatique de la table
        public bool AutoLoad { get; set; }

        // Suppression de l'objet automatiquement lorsque l'objet est supprimé dans le core
        public bool AutoDelete { get; set; }
    }
}
