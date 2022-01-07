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
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace PerformanceMeter {
    public class PluginConfig {
        public static PluginConfig Instance { get; set; }
        public virtual bool enabled { get; set; } = true;
        public virtual MeasurementMode mode { get; set; } = MeasurementMode.Energy;
        public virtual MeasurementSide side { get; set; } = MeasurementSide.Both;
        public virtual Color sideColor { get; set; } = Color.white;
        public virtual MeasurementMode secondaryMode { get; set; } = MeasurementMode.None;
        public virtual MeasurementSide secondarySide { get; set; } = MeasurementSide.Both;
        public virtual Color secondarySideColor { get; set; } = Color.white;
        public virtual bool showMisses { get; set; } = false;
        public virtual float animationDuration { get; set; } = 3.0f;
        public virtual Color color { get; set; } = new Color(1f, 0f, 0f);
        public virtual Color secondaryColor { get; set; } = new Color(0f, 0f, 1f);
        public virtual bool overrideColor { get; set; } = false;
        public virtual bool overrideSecondaryColor { get; set; } = false;
        internal Color GetSideColor(MeasurementSide side) {
            switch (side) {
                case MeasurementSide.Left:
                    return Color.red;
                case MeasurementSide.Right:
                    return Color.blue;
                case MeasurementSide.Both:
                    return Color.white;
                default:
                    return Color.white;
            };
        }

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
