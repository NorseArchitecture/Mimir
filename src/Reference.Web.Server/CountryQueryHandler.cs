using Norse.Abstractions.Backend;
using Norse.Abstractions.Contracts;
using Norse.Abstractions.Web.Server.Mediator;
using Norse.Primitives;
using Norse.Reference.Data;

namespace Norse.Reference.Web.Server;

/// <summary>
/// The slice's read path (well-and-wire spec §7.2): parse at the edge, resolve the v5 identity from
/// the generated lookup with zero database involvement, then one identity-path repository call with
/// SQL-side projection to the wire record.
/// </summary>
sealed class CountryQueryHandler(IReadRepository<CountryOrAreaView> repository) : IRequestHandler<CountryQuery, CountryResponse>
{
	public ValueTask<Outcome<CountryResponse>> Handle(CountryQuery request, CancellationToken cancellationToken = default)
	{
		var parsed = IsoCountryCodes.Parse(request.Code);
		if (!parsed.TryGetValue(out Success<IsoCountryCode> success))
			return ValueTask.FromResult(Outcome<CountryResponse>.Err(
				ErrorCategory.Validation,
				new Dictionary<string, string[]> { ["code"] = [request.Code] }));

		return new(repository.GetAsync(
			Iso3166.Ids[success.Value],
			v => new CountryResponse { Id = v.Id, Alpha2 = v.Alpha2, Alpha3 = v.Alpha3, Name = v.Name },
			cancellationToken));
	}
}
