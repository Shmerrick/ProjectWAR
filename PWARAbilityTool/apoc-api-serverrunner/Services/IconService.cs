using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace PWARAbilityTool
{
    public class IconService
    {
        public XmlDocument IconDocument { get; set; }
        public Dictionary<string, string> IconObjectMap { get; set; }

        /// <summary>
        /// Get an Icon. Id = ObjectId if the object is mapped in objects.csv. Otherwise Id = iconId.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="objectMapped"></param>
        /// <returns></returns>
        public Icon GetIcon(int id, bool objectMapped = true)
        {
            InitMembers();
            return findIcon(objectMapped, id);
        }

        private Icon findIcon(bool objectMapped, int id)
        {
            XmlNode selectedNode = null;

            try
            {

                // selectedNode represents the object. Now find the icon id
                if (objectMapped)
                {
                    string iconId = IconObjectMap[id.ToString()];
                    selectedNode = IconDocument.SelectSingleNode($"/Interface/Assets/Icon[@id={iconId}]");
                }
                else
                {
                    selectedNode = IconDocument.SelectSingleNode($"/Interface/Assets/Icon[@id={id}]");
                }
            }
            catch (Exception)
            {
                // Suppress exception - if the icon doesnt exist, move on.
            }

            if (selectedNode == null)
                return null;
            else
            {

                string fileName = selectedNode.Attributes["texture"].Value.Replace(".dds", ".jpg");
                string fullPathFileName =
                    System.Configuration.ConfigurationManager.AppSettings["icon-file-path"] + fileName;

                // Sometimes the icon doesnt exist.

                if (System.IO.File.Exists(fullPathFileName))
                {
                    Byte[] bytes = System.IO.File.ReadAllBytes(fullPathFileName);
                    Icon icon = new Icon
                    {
                        Id = id,
                        Name = selectedNode.Attributes["name"].InnerText,
                        TextureFileName = fileName,
                        TextByteArray = bytes
                    };

                    return icon;
                }
                else
                {
                    return new Icon
                    {
                        Id = id,
                        Name = selectedNode.Attributes["name"].InnerText,
                        TextureFileName = "",
                        TextByteArray = null
                    };
                }
            }
        }

        private void InitMembers()
        {
            if (IconDocument == null)
            {
                IconDocument = new XmlDocument();
                IconObjectMap = new Dictionary<string, string>();
            }

            if (IconDocument.ChildNodes.Count == 0)
            {
                IconDocument.Load(System.Configuration.ConfigurationManager.AppSettings["icon-file-index"]);
            }

            if (IconObjectMap.Count() == 0)
            {
                string[] iconObjectLines =
                    File.ReadAllLines(System.Configuration.ConfigurationManager.AppSettings["object-file-index"]);

                foreach (string iconObjectLine in iconObjectLines)
                {
                    //9348,mnt_o_dumpyhorse_01,,20225
                    IconObjectMap.Add(iconObjectLine.Split(',')[0], iconObjectLine.Split(',')[3]);
                }
            }
        }

        public Dictionary<string, Icon> loadIcons()
        {//TODO: IconConverter, maybe load only abs-icons?? But is still a lot of stuff.
            InitMembers();
            Dictionary<string, Icon> icons = new Dictionary<string, Icon>();
            //XmlNodeList iconNodes = IconDocument.SelectNodes("/Interface/Assets/Icon");
            //for (int i = 0; i < iconNodes.Count; i++)
            //{
            //    string outerXml = iconNodes.Item(i).OuterXml;
            //    string id = outerXml.Substring(6, 10).Substring(4, 5);
            //    int.TryParse(id, out int iId);
            //    Icon icon = findIcon(false, iId);
            //    if (icon != null)
            //        icons.Add(id, icon);
            //}
            return icons;
        }
    }
}
