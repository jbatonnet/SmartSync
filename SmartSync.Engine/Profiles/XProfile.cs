using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SmartSync.Engine
{
    public class XProfile : Profile
    {
        public static XProfile Load(XDocument document)
        {
            XProfile profile = new XProfile();

            // Decode properties
            XElement propertiesElement = document.Root.Element("Properties");
            XElement diffTypeElement = propertiesElement.Element("DiffType");
            XElement syncTypeElement = propertiesElement.Element("SyncType");
            XElement exclusionsElement = propertiesElement.Element("Exclusions");

            DiffType diffType;
            if (diffTypeElement != null && Enum.TryParse(diffTypeElement.Value, out diffType))
                profile.DiffType = diffType;

            SyncType syncType;
            if (syncTypeElement != null && Enum.TryParse(syncTypeElement.Value, out syncType))
                profile.SyncType = syncType;

            List<string> exclusions = new List<string>();
            if (exclusionsElement != null)
            {
                foreach (XElement exclusionElement in exclusionsElement.Elements("Exclusion"))
                    exclusions.Add(exclusionElement.Value);
            }

            // TODO: Handle assembly references

            // Decode storages
            XElement leftElement = document.Root.Element("Left");
            if (leftElement == null)
                throw new Exception("Sync profiles must define left storage");
            profile.Left = ReadStorage(leftElement);

            XElement rightElement = document.Root.Element("Right");
            if (rightElement == null)
                throw new Exception("Sync profiles must define right storage");
            profile.Right = ReadStorage(rightElement);

            return profile;
        }

        private static Storage ReadStorage(XElement storageElement)
        {
            XAttribute typeAttribute = storageElement.Attribute("Type");


        }
    }
}