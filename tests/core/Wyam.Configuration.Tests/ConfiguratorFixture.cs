using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Shouldly;
using Wyam.Configuration.ConfigScript;
using Wyam.Core.Execution;
using Wyam.Testing;

namespace Wyam.Configuration.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class ConfiguratorFixture : BaseFixture
    {
        public class ConfigureTests : ConfiguratorFixture
        {
            [Test]
            public void ErrorContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Configurator configurator = GetConfigurator();
                const string configScript = @"
int z = 0;

foo bar;
";

                // When
                ScriptCompilationException exception = null;
                try
                {
                    configurator.Configure(configScript);
                }
                catch (ScriptCompilationException ex)
                {
                    exception = ex;
                }

                // Then
                exception.ErrorMessages.Count.ShouldBe(1);
                exception.ErrorMessages[0].ShouldStartWith("Line 4");
            }

            [Test]
            public void ErrorAfterLambdaExpansionContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Configurator configurator = GetConfigurator();
                const string configScript = @"
Pipelines.Add(
    Content(true
        && @doc.Bool(""Key"") == false
    )
);

foo bar;
";

                // When
                ScriptCompilationException exception = null;
                try
                {
                    configurator.Configure(configScript);
                }
                catch (ScriptCompilationException ex)
                {
                    exception = ex;
                }

                // Then
                exception.ErrorMessages.Count.ShouldBe(1);
                exception.ErrorMessages[0].ShouldStartWith("Line 8");
            }

            [Test]
            public void ErrorAfterLambdaExpansionOnNewLineContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Configurator configurator = GetConfigurator();
                const string configScript = @"
Pipelines.Add(
    Content(
        @doc.Bool(""Key"") == false
    )
);

foo bar;
";

                // When
                ScriptCompilationException exception = null;
                try
                {
                    configurator.Configure(configScript);
                }
                catch (ScriptCompilationException ex)
                {
                    exception = ex;
                }

                // Then
                exception.ErrorMessages.Count.ShouldBe(1);
                exception.ErrorMessages[0].ShouldStartWith("Line 8");
            }

            [Test]
            public void ErrorAfterLambdaExpansionWithArgumentSeparatorNewLinesContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Configurator configurator = GetConfigurator();
                const string configScript = @"
Pipelines.Add(
    If(
        @doc.Bool(""Key""),
        Content(""Baz"")
    )
);

