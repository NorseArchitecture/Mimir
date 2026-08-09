using System.Reflection;
using System.Runtime.Serialization;
using Norse.Primitives;

namespace Norse.Reference.Contracts.Tests;

public sealed class WireContractShapeTests
{
	[Theory]
	[InlineData(typeof(CountryRequest))]
	[InlineData(typeof(CountryResponse))]
	void Wire_records_carry_data_contract_with_unique_ordered_members(Type wireType)
	{
		wireType.GetCustomAttribute<DataContractAttribute>().ShouldNotBeNull();
		var orders = wireType.GetProperties()
			.Where(p => !p.Name.EndsWith("Input", StringComparison.Ordinal))
			.Select(p => p.GetCustomAttribute<DataMemberAttribute>().ShouldNotBeNull().Order)
			.ToList();
		orders.ShouldBeUnique();
		orders.ShouldBe([.. orders.OrderBy(o => o)]);
	}

	[Fact]
	void The_stamp_is_the_serialized_member_and_the_buffer_never_is()
	{
		// Wire-stamped request scalars law (2026-08-08 spec): the Result<T> stamp rides the wire,
		// the raw form buffer is a client-side artifact the sanctioned deserialization path never
		// assigns. A [DataMember] on the buffer would ship the claim beside the verdict.
		typeof(CountryRequest).GetProperty(nameof(CountryRequest.Code))!
			.PropertyType.ShouldBe(typeof(Result<IsoCountryCode>));
		typeof(CountryRequest).GetProperty(nameof(CountryRequest.Code))!
			.GetCustomAttribute<DataMemberAttribute>().ShouldNotBeNull();
		typeof(CountryRequest).GetProperty(nameof(CountryRequest.CodeInput))!
			.GetCustomAttribute<DataMemberAttribute>()
			.ShouldBeNull("the buffer is never serialized — the stamp is the wire member");
	}

	[Fact]
	void Assigning_the_buffer_stamps_the_verdict()
	{
		CountryRequest request = new() { CodeInput = "US" };
		request.Code.TryGetValue(out Success<IsoCountryCode> success).ShouldBeTrue();
		success.Value.ShouldBe(IsoCountryCode.UnitedStatesOfAmerica);

		request.CodeInput = "banana";
		request.Code.TryGetValue(out Success<IsoCountryCode> _)
			.ShouldBeFalse("re-assignment must re-stamp — no code path sets text without refreshing the verdict");
	}
}
