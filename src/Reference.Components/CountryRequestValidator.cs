using FluentValidation;
using Norse.Primitives;

namespace Norse.Reference.Components;

/// <summary>
///     Validator for <see cref="CountryRequest" /> — the single source of truth for country-lookup
///     validation, run client-side by Asgard's <c>FormValidator</c> so unproven input never buys
///     a round trip. THE RULE REGISTERS ON THE STAMP — <see cref="CountryRequest.Code" />, the
///     <c>Result&lt;IsoCountryCode&gt;</c> — so every predicate reads the parsed verdict: the
///     generated quad-form parser owns format truth, this class owns business truth, no rule exists
///     twice. <c>WithName</c> carries the buffer's name for message display only; field-change
///     selection on blur is <c>OutcomeFormComponentBase.EditContextFor</c>'s echo mechanic (the
///     buffer's change is echoed as the stamp's), so <c>PropertyName</c> — and therefore server
///     error keys — stay <c>Code</c>, wire-stable.
/// </summary>
public sealed class CountryRequestValidator : AbstractValidator<CountryRequest>
{
	/// <summary>Initializes a new instance of the <see cref="CountryRequestValidator" /> class.</summary>
	public CountryRequestValidator()
	{
		RuleFor(x => x.Code)
			.Cascade(CascadeMode.Stop)
			.Must(code => !(code.TryGetValue(out Failure failure) && failure.Reason == ParseFailure.Empty))
			.WithMessage("Enter a country code.")
			.WithName(nameof(CountryRequest.CodeInput))
			.Must(code => code.TryGetValue(out Success<IsoCountryCode> _))
			.WithMessage("Not a recognized ISO 3166-1 code or identifier.");
	}
}
