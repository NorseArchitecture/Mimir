using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Norse.Abstractions.Contracts;

namespace Norse.Reference.Components.Tests;

public sealed class CountryLookupTests : BunitContext
{
	public CountryLookupTests()
	{
		Services.AddFluentUIComponents();
		// FluentUI components make JS interop calls bunit has no way to know about in advance —
		// loose mode is bunit's own documented answer, rather than hand-enumerating every internal
		// call FluentUI might make.
		JSInterop.Mode = JSRuntimeMode.Loose;
	}

	[Fact]
	async Task An_unrecognized_code_renders_the_validation_message()
	{
		var service = Substitute.For<IReferenceService>();
		service.GetCountry(Arg.Any<CountryRequest>(), Arg.Any<CancellationToken>())
			.Returns(_ => Task.FromResult<Outcome<CountryResponse>>(
				new Failed(Problem.ModelError(ErrorCategory.Validation, "Unparseable."))));
		Services.AddSingleton(service);

		var component = Render<CountryLookup>();
		await component.InvokeAsync(() => component.Find("fluent-button").Click());

		component.Markup.ShouldContain("is not a recognized ISO 3166-1 code");
	}

	[Fact]
	async Task A_parseable_but_unseeded_code_renders_the_not_found_message()
	{
		var service = Substitute.For<IReferenceService>();
		service.GetCountry(Arg.Any<CountryRequest>(), Arg.Any<CancellationToken>())
			.Returns(_ => Task.FromResult<Outcome<CountryResponse>>(
				new Failed(Problem.ModelError(ErrorCategory.NotFound, "No row."))));
		Services.AddSingleton(service);

		var component = Render<CountryLookup>();
		await component.InvokeAsync(() => component.Find("fluent-button").Click());

		component.Markup.ShouldContain("parsed, but no seeded country matches it");
	}

	[Fact]
	async Task A_successful_lookup_renders_the_wire_response_fields()
	{
		var id = Guid.NewGuid();
		var service = Substitute.For<IReferenceService>();
		service.GetCountry(Arg.Any<CountryRequest>(), Arg.Any<CancellationToken>())
			.Returns(_ => Task.FromResult(Outcome<CountryResponse>.Ok(new CountryResponse
			{
				Id = id,
				Alpha2 = "US",
				Alpha3 = "USA",
				Name = "United States of America"
			})));
		Services.AddSingleton(service);

		var component = Render<CountryLookup>();
		await component.InvokeAsync(() => component.Find("fluent-button").Click());

		component.Markup.ShouldContain("USA");
		component.Markup.ShouldContain("United States of America");
		component.Markup.ShouldContain(id.ToString());
	}
}
