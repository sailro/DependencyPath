using Spectre.Console.Cli;
using Spectre.Console;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace DependencyPath;

[UsedImplicitly]
internal class ScanCommand : AsyncCommand<ScanCommandSettings>
{
	private ScanCommandSettings Settings { get; set; } = null!;
	private ReaderParameters Parameters { get; set; } = null!;

	public override Task<int> ExecuteAsync(CommandContext context, ScanCommandSettings settings)
	{
		try
		{
			Settings = settings;

			var path = Path.GetDirectoryName(settings.Assemblies);
			if (string.IsNullOrEmpty(path))
				path = ".";

			var searchPattern = Path.GetFileName(settings.Assemblies);
			var assemblies = Directory
				.EnumerateFiles(path, searchPattern, settings.Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
				.ToArray();

			var resolver = new DefaultAssemblyResolver();
			var directories = assemblies
				.Select(Path.GetDirectoryName)
				.Concat(settings.SearchPaths ?? Array.Empty<string>())
				.Distinct();

			foreach (var directory in directories)
				resolver.AddSearchDirectory(directory);

			Parameters = new ReaderParameters
			{
				AssemblyResolver = resolver,
				ReadWrite = false,
				InMemory = true,
				ReadingMode = ReadingMode.Deferred,
				ReadSymbols = false,
			};

			foreach (var assembly in assemblies)
				VisitAssemblyFile(assembly);

			return Task.FromResult(0);
		}
		catch (Exception ex)
		{
			AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
			return Task.FromResult(1);
		}
	}

	private void VisitAssemblyFile(string assemblyFile)
	{
		try
		{
			if (Settings.Verbose)
				AnsiConsole.MarkupLine($"[gray]Processing: {assemblyFile}[/]");

			var assembly = AssemblyDefinition.ReadAssembly(assemblyFile, Parameters);
			VisitAssembly(assembly, Array.Empty<AssemblyDefinition>());
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[yellow]Warning: unable to load {assemblyFile}: {ex.Message}[/]");
		}
	}

	private void VisitAssembly(AssemblyDefinition assembly, AssemblyDefinition[] path)
	{
		if (path.Length >= Settings.Depth)
			return;

		if (Settings.Tokens != null && Settings.Tokens.Contains(assembly.Name.GetPublicKeyTokenAsString()))
			return;

		var name = assembly.Name.Name;

		path = path
			.Append(assembly)
			.ToArray();

		if (Matches(name))
			DisplayResult(path);

		foreach (var module in assembly.Modules)
			VisitModule(module, path);
	}

	private void VisitModule(ModuleDefinition module, AssemblyDefinition[] path)
	{
		VisitAssemblyNameReferences(module.AssemblyReferences, path);
	}

	private void VisitAssemblyNameReferences(Collection<AssemblyNameReference> assemblyNameReferences, AssemblyDefinition[] path)
	{
		foreach (var assemblyNameReference in assemblyNameReferences)
			VisitAssemblyNameReference(assemblyNameReference, path);
	}

	private void VisitAssemblyNameReference(AssemblyNameReference assemblyNameReference, AssemblyDefinition[] path)
	{
		try
		{
			var assembly = Parameters.AssemblyResolver.Resolve(assemblyNameReference);
			VisitAssembly(assembly, path);
		}
		catch (Exception)
		{
			if (Settings.Verbose)
				AnsiConsole.MarkupLine($"[yellow]Warning: unable to resolve {assemblyNameReference}[/]");
		}
	}

	private bool Matches(string name)
	{
		return Settings.Dependency.Equals(name, StringComparison.OrdinalIgnoreCase);
	}

	private void DisplayResult(AssemblyDefinition[] path)
	{
		for (var i = 0; i < path.Length; i++)
		{
			if (i > 0)
				AnsiConsole.Markup(" [grey]->[/] ");

			var color = i == 0 ? "blue" : i == path.Length - 1 ? "green" : "white";
			AnsiConsole.Markup($"[{color}]{path[i].Name.Name}[/]");
			if (Settings.DisplayVersions)
				AnsiConsole.Markup($" [teal]({path[i].Name.Version})[/]");
		}

		AnsiConsole.MarkupLine(string.Empty);
	}
}
