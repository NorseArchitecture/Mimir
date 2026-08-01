namespace Norse.Reference;

/// <summary>
/// Named authorization policies for the reference-data service surface. <see cref="Public"/> is
/// satisfied by any principal, anonymous-role cookie included — reference lookups carry no
/// per-user semantics, but the mediator's registration generator enforces NORSE011 (every request
/// names its policy, no exceptions) uniformly across the platform, so a genuinely public read
/// still declares one, same as <c>Norse.AuthN.Services.AuthNPolicies.Public</c>.
/// </summary>
public static class ReferencePolicies
{
	/// <summary>Satisfied by any authenticated-or-anonymous-cookie principal — no real requirement.</summary>
	public const string Public = "Reference.Public";
}
