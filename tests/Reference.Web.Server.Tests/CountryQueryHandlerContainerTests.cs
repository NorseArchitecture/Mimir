using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Norse.Abstractions.Backend;
using Norse.Abstractions.Contracts;
using Norse.Infrastructure.Persistence.EntityFramework;
using Norse.Persistence.EntityFramework;
using Norse.Primitives;
using Norse.Primitives.Identifiers;
using Norse.Reference.Data.EntityFramework;

namespace Norse.Reference.Web.Server.Tests;

/// <summary>
/// Exercises the real well-repository query path (Midgard's <c>Repository&lt;TContext,TEntity,TView&gt;</c>,
/// <c>ViewSelector</c>/<c>WellMap</c>) against a real Postgres container, with zero gRPC/mediator
/// pipeline/Yggdrasil composition in the loop -- isolates whether a resolvable, well-formed row round
/// trips through <see cref="CountryQueryHandler"/> at all before any wire-level suspicion is warranted.
/// </summary>
[Collection("Postgres")]
public sealed class CountryQueryHandlerContainerTests(PostgresContainerFixture fixture)
{
	static async Task<IReadRepository<CountryOrAreaView>> BuildRepositoryAsync(string connectionString, CancellationToken cancellationToken)
	{
		ServiceCollection services = new();
		services.AddDbContextFactory<ReferenceDbContext>(o =>
		{
			o.UseNpgsql(connectionString);
			o.ApplyNorseConventions(NorseNameRewriters.LowerSnakeCase);
			o.ApplyNorseTrackingBehavior();
		});
		services.AddWell<ReferenceDbContext>();
		var provider = services.BuildServiceProvider();

		var factory = provider.GetRequiredService<IDbContextFactory<ReferenceDbContext>>();
		await using var context = await factory.CreateDbContextAsync(cancellationToken);
		await context.Database.EnsureCreatedAsync(cancellationToken);

		return provider.GetRequiredService<IReadRepository<CountryOrAreaView>>();
	}

	[Fact]
	async Task A_well_formed_row_seeded_exactly_like_the_real_seed_contributor_resolves_through_the_handler()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var repository = await BuildRepositoryAsync(fixture.ConnectionString, cancellationToken);

		// Mirrors ReferenceDataSeedContributor's own id formula exactly (Id = Iso3166.Ids[code],
		// View.Id the same) -- if this doesn't resolve, the bug is in the well/repository/view-shaper
		// path, not in Mimisbrunnr's seed data or Mimir's parse/lookup logic.
		DeterministicGuid id = new(Iso3166.Ids[IsoCountryCode.UnitedStatesOfAmerica]);
		ServiceCollection seedServices = new();
		seedServices.AddDbContextFactory<ReferenceDbContext>(o =>
		{
			o.UseNpgsql(fixture.ConnectionString);
			o.ApplyNorseConventions(NorseNameRewriters.LowerSnakeCase);
			o.ApplyNorseTrackingBehavior();
		});
		var seedFactory = seedServices.BuildServiceProvider().GetRequiredService<IDbContextFactory<ReferenceDbContext>>();
		await using (var context = await seedFactory.CreateDbContextAsync(cancellationToken))
		{
			context.Set<CountryOrArea>().Add(new()
			{
				Id = id,
				Code = IsoCountryCode.UnitedStatesOfAmerica,
				Alpha2 = "US",
				Alpha3 = "USA",
				Name = "United States of America",
				Classification = Classification.None,
				View = new()
				{
					Id = id,
					Code = IsoCountryCode.UnitedStatesOfAmerica,
					Alpha2 = "US",
					Alpha3 = "USA",
					Name = "United States of America",
					Classification = Classification.None,
				},
			});
			await context.SaveChangesAsync(cancellationToken);
		}

