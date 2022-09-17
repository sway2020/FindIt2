// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ColossalFramework.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace FindIt
{
    public class CustomTagsLibrary
    {
        public const string filename = "FindItCustomTags.xml";

        public static Dictionary<string, string> assetTags = new Dictionary<string, string>();

        public struct TagEntry
        {
            [XmlAttribute]
            public string key;
            [XmlAttribute]
            public string value;
        }

        public static void Serialize()
        {
            try
            {
                string path = Path.Combine(DataLocation.localApplicationData, filename);

                if (assetTags.Count == 0)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    return;
                }

                TagEntry[] tagsEntries = new TagEntry[assetTags.Count];

                int i = 0;
                foreach (string key in assetTags.Keys)
                {
                    tagsEntries[i].key = key;
                    tagsEntries[i].value = assetTags[key];
                    i++;
                }

                using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    stream.SetLength(0); // Emptying the file !!!
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(TagEntry[]));
                    xmlSerializer.Serialize(stream, tagsEntries);
                }
            }
            catch (Exception e)
            {
                Debugging.Message("Couldn't serialize custom tags");
                Debugging.LogException(e);
            }
        }

        public static void Deserialize()
        {
            try
            {
                assetTags.Clear();

                string path = Path.Combine(DataLocation.localApplicationData, filename);

                if (!File.Exists(path)) return;

                TagEntry[] tagsEntries;

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(TagEntry[]));
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    tagsEntries = (TagEntry[])xmlSerializer.Deserialize(stream);
                }

                for (int i = 0; i < tagsEntries.Length; i++)
                {
                    // Debugging.Message(tagsEntries[i].key + " " + tagsEntries[i].value);
                    assetTags[tagsEntries[i].key] = tagsEntries[i].value;
                }
            }
            catch (Exception e)
            {
                Debugging.Message("Couldn't serialize custom tags");
                Debugging.LogException(e);
            }
        }
    }
}
