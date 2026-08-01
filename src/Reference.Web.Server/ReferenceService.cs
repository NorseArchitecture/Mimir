using Microsoft.AspNetCore.Authorization;
using Norse.Abstractions.Contracts;
using Norse.Abstractions.Web.Server.Mediator;

namespace Norse.Reference.Web.Server;

/// <summary>
/// The reference backend for Mímir's <see cref="IReferenceService"/> contract. Pure hydrate-and-send:
/// the method wraps the incoming pure wire DTO's <see cref="CountryRequest.Code"/> in the
/// server-sovereign <see cref="CountryQuery"/> and sends it through Midgard's mediator pipeline
/// (validation-as-parse, authorization, telemetry, exception translation) via Asgard's
/// <see cref="ISender"/> — the handler's payload type <em>is</em> the wire result type, so egress is
/// pure passthrough, no mapping switch. Mímir stays Midgard-blind: it depends only on Asgard's
/// mediator contracts, never on Midgard's pipeline implementation. The one throw point in the whole
/// chain is the gRPC server interceptor (Midgard's <c>OutcomeServerInterceptor</c>), pattern-matching
/// the returned envelope at the transport boundary, never here. Public: Yggdrasil's composition root
/// maps this type directly.
///
/// <c>[Authorize]</c> is mirrored from the interface arm onto this method deliberately, not
/// redundantly — ASP.NET Core's gRPC endpoint metadata is gathered by reflecting on this concrete
/// runtime type, not the interface it implements; an interface method's attributes are not visible
/// to that discovery. Without this mirror, decided law item 4's "enforced on every channel" claim is
/// false for the wire channel specifically, even though <see cref="CountryQuery"/> declares the
/// policy correctly for the mediator pipeline's own <c>AuthorizationBehavior</c>.
/// </summary>
public sealed class ReferenceService(ISender sender) : IReferenceService
{
	/// <inheritdoc />
	[Authorize(Policy = ReferencePolicies.Public)]
	public Task<Outcome<CountryResponse>> GetCountry(CountryRequest request, CancellationToken cancellationToken = default) =>
		sender.Send(new CountryQuery(request.Code), cancellationToken).AsTask();
}
