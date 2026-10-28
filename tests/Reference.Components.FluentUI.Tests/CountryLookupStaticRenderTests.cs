using Bunit;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Norse.Reference.Components.FluentUI.Tests;

// Derives from BunitContext, not InteractiveBunitContext: that base would overwrite the static
// renderer these facts are about.
public sealed class CountryLookupStaticRenderTests : BunitContext
{
	readonly IReferenceService _service = Substitute.For<IReferenceService>();

	// The prerendered form a user can reach before Blazor boots (or for good, if the interactive
	// runtime never starts): its model was never bound from the post, so a submit would dispatch the
	// initial empty code. Touching Renderer seals the service collection, so it comes last.
	public CountryLookupStaticRenderTests()
	{
		Services.AddFluentUIComponents();
		JSInterop.Mode = JSRuntimeMode.Loose;
		Services
			.AddScoped<IValidator<CountryRequest>, CountryRequestValidator>()
			.AddSingleton(_service);
		SetRendererInfo(new("Static", false));
	}

	[Fact]
	void A_statically_rendered_form_offers_no_submit()
	{
		var component = Render<CountryLookup>();

		component.Find("fluent-button[type='submit']").HasAttribute("disabled").ShouldBeTrue();
	}

	[Fact]
	async Task A_statically_rendered_submit_never_calls_the_service_and_asks_the_user_to_retry()
	{
		var component = Render<CountryLookup>();

		await component.Find("form").SubmitAsync();

		await _service.DidNotReceiveWithAnyArgs().GetCountry(default!, Xunit.TestContext.Current.CancellationToken);
		component.Find(".validation-errors").TextContent.Trim().ShouldBe("This page hasn't finished loading — try again.");
	}
}
