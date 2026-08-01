using System.Globalization;
using Norse.Primitives.Identifiers;

namespace Norse.Reference.Contracts.Tests;

public sealed class NamespaceSelfVerificationTests
{
	[Fact]
	void The_iso3166_namespace_rechains_from_root() =>
		new DeterministicGuid(MimirNamespaces.Root, "iso3166-1").Value.ShouldBe(MimirNamespaces.Iso3166);

	[Fact]
	void Every_shipped_row_guid_recomputes_via_deterministic_guid()
	{
		foreach (var country in Iso3166.All)
			new DeterministicGuid(MimirNamespaces.Iso3166, ((ushort)country.Code).ToString("D3", CultureInfo.InvariantCulture))
				.Value.ShouldBe(country.Id, $"{country.Alpha3} drifted");
	}

	[Fact]
	void The_dataset_carries_every_iso_bearing_row() =>
		// Arithmetic, verified against the committed export 2026-07-31: the raw file is 249 lines
		// = 1 header + 248 data rows, and ZERO rows lack ISO alpha codes — so the ISO-bearing
		// count equals the data-row count exactly. If this assertion ever fails, the EXPORT
		// changed (a UNSD reissue): re-run the arithmetic against the new file and update this
		// number with the new count-minus-ISO-less breakdown in this comment — never edit the
		// number to whatever passes.
		Iso3166.All.Count.ShouldBe(248);
}
