using System.Runtime.Serialization;
using Norse.Primitives;

namespace Norse.Reference;

/// <summary>
///     A country lookup by any of the three ISO 3166-1 code forms (numeric incl. unpadded, alpha-2,
///     alpha-3). Deliberately mutable — this is the direct two-way <c>EditForm</c> binding target for
///     <c>Reference.Components</c>' <c>CountryLookup</c>. A pure wire DTO — no mediator marker, no
///     <c>[Authorize]</c>; <c>Reference.Web.Server</c>'s server-sovereign <c>CountryQuery</c> wraps
///     the stamp for mediator identity.
/// </summary>
[DataContract]
public sealed record CountryRequest
{
	/// <summary>
	///     The code as a wire-stamped scalar — the serialized member. Non-nullable, so the field is
	///     required: the forge mints the verdict, the request declares the obligation (spec
	///     2026-08-08-wire-stamped-request-scalars). Deserialization is the parse event; the server
	///     holds its own verdict regardless of what the client claimed.
	/// </summary>
	[DataMember(Order = 1)]
	public Result<IsoCountryCode> Code { get; set; }

	/// <summary>
	///     The form's raw buffer — never serialized; assignment stamps <see cref="Code" />, so there
	///     is no code path that sets the text without refreshing the verdict. A client-side artifact:
	///     the sanctioned deserialization path never assigns it, so on the server it legitimately
	///     holds its empty default.
	/// </summary>
	public required string CodeInput
	{
		get;
		set
		{
			field = value;
			Code = IsoCountryCodes.Parse(value);
		}
	} = string.Empty;
}
