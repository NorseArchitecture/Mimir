using Microsoft.AspNetCore.Authorization;
using Norse.Abstractions.Web.Server.Mediator;
using Norse.Primitives;

namespace Norse.Reference.Web.Server;

/// <summary>
///     The mediator wrapper for a country lookup (server-only; the wire record stays mediator-blind).
///     Carries the wire-stamped scalar, not a raw string — the verdict was minted at deserialization
///     and travels proven or failed from there. <see cref="ReferencePolicies.Public" /> is satisfied
///     by any principal, anonymous-role cookie included — reference lookups carry no per-user
///     semantics, but the registration generator's NORSE011 check still requires every request to
///     name a policy (decided law item 4), same as Himinbjörg's <c>LoginCommand</c>/
///     <c>RegisterCommand</c>/<c>LogoutCommand</c>.
/// </summary>
[Authorize(Policy = ReferencePolicies.Public)]
sealed record CountryQuery(Result<IsoCountryCode> Code) : IQueryRequest<CountryResponse>;
