using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Provausio.Core.Logging;
using Provausio.Practices.Validation.Assertion;

namespace Provausio.Practices.EventSourcing
{
    public class EventTypeCache
    {
        private static readonly object Lock = new object();
        private static EventTypeCache _instance;
        private List<Type> _decoratedTypes = new List<Type>();

        private EventTypeCache() { }
        
        public static EventTypeCache GetInstance(string path)
        {
            // double-check lock (seems redundant, but it's not)
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        Logger.Debug($"Event type cache is uninitialized. Initializing now from {path}...", null);
                        _instance = new EventTypeCache();
                        var allTypes = GetAssemblyTypes(path);
                        _instance.SetDecoratedTypes(allTypes);
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <param name="typeRefs">List of types that live in the library you wanna scan.</param>
        /// <returns></returns>
        public static EventTypeCache GetInstance(params Type[] typeRefs)
        {
            // double-check lock (seems redundant, but it's not)
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new EventTypeCache();
                        var allTypes = GetAssemblyTypes(typeRefs);
                        _instance.SetDecoratedTypes(allTypes);
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// Attempts to find the object type that matches the requested namespace.
        /// </summary>
        /// <param name="nameSpaces">The namespaces associated with the object. Semicolon (;) delimited. NOTE: "namespace" refers to the value of <see cref="EventNamespaceAttribute"/></param>
        /// <param name="throwOnNotFound">if set to <c>true</c> [throw on not found]. If false, method will return null.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public Type ResolveType(string nameSpaces, bool throwOnNotFound = true)
        {
            Ensure.IsNotNullOrEmpty(nameSpaces, nameof(nameSpaces));

            var namespacesThatCanMatch = nameSpaces.Split(';');
            
            var matchingTypes = new List<Type>();
            foreach (var type in _decoratedTypes)
            {
                // we're just checking each type to see if at least one of 
                // the namespaces match the attribute value
                try
                {
                    var attributes = type
                        .GetCustomAttributes(typeof(EventNamespaceAttribute), false)
                        .FirstOrDefault(
                            t => namespacesThatCanMatch.Contains(
                                ((EventNamespaceAttribute) t).Namespace)); 

                    if (attributes != null)
                        matchingTypes.Add(type);
                }
                catch (InvalidOperationException ex)
                {
                    var x = type
                        .GetCustomAttributes(typeof(EventNamespaceAttribute), false)
                        .Where(member => namespacesThatCanMatch.Contains(((EventNamespaceAttribute) member).Namespace))
                        .Select(o => o.GetType().FullName);

                    Logger.Fatal($"Failed to gather EventNamespaceAttribute for {nameSpaces}. The following types were found to host the specified namespaces: {string.Join(", ", x)}", this, ex);
                    throw;
                }
            }

            if (matchingTypes.Count == 1)
                return matchingTypes.First();

            if (matchingTypes.Count > 1)
                throw new InvalidOperationException($"More than 1 object was found with the namespace '{nameSpaces}'");

            if (throwOnNotFound)
                throw new Exception($"No objects were found with the event namespace '{nameSpaces}'");

            return null;
        }

        private static IEnumerable<Type> GetAssemblyTypes(params Type[] typeRefs)
        {
            var types = new List<Type>();
            foreach (var type in typeRefs)
            {
                var eventInfoTypes = GetTypes(type.Assembly);
                types.AddRange(eventInfoTypes);
            }

            return types;
        }

        private static IEnumerable<Type> GetTypes(Assembly assy)
        {
            Logger.Debug($"Scanning types in {assy.FullName}", null);
            var eventInfoTypes = assy
                    .GetTypes()
                    .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(EventInfo)));

            return eventInfoTypes;
        }

        private static IEnumerable<Type> GetAssemblyTypes(string path)
        {
            try
            {
                var paths = Directory.GetFiles(path, "*.dll");

                Logger.Debug($"Attempting to load {string.Join(",", paths)}", null);

                var types = new List<Type>();
                foreach (var filePath in paths)
                {
                    var assy = Assembly.LoadFile(filePath);
                    var eventInfoTypes = GetTypes(assy);
                    types.AddRange(eventInfoTypes);
                }

                Logger.Debug($"Found {types.Count} types in {paths.Length} assemblies at {path}.", null);
                return types;
            }
            catch (ReflectionTypeLoadException ex)
            {
                var message = ex.LoaderExceptions.Select(e => e.Message);
                Logger.Fatal($"Failed to load assembly: \r\n{message}", null, ex);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, null, ex);
                throw;
            }
        }

        private void SetDecoratedTypes(IEnumerable<Type> types)
        {
            if(types == null || !types.Any())
            {
                _decoratedTypes = new List<Type>();
                return;
            }

            _decoratedTypes = types
                .Where(t => t.GetCustomAttributes(typeof(EventNamespaceAttribute), false).Length > 0 && t.IsSubclassOf(typeof(EventInfo)))
                .ToList();

            var typeNames = string.Join(",", _decoratedTypes.Select(t => t.Name));
            Logger.Debug($"Cached {_decoratedTypes.Count} decorated event types: {typeNames}", this);
        }
    }
}