using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Norse.Abstractions.Contracts;
using Norse.Primitives;

namespace Norse.Reference.Web.Server.Tests;

public sealed class CountriesControllerTests
{
	[Fact]
	async Task The_route_bound_code_hydrates_the_request_buffer_and_the_stamp_mints_on_assignment()
	{
		CountryRequest? seen = null;
		var service = Substitute.For<IReferenceService>();
		service.GetCountry(Arg.Do<CountryRequest>(r => seen = r), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<Outcome<CountryResponse>>(new Failed(
				Problem.ModelError(ErrorCategory.NotFound, "No row."))));
		CountriesController controller = new(service);

		await controller.GetCountry("US", TestContext.Current.CancellationToken);

		// The facade is a dumb door: it assigns the buffer (the parse event) and forwards — the
		// stamp must arrive at the service already minted, verdict and all.
		seen.ShouldNotBeNull();
		seen.CodeInput.ShouldBe("US");
		seen.Code.TryGetValue(out Success<IsoCountryCode> parsed).ShouldBeTrue();
		parsed.Value.ShouldBe(IsoCountryCode.UnitedStatesOfAmerica);
	}

	[Fact]
	async Task A_successful_resolution_folds_to_ok_with_the_wire_payload()
	{
		CountryResponse response = new()
		{
			Id = Iso3166.Ids[IsoCountryCode.UnitedStatesOfAmerica],
			Alpha2 = "US",
			Alpha3 = "USA",
			Name = "United States of America",
			Code = IsoCountryCode.UnitedStatesOfAmerica,
			Classification = Classification.None
		};
		var service = Substitute.For<IReferenceService>();
		service.GetCountry(Arg.Any<CountryRequest>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Outcome<CountryResponse>.Ok(response)));
		CountriesController controller = new(service);

		var result = await controller.GetCountry("US", TestContext.Current.CancellationToken);

		var ok = result.Result.ShouldBeOfType<OkObjectResult>();
		ok.Value.ShouldBe(response);
	}

	[Fact]
	async Task A_not_found_outcome_folds_to_a_bodiless_404()
	{
		var service = Substitute.For<IReferenceService>();
		service.GetCountry(Arg.Any<CountryRequest>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<Outcome<CountryResponse>>(new Failed(
				Problem.ModelError(ErrorCategory.NotFound, "No row."))));
		CountriesController controller = new(service);

		var result = await controller.GetCountry("XX", TestContext.Current.CancellationToken);

		result.Result.ShouldBeOfType<NotFoundResult>();
	}

	[Fact]
	async Task A_validation_outcome_folds_to_problem_details_with_the_rfc_9457_media_types()
	{
		var service = Substitute.For<IReferenceService>();
		service.GetCountry(Arg.Any<CountryRequest>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Outcome<CountryResponse>.Err(ErrorCategory.Validation,
				new Dictionary<string, string[]> { ["code"] = ["banana"] })));
		CountriesController controller = new(service)
		{
			// ControllerBase.Problem reads ProblemDetailsFactory off the request services; a bare
			// controller has no HttpContext, so give it the minimal one.
			ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
		};

		var result = await controller.GetCountry("banana", TestContext.Current.CancellationToken);

		var problemResult = result.Result.ShouldBeOfType<ObjectResult>();
		problemResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
		problemResult.ContentTypes.ShouldContain("application/problem+json");
		problemResult.ContentTypes.ShouldContain("application/problem+xml");
	}
}
