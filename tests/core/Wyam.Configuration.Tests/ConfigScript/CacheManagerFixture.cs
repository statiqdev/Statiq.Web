using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NSubstitute;

using NUnit.Framework;

using Ploeh.AutoFixture;

using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Configuration.ConfigScript;
using Wyam.Testing;
using Wyam.Testing.IO;

namespace Wyam.Configuration.Tests.ConfigScript
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class CacheManagerFixture : BaseFixture
    {
        public class CacheManagerTests : CacheManagerFixture
        {
            private static readonly Fixture Autofixture = new Fixture();

            [Test]
            public void WhenNoCacheExistsThenEvaluate()
            {
                // Given
                string fakeCode = Autofixture.Create<string>();
                IReadOnlyCollection<Type> fakeClasses = Autofixture.CreateMany<Type>().ToList();

                CacheManager cacheManager = CreateCacheManager(out IEngine mockEngine, out IScriptManager mockScriptManager);
                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigHashPath).Exists.Returns(false);
                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigDllPath).OpenWrite().Returns(new MemoryStream());

                // When
                cacheManager.EvaluateCode(fakeCode, fakeClasses, false, false, true);

                // Then
                mockScriptManager.Received().Create(fakeCode, fakeClasses, Arg.Any<IEnumerable<string>>());
            }

            [Test]
            public void WhenNoCacheExistsThenSaveNewHashToCache()
            {
                // Given
                const string fakeCode = "8AC5716A-3272-4CBA-A787-B93B198E77B4";
                const string fakeCodeHash = "6ED6D026FFFFB8CB85ED6AA8410D942EBB277792EC9C71EC9F88E5F6AFB67DA56AB2AD217C7907FFFE450F25646D78E73D5356FBA4FE8BDB09A1383EDF3970F7";

                CacheManager cacheManager = CreateCacheManager(out IEngine mockEngine, out IScriptManager _);
                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigHashPath).Exists.Returns(false);
                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigDllPath).OpenWrite().Returns(new MemoryStream());

                // When
                cacheManager.EvaluateCode(fakeCode, Autofixture.CreateMany<Type>().ToList(), false, false, false);

                // Then
                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigHashPath).Received().WriteAllText(fakeCodeHash);
            }

            [Test]
            public void WhenNoCacheExistsThenSaveCompiledDllToCache()
            {
                // Given
                CacheManager cacheManager = CreateCacheManager(out IEngine mockEngine, out IScriptManager scriptManager);
                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigHashPath).Exists.Returns(false);

                scriptManager.RawAssembly.Returns(Autofixture.CreateMany<byte>().ToArray());

                MockStream cacheStream = new MockStream();
                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigDllPath).OpenWrite().Returns(cacheStream);

                // When
                cacheManager.EvaluateCode(Autofixture.Create<string>(), Autofixture.CreateMany<Type>().ToList(), false, false, false);

                // Then
                Assert.AreEqual(scriptManager.RawAssembly, cacheStream.MemoryStream.ToArray());
            }

            [Test]
            public void WhenNoCacheExistsAndCachingSavingDisabledThenDoNotSaveCompiledDll()
            {
                // Given
                CacheManager cacheManager = CreateCacheManager(out IEngine mockEngine, out IScriptManager scriptManager);
                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigHashPath).Exists.Returns(false);

                scriptManager.RawAssembly.Returns(Autofixture.CreateMany<byte>().ToArray());

                MockStream cacheStream = new MockStream();
                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigDllPath).OpenWrite().Returns(cacheStream);

                // When
                cacheManager.EvaluateCode(Autofixture.Create<string>(), Autofixture.CreateMany<Type>().ToList(), false, false, true);

                // Then
                Assert.IsEmpty(cacheStream.MemoryStream.ToArray());
            }

            [Test]
            public void WhenCacheExistsAndHashesAreSameThenUseCachedConfig()
            {
                // Given
                const string fakeCode = "8AC5716A-3272-4CBA-A787-B93B198E77B4";
                const string fakeCodeHash = "6ED6D026FFFFB8CB85ED6AA8410D942EBB277792EC9C71EC9F88E5F6AFB67DA56AB2AD217C7907FFFE450F25646D78E73D5356FBA4FE8BDB09A1383EDF3970F7";

                CacheManager cacheManager = CreateCacheManager(out IEngine mockEngine, out IScriptManager scriptManager);

                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigHashPath).Exists.Returns(true);
                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigHashPath).ReadAllText().Returns(fakeCodeHash);

                byte[] fakeAssembly = Autofixture.CreateMany<byte>().ToArray();
                MemoryStream cacheStream = new MemoryStream(fakeAssembly);
                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigDllPath).Exists.Returns(true);
                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigDllPath).OpenRead().Returns(cacheStream);

                // When
                cacheManager.EvaluateCode(fakeCode, Autofixture.CreateMany<Type>().ToList(), false, false, true);

                // Then
                scriptManager.Received().LoadCompiledConfig(Arg.Is<byte[]>(x => fakeAssembly.SequenceEqual(x)));
            }

            private CacheManager CreateCacheManager(out IEngine mockEngine, out IScriptManager mockScriptManager)
            {
                mockEngine = Substitute.For<IEngine>();
                mockEngine.FileSystem.Returns(Substitute.For<IFileSystem>());
                mockScriptManager = Substitute.For<IScriptManager>();

                CacheManager cacheManager = new CacheManager(mockEngine, mockScriptManager, FakeFilePath(), FakeFilePath(), FakeFilePath());

                IFile mockHashFile = Substitute.For<IFile>();
                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigHashPath).Returns(mockHashFile);

                IFile mockCacheFile = Substitute.For<IFile>();
                mockEngine.FileSystem.GetRootFile(cacheManager.ConfigDllPath).Returns(mockCacheFile);

                IFile mockOutputScript = Substitute.For<IFile>();
                mockEngine.FileSystem.GetRootFile(cacheManager.OutputScriptPath).Returns(mockOutputScript);

                return cacheManager;
            }

            private FilePath FakeFilePath()
            {
                string fakePathStr = Autofixture.Create<string>();
                FilePath path = new FilePath(fakePathStr);
                return path;
            }

            private class MockStream : Stream
            {
                public MemoryStream MemoryStream { get; } = new MemoryStream();

                public override void Flush()
                {
                    MemoryStream.Flush();
                }

                public override long Seek(long offset, SeekOrigin origin)
                {
                    return MemoryStream.Seek(offset, origin);
                }

                public override void SetLength(long value)
                {
                    MemoryStream.SetLength(value);
                }

                public override int Read(byte[] buffer, int offset, int count)
                {
                    return MemoryStream.Read(buffer, offset, count);
                }

                public override void Write(byte[] buffer, int offset, int count)
                {
                    MemoryStream.Write(buffer, offset, count);
                }

                public override bool CanRead => MemoryStream.CanRead;
                public override bool CanSeek => MemoryStream.CanSeek;
                public override bool CanWrite => MemoryStream.CanWrite;
                public override long Length => MemoryStream.Length;
                public override long Position { get => MemoryStream.Position; set => MemoryStream.Position = value; }
            }
        }
    }
}