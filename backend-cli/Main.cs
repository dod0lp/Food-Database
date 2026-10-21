namespace ProgramTestFood;

/// <summary>
/// Entry point for the interactive food database console application.
/// </summary>
public static class ProgramTestFood {
    /// <summary>
    /// Starts the interactive CLI.
    /// </summary>
    static async Task<int> Main() {
        return await CliApplication.RunAsync();
    }
}