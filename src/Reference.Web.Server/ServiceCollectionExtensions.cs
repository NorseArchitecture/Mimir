using Microsoft.EntityFrameworkCore;
using Norse.Infrastructure.Persistence.EntityFramework;
using Norse.Persistence.EntityFramework;
using Norse.Reference.Data.EntityFramework;

namespace Norse.Reference.Web.Server;

/// <summary>Composition-root wiring for Reference.Web.Server's gRPC reference-data service.</summary>
public static class ServiceCollectionExtensions
{
	extension(IServiceCollection services)
	{
		/// <summary>
		/// Registers <see cref="ReferenceDbContext"/> as an <see cref="IDbContextFactory{TContext}"/> —
		/// factory, not a plain scoped context, because Midgard's <c>AddWell&lt;TContext&gt;()</c> resolves
		/// its repositories through the factory, never through a directly-injected context — the well
		/// itself (<c>IReadRepository&lt;CountryOrAreaView&gt;</c>), the generated mediator handler/dispatch
		/// registration (<c>AddNorseReferenceWebServerHandlers()</c>, emitted by Asgard's registration
		/// generator), and the code-first gRPC host with <see cref="IReferenceService"/>.
		/// </summary>
		public IServiceCollection AddNorseReferenceService(string connectionString) => services
			.AddDbContextFactory<ReferenceDbContext>(o =>
			{
				o.UseNpgsql(connectionString);
				o.ApplyNorseConventions(NorseNameRewriters.LowerSnakeCase);
				o.ApplyNorseTrackingBehavior();
			})
			.AddWell<ReferenceDbContext>()
			.AddNorseReferenceWebServerHandlers()
			.AddScoped<IReferenceService, ReferenceService>();
	}
}
