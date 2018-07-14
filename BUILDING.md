# How to build

## Using the build script

Wyam uses [Cake](http://cakebuild.net/) to handle scripted build activities. Right now, Wyam is Windows-only (both build and execution). If you just want to build Wyam and all the extensions, run

```
build.cmd
``` 

If you want to build and run tests, run

```
build.cmd -target Run-Unit-Tests
```

You can also clean the build by running

```
build.cmd -target Clean
```

## From Visual Studio

If you want to open and build Wyam from Visual Studio, the main solution is in the root folder as `Wyam.sln`.

The `src\Wyam.Windows.sln` solution is only for the Windows-specific installer application and you'll probably never need to view or edit it.
