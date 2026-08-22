using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Norse.Abstractions.Components.Authorization;

namespace Norse.Reference.Web.Server.Tests;

public sealed class PolicyDeclarationTests
{
	static MethodInfo Declaration(string name) =>
		typeof(ReferencePolicyDeclarations).GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Single(m => m.GetCustomAttribute<NorsePolicyAttribute>()?.Name == name);

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
