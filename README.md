# Wyam
[![AppVeyor](https://img.shields.io/appveyor/ci/Wyam/wyam/master.svg?label=appveyor)](https://ci.appveyor.com/project/Wyam/wyam/branch/master) 
[![Join the chat at https://gitter.im/Wyamio/Wyam](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Wyamio/Wyam?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Wyam is a simple to use, highly modular, and extremely configurable static content generator that can be used to generate web sites, produce documentation, create ebooks, and much more. Since everything is configured by chaining together flexible modules (that you can even write yourself), the only limits to what it can create are your imagination.

```
c:\MySite>Wyam.exe --preview --watch
Loading configuration from c:\MySite\config.wyam.
Cleaning output directory c:\MySite\.\Output...
Cleaned output directory.
Executing 3 pipelines...
    Executing pipeline "Markdown" (1/3) with 5 child module(s)...
    Executed pipeline "Markdown" (1/3) resulting in 0 output document(s).
    Executing pipeline "Razor" (2/3) with 4 child module(s)...
    Executed pipeline "Razor" (2/3) resulting in 2 output document(s).
    Executing pipeline "Resources" (3/3) with 1 child module(s)...
    Executed pipeline "Resources" (3/3) resulting in 21 output document(s).
Executed 3 pipelines.
Preview server running on port 5080...
Watching folder c:\MySite\.\Input...
Hit any key to exit...	
```

For more information see [Wyam.io](https://wyam.io).

## Limitations

* Only runs on .NET Framework (i.e. Windows) at the moment. Work is underway to make it run on .NET Core also: [#300](https://github.com/Wyamio/Wyam/issues/300).

## Acknowledgements

* Portions of the IO support were originally inspired from [Cake](http://cakebuild.net) under an MIT license.
* The RSS/Atom support was originally ported from [WebFeeds](https://github.com/mckamey/web-feeds.net) under an MIT license.
* Many other fantastic OSS libraries are used directly as NuGet packages, thanks to all the OSS authors out there!
