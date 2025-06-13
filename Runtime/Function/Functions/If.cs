﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualFunctions
{
    [Serializable]
    public class If : Function
    {
        public static readonly string Name = "If";
        public static readonly string Description = "If the condition is true, it will execute the if branch, otherwise it will execute the else branch.\n" +
                                                    "If there is multiple lines (;), it will check the ones that are a boolean.";
        public static readonly FunctionCategory Category = FunctionCategory.Executor;

        public Functions Then = new Functions().DisableGlobalVariables().DisableImport();
        public Functions Otherwise = new Functions().DisableGlobalVariables().DisableImport();
        
        private List<IVariable> _globalVariables = new();

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            EditableAttributes.Add(nameof(Then));
            EditableAttributes.Add(nameof(Otherwise));
            Inputs.Add(new Field("Condition", typeof(Formula)).AllowAnyMethod());
            AllowAddInput(new FunctionSettings().AllowMethods());
        }
#endif

        protected override bool Process(List<IVariable> variables)
        {
            _globalVariables.Clear();
            _globalVariables.Capacity = variables.Count + Inputs.Count;
            _globalVariables.AddRange(variables);
            _globalVariables.AddRange(Inputs);
            
            if (CheckCondition())
            {
                _globalVariables.AddRange(Then.GlobalVariables);
                
                if (Then.FunctionsList.Any(function => !function.Invoke(_globalVariables))) return false;
            }
            else
            {
                _globalVariables.AddRange(Otherwise.GlobalVariables);
                
                if (Otherwise.FunctionsList.Any(function => !function.Invoke(_globalVariables))) return false;
            }

            return true;
        }

        private bool CheckCondition()
        {
            var formula = GetInput<string>("Condition").Value;
            var results = Evaluator.Process(Uid, formula, _globalVariables);

            foreach (var result in results)
            {
                if (ExpressionUtility.ExtractValue(result, Uid, _globalVariables) is not bool res) continue;
                if (!res) return false;
            }
            
            return true;
        }
    }
}