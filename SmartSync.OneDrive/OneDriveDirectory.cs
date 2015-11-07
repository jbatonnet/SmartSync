using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartSync.Common;

using Microsoft.OneDrive.Sdk;

namespace SmartSync.OneDrive
{
    public class OneDriveDirectory : Directory
    {
        public override string Name
        {
            get
            {
                return directory.Name;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public override Directory Parent
        {
            get
            {
                return parent;
            }
        }
        public override Storage Storage
        {
            get
            {
                return storage;
            }
        }

        public override IEnumerable<Directory> Directories
        {
            get
            {
                Task<IChildrenCollectionPage> task = storage.Client.Drive.Items[directory.Id].Children.Request().GetAsync();
                task.Wait();

                foreach (Item child in task.Result)
                {
                    if (child.Folder == null)
                        continue;

                    yield return new OneDriveDirectory(storage, this, child);
                }
            }
        }
        public override IEnumerable<Common.File> Files
        {
            get
            {
                Task<IChildrenCollectionPage> task = storage.Client.Drive.Items[directory.Id].Children.Request().GetAsync();
                task.Wait();

                foreach (Item child in task.Result)
                {
                    if (child.File == null)
                        continue;

                    yield return new OneDriveFile(storage, this, child);
                }
            }
        }

        private OneDriveStorage storage;
        private OneDriveDirectory parent;
        internal Item directory;

        public OneDriveDirectory(OneDriveStorage storage, OneDriveDirectory parent, Item directory)
        {
            this.storage = storage;
            this.parent = parent;
            this.directory = directory;
        }

        public override Directory CreateDirectory(string name)
        {
            if (!Storage.IsNameValid(name))
                throw new ArgumentException("The specified name contains invalid characters");

            Item item = new Item();
            item.Name = name;
            item.Folder = new Folder();

            Task<Item> task = storage.Client.Drive.Items[directory.Id].Children.Request().AddAsync(item);
            task.Wait();

            return new OneDriveDirectory(storage, this, task.Result);
        }
        public override void DeleteDirectory(Directory directory)
        {
            if (!directory.Parent.Equals(this))
                throw new ArgumentException("The specified directory could not be found");

            OneDriveDirectory oneDriveDirectory = directory as OneDriveDirectory;

            Task task = storage.Client.Drive.Items[oneDriveDirectory.directory.Id].Request().DeleteAsync();
            task.Wait();
        }

        public override Common.File CreateFile(string name)
        {
            if (!Storage.IsNameValid(name))
                throw new ArgumentException("The specified name contains invalid characters");

            Item item = new Item();
            item.Name = name;
            item.File = new Microsoft.OneDrive.Sdk.File();

            Task<Item> task = storage.Client.Drive.Items[directory.Id].ItemWithPath(name).Content.Request().PutAsync<Item>(new System.IO.MemoryStream());
            task.Wait();

            return new OneDriveFile(storage, this, task.Result);
        }
        public override void DeleteFile(Common.File file)
        {
            if (!file.Parent.Equals(this))
                throw new ArgumentException("The specified directory could not be found");

            OneDriveFile oneDriveFile = file as OneDriveFile;

            Task task = storage.Client.Drive.Items[oneDriveFile.file.Id].Request().DeleteAsync();
            task.Wait();
        }
    }
}