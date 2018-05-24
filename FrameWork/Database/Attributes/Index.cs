using System;
using System.Collections.Generic;
using System.Linq;

namespace FrameWork
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Index : Attribute
    {
        public Index(string name, bool unique, params string[] columns)
        {
            Name = name;
            Unique = unique;
            if(columns != null)
                Columns = columns.ToList();
        }

        public Index(params string[] info)
        {
        }


        // Défini le nom de la table a charger
        public string Name { get; set; }

        // Indique si la var est unique
        public bool Unique { get; set; }

        public List<string> Columns { get; set; }
    }

}