foo bar;
";

                // When
                ScriptCompilationException exception = null;
                try
                {
                    configurator.Configure(configScript);
                }
                catch (ScriptCompilationException ex)
                {
                    exception = ex;
                }

                // Then
                exception.ErrorMessages.Count.ShouldBe(1);
                exception.ErrorMessages[0].ShouldStartWith("Line 9");
            }

            [Test]
            public void CanSetCustomDocumentFactory()
            {
                // Given
                Engine engine = new Engine();
                Configurator configurator = GetConfigurator(engine);
                const string configScript = @"
public class MyDocument : CustomDocument
{
    protected override CustomDocument Clone()
    {
        return new MyDocument();
    }
}

DocumentFactory = new CustomDocumentFactory<MyDocument>(DocumentFactory);
";

                // When
                configurator.Configure(configScript);

                // Then
                engine.DocumentFactory.GetType().Name.ShouldBe("CustomDocumentFactory`1");
            }

            [Test]
            public void SetCustomDocumentTypeSetsDocumentFactory()
            {
                // Given
                Engine engine = new Engine();
                Configurator configurator = GetConfigurator(engine);
                const string configScript = @"
public class MyDocument : CustomDocument
{
    protected override CustomDocument Clone()
    {
        return new MyDocument();
    }
}

SetCustomDocumentType<MyDocument>();
";

                // When
                configurator.Configure(configScript);

                // Then
                engine.DocumentFactory.GetType().Name.ShouldBe("CustomDocumentFactory`1");
            }

            [Test]
            public void SetsPrimitiveMetadata()
            {
                // Given
                Engine engine = new Engine();
                Configurator configurator = GetConfigurator(engine);
                const string configScript = @"
                    Settings[""TestString""] = ""teststring"";
                    Settings[""TestInt""] = 1234;
                    Settings[""TestFloat""] = 1234.567;
                    Settings[""TestBool""] = true;
                ";

                // When
                configurator.Configure(configScript);

                // Then
                engine.Settings["TestString"].ShouldBe("teststring");
                engine.Settings["TestInt"].ShouldBe(1234);
                engine.Settings["TestFloat"].ShouldBe(1234.567);
                engine.Settings["TestBool"].ShouldBe(true);
            }

            [Test]
            public void AddsPipelineAndModules()
            {
                // Given
                Engine engine = new Engine();
                Configurator configurator = GetConfigurator(engine);
                const string configScript = @"
                    Pipelines.Add(
                        new ReadFiles(""*.cshtml""),
	                    new WriteFiles("".html""));
                ";

                // When
                configurator.Configure(configScript);

                // Then
                engine.Pipelines.Count.ShouldBe(1);
                engine.Pipelines.Values.First().Count.ShouldBe(2);
            }

            [Test]
            public void SupportsGlobalConstructorMethods()
            {
                // Given
                Engine engine = new Engine();
                Configurator configurator = GetConfigurator(engine);
                const string configScript = @"
                    Pipelines.Add(
                        ReadFiles(""*.cshtml""),
	                    WriteFiles("".html""));
                ";

                // When
                configurator.Configure(configScript);

                // Then
                engine.Pipelines.Count.ShouldBe(1);
                engine.Pipelines.Values.First().Count.ShouldBe(2);
            }

            [Test]
            public void AppliesRecipeBeforeMetadata()
            {
                // Given
                Engine engine = new Engine();
                Configurator configurator = GetConfigurator(engine);
                configurator.AssemblyLoader.Add(Assembly.GetExecutingAssembly().FullName);
                configurator.RecipeName = nameof(MetadataTestRecipe);
                configurator.Settings = new Dictionary<string, object>
                {
                    { "Foo", "Baz" }
                };

                // When
                configurator.Configure(string.Empty);

                // Then
                engine.Settings["Foo"].ShouldBe("Baz");
            }

            [Test]
            public void ThrowsForDuplicateDirectives()
            {
                // Given
                RemoveListener();
                Configurator configurator = GetConfigurator();
                const string configScript = @"
#recipe foo
#recipe bar
";

                // When
                Exception exception = null;
                try
                {
                    configurator.Configure(configScript);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                // Then
                Assert.IsNotNull(exception);
                exception.Message.ShouldContain("Directive was previously specified");
            }
        }

        public class AddRecipePackageAndSetThemeTests : ConfiguratorFixture
        {
            [Test]
            public void AddsRecipePackage()
            {
                // Given
                Configurator configurator = GetConfigurator();
                configurator.RecipeName = nameof(KnownRecipe.Blog);
                configurator.PackageInstaller.AddPackage("Foobar");

                // When
                configurator.AddRecipePackageAndSetTheme();

                // Then
                configurator.PackageInstaller.PackageIds.Count.ShouldBe(2);
            }

            [Test]
            public void DoesNotAddRecipePackageIfAlreadyAdded()
            {
                // Given
                Configurator configurator = GetConfigurator();
                configurator.RecipeName = nameof(KnownRecipe.Blog);
                configurator.PackageInstaller.AddPackage(KnownRecipe.Blog.PackageId);

                // When
                configurator.AddRecipePackageAndSetTheme();

                // Then
                configurator.PackageInstaller.PackageIds.Count.ShouldBe(1);
            }

            [Test]
            public void SetsDefaultTheme()
            {
                // Given
                Configurator configurator = GetConfigurator();
                configurator.RecipeName = nameof(KnownRecipe.Blog);

                // When
                configurator.AddRecipePackageAndSetTheme();

                // Then
                configurator.Theme.ShouldBe(KnownRecipe.Blog.DefaultTheme);
            }

            [Test]
            public void DoesNotSetThemeIfThemeAlreadySet()
            {
                // Given
                Configurator configurator = GetConfigurator();
                configurator.RecipeName = nameof(KnownRecipe.Blog);
                configurator.Theme = "Foo";

                // When
                configurator.AddRecipePackageAndSetTheme();

                // Then
                configurator.Theme.ShouldBe("Foo");
            }

            [Test]
            public void DoesNotFailIfRecipeNotSet()
            {
                // Given
                Configurator configurator = GetConfigurator();

                // When
                configurator.AddRecipePackageAndSetTheme();
            }

            [Test]
            public void DoesNotFailIfRecipeNotFound()
            {
                // Given
                Configurator configurator = GetConfigurator();
                configurator.RecipeName = "Foo";

                // When
                configurator.AddRecipePackageAndSetTheme();
            }
        }

        public class AddThemePackagesAndPathTests : ConfiguratorFixture
        {
            [Test]
            public void AddsThemePackage()
            {
                // Given
                Configurator configurator = GetConfigurator();
                configurator.Theme = nameof(KnownTheme.CleanBlog);
                configurator.PackageInstaller.AddPackage("Foobar");

                // When
                configurator.AddThemePackagesAndPath();

                // Then
                configurator.PackageInstaller.PackageIds.Count.ShouldBe(2);
            }

            [Test]
            public void DoesNotAddThemePackageIfAlreadyAdded()
            {
                // Given
                Configurator configurator = GetConfigurator();
                configurator.Theme = nameof(KnownTheme.CleanBlog);
                configurator.PackageInstaller.AddPackage(KnownTheme.CleanBlog.PackageId);

                // When
                configurator.AddThemePackagesAndPath();

                // Then
                configurator.PackageInstaller.PackageIds.Count.ShouldBe(1);
            }

            [Test]
            public void OutputsWarningIfRecipeAndThemeDoNotMatch()
            {
                // Given
                Configurator configurator = GetConfigurator();
                configurator.RecipeName = "Foo";
                configurator.Theme = nameof(KnownTheme.CleanBlog);

                // When
                Should.Throw<Exception>(() => configurator.AddThemePackagesAndPath());
            }

            [Test]
            public void DoesNotOutputWarningIfRecipeAndThemeDoMatch()
            {
                // Given
                Configurator configurator = GetConfigurator();
                configurator.RecipeName = KnownTheme.CleanBlog.Recipe;
                configurator.Theme = nameof(KnownTheme.CleanBlog);

                // When
                configurator.AddThemePackagesAndPath();
            }

            [Test]
            public void DoesNotOutputWarningIfRecipeAndThemeCaseDoNotMatch()
            {
                // Given
                Configurator configurator = GetConfigurator();
                configurator.RecipeName = KnownTheme.CleanBlog.Recipe.ToUpper();
                configurator.Theme = nameof(KnownTheme.CleanBlog);

                // When
                configurator.AddThemePackagesAndPath();
            }

            [Test]
            public void UnknownThemeInsertsInputPath()
            {
                // Given
                Engine engine = new Engine();
                Configurator configurator = GetConfigurator(engine);
                configurator.Theme = "../MyTheme";

                // When
                configurator.AddThemePackagesAndPath();

                // Then
                engine.FileSystem.InputPaths[0].FullPath.ShouldBe("../MyTheme");
            }
        }

        private Configurator GetConfigurator()
        {
            Engine engine = new Engine();
            return GetConfigurator(engine);
        }

        private Configurator GetConfigurator(Engine engine)
        {
            Configurator configurator = new Configurator(engine);
            return configurator;
        }
    }
}
