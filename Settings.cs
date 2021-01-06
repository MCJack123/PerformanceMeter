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
        // For this method of setting the ResourceName, this class must be the first class in the file.
        //public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        [UIValue("mode-options")]
        public List<object> modeOptions = new object[] { "Energy", "Percentage (Modified)", "Percentage (Raw)", "Cut Value", "Average Cut Value" }.ToList();

        [UIValue("mode")]
        public string listChoice = "Energy";

        [UIValue("enabled")]
        public bool _enabled = PluginConfig.Instance.enabled;

        [UIAction("#apply")]
        public void OnApply() {
            PluginConfig.Instance.enabled = _enabled;
            for (int i = 0; i < modeOptions.Count; i++) {
                if (modeOptions[i] as string == listChoice) {
                    PluginConfig.Instance.mode = i;
                    break;
                }
            }
            PluginConfig.Instance.Changed();
        }

        Settings() { listChoice = modeOptions[PluginConfig.Instance.mode] as string; }
    }
}
