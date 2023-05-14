﻿using System.Text;
using Issueneter.Annotation;

namespace Issueneter.ScanSourcesGenerator;

public class ScanSourcesGenerationHelper
{
    private const string Start = @"
using System;
using System.Collections.Generic;
using Issueneter.Annotation;

namespace Issueneter.Mappings;

public static class ModelsInfo
{
    private static readonly List<ScanSource> _availableSources = new List<ScanSource>()
    {
";

    private const string End = @"
    };

    public static IReadOnlyCollection<ScanSource> AvailableScanSources => _availableSources;
}";

    public static string Generate(IEnumerable<ModelProperties> sources)
    {
        var builder = new StringBuilder();
        builder.Append(Start);
        foreach (var source in sources)
        {
            builder.Append($"\t\tnew ScanSource(\"{source.Name}\", new List<string>(){{{string.Join(", ", source.Properties.Select(p => $"\"{p.Name}\""))}}}),\n");
        }

        builder.Append(End);
        return builder.ToString();
    }
}