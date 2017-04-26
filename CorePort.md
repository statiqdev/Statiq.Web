# Core Porting Status

This file tracks the status of porting Wyam to .NET Core (also see [issue #300](https://github.com/Wyamio/Wyam/issues/300)).

Before anything else, Wyam.Common needs to be ported. Then we can port libraries in the order below:

## Modules and Extensions

| Project | Ready For Porting | Ported | Notes |
|---------|-------------------|--------|-------|
|Wyam.AmazonWebServices| [ ] | [ ] | |
|Wyam.CodeAnalysis| [ ] | [ ] | |
|Wyam.Feeds| [ ] | [ ] | |
|Wyam.Git| [ ] | [ ] | |
|Wyam.GitHub| [ ] | [ ] | |
|Wyam.Highlight| [ ] | [ ] | |
|Wyam.Html| [ ] | [ ] | |
|Wyam.Images| [ ] | [ ] | |
|Wyam.Json| [ ] | [ ] | |
|Wyam.Less| [ ] | [ ] | |
|Wyam.Markdown| [ ] | [ ] | |
|Wyam.Minification| [ ] | [ ] | |
|Wyam.Razor| [ ] | [ ] | |
|Wyam.SearchIndex| [ ] | [ ] | |
|Wyam.Tables| [ ] | [ ] | |
|Wyam.TextGeneration| [ ] | [ ] | |
|Wyam.Xml| [ ] | [ ] | |
|Wyam.Xslt2| [ ] | [ ] | |
|Wyam.Yaml| [ ] | [ ] | |

## Recipes

| Project | Ready For Porting | Ported | Notes |
|---------|-------------------|--------|-------|
|Wyam.Web| [ ] | [ ] | |
|Wyam.BookSite| [ ] | [ ] | |
|Wyam.Blog| [ ] | [ ] | |
|Wyam.Docs| [ ] | [ ] | |

## Core

| Project | Ready For Porting | Ported | Notes |
|---------|-------------------|--------|-------|
|Wyam.Hosting| [ ] | [ ] | |
|Wyam.Testing| [ ] | [ ] | |
|Wyam.Testing.JavaScript| [ ] | [ ] | |
|Wyam.Configuration| [ ] | [ ] | |
|Wyam.Core| [ ] | [ ] | |

## Clients

| Project | Ready For Porting | Ported | Notes |
|---------|-------------------|--------|-------|
|Cake.Wyam| [ ] | [ ] | |
|Wyam| [ ] | [ ] | Haven't decided yet what kind of console app to make, to multi-target, or a distribution strategy (package runtime and framework, use system runtime and framework, AOT, etc.)|