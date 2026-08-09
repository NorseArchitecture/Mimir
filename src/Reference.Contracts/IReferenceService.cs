using System.ServiceModel;
using Norse.Abstractions.Contracts;

namespace Norse.Reference;

/// <summary>
///     Read surface — real, network-callable gRPC methods that resolve canonical reference data.
///     The <c>CancellationToken</c> parameter rides every method so components can cancel operations
///     without a gateway wrapper.
///     Every request/response type here is a pure <c>[DataContract]</c> wire shape — no mediator
///     marker, no <c>[Authorize]</c>. Reference lookups are public/anonymous by nature (no per-user
///     semantics, nothing to enumerate), but the platform's mediator law still names a policy on the
///     server-sovereign request type (<see cref="ReferencePolicies.Public" />, decided law item 4 —
///     the same "every request names its policy, even one that imposes no real requirement" rule
///     Heimdall's <c>AuthNPolicies.Public</c> follows) — that policy lives entirely server-side, on
///     Mímir's <c>CountryQuery</c> wrapper; this assembly never references
///     <c>Abstractions.Web.Server</c>, keeping the WASM footprint lean. The concrete
///     <c>ReferenceService</c> implementation mirrors <see cref="ReferencePolicies.Public" /> on its own
///     methods purely for gRPC endpoint metadata; that mirror is the only place the policy name touches
///     the wire tier at all.
///     Every method returns <see cref="Outcome{T}" /> directly (spec §9, 2026-07-24 amendment to decided
///     law item 3) — the envelope <em>is</em> the wire method signature. Nothing in-process throws to
///     communicate a business failure; the one throw point in the whole chain is the gRPC server
///     interceptor (Midgard), pattern-matching the returned <see cref="Outcome{T}" /> at the transport
///     boundary, never here.
/// </summary>
[ServiceContract(Name = "grpc.reference.v1.ReferenceService")]
public interface IReferenceService
{
	/// <summary>
	///     Resolves the full country document — codes, name, classification flags, and the M49
	///     ancestry chain — by any of the four accepted input forms: ISO 3166-1 alpha-2, alpha-3,
	///     M49 numeric (padded or unpadded), or the baked deterministic v5 identifier.
	/// </summary>
	[OperationContract]
	Task<Outcome<CountryResponse>> GetCountry(CountryRequest request, CancellationToken cancellationToken = default);
}
