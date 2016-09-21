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
            XmlNode n = SelectNodeFromXml(file, node);

            if (n == null)
                throw new KeyNotFoundException($"{nameof(SaveToNode)}: Specified node does not exists!");

            n.InnerText = value;
            var doc = new XmlDocument();
            doc.Save(file);
        }

        public static string ReadFromNode(string file, string node)
        {   
            return SelectNodeFromXml(file, node).InnerText;
        }

        private static XmlNode SelectNodeFromXml(string file, string node)
        {
            var doc = new XmlDocument();
            using (TextReader reader = new StreamReader(file))
            {
                doc.Load(reader);
            }
            return doc.SelectSingleNode(node);
        }

        public static List<T> LoadAllXML<T>(string path)
        {
            return LoadAllXML<T>(path, SearchOption.AllDirectories);            
        }

        public static List<T> LoadAllXML<T>(string path, SearchOption searchOption)
        {
            List<T> result = new List<T>();

            string[] files = Directory.GetFiles(path, "*.xml", searchOption);

            for (int i = 0; i < files.Length; i++)
            {
                result.AddRange(LoadFromXML<T>(files[i]));
            }

            return result;
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

        public static void AppendToXML<T>(T objectToAdd, string path)
        {
            List<T> list = new List<T>();

            if (ValidatePath(path)) list = LoadFromXML<T>(path);

            list.Add(objectToAdd);

            SaveToXML<T>(list, path);
        }
        /// <summary>
        /// Creates folder in case it doesn't exist and checks file existance.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>false when a file does not exist.</returns>
        private static bool ValidatePath(string path)
        {
            //TODO: implement
            // - check extension
            // - bool param: createDir
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                return false;
            }

            return File.Exists(path);
        }
    }
}
