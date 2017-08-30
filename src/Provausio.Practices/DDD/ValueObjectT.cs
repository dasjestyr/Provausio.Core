using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Provausio.Practices.DDD
{
    public abstract class ValueObject<T> : IEquatable<T>
        where T : ValueObject<T>
    {
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var other = obj as T;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            var fields = GetFields().ToList();
            
            const int multiplier = 397;

            unchecked
            {
                var result = fields.First().GetValue(this)?.GetHashCode() ?? 0;
                
                var rest = fields.Skip(1);
                foreach (var field in rest)
                {
                    var value = field.GetValue(this);
                    result = (result * multiplier) ^ (value?.GetHashCode() ?? 0);
                }
                return result;
            }           
        }

        public virtual bool Equals(T other)
        {
            if (other == null)
                return false;

            var t = GetType();
            var otherType = other.GetType();

            if (t != otherType)
                return false;

            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var field in fields)
            {
                var value1 = field.GetValue(other);
                var value2 = field.GetValue(this);

                if (value1 == null)
                {
                    if (value2 != null)
                        return false;
                }

                else if (!value1.Equals(value2))
                    return false;
            }

            return true;

            //return other != null && GetHashCode() == other.GetHashCode();
        }

        private IEnumerable<FieldInfo> GetFields()
        {
            var t = GetType();
            var fields = new List<FieldInfo>();

            while (t != typeof(object))
            {
                if (t == null) continue;

                fields.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
                t = t.BaseType;
            }

            return fields;
        }

        public static bool operator ==(ValueObject<T> x, ValueObject<T> y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (((object)x == null) || ((object)y == null))
            {
                return false;
            }

            return x.Equals(y);
        }

        public static bool operator !=(ValueObject<T> x, ValueObject<T> y)
        {
            return !(x == y);
        }

        
    }

    public class Test
    {
        public string prop1 { get; }
        public string prop2 { get; }
        public string prop3 { get; }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        protected bool Equals(Test other)
        {
            return string.Equals(prop1, other.prop1) && string.Equals(prop2, other.prop2) && string.Equals(prop3, other.prop3);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (prop1 != null ? prop1.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (prop2 != null ? prop2.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (prop3 != null ? prop3.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
