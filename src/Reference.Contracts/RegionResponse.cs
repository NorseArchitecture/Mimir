using System.Runtime.Serialization;

namespace Norse.Reference;

/// <summary>The Region ancestor of a resolved country — the top of the M49 ancestry chain.</summary>
[DataContract]
public sealed record RegionResponse
{
	/// <summary>The Region's deterministic v5 identifier.</summary>
	[DataMember(Order = 1)]
	public required Guid Id { get; init; }

	/// <summary>The Region's UN M49 code.</summary>
	[DataMember(Order = 2)]
	public required string Code { get; init; }

	/// <summary>The Region's name.</summary>
	[DataMember(Order = 3)]
	public required string Name { get; init; }

	/// <summary>The Subregion beneath this Region, if the country resolved through one.</summary>
	[DataMember(Order = 4)]
	public SubregionResponse? Subregion { get; init; }
}
