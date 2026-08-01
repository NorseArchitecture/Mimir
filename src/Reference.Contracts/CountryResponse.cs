using System.Runtime.Serialization;

namespace Norse.Reference;

/// <summary>The resolved country: its deterministic identity plus the canonical codes and English name.</summary>
[DataContract]
public sealed record CountryResponse
{
	/// <summary>The deterministic v5 identifier — recomputable client-side from <see cref="MimirNamespaces.Iso3166"/> and the zero-padded numeric code.</summary>
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
}
