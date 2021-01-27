/*
 * Plugin.cs
 * PerformanceMeter
 *
 * This file defines the entry points of PerformanceMeter.
 *
 * This code is licensed under the MIT license.
 * Copyright (c) 2021 JackMacWindows.
 */

using IPA;
using IPA.Config.Stores;
using UnityEngine.SceneManagement;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using BS_Utils.Utilities;
using BeatSaberMarkupLanguage.Settings;

namespace PerformanceMeter {
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin {
        internal static Plugin instance { get; private set; }
        internal static string Name => "PerformanceMeter";

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger, IPA.Config.Config conf) {
            instance = this;
            Logger.log = logger;
            PluginConfig.Instance = conf.Generated<PluginConfig>();
            Logger.log.Debug("Logger initialized.");
        }

        [OnStart]
        public void OnApplicationStart() {
            Logger.log.Debug("OnApplicationStart");
            new GameObject("PerformanceMeterController").AddComponent<PerformanceMeterController>();
            BSEvents.gameSceneActive += GameSceneActive;
            SceneManager.activeSceneChanged += ActiveSceneChanged;
            BSMLSettings.instance.AddSettingsMenu("PerformanceMeter", "PerformanceMeter.Settings", Settings.instance);
        }

        [OnExit]
        public void OnApplicationQuit() {
            Logger.log.Debug("OnApplicationQuit");
            BSEvents.gameSceneActive -= GameSceneActive;
            SceneManager.activeSceneChanged -= ActiveSceneChanged;
        }

        void GameSceneActive() {
            if (PluginConfig.Instance.enabled) PerformanceMeterController.instance.GetControllers();
        }

        void ActiveSceneChanged(Scene oldScene, Scene newScene) {
            if (PluginConfig.Instance.enabled && newScene.name == "MenuViewControllers") PerformanceMeterController.instance.ShowResults();
        }
    }

    internal static class Logger {
        internal static IPALogger log { get; set; }
    }
}
