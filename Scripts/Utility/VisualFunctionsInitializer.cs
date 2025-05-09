﻿using UnityEditor;
using UnityEngine;

namespace VisualFunctions
{
    [InitializeOnLoad]
    public static class VisualFunctionsInitializer
    {
        static VisualFunctionsInitializer()
        {
            LoadSettings();
        }

        public static void LoadSettings()
        {
            var settings = Resources.Load<VisualFunctionsSettings>("");
            
            if (!settings)
            {
                Debug.LogWarning("VisualFunctionsSettings asset not found. Using default values.");
                return;
            }

            GlobalSettings.PathToGlobalVariables = settings.PathToGlobalVariables;
            GlobalSettings.PathToVariables = settings.PathToVariables;
            GlobalSettings.GlobalValuesPrefix = settings.GlobalValuesPrefix;
        }
    }
}