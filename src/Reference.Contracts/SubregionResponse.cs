using System.Runtime.Serialization;

namespace Norse.Reference;

/// <summary>The Subregion ancestor nested within a <see cref="RegionResponse" />.</summary>
[DataContract]
public sealed record SubregionResponse
{
	/// <summary>The Subregion's deterministic v5 identifier.</summary>
	[DataMember(Order = 1)]
	public required Guid Id { get; init; }

	/// <summary>The Subregion's UN M49 code.</summary>
	[DataMember(Order = 2)]
	public required string Code { get; init; }

	/// <summary>The Subregion's name.</summary>
	[DataMember(Order = 3)]
	public required string Name { get; init; }

	/// <summary>The Intermediate Region beneath this Subregion, if one exists.</summary>
	[DataMember(Order = 4)]
	public IntermediateRegionResponse? IntermediateRegion { get; init; }
}
