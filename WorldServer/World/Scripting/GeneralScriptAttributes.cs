using System;

namespace WorldServer.World.Scripting
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GeneralScriptAttribute : Attribute
    {
        public bool GlobalScript; // True if the script is general to all objects or false if the script me be generated for each object
        public string ScriptName;
        public uint CreatureEntry;
        public uint GameObjectEntry;

        public GeneralScriptAttribute(bool GlobalScript, string ScriptName, uint CreatureEntry = 0, uint GameObjectEntry = 0)
        {
            this.GlobalScript = GlobalScript;
            this.ScriptName = ScriptName;
            this.CreatureEntry = CreatureEntry;
            this.GameObjectEntry = GameObjectEntry;
        }
    }
}
