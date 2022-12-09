﻿using System.ComponentModel;
using Spectre.Console.Cli;

namespace DependencyPath;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal class ScanCommandSettings : CommandSettings
{
	[Description("Assemblies")]
	[CommandArgument(0, "<assemblies>")]
	public string Assemblies { get; set; } = string.Empty;

	[Description("Dependency to search")]
	[CommandArgument(0, "<dependency>")]
	public string Dependency { get; set; } = string.Empty;

	[Description("Display versions")]
	[CommandOption("-v|--version")]
	[DefaultValue(false)]
	public bool DisplayVersions { get; set; }

	[Description("Skip public key token")]
	[CommandOption("-t|--token")]
	public string[]? Tokens { get; set; }

	[Description("Recurse sub-directories")]
	[CommandOption("-r|--recurse")]
	[DefaultValue(false)]
	public bool Recurse { get; set; }

	[Description("Verbose")]
	[CommandOption("--verbose")]
	[DefaultValue(false)]
	public bool Verbose { get; set; }

	[Description("Max search depth")]
	[CommandOption("-d|--depth")]
	[DefaultValue(8)]
	public int Depth { get; set; }
}
