using System;
using Provausio.Practices.Validation.Assertion;

namespace Provausio.Practices.EventSourcing.Deserialization
{
    public class EventNamespace
    {
        public string AssemblyQualifiedName { get; }

        public string SimplifiedName { get; }
        
        public EventNamespace(string assemblyQualifiedName)
        {
            AssemblyQualifiedName = Ensure.IsNotNullOrEmpty(assemblyQualifiedName, nameof(assemblyQualifiedName));
            SimplifiedName = GetSimpleNamespace();
        }
        
        public string GetSimpleNamespace()
        {
            var parts = AssemblyQualifiedName.Split(',');
            if (parts.Length < 3)
                throw new FormatException($"Unexpected format for assembly qualified name: {AssemblyQualifiedName}");

            var className = parts[0].Trim();
            var assemblyName = parts[1].Trim();
            var version = parts[2].Trim();

            var v = new Version(version.Split('=')[1]);

            return $"{className}, {assemblyName}, Version={v.Major}.{v.Minor}";
        }

        public override bool Equals(object obj)
        {
            return obj != null && Equals(obj as EventNamespace);
        }

        protected bool Equals(EventNamespace other)
        {
            return string.Equals(AssemblyQualifiedName, other.AssemblyQualifiedName) && string.Equals(SimplifiedName, other.SimplifiedName);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((AssemblyQualifiedName?.GetHashCode() ?? 0) * 397) ^ (SimplifiedName?.GetHashCode() ?? 0);
            }
        }
    }
}