using Spectre.Console.Cli;

namespace DependencyPath;

public static class Program
{
	private static async Task<int> Main(string[] args)
	{
		var app = new CommandApp<ScanCommand>();

		app.Configure(config =>
		{
			config
				.AddCommand<ScanCommand>("scan")
				.WithDescription("Scan assemblies");
		});

		return await app.RunAsync(args);
	}
}
