﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.Android.Tools.Bytecode;

namespace Xamarin.AndroidX.Migration
{
	public class CecilMigrator
	{
		private const string RegisterAttributeFullName = "Android.Runtime.RegisterAttribute";
		private const string StringFullName = "System.String";

		private const string EmbeddedAarName = "__AndroidLibraryProjects__.zip";
		private const string EmbeddedJarExtension = ".jar";

		public CecilMigrator()
			: this(new AndroidXTypesCsvMapping())
		{
		}

		public CecilMigrator(Stream stream)
			: this(new AndroidXTypesCsvMapping(stream))
		{
		}

		public CecilMigrator(AndroidXTypesCsvMapping mapping)
		{
			Mapping = mapping;
		}

		public AndroidXTypesCsvMapping Mapping { get; }

		public bool Verbose { get; set; }

		public bool SkipEmbeddedResources { get; set; }

		public CecilMigrationResult Migrate(IEnumerable<MigrationPair> assemblies)
		{
			var result = CecilMigrationResult.Skipped;

			foreach (var pair in assemblies)
			{
				result |= Migrate(pair.Source, pair.Destination);
			}

			return result;
		}

		public CecilMigrationResult Migrate(MigrationPair assemblies) =>
			Migrate(assemblies.Source, assemblies.Destination);

		public CecilMigrationResult Migrate(string source, string destination)
		{
			if (string.IsNullOrWhiteSpace(source))
				throw new ArgumentException($"Invalid source assembly path specified: '{source}'.", nameof(source));
			if (string.IsNullOrWhiteSpace(destination))
				throw new ArgumentException($"Invalid destination assembly path specified: '{destination}'.", nameof(destination));
			if (!File.Exists(source))
				throw new FileNotFoundException($"Source assembly does not exist: '{source}'.", source);

			var pdbPath = Path.ChangeExtension(source, "pdb");
			var destPdbPath = Path.ChangeExtension(destination, "pdb");
			var tempDllPath = Path.ChangeExtension(destination, "temp.dll");
			var tempPdbPath = Path.ChangeExtension(destination, "temp.pdb");

			var hasPdb = File.Exists(pdbPath);
			var result = CecilMigrationResult.Skipped;

			using (var resolver = new DefaultAssemblyResolver())
			{
				resolver.AddSearchDirectory(Path.GetDirectoryName(source));
				var readerParams = new ReaderParameters
				{
					ReadSymbols = hasPdb,
					AssemblyResolver = resolver,
				};

				var requiresSave = false;

				using (var assembly = AssemblyDefinition.ReadAssembly(source, readerParams))
				{
					if (Verbose)
						Console.WriteLine($"Processing assembly '{source}'...");

					result = MigrateAssembly(assembly);

					requiresSave =
						result.HasFlag(CecilMigrationResult.ContainedSupport) ||
						result.HasFlag(CecilMigrationResult.ContainedJni);

					var dir = Path.GetDirectoryName(destination);
					if (!Directory.Exists(dir))
						Directory.CreateDirectory(dir);

					if (requiresSave)
					{
						Stream symbolStream = null;
						PdbWriterProvider symbolWriter = null;
						if (hasPdb)
						{
							symbolStream = File.Create(tempPdbPath);
							symbolWriter = new PdbWriterProvider();
						}

						try
						{
							assembly.Write(tempDllPath, new WriterParameters
							{
								WriteSymbols = hasPdb,
								SymbolStream = symbolStream,
								SymbolWriterProvider = symbolWriter
							});
						}
						finally
						{
							symbolStream?.Dispose();
						}

						Console.WriteLine($"Migrated assembly to '{destination}'.");
					}
					else
					{
						if (Verbose)
							Console.WriteLine($"Skipped assembly '{source}' due to lack of support types.");

						if (!source.Equals(destination, StringComparison.OrdinalIgnoreCase))
						{
							if (Verbose)
								Console.WriteLine($"Copying source assembly '{source}' to '{destination}'.");

							File.Copy(source, destination, true);
							if (hasPdb)
								File.Copy(pdbPath, destPdbPath, true);
						}
					}
				}

				if (requiresSave)
				{
					if (File.Exists(tempDllPath))
					{
						File.Copy(tempDllPath, destination, true);
						File.Delete(tempDllPath);
					}
					if (File.Exists(tempPdbPath))
					{
						File.Copy(tempPdbPath, destPdbPath, true);
						File.Delete(tempPdbPath);
					}
				}
			}

			return result;
		}

		public bool MigrateJniString(string jniString, out string newJniString)
		{
			if (string.IsNullOrEmpty(jniString))
			{
				newJniString = null;
				return false;
			}

			// process a straight type
			if (TryMapJavaClass(jniString, out var registration))
			{
				newJniString = registration;
				return true;
			}

			// process a method signature
			var method = ParseMethodSignature(jniString, out var name);
			if (method != null)
			{
				var changed = false;
				var newjni = new StringBuilder();

				newjni.Append("(");
				foreach (var param in method.Parameters)
				{
					changed |= GetNewJniPart(param, out var newParam);
					newjni.Append(newParam ?? param);
				}
				newjni.Append(")");

				changed |= GetNewJniPart(method.ReturnTypeSignature, out var newReturn);
				newjni.Append(newReturn ?? method.ReturnTypeSignature);

				if (changed)
				{
					newJniString = newjni.ToString();
					if (!string.IsNullOrEmpty(name))
						newJniString = $"{name}.{newJniString}";
				}
				else
				{
					newJniString = null;
				}

				return changed;
			}

			newJniString = null;
			return false;
		}

