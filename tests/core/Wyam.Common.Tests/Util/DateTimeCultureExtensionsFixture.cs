using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Util;
using Wyam.Testing;
using Wyam.Testing.Execution;

namespace Wyam.Common.Tests.Util
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class DateTimeCultureExtensionsFixture : BaseFixture
    {
        public class GetDateTimeInputCultureTests : DateTimeCultureExtensionsFixture
        {
            [SetUp]
            public void SetThreadCulture()
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            }

            [Test]
            public void GetsCurrentCultureIfNoSetting()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                CultureInfo result = context.GetDateTimeInputCulture();

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.CurrentCulture));
            }

            [Test]
            public void GetsSettingCulture()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.DateTimeInputCulture] = CultureInfo.GetCultureInfo("en-GB");

                // When
                CultureInfo result = context.GetDateTimeInputCulture();

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.GetCultureInfo("en-GB")));
            }
        }

        public class GetDateTimeDisplayCultureTests : DateTimeCultureExtensionsFixture
        {
            [SetUp]
            public void SetThreadCulture()
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            }

            [Test]
            public void GetsCurrentCultureIfNoSetting()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                CultureInfo result = context.GetDateTimeDisplayCulture();

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.CurrentCulture));
            }

            [Test]
            public void GetsTargetCultureIfNoSetting()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                CultureInfo result = context.GetDateTimeDisplayCulture("fr-FR");

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.GetCultureInfo("fr-FR")));
            }

            [Test]
            public void GetsSettingCultureIfSettingMatchesDefault()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.DateTimeDisplayCulture] = CultureInfo.GetCultureInfo("en-US");

                // When
                CultureInfo result = context.GetDateTimeDisplayCulture();

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.GetCultureInfo("en-US")));
            }

            [Test]
            public void GetsSettingCultureIfSettingDoesNotMatchDefault()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.DateTimeDisplayCulture] = CultureInfo.GetCultureInfo("fr-FR");

                // When
                CultureInfo result = context.GetDateTimeDisplayCulture();

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.GetCultureInfo("fr-FR")));
            }

            [Test]
            public void GetsSettingCultureIfSettingDoesNotMatchDefaultAndNeutralTargetSpecified()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.DateTimeDisplayCulture] = CultureInfo.GetCultureInfo("fr-FR");

                // When
                CultureInfo result = context.GetDateTimeDisplayCulture("fr");

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.GetCultureInfo("fr-FR")));
            }

            [Test]
            public void GetsSettingCultureIfSettingDoesNotMatchDefaultAndSpecifiedTargetSpecified()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.DateTimeDisplayCulture] = CultureInfo.GetCultureInfo("fr-FR");

                // When
                CultureInfo result = context.GetDateTimeDisplayCulture("fr-LU");

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.GetCultureInfo("fr-FR")));
            }

            [Test]
            public void GetsDefaultCultureIfCurrentCultureDoesNotMatch()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

                // When
                CultureInfo result = context.GetDateTimeDisplayCulture();

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.GetCultureInfo("en-GB")));
            }
        }
    }
}
