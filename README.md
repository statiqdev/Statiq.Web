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

## Modules

More information about the available modules will be added soon, including those that are still planned as development is still in the early stages. As a general rule, only modules that don't have any extra dependencies are defined in the core library. Any modules that do require support libraries (such as Markdown support) are typically defined in optional libraries and available on NuGet. These can be easily specified in your configuration file and Wyam will automatically pull them down when running.

### Input/Output
These modules manipulate input documents and return output documents.

#### Content
**Library:** Wyam.Core
**Signature:** `Content(string)`
**Input Content:** Any
**Output Content:** Same as input
**Required Metadata:** None
**Output Metadata:** None
**Description:** Replaces the content of each input document with the specified content.

### Control Flow
These modules alter the flow of the pipeline and can be used to enable more sophisticated scenarios.

### Analysis
These modules are used for validation and reporting and typically output the same documents that were input.

## Examples

There isn't a lot of documentation yet because the project is in such an early stage. The best place to start is the Examples folder in the source repository. It has some very simple examples of using Wyam from the console with configuration files. You'll just need to build the console application, navigate to the appropriate example folder, and run `Wyam.exe` from there. It'll pick up the `config.wyam` file and go from there.

You can also browse the unit tests to get an idea of how to integrate Wyam into your own application using the core library or how to write your own modules.
