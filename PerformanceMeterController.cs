/*
 * PerformanceMeterController.cs
 * PerformanceMeter
 *
 * This file defines the main functionality of PerformanceMeter.
 *
 * This code is licensed under the MIT license.
 * Copyright (c) 2021 JackMacWindows.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using BS_Utils.Utilities;

namespace PerformanceMeter
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    public class PerformanceMeterController : MonoBehaviour
    {
        public static PerformanceMeterController instance { get; private set; }
        public List<float> energyList = new List<float>();
        ScoreController scoreController;
        GameEnergyCounter energyCounter;
        RelativeScoreAndImmediateRankCounter rankCounter;
        ResultsViewController resultsController;
        GameObject panel;
        ILevelEndActions endActions;
        bool levelOk = false;

        public void ShowResults() {
            if (!levelOk) return;
            levelOk = false;
            Logger.log.Debug("Found " + energyList.Count() + " notes");
            panel = new GameObject("PerformanceMeter");
            Canvas canvas = panel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, 2.25f, 2.0f);
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.75f);
            canvas.GetComponent<RectTransform>().rotation = Quaternion.AngleAxis(30, Vector3.left);
            GameObject imageObj = new GameObject();
            imageObj.transform.parent = canvas.transform;
            imageObj.name = "Background";
            Image img = imageObj.AddComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.25f, 0.5f);
            img.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            img.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.75f);
            GameObject graphObj = new GameObject("GraphContainer");
            graphObj.AddComponent<RectTransform>().localPosition = new Vector3(0.0f, 2.25f, 2.0f);
            graphObj.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.75f);
            graphObj.transform.SetParent(panel.transform);
            graphObj.name = "GraphContainer";
            graphObj.transform.name = "GraphContainer";
            WindowGraph graph = panel.AddComponent<WindowGraph>();
            graph.ShowGraph(energyList, false, true, true);
            StartCoroutine(WaitForMenu());
        }

        #region Monobehaviour Messages
        IEnumerator WaitForMenu() {
            bool loaded = false;
            while (!loaded) {
                if (resultsController == null) resultsController = Resources.FindObjectsOfTypeAll<ResultsViewController>().FirstOrDefault();
                if (resultsController != null) loaded = true;
                else yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.1f);
            resultsController.continueButtonPressedEvent += DismissGraph;
            resultsController.restartButtonPressedEvent += DismissGraph;
            Logger.log.Debug("PerformanceMeter menu created successfully");
        }

        IEnumerator PauseAndRecord() {
            yield return new WaitForSeconds(0.01f);
            float newEnergy = 0f;
            switch (PluginConfig.Instance.GetMode()) {
                case PluginConfig.MeasurementMode.Energy: newEnergy = energyCounter.energy; break;
                case PluginConfig.MeasurementMode.PercentModified: newEnergy = (float)scoreController.prevFrameModifiedScore / (float)scoreController.immediateMaxPossibleRawScore; break;
                case PluginConfig.MeasurementMode.PercentRaw: newEnergy = rankCounter.relativeScore; break;
                default: Logger.log.Error("An invalid mode was specified! PerformanceMeter will not record a score."); break;
            }
            Logger.log.Debug(newEnergy.ToString());
            energyList.Add(newEnergy);
        }

        void DismissGraph(ResultsViewController vc) {
            if (panel != null) {
                Destroy(panel);
                panel = null;
                resultsController = null;
            }
        }

        public void GetControllers() {
            scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();
            energyCounter = Resources.FindObjectsOfTypeAll<GameEnergyCounter>().FirstOrDefault();
            rankCounter = Resources.FindObjectsOfTypeAll<RelativeScoreAndImmediateRankCounter>().FirstOrDefault();
            endActions = Resources.FindObjectsOfTypeAll<StandardLevelGameplayManager>().FirstOrDefault();
            if (scoreController != null && energyCounter != null && endActions != null) {
                scoreController.noteWasCutEvent += NoteHit;
                scoreController.noteWasMissedEvent += NoteMiss;
                endActions.levelFinishedEvent += LevelFinished;
                endActions.levelFailedEvent += LevelFinished;
                Logger.log.Debug("PerformanceMeter reloaded successfully");
            }
        }

        private void NoteHit(NoteData data, NoteCutInfo info, int score) {
            StartCoroutine(PauseAndRecord());
        }

        private void NoteMiss(NoteData data, int score) {
            StartCoroutine(PauseAndRecord());
        }

        private void LevelFinished() {
            levelOk = true;
        }

        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake()
        {
            // For this particular MonoBehaviour, we only want one instance to exist at any time, so store a reference to it in a static property
            //   and destroy any that are created while one already exists.
            if (instance != null)
            {
                Logger.log?.Warn($"Instance of {this.GetType().Name} already exists, destroying.");
                GameObject.DestroyImmediate(this);
                return;
            }
            GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            instance = this;
            Logger.log?.Debug($"{name}: Awake()");
            //StartCoroutine(WaitForLoad());
        }
        /// <summary>
        /// Only ever called once on the first frame the script is Enabled. Start is called after any other script's Awake() and before Update().
        /// </summary>
        private void Start()
        {

        }

        /// <summary>
        /// Called every frame if the script is enabled.
        /// </summary>
        private void Update()
        {

        }

        /// <summary>
        /// Called every frame after every other enabled script's Update().
        /// </summary>
        private void LateUpdate()
        {

        }

        /// <summary>
        /// Called when the script becomes enabled and active
        /// </summary>
        private void OnEnable()
        {

        }

        /// <summary>
        /// Called when the script becomes disabled or when it is being destroyed.
        /// </summary>
        private void OnDisable()
        {

        }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Logger.log?.Debug($"{name}: OnDestroy()");
            instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.

        }
        #endregion
    }
}
