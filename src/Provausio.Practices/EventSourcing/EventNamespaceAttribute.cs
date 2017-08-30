using System;

namespace Provausio.Practices.EventSourcing
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EventNamespaceAttribute : Attribute
    {
        public string Namespace { get; set; }

        public EventNamespaceAttribute(string nameSpace)
        {
            Namespace = nameSpace;
        }
    }
}