		try
		{
			CountryQueryHandler handler = new(repository);
			var outcome = await handler.Handle(new CountryQuery("US"), cancellationToken);

			var isOk = outcome.TryGetValue(out Success<CountryResponse> success);
			var isFailed = outcome.TryGetValue(out Failed failed);
			(isOk ? "OK" : isFailed ? $"FAILED:{failed.Problem.Category}" : "?").ShouldBe("OK");
			success.Value.Alpha2.ShouldBe("US");
			success.Value.Id.ShouldBe(id);
		}
		finally
		{
			await using var context = await seedFactory.CreateDbContextAsync(cancellationToken);
			await context.Set<CountryOrArea>().Where(c => c.Id == id).ExecuteDeleteAsync(cancellationToken);
		}
	}

	[Fact]
	async Task A_row_with_the_real_two_level_region_hierarchy_resolves_through_the_handler()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var repository = await BuildRepositoryAsync(fixture.ConnectionString, cancellationToken);

		// The real seed data's USA shape -- Region(Americas)/Subregion(Northern America), no
		// IntermediateRegion -- exercises the owned-JSON nesting the single-row test above skips
		// entirely (that one leaves View.Region null, the Antarctica shape).
		DeterministicGuid
			id = new(Iso3166.Ids[IsoCountryCode.UnitedStatesOfAmerica]),
			regionId = new(DeterministicGuid.Namespaces.Dns, "019"),
			subregionId = new(DeterministicGuid.Namespaces.Dns, "021");
		ServiceCollection seedServices = new();
		seedServices.AddDbContextFactory<ReferenceDbContext>(o =>
		{
			o.UseNpgsql(fixture.ConnectionString);
			o.ApplyNorseConventions(NorseNameRewriters.LowerSnakeCase);
			o.ApplyNorseTrackingBehavior();
		});
		var seedFactory = seedServices.BuildServiceProvider().GetRequiredService<IDbContextFactory<ReferenceDbContext>>();
		await using (var context = await seedFactory.CreateDbContextAsync(cancellationToken))
		{
			context.Set<CountryOrArea>().Add(new()
			{
				Id = id,
				Code = IsoCountryCode.UnitedStatesOfAmerica,
				Alpha2 = "US",
				Alpha3 = "USA",
				Name = "United States of America",
				Classification = Classification.None,
				View = new()
				{
					Id = id,
					Code = IsoCountryCode.UnitedStatesOfAmerica,
					Alpha2 = "US",
					Alpha3 = "USA",
					Name = "United States of America",
					Classification = Classification.None,
					Region = new()
					{
						Id = regionId,
						Code = "019",
						Name = "Americas",
						Subregion = new() { Id = subregionId, Code = "021", Name = "Northern America" },
					},
				},
			});
			await context.SaveChangesAsync(cancellationToken);
		}

		try
		{
			CountryQueryHandler handler = new(repository);
			var outcome = await handler.Handle(new CountryQuery("USA"), cancellationToken);

			var isOk = outcome.TryGetValue(out Success<CountryResponse> success);
			var isFailed = outcome.TryGetValue(out Failed failed);
			(isOk ? "OK" : isFailed ? $"FAILED:{failed.Problem.Category}" : "?").ShouldBe("OK");
			success.Value.Id.ShouldBe(id);
		}
		finally
		{
			await using var context = await seedFactory.CreateDbContextAsync(cancellationToken);
			await context.Set<CountryOrArea>().Where(c => c.Id == id).ExecuteDeleteAsync(cancellationToken);
		}
	}

	[Fact]
	async Task Garbage_fails_validation_without_touching_the_repository()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var repository = await BuildRepositoryAsync(fixture.ConnectionString, cancellationToken);
		CountryQueryHandler handler = new(repository);

		var outcome = await handler.Handle(new CountryQuery("banana"), cancellationToken);

		outcome.TryGetValue(out Failed failed).ShouldBeTrue();
		failed.Problem.Category.ShouldBe(ErrorCategory.Validation);
	}
}
