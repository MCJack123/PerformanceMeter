/*
 * Settings.cs
 * PerformanceMeter
 *
 * This file defines the settings panel controller.
 *
 * This code is licensed under the MIT license.
 * Copyright (c) 2021 JackMacWindows.
 */

using BeatSaberMarkupLanguage.Attributes;
using System.Collections.Generic;
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

        [UIAction("#apply")]
        public void OnApply() {
            PluginConfig.Instance.enabled = _enabled;
            PluginConfig.Instance.showMisses = showMisses;
            int ok = 0;
            for (int i = 0; i < modeOptions.Count; i++) {
                if (modeOptions[i] as string == listChoice) {
                    PluginConfig.Instance.mode = i >= (int)PluginConfig.MeasurementMode.None ? i + 1 : i;
                    break;
                }
            }
            for (int i = 0; i < secondaryModeOptions.Count; i++) {
                if (secondaryModeOptions[i] as string == secondaryListChoice) {
                    PluginConfig.Instance.secondaryMode = i;
                    break;
                }
            }
            for (int i = 0; i < sideOptions.Count; i++) {
                if (sideOptions[i] as string == sideChoice) {
                    PluginConfig.Instance.side = i;
                    ok++;
                }
                if (sideOptions[i] as string == secondarySideChoice) {
                    PluginConfig.Instance.secondarySide = i;
                    ok++;
                }
                if (ok >= 2) break;
            }
            PluginConfig.Instance.Changed();
        }

        Settings() {
            listChoice = modeOptions[PluginConfig.Instance.mode] as string;
            sideChoice = sideOptions[PluginConfig.Instance.side] as string;
            secondaryListChoice = secondaryModeOptions[PluginConfig.Instance.secondaryMode] as string;
            secondarySideChoice = sideOptions[PluginConfig.Instance.secondarySide] as string;
        }
    }
}
