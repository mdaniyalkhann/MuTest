﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MuTest.DynamicAsserts.ArrayExtensions;

namespace MuTest.DynamicAsserts
{
    public static class ObjectExtensions
    {
        private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
        private static bool _compareChildren;

        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }

            return (type.IsValueType & type.IsPrimitive);
        }

        private static object Copy(this object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<object, object>(new ReferenceEqualityComparer()));
        }

        private static object InternalCopy(object originalObject, IDictionary<object, object> visited)
        {
            if (originalObject == null)
            {
                return null;
            }

            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect))
            {
                return originalObject;
            }

            if (visited.ContainsKey(originalObject))
            {
                return visited[originalObject];
            }

            if (typeof(Delegate).IsAssignableFrom(typeToReflect))
            {
                return null;
            }

            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (IsPrimitive(arrayType) == false)
                {
                    Array clonedArray = (Array)cloneObject;
                    clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                }

            }

            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);

            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static void CopyFields(
            object originalObject,
            IDictionary<object, object> visited,
            object cloneObject,
            IReflect typeToReflect,
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            var validFields = typeToReflect.GetFields(bindingFlags).Where(x => x.IsValidField()).ToList();
            foreach (FieldInfo fieldInfo in validFields)
            {
                if (filter != null && filter(fieldInfo) == false) continue;
                if (IsPrimitive(fieldInfo.FieldType)) continue;
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }

        public static T Copy<T>(this T original, bool onlySimpleTypes)
        {
            _compareChildren = onlySimpleTypes;
            return (T)Copy(original);
        }

        private static bool IsValidField(this FieldInfo fieldInfo)
        {
            var fullName = fieldInfo.FieldType.FullName;

            if (!_compareChildren)
            {
                return fieldInfo.FieldType.IsSimple() ||
                       fieldInfo.FieldType.IsACollection() ||
                       fieldInfo.FieldType.IsAction() ||
                       fieldInfo.FieldType.IsObject();
            }

            return fullName != null &&
                   !fullName.Contains("CultureInfo") &&
                   !fullName.Contains("DataSet") &&
                   !fullName.Contains("DataTable") &&
                   !fullName.Contains("DropDownList") &&
                   !fullName.Contains("IDisposable") &&
                   !fullName.Contains("Microsoft.Win32.SafeHandles.SafeFileMappingHandle") &&
                   !fullName.Contains("Microsoft.Win32.SafeHandles.SafeViewOfFileHandle") &&
                   !fullName.Contains("Microsoft.Win32.SafeHandles.SafeWaitHandle") &&
                   !fullName.Contains("PrivateObject") &&
                   !fullName.Contains("PrivateType") &&
                   !fullName.Contains("RequiredFieldValidator") &&
                   !fullName.Contains("Shim") &&
                   !fullName.Contains("System.IO.Stream") &&
                   !fullName.Contains("System.IntPtr") &&
                   !fullName.Contains("System.Reflection.INVOCATION_FLAGS") &&
                   !fullName.Contains("System.Runtime") &&
                   !fullName.Contains("System.Threading") &&
                   !fullName.Contains("System.Void*") &&
                   !fullName.Contains("log4net");
        }
    }

    public class ReferenceEqualityComparer : EqualityComparer<object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }
        public override int GetHashCode(object obj)
        {
            if (obj == null) return 0;
            return obj.GetHashCode();
        }
    }

    namespace ArrayExtensions
    {
        public static class ArrayExtensions
        {
            public static void ForEach(this Array array, Action<Array, int[]> action)
            {
                if (array.LongLength == 0) return;
                ArrayTraverse walker = new ArrayTraverse(array);
                do action(array, walker.Position);
                while (walker.Step());
            }
        }

        internal class ArrayTraverse
        {
            public int[] Position { get; set; }
            private readonly int[] _maxLengths;

            public ArrayTraverse(Array array)
            {
                _maxLengths = new int[array.Rank];
                for (var i = 0; i < array.Rank; ++i)
                {
                    _maxLengths[i] = array.GetLength(i) - 1;
                }
                Position = new int[array.Rank];
            }

            public bool Step()
            {
                for (var i = 0; i < Position.Length; ++i)
                {
                    if (Position[i] < _maxLengths[i])
                    {
                        Position[i]++;
                        for (var j = 0; j < i; j++)
                        {
                            Position[j] = 0;
                        }
                        return true;
                    }
                }
                return false;
            }
        }
    }

}