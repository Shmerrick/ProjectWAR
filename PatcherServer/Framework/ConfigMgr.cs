using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace PatcherServer.Framework
{
    public static class ConfigMgr
    {
        public static List<aConfig> _Configs = new List<aConfig>();

        public static T LoadConfig<T>() where T : aConfig
        {

            T Obj = default(T);
            var type = typeof(T);

            // Pick up a class
            if (type.IsClass != true)
                return default(T);

            object[] attrib = type.GetCustomAttributes(typeof(aConfigAttributes), true);
            if (attrib.Length <= 0)
                return default(T);

            aConfigAttributes[] ConfigAttribs = (aConfigAttributes[])type.GetCustomAttributes(typeof(aConfigAttributes), true);

            if (ConfigAttribs.Length > 0)
            {

                XmlSerializer Xml = new XmlSerializer(type);

                try
                {
                    FileInfo FInfo = new FileInfo(ConfigAttribs[0].FileName);
                    Directory.CreateDirectory(FInfo.DirectoryName);
                }
                catch (Exception)
                {
                }


                FileStream fs = new FileStream(ConfigAttribs[0].FileName, FileMode.OpenOrCreate);
                bool FirstLoad = false;

                if (fs.Length <= 0)
                {
                    FirstLoad = true;
                    Obj = Activator.CreateInstance(type) as T;
                }
                else
                {
                    Obj = Xml.Deserialize(fs) as T;
                    if (!Obj.IConfiguredTheFile)
                    {
                        Console.WriteLine(ConfigAttribs[0].FileName + " : IConfiguredTheFile value is false.");
                    }
                }

                fs.SetLength(0);
                Xml.Serialize(fs, Obj);
                fs.Close();

                if (FirstLoad || !Obj.IConfiguredTheFile)
                    return default(T);
            }

            return Obj;
        }

    }
}
