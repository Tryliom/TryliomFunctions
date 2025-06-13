﻿using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualFunctions
{
    [Serializable]
    public class ListOf<TType> : IValue<List<TType>>
    {
        public List<TType> ListValue = new ();
        
        object IValue.Value
        {
            get => Value;
            set => Value = (List<TType>) value;
        }

        public List<TType> Value
        {
            get => ListValue;
            set => ListValue = value;
        }
        public Type Type => typeof(List<TType>);
        
        public IValue Clone()
        {
            return new ListOf<TType>
            {
                Value = new List<TType>(Value)
            };
        }
    }
    
    [Serializable]
    public class ListOf : IRefType, IRefValue
    {
        [SerializeReference] public IValue ListValue;
        
        public Type Type => ListValue != null ? ListValue.Type : typeof(ListOf);
        
        public object RefValue
        {
            get => ListValue?.Value;
            set => ListValue.Value = value;
        }
        
        public void SetList(Type type)
        {
            var listType = typeof(ListOf<>).MakeGenericType(type);
            var listValue = Activator.CreateInstance(listType);
            ListValue = (IValue) listValue;
        }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ListOf))]
    public class ListOfDrawer : PropertyDrawer
    {
        private SerializedProperty _property;
        private GameObject _targetObject;
        private ListOf _listOf;
        
        private void Refresh()
        {
            if (_property == null || _targetObject == null) return;

            PropertyDrawerUtility.SaveAndRefresh(_property, _targetObject);
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _property = property;
            _targetObject = Selection.activeObject as GameObject;
            _listOf = _property.serializedObject.targetObject is ListOfVariable comp ? comp.Value : (ListOf) PropertyDrawerUtility.RetrieveTargetObject(_property);
            
            var isListDefined = _listOf.ListValue is not null;
            var buttonText = isListDefined ? "..." : "Select Type";
            var buttonWidth = isListDefined ? 20 : buttonText.Length * 8f;

            position.height = EditorGUIUtility.singleLineHeight;
            
            if (GUI.Button(new Rect(position.x, position.y, buttonWidth, position.height), buttonText))
            {
                var menu = new GenericMenu();
                
                AddItemsToMenu(menu, "Popular", ExpressionUtility.PopularTypes);
                AddItemsToMenu(menu, "Unity", ExpressionUtility.UnityTypes);
                AddItemsToMenu(menu, "Other", ExpressionUtility.OtherTypes);

                menu.ShowAsContext();
            }

            if (isListDefined)
            {
                position.x += buttonWidth + 5f;
                position.width -= buttonWidth + 5f;
                
                var listValue = _property.FindPropertyRelative("ListValue");
                EditorGUI.PropertyField(position, listValue, true);
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var listValue = property.FindPropertyRelative("ListValue");

            return EditorGUI.GetPropertyHeight(listValue, true);
        }

        private void AddItemsToMenu(GenericMenu menu, string prefix, List<Type> types)
        {
            if (_listOf.ListValue == null)
            {
                menu.AddItem(new GUIContent("None"), true, () => {});
            }
            
            foreach (var type in types)
            {
                var name = ObjectNames.NicifyVariableName(ExpressionUtility.GetBetterTypeName(type));
                var startStr = prefix != string.Empty ? $"{prefix}/" : "";
                var namespaceParts = type.FullName?.Split(".") ?? Array.Empty<string>();

                for (var i = 0; i < namespaceParts.Length - 1; i++)
                {
                    startStr += namespaceParts[i] + "/";
                }
                
                startStr += name;
                
                menu.AddItem(new GUIContent(startStr), _listOf.ListValue?.Type == type, () =>
                {
                    _listOf.SetList(type);
                    Refresh();
                });
            }
        }
    }
    
    [CustomPropertyDrawer(typeof(ListOf<>))]
    public class ListOfGenericDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var listValue = property.FindPropertyRelative("ListValue");
            
            if (listValue == null)
            {
                EditorGUI.LabelField(position, "No GUI available for this type");
                return;
            }
            
            EditorGUI.PropertyField(position, listValue, new GUIContent("List of " + ObjectNames.NicifyVariableName(listValue.arrayElementType).Replace("P Ptr$", "")));
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var listValue = property.FindPropertyRelative("ListValue");

            return listValue switch
            {
                null => EditorGUIUtility.singleLineHeight,
                { isExpanded: true } => EditorGUI.GetPropertyHeight(listValue, true),
                _ => EditorGUIUtility.singleLineHeight
            };
        }
    }
#endif
}