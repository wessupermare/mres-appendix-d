#!/usr/bin/env dotnet-script
#nullable enable

#r "System.Console"
#r "System.Diagnostics.Process"

using System.Collections.Generic;
using System.IO;

/// <summary>
/// This script performs automated ambiguity class checking over a corpus of files.
/// NOTE: This script is current for SLELabs ART builds from March/April 2022 and requires a Windows
///     environment with corresponding ART batch files.
/// </summary>

var args = Environment.GetCommandLineArgs();
while (args.Length > 0 && !Directory.Exists(args[0]))
	args = args[1..];

if (args.Length == 0)
{
	Console.WriteLine("Usage: dotnet script runall.csx <artHomeFolder> <smlScript> [<smlScript> ...]");
	Environment.Exit(-1);
}
string artHome = args[0];

ProcessStartInfo psi = new("cmd.exe")
{
	RedirectStandardOutput = true,
	RedirectStandardError = true,
	RedirectStandardInput = true,
	CreateNoWindow = true,
	UseShellExecute = false,
	WorkingDirectory = artHome
};

var p = Process.Start(psi);
p?.BeginOutputReadLine();

string[] RunCommand(string command)
{
	List<string> output = new(), error = new();
	bool stop = false;

	var outputReceived = new DataReceivedEventHandler((_, drea) =>
	{
		if (drea is null || drea.Data is null)
		{
			stop = true;
			return;
		}

		if (drea.Data.Trim() == "STOP")
			stop = true;
		else
			output.Add(drea.Data);
	});

	var errorReceived = new DataReceivedEventHandler((_, drea) =>
	{
		if (drea is null || drea.Data is null)
			return;

		error.Add(drea.Data);
	});

	p.OutputDataReceived += outputReceived;
	p.ErrorDataReceived += errorReceived;
	p.StandardInput.WriteLine(command);
	p.StandardInput.WriteLine("echo STOP");

	while (!stop)
		System.Threading.Thread.Sleep(50);

	p.OutputDataReceived -= outputReceived;
	p.ErrorDataReceived -= errorReceived;

	if (error.Any())
	{
		Console.WriteLine("ERROR THROWN!");
		throw new Exception(string.Join(Environment.NewLine, error));
	}

	return output.ToArray();
}

RunCommand("clean.bat");
RunCommand("artHome.bat .");
RunCommand("grammarWrite.bat sml.art");

HashSet<HashSet<string>> knownAmbiguities = new(HashSet<string>.CreateSetComparer());
Dictionary<HashSet<string>, List<int>>? AmbiguousLeftExtents(string[] lines)
{
	if (!lines.Contains("** Accept"))
		return null;

	while (lines.Count(l => l.StartsWith("TWE set cardinality")) > 1)
		lines = lines[1..];

	Dictionary<HashSet<string>, List<int>> retval = new(HashSet<string>.CreateSetComparer());
	foreach (string lineIter in lines)
	{
		string line = lineIter;
		if (!int.TryParse(line.Split(':')[0], out int index) || line.Contains("EOS $"))
			continue;

		line = line[(line.IndexOf('{') + 1)..^2].Trim();
		HashSet<string> les = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();

		if (les.Count <= 1)
			continue;

		les = les.Select(l => l[..(l.LastIndexOf('.'))]).ToHashSet();

		knownAmbiguities.Add(les);

		if (!retval.ContainsKey(les))
			retval.Add(les, new());

		retval[les].Add(index);
	}

	return retval;
}

Dictionary<string, Dictionary<HashSet<string>, List<int>>?> results = new();
foreach (string path in args[1..].Select(p => Path.GetFullPath(p)))
{
	Console.Error.WriteLine(path);
	RunCommand($"lexGLL.bat {path} !tweLongest !twePriority !tweDead !tweWrite");
	try
	{
		results.Add(Path.GetFileNameWithoutExtension(path), AmbiguousLeftExtents(RunCommand($"parseMGLL.bat {path} !tweFromSPPF !twePrint")));
	}
	catch (Exception e)
	{
		System.Diagnostics.Debug.WriteLine(e.Message);
	}
}

void printCSVElem(object? element) => Console.Write((element?.ToString() ?? "Reject") + ',');
void printCSVFinalElem(object? element) => Console.WriteLine(element?.ToString() ?? "Reject");

HashSet<string>[] ambiguities = knownAmbiguities.ToArray();
printCSVElem("filename");
foreach (string a in ambiguities.Select(hs => string.Join('/', hs)))
	printCSVElem(a);
printCSVFinalElem("total");

foreach (KeyValuePair<string, Dictionary<HashSet<string>, List<int>>?> r in results.OrderBy(kvp => kvp.Key))
{
	printCSVElem(r.Key);

	foreach (HashSet<string> a in ambiguities)
	{
		if (r.Value is null)
		{
			printCSVElem(null);
			continue;
		}
		else
			printCSVElem(r.Value.ContainsKey(a) ? string.Join(' ', r.Value[a]) : "");
	}

	printCSVFinalElem(r.Value?.Values?.Select(d => d.Count)?.Sum());
}

RunCommand("exit");
