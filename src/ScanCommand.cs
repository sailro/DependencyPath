using Spectre.Console.Cli;
using Spectre.Console;
using Mono.Cecil;
using Mono.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using Humanizer;

namespace DependencyPath;

[UsedImplicitly]
internal class ScanCommand : AsyncCommand<ScanCommandSettings>
{
	private ScanCommandSettings _settings = null!;
	private ReaderParameters _parameters = null!;

	private readonly ConcurrentAssemblyResolver _resolver = new();
	private readonly ConcurrentDictionary<string /* fullname */, AssemblyNameReference> _skipList = new();
	private readonly ConcurrentBag<string> _results = new();

	private static void ForEach<T>(IEnumerable<T> items, Action<T> action)
	{
#if DEBUG
		foreach (var item in items)
		{
			action(item);
		}
#else
		Parallel.ForEach(items, action);
#endif
	}

	public override Task<int> ExecuteAsync(CommandContext context, ScanCommandSettings settings)
	{
		try
		{
			_settings = settings;

			var assemblies = SearchAssemblies(settings);
			SetupAssemblyResolver(settings, assemblies);

			AnsiConsole
				.Status()
				.Spinner(Spinner.Known.Star)
				.SpinnerStyle(Style.Parse("green bold"))
				.Start($"Searching [green]{settings.Dependency}[/] dependency in [blue]{"assembly".ToQuantity(assemblies.Length)}[/] ...", ctx =>
				{
					ForEach(assemblies, VisitAssemblyFile);
					ctx.Refresh();
				});

			DisplayResults();
			return Task.FromResult(0);
		}
		catch (Exception ex)
		{
			AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
			return Task.FromResult(1);
		}
		finally
		{
			_results.Clear();
			_skipList.Clear();
			_resolver.Dispose();
		}
	}

	private static string[] SearchAssemblies(ScanCommandSettings settings)
	{
		var path = Path.GetDirectoryName(settings.Assemblies);
		if (string.IsNullOrEmpty(path))
			path = ".";

		var searchPattern = Path.GetFileName(settings.Assemblies);
		var assemblies = Directory.EnumerateFiles(path, searchPattern, settings.Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
			.ToArray();
		
		return assemblies;
	}

	private void SetupAssemblyResolver(ScanCommandSettings settings, string[] assemblies)
	{
		var directories = assemblies
			.Select(Path.GetDirectoryName)
			.Concat(settings.SearchPaths ?? Array.Empty<string>())
			.Distinct();

		foreach (var directory in directories)
			_resolver.AddSearchDirectory(directory);

		_parameters = new ReaderParameters
		{
			AssemblyResolver = _resolver,
			ReadWrite = false,
			InMemory = true,
			ReadingMode = ReadingMode.Deferred,
			ReadSymbols = false,
		};
	}

	private void VisitAssemblyFile(string assemblyFile)
	{
		try
		{
			if (_settings.Verbose)
				AnsiConsole.MarkupLine($"[gray]Processing: {assemblyFile}[/]");

			var assembly = AssemblyDefinition.ReadAssembly(assemblyFile, _parameters);
			_resolver.RegisterAssembly(assembly);

			VisitAssembly(assembly, Array.Empty<AssemblyDefinition>());
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[yellow]Warning: unable to load {assemblyFile}: {ex.Message}[/]");
		}
	}

	private void VisitAssembly(AssemblyDefinition assembly, AssemblyDefinition[] path)
	{
		if (path.Length >= _settings.Depth)
			return;

		if (_settings.Tokens != null && _settings.Tokens.Contains(assembly.Name.GetPublicKeyTokenAsString()))
			return;

		var name = assembly.Name.Name;

		path = path
			.Append(assembly)
			.ToArray();

		if (Matches(name))
			OnDependencyPathFound(path);

		ForEach(assembly.Modules, module => VisitModule(module, path));
	}

	private void VisitModule(ModuleDefinition module, AssemblyDefinition[] path)
	{
		VisitAssemblyNameReferences(module.AssemblyReferences, path);
	}

	private void VisitAssemblyNameReferences(Collection<AssemblyNameReference> assemblyNameReferences, AssemblyDefinition[] path)
	{
		ForEach(assemblyNameReferences, assemblyNameReference => VisitAssemblyNameReference(assemblyNameReference, path));
	}

	private void VisitAssemblyNameReference(AssemblyNameReference assemblyNameReference, AssemblyDefinition[] path)
	{
		try
		{
			if (_skipList.TryGetValue(assemblyNameReference.FullName, out _))
				return;

			var assembly = _parameters.AssemblyResolver.Resolve(assemblyNameReference);
			
			VisitAssembly(assembly, path);
		}
		catch (AssemblyResolutionException)
		{
			_skipList.TryAdd(assemblyNameReference.FullName, assemblyNameReference);

			if (_settings.Verbose)
				AnsiConsole.MarkupLine($"[yellow]Warning: unable to resolve {assemblyNameReference}[/]");
		}
	}

	private bool Matches(string name)
	{
		return _settings.Dependency.Equals(name, StringComparison.OrdinalIgnoreCase);
	}

	private void OnDependencyPathFound(AssemblyDefinition[] path)
	{
		var sb = new StringBuilder();
		for (var i = 0; i < path.Length; i++)
		{
			if (i > 0)
				sb.Append(" [grey]->[/] ");

			var color = i == 0 ? "blue" : i == path.Length - 1 ? "green" : "white";
			var current = path[i];
			var previous = i == 0 ? null : path[i-1];

			sb.Append($"[{color}]{current.Name.Name}[/]");
			if (_settings.DisplayVersions || _settings.DisplayAllVersions)
			{
				var resolved = current.Name.Version;
				var expected = previous == null || !_settings.DisplayAllVersions ? resolved : previous.Modules.SelectMany(module => module.AssemblyReferences.Where(reference => reference.Name == current.Name.Name)).FirstOrDefault()?.Version ?? resolved;

				sb.Append($" [teal]({expected}");
				if (expected != resolved)
				{
					sb.Append($"/{resolved}");
				}
				sb.Append(")[/]");
			}
		}

		_results.Add(sb.ToString());
	}

	private void DisplayResults()
	{
		foreach (var result in _results.OrderBy(r => r))
			AnsiConsole.MarkupLine(result);
	}
}
