using Norse.Infrastructure.Persistence.EntityFramework;
using Norse.Persistence.EntityFramework.PostgreSQL;
using Norse.Reference.Data;

namespace Norse.Reference.Web.Server;

/// <summary>Composition-root wiring for Reference.Web.Server's gRPC reference-data service.</summary>
public static class ServiceCollectionExtensions
{
	extension(IHostApplicationBuilder builder)
	{
		/// <summary>
		/// Registers <see cref="ReferenceDbContext"/> (via <see cref="Norse.Infrastructure.Persistence.EntityFramework.ServiceCollectionExtensions.AddNorseWell{TContext}"/> — Postgres
		/// only today, see the well-composition spec §5 for SQL Server's deferred status), the generated
		/// mediator handler wiring, and <see cref="IReferenceService"/> itself.
		/// </summary>
		/// <param name="connectionStringName">The configuration key under <c>ConnectionStrings</c>.</param>
		/// <returns>The same <paramref name="builder"/> for chaining.</returns>
		public IHostApplicationBuilder AddNorseReferenceService(string connectionStringName)
		{
			builder.AddNorseWell<ReferenceDbContext>(NorsePostgresEfProvider.Instance, connectionStringName);
			builder.Services.AddNorseReferenceWebServerHandlers();

			builder.Services.AddScoped<IReferenceService, ReferenceService>();

			return builder;
		}
	}
}
