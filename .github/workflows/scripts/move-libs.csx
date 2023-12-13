#!/usr/bin/env dotnet-script

using System;
using System.IO;
using System.Linq;

var sourceDirectory = Args[0];
var targetDirectory = "./Packages/com.walletconnect.core/Runtime/WalletConnectSharp";
var externalDirectory = Path.Combine(targetDirectory, "External");

Directory.CreateDirectory(targetDirectory);
Directory.CreateDirectory(externalDirectory);

var excludedDlls = Environment.GetEnvironmentVariable("EXCLUDED_DLLS")?.Split(',');

foreach (var file in Directory.GetFiles(sourceDirectory, "*.dll"))
{
    var fileName = Path.GetFileName(file);

    if (fileName.StartsWith("WalletConnectSharp"))
    {
        Console.WriteLine($"Moving {fileName} to {targetDirectory}");
        File.Move(file, Path.Combine(targetDirectory, fileName), true);
    }
    else if (excludedDlls == null || !excludedDlls.Contains(fileName))
    {
        Console.WriteLine($"Moving {fileName} to {externalDirectory}");
        File.Move(file, Path.Combine(externalDirectory, fileName), true);
    }
}