using System.Runtime.Serialization;

namespace Norse.Reference;

/// <summary>The Intermediate Region ancestor nested within a <see cref="SubregionResponse" />.</summary>
[DataContract]
public sealed record IntermediateRegionResponse
{
	/// <summary>The Intermediate Region's deterministic v5 identifier.</summary>
	[DataMember(Order = 1)]
	public required Guid Id { get; init; }

	/// <summary>The Intermediate Region's UN M49 code.</summary>
	[DataMember(Order = 2)]
	public required string Code { get; init; }

	/// <summary>The Intermediate Region's name.</summary>
	[DataMember(Order = 3)]
	public required string Name { get; init; }
}
