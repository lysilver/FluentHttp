using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentHttp.SourceGenerator
{
    public class ClassModel
    {
        public ClassModel(string name, string nameSpace, EquatableList<InterfaceModel> interfaces)
        {
            Name = name;
            NameSpace = nameSpace;
            Interfaces = interfaces;
        }

        public string Name { get; set; }
        public string NameSpace { get; set; }
        public EquatableList<InterfaceModel> Interfaces { get; set; }
    }

    public class InterfaceModel
    {
        public string FullyQualifiedName { get; set; }
        public EquatableList<string> Properties { get; set; }

        public InterfaceModel(string fullyQualifiedName, EquatableList<string> properties)
        {
            FullyQualifiedName = fullyQualifiedName;
            Properties = properties;
        }
    }

    public class EquatableList<T> : List<T>, IEquatable<EquatableList<T>>
    {
        public bool Equals(EquatableList<T>? other)
        {
            // If the other list is null or a different size, they're not equal
            if (other is null || Count != other.Count)
            {
                return false;
            }

            // Compare each pair of elements for equality
            for (int i = 0; i < Count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(this[i], other[i]))
                {
                    return false;
                }
            }

            // If we got this far, the lists are equal
            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EquatableList<T>);
        }

        public override int GetHashCode()
        {
            return this.Select(item => item?.GetHashCode() ?? 0).Aggregate((x, y) => x ^ y);
        }

        public static bool operator ==(EquatableList<T> list1, EquatableList<T> list2)
        {
            return ReferenceEquals(list1, list2)
                || list1 is not null && list2 is not null && list1.Equals(list2);
        }

        public static bool operator !=(EquatableList<T> list1, EquatableList<T> list2)
        {
            return !(list1 == list2);
        }
    }
}