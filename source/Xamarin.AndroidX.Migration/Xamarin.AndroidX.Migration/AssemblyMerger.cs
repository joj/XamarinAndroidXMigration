﻿using ILRepacking;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xamarin.AndroidX.Migration
{
	public class AssemblyMerger : MigrationTool
	{
		private const string InjectedAttributeNamespace = "Xamarin.AndroidX.Internal";
		private const string InjectedAttributeTypeName = "InjectedAssemblyNameAttribute";

		public List<string> SearchDirectories { get; set; } = new List<string>();

		public bool InjectAssemblyNames { get; set; }
		
		public void Merge(IEnumerable<string> assemblies, string outputPath)
		{
			var assembliesToMerge = assemblies?.ToList() ?? throw new ArgumentNullException(nameof(assemblies));

			LogVerboseMessage("Merging:");
			foreach (var include in assembliesToMerge)
				LogVerboseMessage($" - {include}");

			var tempRoot = Path.Combine(Path.GetTempPath(), "Xamarin.AndroidX.Migration", "AssemblyMerger", Guid.NewGuid().ToString());

			if (InjectAssemblyNames)
			{
				if (!string.IsNullOrWhiteSpace(tempRoot) && !Directory.Exists(tempRoot))
					Directory.CreateDirectory(tempRoot);

				assembliesToMerge = assembliesToMerge.ToList();

				for (int i = 0; i < assembliesToMerge.Count; i++)
				{
					var ass = assembliesToMerge[i];
					var temp = Path.Combine(tempRoot, Guid.NewGuid().ToString() + ".dll");
					InjectAssemblyName(ass, temp);
					assembliesToMerge[i] = temp;
				}

				LogVerboseMessage("Temporary assemblies:");
				foreach (var include in assembliesToMerge)
					LogVerboseMessage($" - {include}");
			}

			var options = new RepackOptions
			{
				InputAssemblies = assembliesToMerge.ToArray(),
				OutputFile = outputPath,
				SearchDirectories = SearchDirectories.ToArray(),
				CopyAttributes = true,
				AllowMultipleAssemblyLevelAttributes = true,
				LogVerbose = Verbose
			};
			var repacker = new ILRepack(options);
			repacker.Repack();

			if (InjectAssemblyNames)
			{
				MergeAssemblyNameAttributes(outputPath);

				if (Directory.Exists(tempRoot))
				{
					try
					{
						Directory.Delete(tempRoot, true);
					}
					catch
					{
					}
				}
			}
		}

		private void InjectAssemblyName(string assemblyPath, string outputPath)
		{
			var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);

			using (var assembly = AssemblyDefinition.ReadAssembly(assemblyPath))
			{
				var module = assembly.MainModule;

				var mscorlibName = module.AssemblyReferences.FirstOrDefault(a => a.Name == "mscorlib");
				var mscorlib = module.AssemblyResolver.Resolve(mscorlibName);
				var attributeType = mscorlib.MainModule.GetType("System.Attribute");
				var baseCtor = attributeType.Methods.FirstOrDefault(m => m.Name == ".ctor");

				var iana = new TypeDefinition(InjectedAttributeNamespace, InjectedAttributeTypeName, TypeAttributes.Class);
				iana.BaseType = module.ImportReference(attributeType);

				var field = new FieldDefinition("assemblyName", FieldAttributes.Private | FieldAttributes.InitOnly, module.TypeSystem.String);

				var getterAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
				var getter = new MethodDefinition("get_AssemblyName", getterAttributes, module.TypeSystem.String);
				getter.DeclaringType = iana;
				getter.HasThis = true;
				getter.IsGetter = true;
				getter.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
				getter.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, field));
				getter.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

				var property = new PropertyDefinition("AssemblyName", PropertyAttributes.None, module.TypeSystem.String);
				property.HasThis = true;
				property.GetMethod = getter;

				var methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
				var ctor = new MethodDefinition(".ctor", methodAttributes, module.TypeSystem.Void);
				var param = new ParameterDefinition("assemblyName", ParameterAttributes.None, module.TypeSystem.String);
				ctor.Parameters.Add(param);

				ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
				ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, module.ImportReference(baseCtor)));
				ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
				ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
				ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
				ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
				ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, field));
				ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

				iana.Fields.Add(field);
				iana.Properties.Add(property);
				iana.Methods.Add(ctor);
				iana.Methods.Add(getter);
				module.Types.Add(iana);

				foreach (var type in module.Types)
				{
					type.CustomAttributes.Add(new CustomAttribute(ctor)
					{
						ConstructorArguments = { new CustomAttributeArgument(module.TypeSystem.String, assemblyName) }
					});
				}

				assembly.Write(outputPath);
			}
		}

		private void MergeAssemblyNameAttributes(string assemblyPath)
		{
			using (var assembly = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { ReadWrite = true }))
			{
				var correct = assembly.MainModule.GetType(InjectedAttributeNamespace + "." + InjectedAttributeTypeName);
				correct.Attributes |= TypeAttributes.Public;
				correct.CustomAttributes.Clear();

				foreach (var type in assembly.MainModule.Types.ToArray())
				{
					var attribute = type.CustomAttributes.FirstOrDefault(a => IsRandomType(a.AttributeType));
					if (attribute != null && attribute.Constructor.DeclaringType != correct)
					{
						type.CustomAttributes.Add(new CustomAttribute(correct.Methods[0])
						{
							ConstructorArguments = { attribute.ConstructorArguments[0] }
						});
						type.CustomAttributes.Remove(attribute);
					}
				}

				foreach (var type in assembly.MainModule.Types.Where(IsRandomType).ToArray())
				{
					assembly.MainModule.Types.Remove(type);
				}

				assembly.Write();
			}

			bool IsRandomType(TypeReference attr)
			{
				return
					attr.Namespace == InjectedAttributeNamespace &&
					attr.Name.EndsWith(InjectedAttributeTypeName) &&
					attr.Name != InjectedAttributeTypeName;
			}
		}
	}
}
