using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Drive.v2;
using SmartSync.Common;

namespace SmartSync.GoogleDrive
{
    public class GoogleDriveDirectory : Directory
    {
        internal const string MimeType = "application/vnd.google-apps.folder";

        public override string Name
        {
            get
            {
                return folder.Title;
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
                FilesResource.ListRequest request = storage.Service.Files.List();
                request.Q = string.Format("'{0}' in parents and trashed = false and mimeType = '{1}'", folder.Id, MimeType);
                request.Fields = "items(id,title,fileSize,mimeType)";

                Google.Apis.Drive.v2.Data.File[] folders = request.Execute().Items.ToArray();
                return folders.Select(f => new GoogleDriveDirectory(storage, this, f));
            }
        }
        public override IEnumerable<File> Files
        {
            get
            {
                FilesResource.ListRequest request = storage.Service.Files.List();
                request.Q = string.Format("'{0}' in parents and trashed = false and mimeType != '{1}'", folder.Id, MimeType);
                request.Fields = "items(id,title,fileSize,mimeType)";

                Google.Apis.Drive.v2.Data.File[] files = request.Execute().Items.ToArray();
                return files.Select(f => new GoogleDriveFile(storage, this, f));
            }
        }

        private GoogleDriveStorage storage;
        private GoogleDriveDirectory parent;
        internal Google.Apis.Drive.v2.Data.File folder;

        public GoogleDriveDirectory(GoogleDriveStorage storage, GoogleDriveDirectory parent, Google.Apis.Drive.v2.Data.File folder)
        {
            this.storage = storage;
            this.parent = parent;
            this.folder = folder;
        }

        public override Directory CreateDirectory(string name)
        {
            if (!Storage.IsNameValid(name))
                throw new ArgumentException("The specified name contains invalid characters");

            Google.Apis.Drive.v2.Data.File folder = new Google.Apis.Drive.v2.Data.File();

            folder.Title = name;
            folder.Parents = new List<Google.Apis.Drive.v2.Data.ParentReference>() { new Google.Apis.Drive.v2.Data.ParentReference() { Id = this.folder.Id } };
            folder.MimeType = MimeType;

            var request = storage.Service.Files.Insert(folder);
            request.Execute();

            return new GoogleDriveDirectory(storage, this, folder);
        }
        public override void DeleteDirectory(Directory directory)
        {
            if (!directory.Parent.Equals(this))
                throw new ArgumentException("The specified directory could not be found");

            GoogleDriveDirectory googleDriveDirectory = directory as GoogleDriveDirectory;

            if (storage.UseTrash)
            {
                FilesResource.TrashRequest request = storage.Service.Files.Trash(googleDriveDirectory.folder.Id);
                request.Execute();
            }
            else
            {
                FilesResource.DeleteRequest request = storage.Service.Files.Delete(googleDriveDirectory.folder.Id);
                request.Execute();
            }
        }

        public override File CreateFile(string name)
        {
            if (!Storage.IsNameValid(name))
                throw new ArgumentException("The specified name contains invalid characters");

            Google.Apis.Drive.v2.Data.File file = new Google.Apis.Drive.v2.Data.File();

            file.Title = name;
            file.Parents = new List<Google.Apis.Drive.v2.Data.ParentReference>() { new Google.Apis.Drive.v2.Data.ParentReference() { Id = this.folder.Id } };

            var request = storage.Service.Files.Insert(file);
            request.Execute();

            return new GoogleDriveFile(storage, this, file);
        }
        public override void DeleteFile(File file)
        {
            if (!file.Parent.Equals(this))
                throw new ArgumentException("The specified directory could not be found");

            GoogleDriveFile googleDriveFile = file as GoogleDriveFile;

            if (storage.UseTrash)
            {
                FilesResource.TrashRequest request = storage.Service.Files.Trash(googleDriveFile.file.Id);
                request.Execute();
            }
            else
            {
                FilesResource.DeleteRequest request = storage.Service.Files.Delete(googleDriveFile.file.Id);
                request.Execute();
            }
        }
    }
}