using System.Text;
using Mono.Cecil;

namespace DependencyPath;

internal static class AssemblyNameDefinitionExtensions
{
	private static string ByteToString(byte[]? input)
	{
		if (input == null)
			return string.Empty;

		var sb = new StringBuilder();
		foreach (var b in input)
			sb.Append(b.ToString("x2"));

		return sb.ToString();
	}

	public static string GetPublicKeyTokenAsString(this AssemblyNameDefinition name)
	{
		return ByteToString(name.PublicKeyToken);
	}
}
