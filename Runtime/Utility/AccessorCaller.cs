﻿using System;
using System.Collections.Generic;

namespace VisualFunctions
{
    public enum AccessorType
    {
        Property,
        Method,
        Constructor,
        CustomFunction,
        ArrayItem,
    }

    public class AccessorCaller
    {
        public readonly AccessorType AccessorType;
        public readonly List<Type> GenericTypes;
        public readonly IValue Instance;
        public readonly string LeftMethod;
        public readonly List<string> Parameters;
        public readonly string Property;
        
        // Used for array item accessors to access the array indexes
        public List<object> ArrayIndexes { get; set; } = new ();

        public AccessorCaller(IValue instance, string property, string leftMethod)
        {
            Instance = instance;
            Property = property;
            LeftMethod = leftMethod;
            AccessorType = AccessorType.Property;
            Parameters = new List<string>();
            GenericTypes = new List<Type>();
        }

        public AccessorCaller(IValue instance, string property, List<string> parameters, string leftMethod, List<Type> genericTypes)
        {
            Instance = instance;
            Property = property;
            LeftMethod = leftMethod;
            AccessorType = AccessorType.Method;
            Parameters = parameters;
            GenericTypes = genericTypes ?? new List<Type>();
        }
        
        public AccessorCaller(IValue instance, List<string> parameters, string leftMethod, List<Type> genericTypes)
        {
            Instance = instance;
            LeftMethod = leftMethod;
            AccessorType = AccessorType.Constructor;
            Parameters = parameters;
            GenericTypes = genericTypes ?? new List<Type>();
        }
        
        public AccessorCaller(IValue instance, List<string> parameters, string leftMethod, AccessorType accessorType)
        {
            Instance = instance;
            LeftMethod = leftMethod;
            AccessorType = accessorType;
            Parameters = parameters;
        }

        public IValue Result { get; set; }

        public void AssignValue(object value)
        {
            if (AccessorType is AccessorType.ArrayItem)
            {
                var callValue = Instance.Value is IRefValue refValue ? refValue.RefValue : value;
                var callType = Instance.Value is IRefType refType ? refType.Type : Instance.Type;
                
                callType.GetProperty("Item")?.SetValue(callValue, value, ArrayIndexes.ToArray());
            }
            
            if (AccessorType is not AccessorType.Property) return;

            var instanceType = Instance.Type;
            var propertyInfo = instanceType.GetProperty(Property);
            var fieldInfo = instanceType.GetField(Property);

            if (propertyInfo != null)
            {
                if (!propertyInfo.CanWrite)
                {
                    throw new Exception($"Property '{Property}' is read-only from type '{instanceType.Name}'.");
                }
                
                if (instanceType.IsValueType && !instanceType.IsGenericType)
                {
                    var tempValue = Instance.Value is IRefValue refValue ? refValue.RefValue : Instance.Value;
                    propertyInfo.SetValue(tempValue, value);
                    Instance.Value = tempValue;
                }
                else
                {
                    propertyInfo.SetValue(Instance.Value switch
                    {
                        AccessorCaller caller => Result.Value,
                        IRefValue refValue => refValue.RefValue,
                        _ => Instance.Value
                    }, value);
                }
            }
            else if (fieldInfo != null)
            {
                if (fieldInfo.IsInitOnly)
                {
                    throw new Exception($"Field '{Property}' is read-only from type '{instanceType.Name}'.");
                }
                
                if (instanceType.IsValueType && !instanceType.IsGenericType)
                {
                    var tempValue = Instance.Value is IRefValue refValue ? refValue.RefValue : Instance.Value;
                    fieldInfo.SetValue(tempValue, value);
                    Instance.Value = tempValue;
                }
                else
                {
                    fieldInfo.SetValue(Instance.Value switch
                    {
                        AccessorCaller caller => Result.Value,
                        IRefValue refValue => refValue.RefValue,
                        _ => Instance.Value
                    }, value);
                }
            }
            else
            {
                throw new Exception($"Property or field '{Property}' not found in type '{instanceType.Name}'.");
            }
        }
    }
}