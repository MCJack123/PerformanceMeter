/*
 * PluginConfig.cs
 * PerformanceMeter
 *
 * This file defines the configuration of PerformanceMeter.
 *
 * This code is licensed under the MIT license.
 * Copyright (c) 2021 JackMacWindows.
 */

using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace PerformanceMeter {
    public class PluginConfig {
        public static PluginConfig Instance { get; set; }
        public virtual bool enabled { get; set; } = true;
        public virtual int mode { get; set; } = (int)MeasurementMode.Energy;
        public virtual int side { get; set; } = (int)MeasurementSide.Both;
        public virtual int secondaryMode { get; set; } = (int)MeasurementMode.None;
        public virtual int secondarySide { get; set; } = (int)MeasurementSide.Both;
        public virtual bool showMisses { get; set; } = false;
        public virtual float animationDuration { get; set; } = 3.0f;
        public virtual int color { get; set; } = 0xFF0000;
        public virtual int secondaryColor { get; set; } = 0x0000FF;
        public virtual bool overrideColor { get; set; } = false;
        public virtual bool overrideSecondaryColor { get; set; } = false;
        internal MeasurementMode GetMode(bool sec) { return (MeasurementMode)(sec ? secondaryMode : mode); }
        internal MeasurementSide GetSide(bool sec) { return (MeasurementSide)(sec ? secondarySide : side); }
        internal UnityEngine.Color GetColor(bool sec) { int c = sec ? secondaryColor : color; return new UnityEngine.Color((float)((c >> 16) & 0xFF) / 255f, (float)((c >> 8) & 0xFF) / 255f, (float)(c & 0xFF) / 255f); }

        public enum MeasurementMode {
            Energy,
            PercentModified,
            PercentRaw,
            CutValue,
            AvgCutValue,
            None
        };

        public enum MeasurementSide {
            Left,
            Right,
            Both
        }

        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload() {
            // Do stuff after config is read from disk.
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed() {
            // Do stuff when the config is changed.
            Logger.log.Debug("Updated configuration");
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other) {
            // This instance's members populated from other
        }
    }
}
