﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VisualFunctions
{
    [Serializable]
    public class Field
    {
        public string FieldName;
        [SerializeReference] public IValue Value;
        
#if UNITY_EDITOR
        public List<SerializableSystemType> SupportedTypes = new();
#endif
        
        public bool AllowRename;
        public bool InEdition;
        public string EditValue;

#if UNITY_EDITOR
        public bool AcceptAnyMethod;

        /**
         * Use this constructor if you want to support a specific type
         */
        public Field(string name, Type type)
        {
            if (!type.GetInterfaces().Contains(typeof(IValue)))
            {
                Debug.LogError($"The type {type} does not implement IValue, the field will not be created");
                return;
            }
            
            FieldName = name;
            Value = (IValue)Activator.CreateInstance(type);
        }

        /**
         * Use this constructor if you want to support specific Reference types
         */
        public Field(string name, SerializableSystemType[] supportedTypes)
        {
            FieldName = name;

            foreach (var type in supportedTypes)
            {
                if (!type.SystemType.GetInterfaces().Contains(typeof(IValue)))
                {
                    Debug.LogError($"The type {type} does not implement IValue, the field will not be created");
                    return;
                }
                
                SupportedTypes.Add(type.SystemType);
            }
        }

        /**
         * Use this constructor if you want to support all Reference types
         */
        public Field(string name)
        {
            FieldName = name;

            foreach (var supportedType in ReferenceUtility.GetAllIValueTypes()) SupportedTypes.Add(supportedType);
        }

        public Field Clone()
        {
            if (MemberwiseClone() is not Field field) return null;
            
            field.Value = field.Value.Clone();
            field.SupportedTypes = new List<SerializableSystemType>(SupportedTypes);
            
            return field;
        }

        /**
         * Allow displaying a button to show all methods from any static class available in the project
         */
        public Field AllowAnyMethod()
        {
            AcceptAnyMethod = true;
            return this;
        }
        
        public Field AllowRenameField()
        {
            AllowRename = true;
            return this;
        }
        
        public void OnEditField(string previousName, string newName)
        {
            if (string.IsNullOrEmpty(newName)) return;

            var formula = Value switch
            {
                CustomValue customValue => customValue.Formula,
                Formula formulaValue => formulaValue.FormulaValue,
                _ => string.Empty
            };

            if (formula != string.Empty)
            {
                var newFormula = "";

                for (var i = 0; i < formula.Length; i++)
                {
                    var currentChar = formula[i];
                    
                    if (formula[i] == '\"' || formula[i] == '\'')
                    {
                        // If the character is a quote, it indicates the start of a string
                        var str = ExpressionUtility.ExtractSurrounded(formula, i);

                        newFormula += formula[i] + str + formula[i];
                        i += str.Length + 1;
                    }
                    else if (char.IsLetter(currentChar))
                    {
                        var variable = ExpressionUtility.ExtractVariable(formula, i);
                        i += variable.Length - 1;

                        if (variable == previousName)
                        {
                            newFormula += newName;
                        }
                        else
                        {
                            newFormula += variable;
                        }
                    }
                    else
                    {
                        newFormula += currentChar;
                    }
                }
                
                switch (Value)
                {
                    case CustomValue customValue1:
                        customValue1.Formula = newFormula;
                        break;
                    case Formula formulaValue1:
                        formulaValue1.FormulaValue = newFormula;
                        break;
                }
            }

            if (previousName != FieldName || !InEdition) return; 
            
            FieldName = newName;
            InEdition = false;
        }
#endif
    }
}