		private CecilMigrationResult MigrateAssembly(AssemblyDefinition assembly)
		{
			var result = MigrateNetTypes(assembly);

			if (result.HasFlag(CecilMigrationResult.PotentialJni))
			{
				result |= MigrateJniStrings(assembly);
			}

			if (!SkipEmbeddedResources)
			{
				result |= MigrateEmbeddedResources(assembly);
			}

			return result;
		}

		private CecilMigrationResult MigrateEmbeddedResources(AssemblyDefinition assembly)
		{
			if (!assembly.MainModule.HasResources)
				return CecilMigrationResult.Skipped;

			var embeddedResources = assembly.MainModule.Resources
				.Where(ShouldJetifyResource)
				.OfType<EmbeddedResource>()
				.ToList();

			if (embeddedResources.Count == 0)
				return CecilMigrationResult.Skipped;

			var tempRoot = Path.Combine(Path.GetTempPath(), GetType().FullName, Guid.NewGuid().ToString());
			if (!Directory.Exists(tempRoot))
				Directory.CreateDirectory(tempRoot);

			if (Verbose)
				Console.WriteLine($"  Migrating embedded resources...");

			var jetifier = new Jetifier();
			jetifier.Verbose = Verbose;

			foreach (var embedded in embeddedResources)
			{
				var tempFile = Path.Combine(tempRoot, Guid.NewGuid().ToString() + Path.GetExtension(embedded.Name));

				Console.WriteLine($"    Migrating embedded resource '{embedded.Name}'...");

				using (var fileStream = File.OpenWrite(tempFile))
				using (var resourceStream = embedded.GetResourceStream())
				{
					resourceStream.CopyTo(fileStream);
				}

				jetifier.Jetify(tempFile, tempFile);

				assembly.MainModule.Resources.Remove(embedded);

				var data = File.ReadAllBytes(tempFile);
				File.Delete(tempFile);

				assembly.MainModule.Resources.Add(new EmbeddedResource(embedded.Name, embedded.Attributes, data));
			}

			return CecilMigrationResult.Skipped;

			bool ShouldJetifyResource(Resource resource) =>
				resource.Name.Equals(EmbeddedAarName, StringComparison.OrdinalIgnoreCase) ||
				Path.GetExtension(resource.Name).Equals(EmbeddedJarExtension, StringComparison.OrdinalIgnoreCase);
		}

		private CecilMigrationResult MigrateNetTypes(AssemblyDefinition assembly)
		{
			var result = CecilMigrationResult.Skipped;

			foreach (var support in assembly.MainModule.GetTypeReferences())
			{
				if (!Mapping.TryGetAndroidXType(support.FullName, out var androidx) || support.FullName == androidx.FullName)
				{
					if (!result.HasFlag(CecilMigrationResult.PotentialJni))
					{
						if (support.FullName == RegisterAttributeFullName)
							result |= CecilMigrationResult.PotentialJni;
					}

					continue;
				}

				if (Verbose)
					Console.WriteLine($"  Processing type reference '{support.FullName}'...");

				var old = support.FullName;
				support.Namespace = androidx.Namespace;

				if (Verbose)
					Console.WriteLine($"    Mapped type '{old}' to '{support.FullName}'.");

				if (!string.IsNullOrWhiteSpace(androidx.Assembly) && support.Scope.Name != androidx.Assembly)
				{
					if (Verbose)
						Console.WriteLine($"    Mapped assembly '{support.Scope.Name}' to '{androidx.Assembly}'.");

					support.Scope.Name = androidx.Assembly;
				}
				else if (support.Scope.Name == androidx.Assembly)
				{
					if (Verbose)
						Console.WriteLine($"    Already mapped assembly '{support.Scope.Name}'.");
				}
				else
				{
					Console.WriteLine($"    *** Potential error for assembly {support.Scope.Name}' to '{androidx.Assembly}'. ***");
				}

				result |= CecilMigrationResult.ContainedSupport;
			}

			return result;
		}

