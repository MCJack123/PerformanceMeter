/*
 * Plugin.cs
 * PerformanceMeter
 *
 * This file defines the entry points of PerformanceMeter.
 *
 * This code is licensed under the MIT license.
 * Copyright (c) 2021 JackMacWindows.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using UnityEngine.SceneManagement;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using BS_Utils.Utilities;

namespace PerformanceMeter
{

    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
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

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        /*
        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Logger.log.Debug("Config loaded");
        }
        */
        #endregion

        [OnStart]
        public void OnApplicationStart() {
            Logger.log.Debug("OnApplicationStart");
            if (PluginConfig.Instance.Enabled) {
                new GameObject("PerformanceMeterController").AddComponent<PerformanceMeterController>();
                BSEvents.gameSceneActive += GameSceneActive;
                BSEvents.menuSceneActive += MenuSceneActive;
                BSEvents.levelCleared += BSEvents_levelCleared;
                BSEvents.levelFailed += BSEvents_levelCleared;
                SceneManager.activeSceneChanged += ActiveSceneChanged;
            }
        }

        private void BSEvents_levelCleared(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2) {
            //PerformanceMeterController.instance.ShowResults();
        }

        [OnExit]
        public void OnApplicationQuit() {
            Logger.log.Debug("OnApplicationQuit");

        }

        void GameSceneActive() {
            PerformanceMeterController.instance.energyList.Clear();
            if (PluginConfig.Instance.GetMode() == PluginConfig.MeasurementMode.Energy) PerformanceMeterController.instance.energyList.Add(0.5f);
            PerformanceMeterController.instance.GetControllers();
        }

        void MenuSceneActive() {
            //PerformanceMeterController.instance.ShowResults();
        }

        void ActiveSceneChanged(Scene oldScene, Scene newScene) {
            if (newScene.name == "MenuViewControllers") PerformanceMeterController.instance.ShowResults();
        }
    }
}
