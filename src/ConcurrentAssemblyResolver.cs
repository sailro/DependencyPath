using System.Collections.Concurrent;
using Mono.Cecil;

namespace DependencyPath;

internal class ConcurrentAssemblyResolver : BaseAssemblyResolver
{
	private readonly IDictionary<string, AssemblyDefinition> _cache;

	public ConcurrentAssemblyResolver()
	{
		_cache = new ConcurrentDictionary<string, AssemblyDefinition>(StringComparer.Ordinal);
	}

	public override AssemblyDefinition Resolve(AssemblyNameReference name)
	{
		if (_cache.TryGetValue(name.FullName, out var assembly))
			return assembly;

		assembly = base.Resolve(name);
		_cache[name.FullName] = assembly;

		return assembly;
	}

	public void RegisterAssembly(AssemblyDefinition assembly)
	{
		if (assembly == null)
			throw new ArgumentNullException(nameof(assembly));

		var name = assembly.Name.FullName;
		if (_cache.ContainsKey(name))
			return;

		_cache[name] = assembly;
	}

	protected override void Dispose(bool disposing)
	{
		foreach (var assembly in _cache.Values)
			assembly.Dispose();

		_cache.Clear();

		base.Dispose(disposing);
	}
}
