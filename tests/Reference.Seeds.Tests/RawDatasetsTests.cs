namespace Norse.Reference.Seeds.Tests;

public sealed class RawDatasetsTests
{
	[Fact]
	async Task The_unsd_m49_stream_opens_on_the_real_header()
	{
		await using var stream = RawDatasets.UnsdM49();
		using StreamReader reader = new(stream);
		(await reader.ReadLineAsync(TestContext.Current.CancellationToken))!.ShouldContain("Country or Area");
	}
}
