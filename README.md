# SmartSync

SmartSync is a small tool to easily synchronize, backup or deploy several files. It aims to provide a simple and elegant way to synchronize things no matter what underlying storage type is used (local storage, remote storage, Zip archive, ...).

SmartSync uses synchronization profiles to describe the way data are meant to be synchronized, featuring several diff and sync modes and basic exclusion filters. Here are the storage types supported at this time :
- Local storage
- Remote SFTP storage over SSH
- Zip archive on any other supported storage

<p align="center">
    <img width="48" src="https://raw.githubusercontent.com/jbatonnet/smartsync/master/Icon.png" />
</p>

## Structure

- **SmartSync.Common** : The core project exposing a storage description model and basic diff and synchronization methods.
- **SmartSync.Engine** : A console application used to load a synchronization profile and run it.
- **FlowTomator.SmartSync** : Available in my [FlowTomator](https://github.com/jbatonnet/flowtomator/tree/master/FlowTomator.SmartSync) repository, this plugin allows to atomate a SmartSync profiles from a flow. 

<p align="center">
    <img width="48" src="https://raw.githubusercontent.com/jbatonnet/smartsync/master/Icon.png" />
</p>

## Development

SmartSync will be able to load and run plugins to support more storage types, I will add more builtin ones as I need them.

Here are some features to be done :
- Better handling of file copies
- Better Zip streams handling
- An elegant visual software to edit and visually run profiles
- Support for more complex exclusion/inclusion filters

## References

To provide Zip and Sftp storages, I use the following libraries, available as NuGet packages :
- DotNetZip (https://dotnetzip.codeplex.com/)
- SSH.Net (https://sshnet.codeplex.com/)

These libraries are bundled in SmartSync.Common assembly during compilation to avoid DLL versioning issues and to improve distribution simplicity.