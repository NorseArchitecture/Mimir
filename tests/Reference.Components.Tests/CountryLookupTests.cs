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
	async Task An_unrecognized_code_renders_locally_and_never_buys_a_round_trip()
	{
		var service = Substitute.For<IReferenceService>();
		Services.AddSingleton(service);

		var component = Render<CountryLookup>();
		await component.Find("fluent-text-input").ChangeAsync("banana");
		await component.InvokeAsync(() => component.Find("fluent-button").Click());

		component.Markup.ShouldContain("is not a recognized ISO 3166-1 code");
		// The stamp is the client's own verdict — unproven input never reaches the wire.
		await service.DidNotReceive().GetCountry(Arg.Any<CountryRequest>(), Arg.Any<CancellationToken>());
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
		await component.Find("fluent-text-input").ChangeAsync("US");
		await component.InvokeAsync(() => component.Find("fluent-button").Click());

		component.Markup.ShouldContain("parsed, but no seeded country matches it");
	}

	[Fact]
	async Task A_successful_lookup_renders_the_wire_fields_and_the_baked_id_matches()
	{
		var bakedId = Iso3166.Ids[IsoCountryCode.UnitedStatesOfAmerica];
		var service = Substitute.For<IReferenceService>();
		service.GetCountry(Arg.Any<CountryRequest>(), Arg.Any<CancellationToken>())
			.Returns(_ => Task.FromResult(Outcome<CountryResponse>.Ok(new CountryResponse
			{
				Id = bakedId,
				Alpha2 = "US",
				Alpha3 = "USA",
				Name = "United States of America",
				Code = IsoCountryCode.UnitedStatesOfAmerica,
				Classification = Classification.None,
				Region = new RegionResponse
				{
					Id = Guid.NewGuid(),
					Code = "019",
					Name = "Americas",
					Subregion = new SubregionResponse
					{
						Id = Guid.NewGuid(),
						Code = "021",
						Name = "Northern America"
					}
				}
			})));
		Services.AddSingleton(service);

		var component = Render<CountryLookup>();
		await component.Find("fluent-text-input").ChangeAsync("US");
		await component.InvokeAsync(() => component.Find("fluent-button").Click());

		component.Markup.ShouldContain("USA");
		component.Markup.ShouldContain("United States of America");
		component.Markup.ShouldContain("Americas → Northern America");
		component.Markup.ShouldContain(bakedId.ToString());
		// The wire id and the client's own baked copy of the same row land identical — the demo's
		// whole point, now read straight off the proven stamp with no client-side re-parse.
		component.Markup.ShouldContain("Match");
	}
}
