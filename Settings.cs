/*
 * Settings.cs
 * PerformanceMeter
 *
 * This file defines the settings panel controller.
 *
 * This code is licensed under the MIT license.
 * Copyright (c) 2021-2022 JackMacWindows.
 */

using BeatSaberMarkupLanguage.Attributes;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace PerformanceMeter {
    internal class Settings : PersistentSingleton<Settings> {
        [UIValue("mode-options")]
        public List<object> modeOptions = new object[] { "Energy", "Percentage (Modified)", "Percentage (Raw)", "Note Cut Value", "Average Cut Value" }.ToList();
        [UIValue("secondary-mode-options")]
        public List<object> secondaryModeOptions = new object[] { "Energy", "Percentage (Modified)", "Percentage (Raw)", "Note Cut Value", "Average Cut Value", "None" }.ToList();
        [UIValue("side-options")]
        public List<object> sideOptions = new object[] { "Left", "Right", "Both" }.ToList();

        [UIValue("mode")]
        public string listChoice = "Energy";

        [UIValue("side")]
        public string sideChoice = "Both";

        [UIValue("secondaryMode")]
        public string secondaryListChoice = "None";

        [UIValue("secondarySide")]
        public string secondarySideChoice = "Both";

        [UIValue("enabled")]
        public bool _enabled = PluginConfig.Instance.enabled;

        [UIValue("showMisses")]
        public bool showMisses = PluginConfig.Instance.showMisses;

        [UIValue("animationDuration")]
        public float animationDuration = PluginConfig.Instance.animationDuration;

        [UIValue("overrideColor")]
        public bool overrideColor = PluginConfig.Instance.overrideColor;

        [UIValue("overrideSecondaryColor")]
        public bool overrideSecondaryColor = PluginConfig.Instance.overrideSecondaryColor;

        [UIValue("color")]
        public UnityEngine.Color color = PluginConfig.Instance.color;

        [UIValue("secondaryColor")]
        public UnityEngine.Color secondaryColor = PluginConfig.Instance.secondaryColor;

        [UIAction("#apply")]
        public void OnApply() {
            PluginConfig.Instance.enabled = _enabled;
            PluginConfig.Instance.showMisses = showMisses;
            PluginConfig.Instance.animationDuration = animationDuration;
            PluginConfig.Instance.overrideColor = overrideColor;
            PluginConfig.Instance.overrideSecondaryColor = overrideSecondaryColor;
            PluginConfig.Instance.color = color;
            PluginConfig.Instance.secondaryColor = secondaryColor;
            PluginConfig.Instance.mode = (PluginConfig.MeasurementMode)modeOptions.FindIndex(a => a.ToString() == listChoice);
            PluginConfig.Instance.secondaryMode = (PluginConfig.MeasurementMode)secondaryModeOptions.FindIndex(a => a.ToString() == secondaryListChoice);
            PluginConfig.Instance.side = (PluginConfig.MeasurementSide)sideOptions.FindIndex(a => a.ToString() == sideChoice);
            PluginConfig.Instance.secondarySide = (PluginConfig.MeasurementSide)sideOptions.FindIndex(a => a.ToString() == secondarySideChoice);
            PluginConfig.Instance.Changed();
        }

        Settings() {
            listChoice = modeOptions[(int)PluginConfig.Instance.mode] as string;
            sideChoice = sideOptions[(int)PluginConfig.Instance.side] as string;
            secondaryListChoice = secondaryModeOptions[(int)PluginConfig.Instance.secondaryMode] as string;
            secondarySideChoice = sideOptions[(int)PluginConfig.Instance.secondarySide] as string;
        }
    }
}
