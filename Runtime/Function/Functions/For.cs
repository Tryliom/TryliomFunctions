﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualFunctions
{
    [Serializable]
    public class For : Function
    {
        public static readonly string Name = "For";
        public static readonly string Description = "It will execute the functions inside the loop a number of times.\nIndex is the current loop index, it's reset to 0 at the start of the loop.";
        public static readonly FunctionCategory Category = FunctionCategory.Executor;
        
        public Functions FunctionsToLoop = new Functions().DisableGlobalVariables().DisableImport();

#if UNITY_EDITOR
        public override void GenerateFields()
        {
            Inputs.Add(new Field("Loops", typeof(IntReference)).AllowRenameField());
            Inputs.Add(new Field("Index", typeof(IntReference)).AllowRenameField());
            EditableAttributes.Add(nameof(FunctionsToLoop));
        }
#endif

        protected override bool Process(List<IVariable> variables)
        {
            var loops = (IntReference) Inputs[0].Value;
            var index = (IntReference) Inputs[1].Value;

            for (index.Value = 0; index.Value < loops.Value; index.Value++)
            {
                if (FunctionsToLoop.FunctionsList.Any(function => !function.Invoke(variables))) break;
            }

            return true;
        }
    }
}