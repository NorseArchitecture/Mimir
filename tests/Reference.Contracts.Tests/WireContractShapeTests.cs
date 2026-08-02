using System.Reflection;
using System.Runtime.Serialization;

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
			.Select(p => p.GetCustomAttribute<DataMemberAttribute>().ShouldNotBeNull().Order)
			.ToList();
		orders.ShouldBeUnique();
		orders.ShouldBe([.. orders.OrderBy(o => o)]);
	}
}
