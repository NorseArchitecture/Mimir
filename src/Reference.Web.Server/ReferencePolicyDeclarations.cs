using Microsoft.AspNetCore.Authorization;
using Norse.Abstractions.Components.Authorization;

namespace Norse.Reference.Web.Server;

/// <summary>
///     Declares the reference surface's authorization policies. Lives server-side because a policy's shape
///     needs <see cref="AuthorizationPolicyBuilder" />, and <c>Reference.Contracts</c> is thin by law — the
///     name constant stays there, the shape lives here, and the generator joins them.
/// </summary>
public static class ReferencePolicyDeclarations
{
	/// <summary>
	///     Configures <see cref="ReferencePolicies.Public" />. Reference reads are the one operation policy a machine
	///     principal shares with the identity cookie, so the lanes are named explicitly rather than left to the generated
	///     default (which would close the policy to <see cref="NorseLanes.Identity" /> alone). The REST facade still
	///     requires <see cref="NorseLanes.Machine" /> and the facade audience; gRPC never selects the machine lane.
	/// </summary>
	/// <param name="policy">The builder to configure.</param>
	[NorsePolicy(ReferencePolicies.Public)]
	public static void ConfigurePublic(AuthorizationPolicyBuilder policy) =>
		policy.RequireAuthenticatedUser().RequireLane(NorseLanes.Identity, NorseLanes.Machine);
}
