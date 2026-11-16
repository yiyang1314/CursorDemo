using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace AyuUI.Controll.Helpers
{
    public static class XmlHelper
    {
        /// <summary>
        /// 对象序列化为 XML 字符串
        /// </summary>
        public static string Serialize<T>(T obj, bool omitXmlDeclaration = false)
        {
            if (obj == null) return string.Empty;

            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = omitXmlDeclaration,
                Encoding = Encoding.UTF8
            };

            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty); // 移除默认的 xmlns 命名空间

            using var sw = new StringWriter();
            using var xw = XmlWriter.Create(sw, settings);
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(xw, obj, ns);
            return sw.ToString();
        }

        /// <summary>
        /// XML 字符串反序列化为对象
        /// </summary>
        public static T? Deserialize<T>(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) return default;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var sr = new StringReader(xml);
                return (T?)serializer.Deserialize(sr);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 将对象保存为 XML 文件
        /// </summary>
        public static void SaveToFile<T>(T obj, string filePath)
        {
            var xml = Serialize(obj);
            File.WriteAllText(filePath, xml, Encoding.UTF8);
        }

        /// <summary>
        /// 从 XML 文件读取并反序列化为对象
        /// </summary>
        public static T? LoadFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath)) return default;
            var xml = File.ReadAllText(filePath, Encoding.UTF8);
            return Deserialize<T>(xml);
        }
    }
}
