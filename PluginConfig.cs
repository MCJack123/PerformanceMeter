/*
 * PluginConfig.cs
 * PerformanceMeter
 *
 * This file defines the configuration of PerformanceMeter.
 *
 * This code is licensed under the MIT license.
 * Copyright (c) 2021-2022 JackMacWindows.
 */

using System.Runtime.CompilerServices;
using IPA.Config.Data;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace PerformanceMeter {
    internal class IntColorConverter : ValueConverter<Color> {
        public override Color FromValue(Value value, object parent) {
            if (!(value is Integer)) throw new System.ArgumentException("Input value is not an integer");
            long num = (value as Integer).Value;
            return new Color(((num >> 16) & 0xFF) / 255.0f, ((num >> 8) & 0xFF) / 255.0f, (num & 0xFF) / 255.0f);
        }

        public override Value ToValue(Color obj, object parent) {
            return new Integer((long)(obj.r * 255) << 16 | (long)(obj.g * 255) << 8 | (long)(obj.b * 255));
        }
    }

    public class PluginConfig {
        public static PluginConfig Instance { get; set; }
        public virtual bool enabled { get; set; } = true;
        [UseConverter(typeof(NumericEnumConverter<MeasurementMode>))]
        public virtual MeasurementMode mode { get; set; } = MeasurementMode.Energy;
        [UseConverter(typeof(NumericEnumConverter<MeasurementSide>))]
        public virtual MeasurementSide side { get; set; } = MeasurementSide.Both;
        [UseConverter(typeof(IntColorConverter))]
        public Color sideColor {
            get {
                return side switch {
                    MeasurementSide.Left => Color.red,
                    MeasurementSide.Right => Color.blue,
                    MeasurementSide.Both => Color.white,
                    _ => Color.white,
                };
            }
        }
        [UseConverter(typeof(NumericEnumConverter<MeasurementMode>))]
        public virtual MeasurementMode secondaryMode { get; set; } = MeasurementMode.None;
        [UseConverter(typeof(NumericEnumConverter<MeasurementSide>))]
        public virtual MeasurementSide secondarySide { get; set; } = MeasurementSide.Both;
        [UseConverter(typeof(IntColorConverter))]
        public Color secondarySideColor {
            get {
                return secondarySide switch {
                    MeasurementSide.Left => Color.red,
                    MeasurementSide.Right => Color.blue,
                    MeasurementSide.Both => Color.white,
                    _ => Color.white,
                };
            }
        }
        public virtual bool showMisses { get; set; } = false;
        public virtual float animationDuration { get; set; } = 3.0f;
        [UseConverter(typeof(IntColorConverter))]
        public virtual Color color { get; set; } = new Color(1f, 0f, 0f);
        [UseConverter(typeof(IntColorConverter))]
        public virtual Color secondaryColor { get; set; } = new Color(0f, 0f, 1f);
        public virtual bool overrideColor { get; set; } = false;
        public virtual bool overrideSecondaryColor { get; set; } = false;

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
