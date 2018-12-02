# Wyam
[![AppVeyor](https://img.shields.io/appveyor/ci/Wyam/wyam/master.svg?label=appveyor)](https://ci.appveyor.com/project/Wyam/wyam/branch/master) 
[![Join the chat at https://gitter.im/Wyamio/Wyam](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Wyamio/Wyam?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Wyam is a simple to use, highly modular, and extremely configurable static content generator that can be used to generate web sites, produce documentation, create ebooks, and much more. Since everything is configured by chaining together flexible modules (that you can even write yourself), the only limits to what it can create are your imagination.

The easiest way to get started is to install as a .NET Core global tool and use a [recipe and theme](https://wyam.io/recipes).

1. Download and install Wyam as a global tool:

    `dotnet tool install -g Wyam.Tool`

2. Scaffold a new blog:

    `wyam new --recipe Blog`

3. Edit the scaffolded files.

4. Build the blog with a theme:

    `wyam --recipe Blog --theme CleanBlog`

To go deeper, read more about the [underlying concepts](https://wyam.io/docs/concepts) and then read about [configuration files](https://wyam.io/docs/usage/configuration) and the [available command line arguments](https://wyam.io/docs/usage/command-line). Then check out the full list of [modules](https://wyam.io/modules).

For more information see [Wyam.io](https://wyam.io).

## Acknowledgements

* Portions of the IO support were originally inspired from [Cake](http://cakebuild.net) under an MIT license.
* The RSS/Atom support was originally ported from [WebFeeds](https://github.com/mckamey/web-feeds.net) under an MIT license.
* Many other fantastic OSS libraries are used directly as NuGet packages, thanks to all the OSS authors out there!