		private CecilMigrationResult MigrateJniStrings(AssemblyDefinition assembly)
		{
			var result = CecilMigrationResult.Skipped;

			foreach (var type in assembly.MainModule.GetTypes())
			{
				// no [Register] attribute means that this type cannot have any JNI
				var registerAttribute = GetRegisterAttribute(type.CustomAttributes);
				if (registerAttribute == null)
					continue;

				if (Verbose)
					Console.WriteLine($"  Processing type '{type.FullName}'...");

				if (MigrateRegisterAttribute(registerAttribute))
					result |= CecilMigrationResult.ContainedJni;

				// migrate the [Register] attributes on the properties
				foreach (var property in type.Properties)
				{
					var propertyRegister = GetRegisterAttribute(property.CustomAttributes);
					if (propertyRegister == null)
						continue;

					if (Verbose)
						Console.WriteLine($"    Processing property '{property.Name}'...");

					if (MigrateRegisterAttribute(propertyRegister))
						result |= CecilMigrationResult.ContainedJni;
				}

				// migrate the [Register] attributes on the methods and the JNI in the bodies
				foreach (var method in type.Methods)
				{
					var methodRegister = GetRegisterAttribute(method.CustomAttributes);

					if (Verbose)
						Console.WriteLine($"    Processing method '{method.Name}'...");

					if (methodRegister != null && MigrateRegisterAttribute(methodRegister))
						result |= CecilMigrationResult.ContainedJni;

					if (MigrateMethodBody(method))
						result |= CecilMigrationResult.ContainedJni;
				}
			}

			return result;
		}

		private bool MigrateRegisterAttribute(CustomAttribute registerAttribute)
		{
			if (!registerAttribute.HasConstructorArguments)
				return false;

			var changed = false;

			var args = registerAttribute.ConstructorArguments;
			for (int i = 0; i < args.Count; i++)
			{
				if (args[i] is CustomAttributeArgument caa &&
					caa.Type.FullName == StringFullName &&
					MigrateJniString(caa.Value as string, out var newValue))
				{
					args[i] = new CustomAttributeArgument(caa.Type, newValue);
					changed = true;
				}
			}

			return changed;
		}

		private bool MigrateMethodBody(MethodDefinition method)
		{
			if (!method.HasBody)
				return false;

			var hasChanges = false;

			foreach (var instruction in method.Body.Instructions)
			{
				if (instruction.OpCode != OpCodes.Ldstr ||
					!(instruction.Operand is string operand) ||
					string.IsNullOrWhiteSpace(operand))
					continue;

				if (MigrateJniString(operand, out var migratedJni))
				{
					instruction.Operand = migratedJni;
					hasChanges = true;
				}
			}

			return hasChanges;
		}

		private CustomAttribute GetRegisterAttribute(IList<CustomAttribute> attributes) =>
			attributes.FirstOrDefault(attr => attr.AttributeType.FullName == RegisterAttributeFullName);

		private bool TryMapJavaClass(string javaClass, out string newJavaClass)
		{
			if (string.IsNullOrEmpty(javaClass))
			{
				newJavaClass = null;
				return false;
			}

			// try it straight
			if (Mapping.TryGetAndroidXClass(javaClass, out var newClass))
			{
				newJavaClass = newClass.JavaFullName;
				return javaClass != newJavaClass;
			}

			// work backwards, removing each nested class
			string nested = "";
			while (javaClass.Contains("$"))
			{
				nested = javaClass.Substring(javaClass.LastIndexOf("$")) + nested;
				javaClass = javaClass.Substring(0, javaClass.LastIndexOf("$"));

				if (Mapping.TryGetAndroidXClass(javaClass, out newClass))
				{
					newJavaClass = newClass.JavaFullName + nested;
					return javaClass != newJavaClass;
				}
			}

			newJavaClass = null;
			return false;
		}

		private bool GetNewJniPart(string javaFullName, out string newJavaFullName)
		{
			newJavaFullName = null;

			if (string.IsNullOrEmpty(javaFullName))
				return false;

			var semicolonIndex = javaFullName.IndexOf(';');
			if (semicolonIndex != javaFullName.Length - 1)
				return false;

			var start = 0;
			while (start < javaFullName.Length && (javaFullName[start] == '[' || javaFullName[start] == 'L'))
				start++;

			if (start <= 0 || start >= semicolonIndex)
				return false;

			var javaClass = javaFullName.Substring(start, semicolonIndex - start);
			if (!TryMapJavaClass(javaClass, out var newFullName))
				return false;

			newJavaFullName = $"{javaFullName.Substring(0, start)}{newFullName};";
			return true;
		}

		private MethodTypeSignature ParseMethodSignature(string support, out string name)
		{
			name = null;

			// must have an open parenthesis
			var openIndex = support.IndexOf('(');
			if (openIndex < 0 || openIndex >= support.Length)
				return null;

			// must have a close parenthesis after the open
			var closeIndex = support.IndexOf(')');
			if (closeIndex <= openIndex || closeIndex >= support.Length)
				return null;

			// try the "(ABC)D"
			if (openIndex == 0)
			{
				name = null;
				return TryCreate(support);
			}

			// try the "name.(ABC)D"
			var dotIndex = support.IndexOf('.');
			if (dotIndex > 0 && dotIndex == openIndex - 1)
			{
				name = support.Substring(0, dotIndex);
				return TryCreate(support.Substring(dotIndex + 1));
			}

			return null;

			MethodTypeSignature TryCreate(string jni)
			{
				try
				{
					return new MethodTypeSignature(jni);
				}
				catch
				{
					return null;
				}
			}
		}
	}
}
