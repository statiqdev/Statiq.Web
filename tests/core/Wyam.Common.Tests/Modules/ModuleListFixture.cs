using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Modules;
using Wyam.Testing;
using Wyam.Testing.Modules;

namespace Wyam.Common.Tests.Modules
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ModuleListFixture : BaseFixture
    {
        public class AddTests : ModuleListFixture
        {
            [Test]
            public void AddModuleWithoutName()
            {
                // Given
                ModuleList list = new ModuleList();
                CountModule count = new CountModule("A");

                // When
                list.Add(count);

                // Then
                Assert.That(((IEnumerable<KeyValuePair<string, IModule>>) list).First(),
                    Is.EqualTo(new KeyValuePair<string, IModule>(null, count)));
            }

            [Test]
            public void AddModuleWithName()
            {
                // Given
                ModuleList list = new ModuleList();
                CountModule count = new CountModule("A");

                // When
                list.Add("Foo", count);

                // Then
                Assert.That(((IEnumerable<KeyValuePair<string, IModule>>) list).First(),
                    Is.EqualTo(new KeyValuePair<string, IModule>("Foo", count)));
            }

            [Test]
            public void AddNamedModule()
            {
                // Given
                ModuleList list = new ModuleList();
                CountModule count = new CountModule("A");

                // When
                list.Add(new NamedModule("Foo", count));

                // Then
                Assert.That(((IEnumerable<KeyValuePair<string, IModule>>) list).First(),
                    Is.EqualTo(new KeyValuePair<string, IModule>("Foo", count)));
            }

            [Test]
            public void ThrowsWhenAddingDuplicateNamedModule()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(new NamedModule("A", count));

                // When, Then
                Assert.That(() => list.Add(new NamedModule("A", count2)), Throws.Exception);
            }
        }

        public class ContainsTests : ModuleListFixture
        {
            [Test]
            public void ContainsModule()
            {
                // Given
                ModuleList list = new ModuleList();
                CountModule count = new CountModule("A");

                // When
                list.Add(count);

                // Then
                Assert.That(list.Contains(count), Is.True);
            }

            [Test]
            public void DoesNotContainModule()
            {
                // Given
                ModuleList list = new ModuleList();
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");

                // When
                list.Add(count);

                // Then
                Assert.That(list.Contains(count2), Is.False);
            }
        }

        public class RemoveTests : ModuleListFixture
        {
            [Test]
            public void RemovesModule()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count, count2);

                // When
                bool result = list.Remove(count);

                // Then
                Assert.That(result, Is.True);
                Assert.That(list, Is.EqualTo(new[] { count2 }));
            }

            [Test]
            public void ReturnsFalseWhenNotRemoved()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count2);

                // When
                bool result = list.Remove(count);

                // Then
                Assert.That(result, Is.False);
                Assert.That(list, Is.EqualTo(new [] { count2 }));
            }

            [Test]
            public void RemovesModuleByName()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(
                    new NamedModule("A", count),
                    new NamedModule("B", count2));

                // When
                bool result = list.Remove("A");

                // Then
                Assert.That(result, Is.True);
                Assert.That(list, Is.EqualTo(new [] { count2 }));
            }

            [Test]
            public void ReturnsFalseWhenNotRemovedByName()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(
                    new NamedModule("B", count2));

                // When
                bool result = list.Remove("A");

                // Then
                Assert.That(result, Is.False);
                Assert.That(list, Is.EqualTo(new [] { count2 }));
            }
        }

        public class IndexOfTests : ModuleListFixture
        {
            [Test]
            public void ReturnsIndex()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count, count2);

                // When
                int index = list.IndexOf(count2);

                // Then
                Assert.That(index, Is.EqualTo(1));
            }

            [Test]
            public void ReturnsNegativeIndexWhenNotFound()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count);

                // When
                int index = list.IndexOf(count2);

                // Then
                Assert.That(index, Is.LessThan(0));
            }

            [Test]
            public void ReturnsIndexByName()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(
                    new NamedModule("A", count),
                    new NamedModule("B", count2));

                // When
                int index = list.IndexOf("B");

                // Then
                Assert.That(index, Is.EqualTo(1));
            }

            [Test]
            public void ReturnsNegativeIndexWhenNotFoundByName()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(
                    new NamedModule("B", count2));

                // When
                int index = list.IndexOf("A");

                // Then
                Assert.That(index, Is.LessThan(0));
            }
        }

        public class InsertTests : ModuleListFixture
        {
            [Test]
            public void InsertsModule()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count2);

                // When
                list.Insert(0, count);

                // Then
                Assert.That(list, Is.EqualTo(new [] {count, count2}));
            }

            [Test]
            public void ThrowsWhenInsertingDuplicateNamedModule()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(
                    new NamedModule("A", count), 
                    new NamedModule("B", count2));

                // When, Then
                Assert.That(() => list.Insert(1, new NamedModule("A", count2)), Throws.Exception);
            }
        }

        public class IndexerTests : ModuleListFixture
        {
            [Test]
            public void ThrowsWhenSettingDuplicateNamedModule()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(
                    new NamedModule("A", count),
                    new NamedModule("B", count2));

                // When, Then
                Assert.That(() => { list[1] = new NamedModule("A", count2); }, Throws.Exception);
            }

            [Test]
            public void DoesNotThrowWhenSettingDuplicateNamedModuleAtSameIndex()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(
                    new NamedModule("A", count),
                    new NamedModule("B", count2));

                // When
                list[1] = new NamedModule("B", count2);

                // Then
                Assert.That(list, Is.EqualTo(new[] { count, count2 }));
            }
        }
    }
}
