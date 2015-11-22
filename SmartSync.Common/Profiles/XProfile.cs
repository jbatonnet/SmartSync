using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SmartSync.Common
{
    public class XProfile : Profile
    {
        public override DiffType DiffType
        {
            get
            {
                return diffType;
            }
        }
        public override SyncType SyncType
        {
            get
            {
                return syncType;
            }
        }

        public override IEnumerable<string> Exclusions
        {
            get
            {
                return exclusions;
            }
        }

        public override Storage Left
        {
            get
            {
                return left;
            }
        }
        public override Storage Right
        {
            get
            {
                return right;
            }
        }

        private DiffType diffType;
        private SyncType syncType;
        private string[] exclusions;
        private Storage left;
        private Storage right;

        public static XProfile Load(XDocument document)
        {
            XProfile profile = new XProfile();

            XElement propertiesElement = document.Root.Element("Properties");
            XElement diffTypeElement = propertiesElement?.Element("DiffType");
            XElement syncTypeElement = propertiesElement?.Element("SyncType");
            XElement exclusionsElement = propertiesElement?.Element("Exclusions");
            XElement referencesElement = propertiesElement?.Element("References");

            // Load references
            XElement[] referenceElements = referencesElement?.Elements()?.ToArray();
            if (referenceElements != null)
            {
                foreach (XElement referenceElement in referenceElements)
                {
                    switch (referenceElement.Name.LocalName)
                    {
                        case "Assembly":
                            string file = referenceElement.Attribute("Path").Value;
                            string path = file;

                            if (!Path.IsPathRooted(path))
                                path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path);

                            try
                            {
                                Assembly assembly = Assembly.LoadFile(path);
                            }
                            catch
                            {
                                throw new Exception("Could not find referenced assembly " + file);
                            }
                            break;
                    }
                }
            }

            // Decode properties
            DiffType diffType;
            if (diffTypeElement != null)
            {
                if (!Enum.TryParse(diffTypeElement.Value, out diffType))
                    throw new FormatException("Could not parse the specified diff type");

                profile.diffType = diffType;
            }

            SyncType syncType;
            if (syncTypeElement != null)
            {
                if (!Enum.TryParse(syncTypeElement.Value, out syncType))
                    throw new FormatException("Could not parse the specified sync type");

                profile.syncType = syncType;
            }

            List<string> exclusions = new List<string>();
            if (exclusionsElement != null)
            {
                foreach (XElement exclusionElement in exclusionsElement.Elements("Exclusion"))
                    exclusions.Add(exclusionElement.Value);
            }
            profile.exclusions = exclusions.ToArray();

            // TODO: Handle assembly references

            // Decode storages
            XElement leftElement = document.Root.Element("Left");
            if (leftElement == null)
                throw new Exception("Sync profiles must define left storage");
            profile.left = ReadStorage(leftElement);

            XElement rightElement = document.Root.Element("Right");
            if (rightElement == null)
                throw new Exception("Sync profiles must define right storage");
            profile.right = ReadStorage(rightElement);

            return profile;
        }

        private static Storage ReadStorage(XElement storageElement)
        {
            XAttribute typeAttribute = storageElement.Attribute("Type");
            if (typeAttribute == null)
                throw new Exception("A storage definition must indicate its type at line " + (storageElement as IXmlLineInfo).LineNumber);

            // Find storage type
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                                               .Select(a => a.GetType(typeAttribute.Value, false))
                                               .FirstOrDefault(t => t != null);
            if (type == null)
                throw new Exception("The specified storage type " + typeAttribute.Value + " could not be found at line " + (storageElement as IXmlLineInfo).LineNumber);

            // Instantiate the storage
            Storage storage = Activator.CreateInstance(type) as Storage;

            // Iterate through each storage property
            foreach (XElement propertyElement in storageElement.Elements())
            {
                string name = propertyElement.Name.LocalName;
                PropertyInfo property = type.GetProperty(name);
                object value;

                if (property.PropertyType == typeof(Storage) || property.PropertyType.IsSubclassOf(typeof(Storage)))
                    value = ReadStorage(propertyElement);
                else if (property.PropertyType == typeof(object) || property.PropertyType == typeof(string))
                    value = propertyElement.Value;
                else
                {
                    try
                    {
                        TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);

                        if (converter.IsValid(propertyElement.Value))
                            value = converter.ConvertFromString(propertyElement.Value);
                        else
                            value = Activator.CreateInstance(property.PropertyType, propertyElement.Value);
                    }
                    catch
                    {
                        throw new Exception("Could not convert the specified object into the property " + name + " at line " + (propertyElement as IXmlLineInfo).LineNumber);
                    }
                }

                property.SetValue(storage, value);
            }

            return storage;
        }
    }
}