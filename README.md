# SmartSync

SmartSync is a small tool to easily synchronize, backup or deploy several files. It aims to provide a simple and elegant way to synchronize things no matter what underlying storage type is used (local storage, remote storage, Zip archive, ...).

SmartSync uses synchronization profiles to describe the way data are meant to be synchronized, featuring several diff and sync modes and basic exclusion filters. Here are the storage types supported at this time :
- Local storage
- Zip archive on any other supported storage
- Remote SFTP storage over SSH
- Remote Google Drive and OneDrive storage

<p align="center">
    <img width="48" src="https://raw.githubusercontent.com/jbatonnet/smartsync/master/Icon.png" />
</p>

## Structure

- **SmartSync.Common** : The core project exposing a storage description model and basic diff and synchronization methods.
- **SmartSync.Engine** : A console application used to load a synchronization profile and run it.
 
## Plugins

- **SmartSync.Sftp** : A plugin to allow SFTP storage connection and manipulation.
- **SmartSync.GoogleDrive** : A plugin to allow Google Drive storage connection and manipulation.
- **SmartSync.OneDrive** : A plugin to allow OneDrive storage connection and manipulation.
- **FlowTomator.SmartSync** : Available in my [FlowTomator](https://github.com/jbatonnet/flowtomator/tree/master/FlowTomator.SmartSync) repository, this plugin allows to atomate a SmartSync profiles from a flow.

<p align="center">
    <img width="48" src="https://raw.githubusercontent.com/jbatonnet/smartsync/master/Icon.png" />
</p>

## Development

SmartSync is able to load and run plugins to support more storage types, I will add more builtin ones as I need them.

Here are some features to be done :
- Better handling of file copies
- An elegant visual software to edit and visually run profiles
- Support for more complex exclusion/inclusion filters

## References

To provide custom storages, I use the following libraries, available as NuGet packages :
- SSH.Net (https://sshnet.codeplex.com)
- Google Drive (https://developers.google.com/drive/web/quickstart/dotnet)
- OneDrive (https://github.com/onedrive/onedrive-sdk-csharp)

These libraries are bundled in plugins assembly during compilation to avoid DLL versioning issues and to improve distribution simplicity.