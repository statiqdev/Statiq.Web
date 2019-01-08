using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
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

        public class ToShortDateStringTests : DateTimeCultureExtensionsFixture
        {
            [SetUp]
            public void SetThreadCulture()
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            }

            [Test]
            public void IncludesShortNameOfFrenchDayAndMonth()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                CultureInfo culture = (CultureInfo)CultureInfo.GetCultureInfo("fr-FR").Clone();
                culture.DateTimeFormat.ShortDatePattern = "ddd MMM";
                context.Settings[Keys.DateTimeDisplayCulture] = culture;
                DateTime dateTime = new DateTime(2000, 3, 1);

                // When
                string result = dateTime.ToShortDateString(context);

                // Then
                result.ShouldBe("mer. mars", StringCompareShould.IgnoreCase);
            }
        }

        public class ToLongDateStringTests : DateTimeCultureExtensionsFixture
        {
            [SetUp]
            public void SetThreadCulture()
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            }

            [Test]
            public void IncludesNameOfGermanDayAndMonth()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.DateTimeDisplayCulture] = CultureInfo.GetCultureInfo("de-DE");
                DateTime dateTime = new DateTime(2000, 3, 1);

                // When
                string result = dateTime.ToLongDateString(context);

                // Then
                Assert.That(result, Is.EqualTo("Mittwoch, 1. März 2000"));
            }
        }
    }
}
