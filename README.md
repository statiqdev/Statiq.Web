# Wyam
Wyam is a simple to use, highly configurable, and modular static content generator. It can be used to generate web sites, scaffold development projects, create ebooks, and much more.

It's currently under active development, but is usable today. Read on for more information if you want to get started now and help influence it's future direction.

## How Does It Work?
The primary concepts in wyam are *documents*, *modules*, and *pipelines*. 

A document is a combination of *content* and *metadata* as it makes it's way through the system. The content of a document is what most modules manipulate and is what you will presumably output at the end of the pipeline. The metadata serves as a way to pass information to and from modules to other modules. Once a value is added to the metadata by one module, it can never be removed by a subsequent one (though it can be overwritten, in which case a warning will be shown). It can be important to note that documents are immutable. Though we often talk about documents being "transformed" or "manipulated" by modules, this isn't strictly accurate. Instead modules return a new copy of the document with different content and/or additional metadata.

A module is a small single-purpose component that takes in documents, does something based on those documents (possibly transforming them), and outputs documents as a result.

A pipeline is a series of modules executed in order that results in final output documents. A given Wyam configuration can have multiple pipelines which are executed in order, and following pipelines have access to the output documents from the previous pipelines.

A simple pipeline looks like:
```
  [Empty Document]
         |
      [Module]
	  /      \
[Document] [Document]
    |           |
  [----Module*----]
    |           |
[Document] [Document]

* modules often handle documents in parallel for performance
```

In the visualization above, the first module may have read some files (in this case 2 files) and stuck some information about those files such as name and path in the document metadata. Then the second module may have transformed the files (for example, from Markdown to HTML).

## Installation

Wyam can be run from the console or directly called from your application. As they become more mature, both the console application and the core library will be available on NuGet. However, at the moment you must obtain and build from source to use Wyam. 

## Configuration

Configuring such a pipeline is easy, and Wyam configuration files are designed to be simple and straightforward:
```
Pipelines.Add(
	ReadFiles("*.md"),
	Markdown(),
	WriteFiles(".html")
);
```

However, don't let the simplicity fool you. Wyam configuration files are C# scripts and as such can make use of the full C# language and the entire .NET ecosystem (including built-in support for NuGet). For example, one of the built-in modules lets you write a delegate to transform the input documents for extreme flexibility.

Note that when supplying modules to the pipeline or to other modules, new instances of the module class are usually required. An astute reader will notice that in the example above modules are being specified with what look like methods. These methods are just shorthand for the actual module class constructors and this convention can be used for any module within the configuration script. The example configuration above could also have been written as:
```
Pipelines.Add(
	new ReadFiles("*.md"),
	new Markdown(),
	new WriteFiles(".html")
);
```

## Examples

There isn't a lot of documentation yet because the project is in such an early stage. The best place to start is the Examples folder in the source repository. It has some very simple examples of using Wyam from the console with configuration files. You'll just need to build the console application, navigate to the appropriate example folder, and run `Wyam.exe` from there. It'll pick up the `config.wyam` file and go from there.

You can also browse the unit tests to get an idea of how to integrate Wyam into your own application using the core library or how to write your own modules.

## Modules

More information about the available modules will be added soon, including those that are still planned as development is still in the early stages. As a general rule, only modules that don't have any extra dependencies are defined in the core library. Any modules that do require support libraries (such as Markdown support) are typically defined in optional libraries and available on NuGet. These can be easily specified in your configuration file and Wyam will automatically pull them down when running.

### Input/Output
These modules manipulate input documents (by changing content, adding metadata, or both) and return output documents.

- #### Content (*Wyam.Core*)
  - Replaces the content of each input document with the string value of the specified content object. In the case where modules are provided, they are executed against an empty initial document and the results are applied to each input document.
  - Usage:
    - `Content(object content)`: Uses the string value of the specified object as the new content for every input document.
    - `Content(Func<IDocument, object> content)`: Uses the string value of the returned object as the new content for each document. This allows you to specify different content for each document depending on the input.
    - `Content(params IModule[] modules)`: The specified modules are executed against an empty initial document and the results are applied to every input document (possibly creating more than one output document for each input document).
    
- #### Metadata (*Wyam.Core*)
  - Adds additional metadata to each input document.
  - Usage:
    - `Metadata(string key, object metadata)`: The specified object is added as metadata for the specified key for every input document.
    - `Metadata(string key, Func<IDocument, object> metadata)`: The specified object is added as metadata for each document. This allows you to specify different metadata for each document depending on the input.
    
