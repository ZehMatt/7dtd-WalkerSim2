using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace CheckApiCompat
{
    internal static class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("usage: CheckApiCompat <mod.dll> <gameManagedDir> [targetAssembly=Assembly-CSharp]");
                return 2;
            }

            string modPath = Path.GetFullPath(args[0]);
            string managedDir = Path.GetFullPath(args[1]);
            string target = args.Length > 2 ? args[2] : "Assembly-CSharp";

            if (!File.Exists(modPath))
            {
                Console.Error.WriteLine("mod assembly not found: " + modPath);
                return 2;
            }
            if (!Directory.Exists(managedDir))
            {
                Console.Error.WriteLine("game managed dir not found: " + managedDir);
                return 2;
            }

            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(managedDir);
            resolver.AddSearchDirectory(Path.GetDirectoryName(modPath));

            var readerParams = new ReaderParameters { AssemblyResolver = resolver };

            ModuleDefinition module;
            try
            {
                module = ModuleDefinition.ReadModule(modPath, readerParams);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("failed to read mod assembly: " + ex.Message);
                return 2;
            }

            var missingTypes = new SortedSet<string>(StringComparer.Ordinal);
            var missingMembers = new SortedSet<string>(StringComparer.Ordinal);
            int typeChecks = 0;
            int memberChecks = 0;

            foreach (var typeRef in module.GetTypeReferences())
            {
                if (!IsTarget(typeRef.Scope, target))
                    continue;

                typeChecks++;
                if (SafeResolveType(typeRef) == null)
                    missingTypes.Add(typeRef.FullName);
            }

            foreach (var member in module.GetMemberReferences())
            {
                var declaring = member.DeclaringType;
                if (declaring == null || !IsTarget(declaring.Scope, target))
                    continue;

                if (SafeResolveType(declaring) == null)
                {
                    missingTypes.Add(declaring.FullName);
                    continue;
                }

                memberChecks++;

                if (member is MethodReference methodRef)
                {
                    if (SafeResolveMethod(methodRef) == null)
                        missingMembers.Add("METHOD " + methodRef.FullName);
                }
                else if (member is FieldReference fieldRef)
                {
                    if (SafeResolveField(fieldRef) == null)
                        missingMembers.Add("FIELD  " + fieldRef.FullName);
                }
            }

            Console.WriteLine($"  checked {typeChecks} type refs, {memberChecks} member refs against {target}");

            if (missingTypes.Count == 0 && missingMembers.Count == 0)
            {
                Console.WriteLine("  OK: all references resolve");
                return 0;
            }

            foreach (var t in missingTypes)
                Console.WriteLine("  MISSING TYPE   " + t);
            foreach (var m in missingMembers)
                Console.WriteLine("  MISSING " + m);

            Console.WriteLine($"  FAIL: {missingTypes.Count} missing type(s), {missingMembers.Count} missing member(s)");
            return 1;
        }

        static bool IsTarget(IMetadataScope scope, string target)
        {
            return scope != null && string.Equals(scope.Name, target, StringComparison.OrdinalIgnoreCase);
        }

        static TypeDefinition SafeResolveType(TypeReference typeRef)
        {
            try { return typeRef.Resolve(); }
            catch { return null; }
        }

        static MethodDefinition SafeResolveMethod(MethodReference methodRef)
        {
            try { return methodRef.Resolve(); }
            catch { return null; }
        }

        static FieldDefinition SafeResolveField(FieldReference fieldRef)
        {
            try { return fieldRef.Resolve(); }
            catch { return null; }
        }
    }
}
