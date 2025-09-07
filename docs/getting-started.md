# Getting Started

Before you start: make sure that the project where you want to use that library is at least on .NET 8.

## Installing the library
Via `dotnet` CLI:
```cmd
dotnet add package Halva.Package.Core
```

Via Visual Studio's Package Manager Console:

```ps
NuGet\Install-Package Halva.Package.Core
```

Or by `PackageReference`:
```xml
<!-->
This is how to install it if you don't use Central Package Management set up.
</-->
<PackageReference Include="Halva.Package.Core" Version="5.0.7" />
```

If you use Central Package Management and don't use the CLI:

On the Direcotry.Packages.props:

```xml
<PackageVersion Include="Halva.Package.Core" Version="5.0.7" />
```

On the project that you want to use it with:

```xml
<PackageReference Include="Halva.Package.Core" />
```

## Creating packages

Now that the library is installed, let's create a package. This can be done in two ways:

- You can use the built-in way on `PackageUtilities` like so:
```cs
// Synchronous way.
PackageUtilities.CreatePackageFromFolder("<source folder>", "<output location>");

//Async way
await PackageUtilities.CreatePackageFromFolderAsync("<source folder>", "<output location>");
```

- Create a `PackageBuilder` object, index the files that you want to include and write the file.

```cs
//Create the object, and set a location for that package.
PackageBuilder packMaker = new PackageBuilder("location of the final archive");

//Index the files and folders that we want inside the package.
packMaker.AddFilesFromFolder("<folder location>", "<relative location>");
packMaker.AddFile("<file location>", "<relative location>");

//OPTIONAL: Set the compression level for the package here:
packMaker.CompressionOption = CompressionLevel.Optimal;

//Once are all added in, create the final package like this:
packMaker.Commit();
//Or in async:
await packMaker.CommitAsync();
```
You can add a password and IV so you can encrypt the package, if so desired.

## Extracting

We now have a package. But how do we extract some or all of the files? It's simple:

- To extract everything, use the method inside `PackageUtilities`:

```cs
PackageUtilities.DecompressPackageToFolder("<source package>", "<output location>");

//Async way
await PackageUtilities.DecompressPackageToFolderAsync("<source package>", "<output location>");
```

- If you want to extract one or more specific files, you can use the `PackageReader` object like this:

```cs
//Create the PackageReader object and set the location of the package.
var packReader = new PackageReader("<source package>");

//And now, just extract like this:
packReader.ExtractFile("<source file>", "<destination path and file name + extension>");

//Or in asynchronous manner:
await packReader.ExtractFileAsync("<source file>", "<destination path and file name + extension>");
```

## Updating files

This library can also update files on a specific folder, by using `PackageReader`:

```cs
//Create the PackageReader object and set the location of the package.
var packReader = new PackageReader("<source package>");

//There are two ways: Either update via metadata:
packReader.FastUpdateFromArchive("<target folder>");

//Or by file hashes (uses XxHash128)
packReader.UpdateFromArchive("<target folder>");

//It can also be done with async:
await packReader.FastUpdateFromArchiveAsync("<target folder>");
await packReader.UpdateFromArchiveAsync("<target folder>");
```