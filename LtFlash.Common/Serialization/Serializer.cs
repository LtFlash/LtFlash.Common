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
            var doc = new XmlDocument();
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
            var doc = new XmlDocument();
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
                //TODO: use LoadFromXML
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

        public static Dictionary<TEnum, ControlSet>
            DeserializeControls<TEnum>(string path)
        {
            var dic = new Dictionary<TEnum, ControlSet>();

            var ctrls = LoadFromXML<ControlsSerializeAdapter<TEnum>>(path);

            for (int i = 0; i < ctrls.Count; i++)
            {
                dic.Add(
                    ctrls[i].Action,
                    new ControlSet(ctrls[i].Key, ctrls[i].Modifier, ctrls[i].ControllerBtn));
            }

            return dic;
        }

        public static void SerializeControls<TEnum>(Dictionary<TEnum, ControlSet> dic, string path)
        {
            var list = new List<ControlsSerializeAdapter<TEnum>>();

            foreach (KeyValuePair<TEnum, ControlSet> item in dic)
            {
                list.Add(new ControlsSerializeAdapter<TEnum>(
                    item.Key, 
                    item.Value.Key, item.Value.Modifier, item.Value.ControllerBtn));
            }

            SaveToXML(list, path);
        }

        public static List<T> LoadFromXML<T>(string path)
        {
            List<T> list = new List<T>();

            var deserializer = new XmlSerializer(typeof(List<T>));
            using (TextReader reader = new StreamReader(path))
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

            string path = sPath + FileName + ".xml";

            if (File.Exists(path))
            {
                List<T> listOfSpawns = LoadFromXML<T>(path);
                listOfSpawns.Add(ObjectToAdd);

                SaveToXML<T>(listOfSpawns, path);
            }
            else
            {
                List<T> list = new List<T>();
                list.Add(ObjectToAdd);

                SaveToXML<T>(list, path);
            }
        }

        private static void ValidatePath(string path)
        {
            //TODO: implement
            // - 1. check dir existance
            // - 2. check extension
        }
    }
}
