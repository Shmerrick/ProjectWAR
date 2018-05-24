using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace FrameWork
{
    public class LogInfo
    {
        public bool Info=true;
        public bool Successs=true;
        public bool Notice=true;
        public bool Error=true;

        public bool Debug;
        public bool Tcp;
        public bool Dump;
    }


    public class LogConfig
    {
        public LogInfo Info = new LogInfo();
        public int _level=1;

        public string PreFileName = "";
        public string FileName = "Log.txt";
        public string LogFolder = "/Logs";

        public int Level
        {
            get { return _level; }
            set 
            {
                _level = (value < 1 ? 1 : ( value > 4 ? 4 : value) );

                switch (_level)
                {
                    case 4:
                        Info.Tcp = true;
                        goto case 3;
                    case 3:
                        Info.Debug = true;
                        Info.Dump = true;
                        goto case 2;
                    case 2:
                        Info.Notice = true;
                        Info.Error = true;
                        goto case 1;
                    case 1:
                        Info.Info = true;
                        Info.Successs = true;
                        break;
                    default:
                        Info.Info = true;
                        Info.Successs = true;
                        break;
                }
            }
        }

        private FileInfo _file;

        public LogConfig()
        {
            Level = _level;
        }

        public LogConfig(int level)
        {
            Level = level;
        }

        public LogConfig(LogInfo Info)
        {
            this.Info = Info;
        }

        public bool LoadInfoFromFile(string filename)
        {
            _file = new FileInfo(filename);
            if (_file == null)
                return false;

            try
            {
                FileInfo FInfo = new FileInfo(filename);
                Directory.CreateDirectory(FInfo.DirectoryName);

                XmlSerializer S = new XmlSerializer(typeof(LogInfo));
                FileStream Stream = new FileStream(filename, FileMode.OpenOrCreate);

                if (Stream.Length <= 0)
                {
                    S.Serialize(Stream, Info);
                }
                else
                    Info = S.Deserialize(Stream) as LogInfo;

            }
            catch (Exception e)
            {
                Console.WriteLine("Log : Error at loading {0}", e);
                return false;
            }

            return true;
        }


    }
}
