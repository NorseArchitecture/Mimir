using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Norse.Abstractions.Contracts;
using Norse.Primitives;

namespace Norse.Reference.Web.Server.Tests;

public sealed class CountriesControllerTests
{
	// The action reads HttpContext.RequestAborted (and the validation fold reads
	// ProblemDetailsFactory off request services), so every bare controller gets the minimal
	// context — with the test framework's own token standing in as the aborted-request signal, so
	// cancellation keeps flowing end to end exactly as it did when the action took a parameter.
	static CountriesController Build(IReferenceService service) => new(service)
	{
		ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext { RequestAborted = TestContext.Current.CancellationToken }
		}
	};

	[Fact]
	async Task The_route_bound_code_hydrates_the_request_buffer_and_the_stamp_mints_on_assignment()
	{
		CountryRequest? seen = null;
		CancellationToken seenToken = default;
		var service = Substitute.For<IReferenceService>();
		service.GetCountry(Arg.Do<CountryRequest>(r => seen = r),
				Arg.Do<CancellationToken>(t => seenToken = t))
			.Returns(Task.FromResult<Outcome<CountryResponse>>(new Failed(
				Problem.ModelError(ErrorCategory.NotFound, "No row."))));
		var controller = Build(service);

		await controller.GetCountry("US");

		// The facade is a dumb door: it assigns the buffer (the parse event) and forwards — the
		// stamp must arrive at the service already minted, verdict and all.
		seen.ShouldNotBeNull();
		seen.CodeInput.ShouldBe("US");
		seen.Code.TryGetValue(out Success<IsoCountryCode> parsed).ShouldBeTrue();
		parsed.Value.ShouldBe(IsoCountryCode.UnitedStatesOfAmerica);
		// And the token the service saw is HttpContext.RequestAborted, not None — the action's
		// switch off the parameter didn't sever cancellation.
		seenToken.ShouldBe(controller.HttpContext.RequestAborted);
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
		var controller = Build(service);

		var result = await controller.GetCountry("US");

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
		var controller = Build(service);

		var result = await controller.GetCountry("XX");

		// Not NotFoundResult: GrpcControllerBase.ToResult folds every body-forbidding category
		// (NotFound included, alongside Unauthorized/InvalidCredentials) to a bare StatusCodeResult
		// via TransportDispositions -- deliberately indistinguishable by result type from the other
		// silent categories, so a 404 can't be told apart from a 401/403 at this layer either.
		var statusResult = result.Result.ShouldBeOfType<StatusCodeResult>();
		statusResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
	}

	[Fact]
	async Task A_validation_outcome_folds_to_problem_details_with_the_rfc_9457_media_types()
	{
		var service = Substitute.For<IReferenceService>();
		service.GetCountry(Arg.Any<CountryRequest>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Outcome<CountryResponse>.Err(ErrorCategory.Validation,
				new Dictionary<string, string[]> { ["code"] = ["banana"] })));
		var controller = Build(service);

		var result = await controller.GetCountry("banana");

		var problemResult = result.Result.ShouldBeOfType<ObjectResult>();
		problemResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
		problemResult.ContentTypes.ShouldContain("application/problem+json");
		problemResult.ContentTypes.ShouldContain("application/problem+xml");
	}
}
