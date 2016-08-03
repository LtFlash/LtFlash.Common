using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using LtFlash.Common.InputHandling;

namespace LtFlash.Common.Serialization
{
    public static class Serializer
    {
        public static void SaveToNode(string file, string node, string value)
        {
            XmlDocument doc = new XmlDocument();
            using (TextReader reader = new StreamReader(file))
            {
                doc.Load(reader);
            }

            XmlNode n = doc.SelectSingleNode(node);

            if (n != null)
            {
                n.InnerText = value;

                doc.Save(file);
            }
        }

        public static string ReadFromNode(string file, string node)
        {
            XmlDocument doc = new XmlDocument();
            using (TextReader reader = new StreamReader(file))
            {
                doc.Load(reader);
            }
            XmlNode n = doc.SelectSingleNode(node);
            return n.InnerText;
        }

        public static void LoadAllXML<T>(ref List<T> listOfItems, string path)
        {
            LoadAllXML(ref listOfItems, path, SearchOption.AllDirectories);            
        }

        public static void LoadAllXML<T>(
            ref List<T> listOfItems, string path, SearchOption searchOption)
        {
            List<T> storage = new List<T>();
            List<T> temp = new List<T>();

            string[] files = Directory.GetFiles(path, "*.xml", searchOption);

            for (int i = 0; i < files.Length; i++)
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(List<T>));
                using (TextReader reader = new StreamReader(files[i]))
                {
                    temp = new List<T>();
                    temp = (List<T>)deserializer.Deserialize(reader);
                }

                storage.AddRange(temp);
            }

            listOfItems = storage;
        }

        public static void SaveToXML<T>(List<T> list, string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
            using (TextWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, list);
            }
        }

        public static void SaveItemToXML<T>(T item, string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (TextWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, item);
            }
        }

        public static T LoadItemFromXML<T>(string sFullPath)
        {
            T item;

            XmlSerializer deserializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StreamReader(sFullPath))
            {
                item = (T)deserializer.Deserialize(reader);
            }

            return item;
        }

        public static Dictionary<EAction, ControlSet>
            DeserializeControls(string path)
        {
            var dic = new Dictionary<EAction,ControlSet>();

            var ctrls = LoadFromXML<ControlsSerializeAdapter>(path);

            for (int i = 0; i < ctrls.Count; i++)
            {
                dic.Add(
                    ctrls[i].Action, 
                    new ControlSet(ctrls[i].Key, ctrls[i].Modifier, ctrls[i].ControllerBtn));
            }
            
            return dic;
        }

        public static Dictionary<T, ControlSet>
            DeserializeControls<T>(string path)
        {
            var dic = new Dictionary<T, ControlSet>();

            var ctrls = LoadFromXML<ControlsSerializeAdapter<T>>(path);

            for (int i = 0; i < ctrls.Count; i++)
            {
                dic.Add(
                    ctrls[i].Action,
                    new ControlSet(ctrls[i].Key, ctrls[i].Modifier, ctrls[i].ControllerBtn));
            }

            return dic;
        }

        public static void SerializeControls(Dictionary<EAction, ControlSet> dic, string path)
        {
            var list = new List<ControlsSerializeAdapter>();

            foreach (KeyValuePair<EAction, ControlSet> item in dic)
            {
                list.Add(new ControlsSerializeAdapter(item.Key, 
                    item.Value.Key, item.Value.Modifier, item.Value.ControllerBtn));
            }

            SaveToXML(list, path);
        }

        public static void SerializeControls<T>(Dictionary<T, ControlSet> dic, string path)
        {
            var list = new List<ControlsSerializeAdapter<T>>();

            foreach (KeyValuePair<T, ControlSet> item in dic)
            {
                list.Add(new ControlsSerializeAdapter<T>(item.Key, 
                    item.Value.Key, item.Value.Modifier, item.Value.ControllerBtn));
            }

            SaveToXML(list, path);
        }

        //TODO: add Combine/other filepath validation
        public static List<T> LoadFromXML<T>(string sPath, string FileName)
        {
            var list = new List<T>();

            var deserializer = new XmlSerializer(typeof(List<T>));

            using (TextReader reader = new StreamReader(sPath + FileName + ".xml"))
            {
                list = (List<T>)deserializer.Deserialize(reader);
            }

            return list;
        }

        public static List<T> LoadFromXML<T>(string sFullPath)
        {
            List<T> list = new List<T>();

            var deserializer = new XmlSerializer(typeof(List<T>));
            using (TextReader reader = new StreamReader(sFullPath))
            {
                list = (List<T>)deserializer.Deserialize(reader);
            }

            return list;
        }
        //TODO: validate xml and Path.Combine
        public static void AppendToXML<T>(T ObjectToAdd, string sPath, string FileName)
        {
            if (!Directory.Exists(sPath))
            {
                Directory.CreateDirectory(sPath);
            }

            if (File.Exists(sPath + FileName + ".xml"))
            {
                List<T> listOfSpawns = LoadFromXML<T>(sPath, FileName);
                listOfSpawns.Add(ObjectToAdd);

                SaveToXML<T>(listOfSpawns, sPath + FileName + ".xml");
            }
            else
            {
                List<T> list = new List<T>();
                list.Add(ObjectToAdd);

                SaveToXML<T>(list, sPath + FileName + ".xml");
            }
        }
    }
}
