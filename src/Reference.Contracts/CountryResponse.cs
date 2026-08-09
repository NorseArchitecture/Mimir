using System.Runtime.Serialization;

namespace Norse.Reference;

/// <summary>
///     The resolved country: its deterministic identity, the canonical codes and English name, the UN
///     classification flags, and the full M49 ancestry chain — the entire
///     <c>CountryOrAreaView</c> document, not a scalar skim.
/// </summary>
[DataContract]
public sealed record CountryResponse
{
	/// <summary>
	///     The deterministic v5 identifier — precomputed at generation time from the ISO 3166-1 dataset namespace (
	///     <c>ReferenceNamespaces.Iso3166</c>, published by Norse.Reference.Data.Namespaces) and the zero-padded numeric code.
	///     Recomputation is a server/tooling/tests act, never the client's.
	/// </summary>
	[DataMember(Order = 1)]
	public required Guid Id { get; init; }

	/// <summary>The ISO 3166-1 alpha-2 code.</summary>
	[DataMember(Order = 2)]
	public required string Alpha2 { get; init; }

	/// <summary>The ISO 3166-1 alpha-3 code.</summary>
	[DataMember(Order = 3)]
	public required string Alpha3 { get; init; }

	/// <summary>The English short name.</summary>
	[DataMember(Order = 4)]
	public required string Name { get; init; }

	/// <summary>The ISO 3166-1 identifier itself — the M49 numeric code as the enum's underlying value.</summary>
	[DataMember(Order = 5)]
	public required IsoCountryCode Code { get; init; }

	/// <summary>The UN classification flags this country or area holds.</summary>
	[DataMember(Order = 6)]
	public required Classification Classification { get; init; }

	/// <summary>The ancestor Region chain, if the country resolves through one — absent only for Antarctica.</summary>
	[DataMember(Order = 7)]
	public RegionResponse? Region { get; init; }
}
