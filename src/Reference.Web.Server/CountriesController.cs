using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Norse.Abstractions.Contracts;
using Norse.Abstractions.Web.Server.Facade;

namespace Norse.Reference.Web.Server;

/// <summary>
///     The realm's REST ambassador desk for country resolution (Futhark §4, residence per the
///     2026-08-09 amendment: facade controllers live in the owning realm's <c>Web.Server</c>, beside the
///     service they front — never in Yggdrasil, which is a proving ground, not a home). Injects
///     <see cref="IReferenceService" /> directly and runs it in-process — no protobuf on the path, the
///     same mediator pipeline (validation, authorization, telemetry) underneath as the gRPC leg — and
///     carries zero wire-format machinery of its own: rendering belongs to the host-registered
///     formatters (NORSE070's standing law; the residence rule's actual substance).
///     Assigning the route-bound code to <see cref="CountryRequest.CodeInput" /> is this channel's
///     boundary-crossing parse event — the buffer's setter mints the <c>Result&lt;IsoCountryCode&gt;</c>
///     stamp exactly as protobuf deserialization does on the gRPC leg; the handler judges the same
///     verdict regardless of which door the request came through.
/// </summary>
[Route("api/reference/countries")]
public sealed class CountriesController(IReferenceService referenceService) : GrpcControllerBase
{
	/// <summary>
	///     Resolves the full country document by any of the four accepted input forms: ISO 3166-1
	///     alpha-2, alpha-3, M49 numeric (padded or unpadded), or the baked deterministic v5 identifier.
	/// </summary>
	/// <param name="code">The country code or identifier, in any accepted form.</param>
	/// <param name="cancellationToken">Cancels the in-process operation.</param>
	/// <returns>200 with the resolved document; 404 when no seeded country matches; 400 problem details for unrecognized input.</returns>
	[HttpGet("{code}")]
	[Authorize(Policy = ReferencePolicies.Public)]
	public Task<ActionResult<CountryResponse>> GetCountry(string code, CancellationToken cancellationToken) =>
		FoldAsync(new ValueTask<Outcome<CountryResponse>>(
			referenceService.GetCountry(new() { CodeInput = code }, cancellationToken)));
}
