using System.Runtime.Serialization;

namespace Norse.Reference;

/// <summary>A country lookup by any of the three ISO 3166-1 code forms (numeric incl. unpadded, alpha-2, alpha-3).</summary>
[DataContract]
public sealed record CountryRequest
{
	/// <summary>The code to resolve.</summary>
	[DataMember(Order = 1)]
	public required string Code { get; init; }
}
