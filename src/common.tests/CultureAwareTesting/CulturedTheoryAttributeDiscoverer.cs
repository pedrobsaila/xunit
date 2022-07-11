﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

public class CulturedTheoryAttributeDiscoverer : TheoryDiscoverer
{
	protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestMethod testMethod,
		_IAttributeInfo theoryAttribute,
		ITheoryDataRow dataRow,
		object?[] testMethodArguments)
	{
		var cultures = GetCultures(theoryAttribute);
		var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod, dataRow, testMethodArguments: testMethodArguments, theoryAttribute);
		var result = cultures.Select(
			// TODO: How do we get source information in here?
			culture => new CulturedXunitTestCase(
				culture,
				details.ResolvedTestMethod,
				details.TestCaseDisplayName,
				details.UniqueID,
				details.Explicit,
				details.SkipReason,
				details.Traits,
				testMethodArguments,
				timeout: details.Timeout
			)
		).CastOrToReadOnlyCollection();

		return new(result);
	}

	protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestMethod testMethod,
		_IAttributeInfo theoryAttribute)
	{
		var cultures = GetCultures(theoryAttribute);
		var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod, theoryAttribute);

		var result =
			cultures
				.Select(
					// TODO: How do we get source information in here?
					culture => new CulturedXunitTheoryTestCase(
						culture,
						details.ResolvedTestMethod,
						details.TestCaseDisplayName,
						details.UniqueID,
						details.Explicit,
						details.SkipReason,
						details.Traits,
						timeout: details.Timeout
					)
				)
				.CastOrToReadOnlyCollection();

		return new(result);
	}

	static string[] GetCultures(_IAttributeInfo culturedTheoryAttribute)
	{
		var ctorArgs = culturedTheoryAttribute.GetConstructorArguments().ToArray();
		var cultures = Reflector.ConvertArguments(ctorArgs, new[] { typeof(string[]) }).Cast<string[]>().Single();

		if (cultures == null || cultures.Length == 0)
			cultures = new[] { "en-US", "fr-FR" };

		return cultures;
	}
}
