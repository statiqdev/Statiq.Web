using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using NUnit.Framework;
using Wyam.Core.Extensibility;

namespace Wyam.Core.Tests
{
    [TestFixture]
    public class RepositoryFixture
    {
        [Test]
        public void NullPackageSourceResultsInDefaultNuGetFeed()
        {
            // Given
            Repository repository = new Repository(null);
            
            // When
            IPackageRepository packageRepository = repository.GetRepository();

            // Then
            Assert.AreEqual("https://packages.nuget.org/api/v2/", packageRepository.Source);
        }

        [Test]
        public void AlternatePackageSource()
        {
            // Given
            Repository repository = new Repository("https://test.com");

            // When
            IPackageRepository packageRepository = repository.GetRepository();

            // Then
            Assert.AreEqual("https://test.com", packageRepository.Source);
        }
    }
}
