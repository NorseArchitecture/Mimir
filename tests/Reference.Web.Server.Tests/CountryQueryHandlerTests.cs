using System.Linq.Expressions;
using Norse.Abstractions.Backend;
using Norse.Abstractions.Contracts;
using Norse.Primitives;
using Norse.Reference.Data;

namespace Norse.Reference.Web.Server.Tests;

public sealed class CountryQueryHandlerTests
{
	[Fact]
	async Task A_parseable_code_resolves_the_id_locally_and_projects_from_the_repository()
	{
		var expectedId = Iso3166.Ids[IsoCountryCode.UnitedStatesOfAmerica];
		var expectedResponse = new CountryResponse { Id = expectedId, Alpha2 = "US", Alpha3 = "USA", Name = "United States of America" };
		var repository = Substitute.For<IReadRepository<CountryOrAreaView>>();
		repository.GetAsync(expectedId, Arg.Any<Expression<Func<CountryOrAreaView, CountryResponse>>>(), Arg.Any<CancellationToken>())
			.Returns(Outcome<CountryResponse>.Ok(expectedResponse));
		CountryQueryHandler handler = new(repository);

		var outcome = await handler.Handle(new CountryQuery("US"), TestContext.Current.CancellationToken);

		outcome.TryGetValue(out Success<CountryResponse> success).ShouldBeTrue();
		success.Value.ShouldBe(expectedResponse);
		// Zero DB involvement in identity resolution: the exact id below is Iso3166.Ids' locally
		// computed value, never anything the repository itself produced or validated.
		await repository.Received(1).GetAsync(expectedId, Arg.Any<Expression<Func<CountryOrAreaView, CountryResponse>>>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	async Task Garbage_fails_validation_with_the_offending_code_in_problem_detail()
	{
		var repository = Substitute.For<IReadRepository<CountryOrAreaView>>();
		CountryQueryHandler handler = new(repository);

		var outcome = await handler.Handle(new CountryQuery("banana"), TestContext.Current.CancellationToken);

		outcome.TryGetValue(out Failed failed).ShouldBeTrue();
		failed.Problem.Category.ShouldBe(ErrorCategory.Validation);
		failed.Problem.Errors["code"].ShouldContain("banana");
		await repository.DidNotReceive().GetAsync(
			Arg.Any<Guid>(), Arg.Any<Expression<Func<CountryOrAreaView, CountryResponse>>>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	async Task A_repository_not_found_flows_through_unchanged()
	{
		var expectedId = Iso3166.Ids[IsoCountryCode.UnitedStatesOfAmerica];
		var repository = Substitute.For<IReadRepository<CountryOrAreaView>>();
		repository.GetAsync(expectedId, Arg.Any<Expression<Func<CountryOrAreaView, CountryResponse>>>(), Arg.Any<CancellationToken>())
			.Returns(Outcome<CountryResponse>.Err(ErrorCategory.NotFound));
		CountryQueryHandler handler = new(repository);

		var outcome = await handler.Handle(new CountryQuery("US"), TestContext.Current.CancellationToken);

		outcome.TryGetValue(out Failed failed).ShouldBeTrue();
		failed.Problem.Category.ShouldBe(ErrorCategory.NotFound);
	}
}