- #### ReadFiles (*Wyam.Core*)
  - Reads the content of files from the file system into the content of new documents. The following metadata is added to each new document:
    - `FileRoot`: The root search path without any nested directories (useful for outputting documents at the same location relative to the root path).
    - `FilePath`: The full path to the file.
    - `FileBase`: Equivalent to `Path.GetFileNameWithoutExtension(FilePath)`.
    - `FileExt`: Equivalent to `Path.GetExtension(FilePath)`.
    - `FileName: Equivalent to `Path.GetFileName(FilePath)`.
    - `FileDir`: Equivalent to `Path.GetDirectoryName(FilePath)`.
  - Usage:
    - `ReadFiles(string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)`: Reads all files that match the specified search pattern with the specified `SearchOption`. If the input metadata contains the key `InputPath` (which is usually set by default), it is used as the root path of the search.
    - `ReadFiles(Func<IDocument, string> path, SearchOption searchOption = SearchOption.AllDirectories)`: Reads all files that match the specified search pattern with the specified `SearchOption`. This allows you to specify different search paths depending on the input.

- #### WriteFiles (*Wyam.Core*)
  - Writes the content of each input document to the file system.
  - Usage:
    - `WriteFiles(string extension)`: Writes the document content to disk with the specified extension with the same base filename and relative path as the input file. This requires metadata values for `OutputPath` (which is usually set by default), `FileRoot`, `FileDir`, and `FileBase` (which are all automatically set by the ReadFiles module).
    - `WriteFiles(Func<IMetadata, string> path)`: Writes the document content to disk at the specified path.
    
- #### Append (*Wyam.Core*)
  - Appends content to each input document.
  - Usage:
    - `Append(object content)`: Appends the string value of the specified object to the content of every input document.
    - `Append(Func<IDocument, object> content)`: Appends the string value of the returned object to to content of each document. This allows you to specify different content to append for each document depending on the input.
    - `Append(params IModule[] modules)`: The specified modules are executed against an empty initial document and the results are appended to the content of every input document (possibly creating more than one output document for each input document).
    
- #### Prepend (*Wyam.Core*)
  - Prepends content to each input document.
  - Usage:
    - `Prepend(object content)`: Prepends the string value of the specified object to the content of every input document.
    - `Prepend(Func<IDocument, object> content)`: Prepends the string value of the returned object to to content of each document. This allows you to specify different content to prepend for each document depending on the input.
    - `Prepend(params IModule[] modules)`: The specified modules are executed against an empty initial document and the results are prepended to the content of every input document (possibly creating more than one output document for each input document).
    
- #### Replace (*Wyam.Core*)
  - Replaces a search string in the content of each input document with new content.

- #### ReplaceIn (*Wyam.Core*)
  - Replaces a search string in the specified content with the content of an input document. This module is very useful for simple template substitution.

- #### Markdown (*Wyam.Modules.Markdown*)
  - Replaces markdown content with rendered HTML content.

- #### Liquid (*Planned*)
  - Renders liquid templates.

- #### Razor (*Planned*)
  - Renders razor templates.

- #### Yaml (*Planned*)
  - Parses YAML and adds the results to the metadata for each input document.

- #### Json (*Planned*)
  - Parses JSON and adds the results to the metadata for each input document.

- #### FrontMatter (*Planned*)
  - Extracts the initial part of the content of each document, stopping at the specified delimiter, and passes it to the sepcified modules (such as Yaml or Json) for further processing.

### Control Flow
These modules alter the flow of the pipeline and can be used to enable more sophisticated scenarios.

- #### Branch (*Wyam.Core*)
  - Executes the specified modules on the input documents and then outputs the input documents without modification. In other words, this module does not affect the primary pipeline control flow but rather allows additional processing to take place.
  
- #### If (*Wyam.Core*)
  - Evaluates a predicate and then executes the specified modules if it is met. Unlike the Branch module, this module outputs the result documents from the specified modules as it's output if the predicate is met. This module also has support for further "else if" and "else" clauses. 

### Analysis
These modules are used for validation and reporting and typically output the same documents that were input.

### Advanced
These modules let you customize the behavior of Wyam and enable advanced scenarios.

- #### Delegate (*Wyam.Core*)
  - This module executes a delegate for each input document. It allows you full customization by executing your own code at any stage in the pipeline. In order words, it's a way to create a light-weight special-purpose module directly in the configuration script.
