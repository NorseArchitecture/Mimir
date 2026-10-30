using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Norse.Abstractions.Components.Authorization;
using Norse.Infrastructure.Web.Server.Authentication;

namespace Norse.Reference.Web.Server.Tests;

public sealed class PolicyDeclarationTests
{
	static MethodInfo Declaration(string name) =>
		typeof(ReferencePolicyDeclarations).GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Single(m => m.GetCustomAttribute<NorsePolicyAttribute>()?.Name == name);

	// The registered JWT audience claim name, carried unchanged onto a Midgard-stamped bearer identity.
	const string AudienceClaimType = "aud";

	// Registered the way Midgard's generated AddNorsePolicies() registers a declaration (configure, then the default
	// lane closure) and evaluated against the handlers a real host gets from AddNorseAuthentication().
	static bool Admits(ClaimsPrincipal principal)
	{
		ServiceCollection services = new();
		services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
		services.AddLogging();
		services.AddAuthorizationCore(options => options.AddPolicy(ReferencePolicies.Public, policy =>
		{
			Declaration(ReferencePolicies.Public).Invoke(null, [policy]);
			policy.CloseLaneByDefault();
		}));
		services.AddNorseAuthentication();
		using var provider = services.BuildServiceProvider();
		return provider.GetRequiredService<IAuthorizationService>()
			.AuthorizeAsync(principal, null, ReferencePolicies.Public)
			.GetAwaiter()
			.GetResult()
			.Succeeded;
	}

	// The shape Midgard's lane handlers hand to policy evaluation: one identity, typed by the lane that produced it.
	static ClaimsPrincipal Stamped(string lane, params Claim[] claims) =>
		new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "subject"), .. claims], lane));

	[Fact]
	void Reference_public_admits_the_identity_cookie() =>
		Admits(Stamped(NorseLanes.Identity)).ShouldBeTrue();

	[Fact]
	void Reference_public_admits_a_machine_principal() =>
		Admits(Stamped(NorseLanes.Machine, new Claim(AudienceClaimType, NorseAudiences.Facade))).ShouldBeTrue();

	[Fact]
	void Reference_public_refuses_a_user_bearer() =>
		Admits(Stamped(NorseLanes.User, new Claim(AudienceClaimType, NorseAudiences.User))).ShouldBeFalse();

	[Fact]
	void Reference_public_refuses_an_empty_principal() =>
		Admits(new ClaimsPrincipal(new ClaimsIdentity())).ShouldBeFalse();

	[Fact]
	void Reference_public_names_its_lanes_explicitly() =>
		BuildPolicy().Requirements.OfType<LaneRequirement>()
			.ShouldHaveSingleItem().Lanes.ShouldBe([NorseLanes.Identity, NorseLanes.Machine], ignoreOrder: true);

	static AuthorizationPolicy BuildPolicy()
	{
		AuthorizationPolicyBuilder builder = new();
		Declaration(ReferencePolicies.Public).Invoke(null, [builder]);
		return builder.Build();
	}

	[Fact]
	void Declares_the_reference_public_policy_in_metadata() =>
		Should.NotThrow(() => Declaration(ReferencePolicies.Public));

	[Fact]
	void The_public_policy_requires_a_principal()
	{
		AuthorizationPolicyBuilder builder = new();
		Declaration(ReferencePolicies.Public).Invoke(null, [builder]);

		builder.Build().Requirements.ShouldContain(r => r is DenyAnonymousAuthorizationRequirement);
	}

	[Fact]
	void The_declaration_carries_the_generator_visible_signature()
	{
		var method = Declaration(ReferencePolicies.Public);

		method.ReturnType.ShouldBe(typeof(void));
		method.GetParameters().Select(p => p.ParameterType).ShouldBe([typeof(AuthorizationPolicyBuilder)]);
	}
}
