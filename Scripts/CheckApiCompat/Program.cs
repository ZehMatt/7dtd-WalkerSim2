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

            var contractFailures = new SortedSet<string>(StringComparer.Ordinal);
            int contractChecks = CheckContracts(module, managedDir, target, contractFailures);

            string tally = $"  checked {typeChecks} type refs, {memberChecks} member refs";
            if (contractChecks > 0)
                tally += $", {contractChecks} contracts";
            Console.WriteLine(tally + $" against {target}");

            if (missingTypes.Count == 0 && missingMembers.Count == 0 && contractFailures.Count == 0)
            {
                Console.WriteLine("  OK: all references resolve");
                return 0;
            }

            foreach (var t in missingTypes)
                Console.WriteLine("  MISSING TYPE   " + t);
            foreach (var m in missingMembers)
                Console.WriteLine("  MISSING " + m);
            foreach (var c in contractFailures)
                Console.WriteLine("  MISSING " + c);

            Console.WriteLine($"  FAIL: {missingTypes.Count} missing type(s), {missingMembers.Count} missing member(s), {contractFailures.Count} broken contract(s)");
            return 1;
        }

        static int CheckContracts(ModuleDefinition modModule, string managedDir, string target, ISet<string> failures)
        {
            var gameApi = modModule.GetType("WalkerSim.GameApi");
            if (gameApi == null)
                return 0;

            var members = gameApi.Fields
                .Where(f => f.IsStatic && f.FieldType is GenericInstanceType git
                    && (git.ElementType.Name.StartsWith("GameMethod") || git.ElementType.Name.StartsWith("GameFunc")))
                .ToList();
            if (members.Count == 0)
                return 0;

            string dll = Path.Combine(managedDir, target + ".dll");
            using var gameModule = ModuleDefinition.ReadModule(dll);

            foreach (var f in members)
            {
                var git = (GenericInstanceType)f.FieldType;
                bool isFunc = git.ElementType.Name.StartsWith("GameFunc");
                var generics = git.GenericArguments;
                var declaring = generics[0];
                TypeReference returnType = isFunc ? generics[1] : null;
                int argStart = isFunc ? 2 : 1;
                var argTypes = generics.Skip(argStart).ToList();

                string methodName = f.Name;
                string firstParam = argTypes.Count > 0 ? argTypes[0].Name : null;
                int minParams = argTypes.Count;

                var type = gameModule.GetType(declaring.FullName);
                if (type == null)
                {
                    failures.Add($"CONTRACT type {declaring.FullName}");
                    continue;
                }

                bool ok = type.Methods.Any(m => m.Name == methodName
                    && m.Parameters.Count >= minParams
                    && (firstParam == null || (m.Parameters.Count > 0 && m.Parameters[0].ParameterType.Name == firstParam))
                    && (returnType == null || m.ReturnType.Name == returnType.Name));
                if (!ok)
                {
                    string ret = isFunc ? $", returns {returnType.Name}" : "";
                    failures.Add($"CONTRACT {declaring.Name}.{methodName}(first={firstParam ?? "any"}, >={minParams} params{ret})");
                }
            }
            return members.Count;
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
