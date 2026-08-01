using System.Reflection;

namespace Norse.Reference.Seeds;

/// <summary>
/// Accessors for the embedded canonical raw datasets. One method per dataset; the returned stream
/// is the caller's to dispose.
/// </summary>
public static class RawDatasets
{
	/// <summary>Opens the embedded UNSD M49 methodology export (semicolon-delimited CSV).</summary>
	public static Stream UnsdM49() =>
		Assembly.GetExecutingAssembly().GetManifestResourceStream("Norse.Reference.Seeds.UnsdM49.csv")
			?? throw new InvalidOperationException("Embedded resource 'Norse.Reference.Seeds.UnsdM49.csv' is missing.");
}
