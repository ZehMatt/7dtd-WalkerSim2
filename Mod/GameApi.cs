using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WalkerSim
{
    // Game APIs whose signature drifts across versions are declared once as strongly typed fields on
    // GameApi and invoked through them (e.g. GameApi.RemoveBuff.Invoke(buffs, name)). The field name is
    // the game method name; the generic arguments are the declaring type, return type (GameFunc) and the
    // leading argument types. The checker (Scripts/CheckApiCompat) reads these same fields from the built
    // assembly and verifies every pinned game version still has a matching method.
    internal abstract class GameMember
    {
        internal string Name { get; private set; }
        internal bool IsResolved => _method != null;

        MethodInfo _method;

        protected abstract bool IsFunc { get; }

        internal void Bind(string name)
        {
            Name = name;
            var generics = GetType().GetGenericArguments();
            var declaringType = generics[0];
            int argStart = IsFunc ? 2 : 1;
            var argTypes = generics.Skip(argStart).ToArray();

            _method = declaringType.GetMethods().FirstOrDefault(m =>
                m.Name == name &&
                m.GetParameters().Length >= argTypes.Length &&
                (argTypes.Length == 0 || m.GetParameters()[0].ParameterType == argTypes[0]));
        }

        protected object InvokeCore(object instance, object[] leadingArgs)
        {
            if (_method == null)
            {
                Logging.Warn("No resolved game API for {0}", Name);
                return null;
            }

            var parameters = _method.GetParameters();
            var args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i < leadingArgs.Length)
                {
                    args[i] = leadingArgs[i];
                    continue;
                }
                var parameter = parameters[i];
                if (parameter.HasDefaultValue)
                    args[i] = parameter.DefaultValue;
                else if (parameter.ParameterType.IsValueType)
                    args[i] = Activator.CreateInstance(parameter.ParameterType);
                else
                    args[i] = null;
            }

            return _method.Invoke(instance, args);
        }
    }

    internal sealed class GameMethod<TDeclaring> : GameMember
    {
        protected override bool IsFunc => false;
        public void Invoke(TDeclaring instance) => InvokeCore(instance, Array.Empty<object>());
    }

    internal sealed class GameMethod<TDeclaring, A1> : GameMember
    {
        protected override bool IsFunc => false;
        public void Invoke(TDeclaring instance, A1 a1) => InvokeCore(instance, new object[] { a1 });
    }

    internal sealed class GameMethod<TDeclaring, A1, A2> : GameMember
    {
        protected override bool IsFunc => false;
        public void Invoke(TDeclaring instance, A1 a1, A2 a2) => InvokeCore(instance, new object[] { a1, a2 });
    }

    internal sealed class GameFunc<TDeclaring, TReturn> : GameMember
    {
        protected override bool IsFunc => true;
        public TReturn Invoke(TDeclaring instance)
        {
            var result = InvokeCore(instance, Array.Empty<object>());
            return result == null ? default : (TReturn)result;
        }
    }

    internal sealed class GameFunc<TDeclaring, TReturn, A1> : GameMember
    {
        protected override bool IsFunc => true;
        public TReturn Invoke(TDeclaring instance, A1 a1)
        {
            var result = InvokeCore(instance, new object[] { a1 });
            return result == null ? default : (TReturn)result;
        }
    }

    internal sealed class GameFunc<TDeclaring, TReturn, A1, A2> : GameMember
    {
        protected override bool IsFunc => true;
        public TReturn Invoke(TDeclaring instance, A1 a1, A2 a2)
        {
            var result = InvokeCore(instance, new object[] { a1, a2 });
            return result == null ? default : (TReturn)result;
        }
    }

    internal static class GameApi
    {
        public static readonly GameMethod<EntityBuffs, string> RemoveBuff =
            new GameMethod<EntityBuffs, string>();

        public static readonly GameFunc<PathAbstractions.SearchDefinition, PathAbstractions.AbstractedLocation, string> GetLocation =
            new GameFunc<PathAbstractions.SearchDefinition, PathAbstractions.AbstractedLocation, string>();

        static GameApi()
        {
            foreach (var field in typeof(GameApi).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.GetValue(null) is GameMember member)
                    member.Bind(field.Name);
            }
        }
    }
}
