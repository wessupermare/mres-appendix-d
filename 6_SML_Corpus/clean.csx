#!/usr/bin/env dotnet-script
#nullable enable

#r "System.Console"

using System.IO;

var args = Environment.GetCommandLineArgs()[2..];
while (args.Length > 0 && !File.Exists(args[0]))
	args = args[1..];

if (args.Length == 0)
{
	Console.WriteLine("Usage: dotnet script clean.csx <smlScript> [<smlScript> ...]");
	Environment.Exit(-1);
}

foreach (string fp in args)
{
	if (!File.Exists(fp))
	{
		Console.WriteLine($"File {fp} not found!");
		continue;
	}

	string fileContents = File.ReadAllText(fp);
	string newFile = string.Empty;

	int commentDepth = 0;
	for (int cntr = 0; cntr < fileContents.Length -1; ++cntr)
	{
		string checkstr = fileContents[cntr..(cntr + 2)];

		if (checkstr == "(*")
			++commentDepth;
		else if (checkstr == "*)")
		{
			--commentDepth;
			++cntr;
		}
		else if (commentDepth == 0)
			newFile += fileContents[cntr];
	}

	if (commentDepth != 0)
		Console.WriteLine($"Mismatched comments in file {fp}.");

	newFile += fileContents[^1];

	File.WriteAllText(fp, newFile.TrimStart());
}