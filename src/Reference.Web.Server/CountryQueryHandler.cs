using Norse.Abstractions.Backend;
using Norse.Abstractions.Contracts;
using Norse.Abstractions.Web.Server.Mediator;
using Norse.Primitives;
using Norse.Reference.Data.EntityFramework;

namespace Norse.Reference.Web.Server;

/// <summary>
///     The slice's read path (well-and-wire spec §7.2), post wire-stamped scalars: no parse happens
///     here — the stamp arrived proven or failed from deserialization, the handler guards the verdict,
///     resolves the v5 identity from the generated lookup with zero database involvement, and makes
///     one identity-path (Guid primary key) repository call with SQL-side projection to the wire
///     record.
/// </summary>
sealed class CountryQueryHandler(IReadRepository<CountryOrAreaView> repository)
	: IRequestHandler<CountryQuery, CountryResponse>
{
	public ValueTask<Outcome<CountryResponse>> Handle(CountryQuery request,
		CancellationToken cancellationToken = default)
	{
		if (!request.Code.TryGetValue(out Success<IsoCountryCode> success))
		{
			// A failed or default stamp is the server's own verdict on the claim — answer it as the
			// typed validation failure it is, echoing the failed input back for the error display.
			var input = request.Code.TryGetValue(out Failure failure) ? failure.Input : "";
			return ValueTask.FromResult(Outcome<CountryResponse>.Err(
				ErrorCategory.Validation,
				new Dictionary<string, string[]> { ["code"] = [input] }));
		}

		return new(repository.GetAsync(
			Iso3166.Ids[success.Value],
			v => new CountryResponse { Id = v.Id, Alpha2 = v.Alpha2, Alpha3 = v.Alpha3, Name = v.Name },
			cancellationToken));
	}
}
