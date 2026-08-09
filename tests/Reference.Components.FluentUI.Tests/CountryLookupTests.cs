using Bunit;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Norse.Abstractions.Contracts;

namespace Norse.Reference.Components.FluentUI.Tests;

public sealed class CountryLookupTests : BunitContext
{
	public CountryLookupTests()
	{
		Services.AddFluentUIComponents();
		Services.AddScoped<IValidator<CountryRequest>, CountryRequestValidator>();
		// FluentUI components make JS interop calls bunit has no way to know about in advance —
		// loose mode is bunit's own documented answer, rather than hand-enumerating every internal
		// call FluentUI might make.
		JSInterop.Mode = JSRuntimeMode.Loose;
	}

	[Fact]
	async Task An_unrecognized_code_fails_client_validation_and_never_buys_a_round_trip()
	{
		var service = Substitute.For<IReferenceService>();
		Services.AddSingleton(service);

		var component = Render<CountryLookup>();
		await component.Find("fluent-text-input").ChangeAsync("banana");
		await component.Find("form").SubmitAsync();

		component.Markup.ShouldContain("Not a recognized ISO 3166-1 code or identifier.");
		// The rule registers on the stamp, the stamp is the client's own verdict — Blazilla's
		// submit pass gates on it, so unproven input never reaches the wire.
		await service.DidNotReceive().GetCountry(Arg.Any<CountryRequest>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	async Task A_parseable_but_unseeded_code_renders_the_server_error_through_the_form()
	{
		var service = Substitute.For<IReferenceService>();
		service.GetCountry(Arg.Any<CountryRequest>(), Arg.Any<CancellationToken>())
			.Returns(_ => Task.FromResult(Outcome<CountryResponse>.Err(ErrorCategory.NotFound)));
		Services.AddSingleton(service);

		var component = Render<CountryLookup>();
		await component.Find("fluent-text-input").ChangeAsync("US");
		await component.Find("form").SubmitAsync();

		// A field-less NotFound renders the ServerValidation bridge's category sentence at the
		// model level — machinery-owned wording, not the component's.
		component.Markup.ShouldContain("The requested resource couldn't be found.");
	}

	[Fact]
	async Task A_successful_lookup_renders_the_full_document_and_the_baked_id_matches()
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
		await component.Find("form").SubmitAsync();

		component.Markup.ShouldContain("USA");
		component.Markup.ShouldContain("United States of America");
		component.Markup.ShouldContain("Americas → Northern America");
		component.Markup.ShouldContain(bakedId.ToString());
		// The wire id and the client's own baked copy of the same row land identical — read back
		// through the response's Code, no client-side re-parse, no second format authority.
		component.Markup.ShouldContain("Match");
	}
}
