﻿using System;
using System.IO;
using System.Text;
using Blah.Pools;
using Blah.Reflection;
using Blah.Services;
using UnityEditor;

namespace Blah.Aot
{
public static class BlahAotPoolsAndServices
{
	private const string PATH = "Assets/Blah/BlahAotGenerated.cs";

	[MenuItem("Blah/Aot/Generate Pools And Services")]
	public static void Generate()
	{
		var sb = new StringBuilder();

		foreach (var type in BlahReflection.EnumerateGameTypes())
		{
			var interfaces = type.GetInterfaces();
			if (Array.IndexOf(interfaces, typeof(IBlahEntrySignal)) != -1)
			{
				sb.AppendLine($"pools.GetSignalConsumer<{type.FullName}>();");
				sb.AppendLine($"pools.GetSignalProducer<{type.FullName}>();");
			}
			if (Array.IndexOf(interfaces, typeof(IBlahEntryData)) != -1)
			{
				sb.AppendLine($"pools.GetDataConsumer<{type.FullName}>();");
				sb.AppendLine($"pools.GetDataProducer<{type.FullName}>();");
			}
			if (Array.IndexOf(interfaces, typeof(IBlahEntryNextFrameSignal)) != -1)
			{
				sb.AppendLine($"pools.GetSignalConsumerNextFrame<{type.FullName}>();");
				sb.AppendLine($"pools.GetSignalProducerNextFrame<{type.FullName}>();");
			}
			if (type.BaseType == typeof(BlahServiceBase))
			{
				sb.AppendLine($"services.Get<{type.FullName}>();");
			}
		}
		string content = TEMPLATE.Replace("[CODEGEN]", sb.ToString());
		CreateDirIfNotExists();
		File.WriteAllText(PATH, content);
		AssetDatabase.ImportAsset(PATH);
	}


	private static void CreateDirIfNotExists()
	{
		const string pathDir = "Assets/Blah";
		if (!Directory.Exists(pathDir))
			Directory.CreateDirectory(pathDir);
	}
	

	private const string TEMPLATE = @"
using Blah.Pools;
using Blah.Services;
using UnityEngine.Scripting;

public static class BlahPoolsAotGenerated 
{
	[Preserve]
	private static void Preserve()
	{
		var pools = new BlahPoolsContext();
		var services = new BlahServicesContext(null);
		[CODEGEN]
	}	
}";
}
}