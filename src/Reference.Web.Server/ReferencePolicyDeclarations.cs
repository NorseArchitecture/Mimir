using Microsoft.AspNetCore.Authorization;
using Norse.Abstractions.Web.Server.Authorization;

namespace Norse.Reference.Web.Server;

/// <summary>
///     Declares the reference surface's authorization policies. Lives server-side because a policy's shape
///     needs <see cref="AuthorizationPolicyBuilder" />, and <c>Reference.Contracts</c> is thin by law — the
///     name constant stays there, the shape lives here, and the generator joins them.
/// </summary>
public static class ReferencePolicyDeclarations
{
	/// <summary>Configures <see cref="ReferencePolicies.Public" />.</summary>
	/// <param name="policy">The builder to configure.</param>
	[NorsePolicy(ReferencePolicies.Public)]
	public static void ConfigurePublic(AuthorizationPolicyBuilder policy) =>
		policy.RequireAuthenticatedUser();
}
