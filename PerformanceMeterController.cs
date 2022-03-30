/*
 * PerformanceMeterController.cs
 * PerformanceMeter
 *
 * This file defines the main functionality of PerformanceMeter.
 *
 * This code is licensed under the MIT license.
 * Copyright (c) 2021-2022 JackMacWindows.
 */

using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;

namespace PerformanceMeter {
    public struct Pair<T, U> {
        public T first;
        public U second;
        public Pair(T a, U b) {first = a; second = b;}
    }

    public class PerformanceMeterController : MonoBehaviour {
        public static PerformanceMeterController instance { get; private set; }

        List<Pair<float, float>> energyList = new List<Pair<float, float>>();
        List<Pair<float, float>> secondaryEnergyList = new List<Pair<float, float>>();
        List<float> misses = new List<float>();
        float averageHitValue = 0.0f;
        int averageHitValueSize = 0;
        float secondaryAverageHitValue = 0.0f;
        int secondaryAverageHitValueSize = 0;
        ScoreController scoreController;
        BeatmapObjectManager objectManager;
        IComboController comboController;
        GameEnergyCounter energyCounter;
        RelativeScoreAndImmediateRankCounter rankCounter;
        AudioTimeSyncController audioController;
        GameObject panel;
        ILevelEndActions endActions;
        bool levelOk = false;
        static readonly FieldInfo _beatmapObjectManager = typeof(ScoreController).GetField("_beatmapObjectManager", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo _comboController = typeof(ComboUIController).GetField("_comboController", BindingFlags.NonPublic | BindingFlags.Instance);

        public void ShowResults() {
            if (!levelOk)
                return;
            levelOk = false;
            Logger.log.Debug("Found " + energyList.Count() + " primary notes, " + secondaryEnergyList.Count() + " secondary notes");

            panel = new GameObject("PerformanceMeter");
            panel.transform.Rotate(22.5f, 0, 0, Space.World);
            Canvas canvas = panel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.scaleFactor = 0.01f;
            canvas.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, 0.4f, 2.25f);
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1.0f, 0.5f);
            canvas.transform.Rotate(22.5f, 0, 0, Space.World);
            panel.AddComponent<CanvasRenderer>();
            panel.AddComponent<HMUI.CurvedCanvasSettings>();

            GameObject imageObj = new GameObject("Background");
            imageObj.transform.SetParent(canvas.transform);
            imageObj.transform.Rotate(22.5f, 0, 0, Space.World);
            Image img = imageObj.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.25f);
            img.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            img.GetComponent<RectTransform>().sizeDelta = new Vector2(1.0f, 0.6f);

            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(canvas.transform);
            textObj.transform.Rotate(22.5f, 0, 0, Space.World);
            HMUI.CurvedTextMeshPro text = textObj.AddComponent<HMUI.CurvedTextMeshPro>();
            text.font = Instantiate(Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(t => t.name == "Teko-Medium SDF"));
            text.fontSize = 9.0f;
            text.alignment = TextAlignmentOptions.Right;
            text.text = "Performance";
            text.enableAutoSizing = true;
            text.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            text.GetComponent<RectTransform>().localPosition = new Vector3(-0.3f, -0.25f, 0.1f);
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(75.0f, 25.0f);

            GameObject textObj2 = new GameObject("Label");
            textObj2.transform.SetParent(canvas.transform);
            textObj2.transform.Rotate(22.5f, 0, 0, Space.World);
            HMUI.CurvedTextMeshPro text2 = textObj2.AddComponent<HMUI.CurvedTextMeshPro>();
            text2.font = Instantiate(Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(t => t.name == "Teko-Medium SDF"));
            text2.fontSize = 9.0f;
            text2.alignment = TextAlignmentOptions.Right;
            text2.text = "By JackMacWindows#9776";
            text2.color = new Color(0.3f, 0.3f, 0.3f);
            text2.enableAutoSizing = true;
            text2.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
            text2.GetComponent<RectTransform>().localPosition = new Vector3(0.31f, -0.2625f, 0.1f);
            text2.GetComponent<RectTransform>().sizeDelta = new Vector2(175.0f, 16.0f);

            GameObject graphMask = new GameObject("GraphMask");
            graphMask.AddComponent<RectTransform>().localPosition = new Vector3(0.0f, 0.45f, 2.275f);
            graphMask.GetComponent<RectTransform>().sizeDelta = new Vector2(1.0f, 0.45f);
            graphMask.transform.Rotate(22.5f, 0, 0, Space.World);
            graphMask.transform.SetParent(panel.transform);
            graphMask.transform.name = "GraphMask";
            graphMask.AddComponent<RectMask2D>();

            GameObject graphObj = new GameObject("GraphContainer");
            graphObj.AddComponent<RectTransform>().localPosition = new Vector3(0.0f, 0.45f, 2.275f);
            graphObj.GetComponent<RectTransform>().sizeDelta = new Vector2(1.0f, 0.45f);
            graphObj.transform.Rotate(22.5f, 0, 0, Space.World);
            graphObj.transform.SetParent(graphMask.transform);
            graphObj.transform.name = "GraphContainer";

            bool hasSecondary = PluginConfig.Instance.secondaryMode != PluginConfig.MeasurementMode.None && secondaryEnergyList.Count > 0;

            float width = 0.0f;
            if (hasSecondary) {
                if (energyList.Last().first > secondaryEnergyList.Last().first)
                    secondaryEnergyList.Add(new Pair<float, float>(energyList.Last().first, secondaryEnergyList.Last().second));
                else if (secondaryEnergyList.Last().first > energyList.Last().first)
                    energyList.Add(new Pair<float, float>(secondaryEnergyList.Last().first, energyList.Last().second));
            }
            width = energyList.Last().first;

            graphMask.AddComponent<WindowGraph>().ShowGraph(energyList, PluginConfig.Instance.mode, width, PluginConfig.Instance.overrideColor, PluginConfig.Instance.sideColor, true);

            if (hasSecondary)
                graphMask.AddComponent<WindowGraph>().ShowGraph(secondaryEnergyList, PluginConfig.Instance.secondaryMode, width, PluginConfig.Instance.overrideSecondaryColor, PluginConfig.Instance.secondarySideColor, false);

            if (PluginConfig.Instance.showMisses) {
                var GraphTransform = graphObj.GetComponent<RectTransform>();
                var xSize = GraphTransform.sizeDelta.x / width;
                var ySize = GraphTransform.sizeDelta.y;
                foreach (float pos in misses) {
                    var xPosition = pos * xSize;
                    var dotPositionA = new Vector2(xPosition, 0);
                    var dotPositionB = new Vector2(xPosition, ySize);
                    var gameObject = new GameObject("DotConnection", typeof(Image));
                    gameObject.transform.SetParent(GraphTransform, false);
                    var image = gameObject.GetComponent<Image>();
                    image.color = new Color(0.5f, 0.5f, 0.5f, 0.75f);
                    var rectTransform = gameObject.GetComponent<RectTransform>();
                    var dir = (dotPositionB - dotPositionA).normalized;
                    var distance = Vector2.Distance(dotPositionA, dotPositionB);
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 0);
                    rectTransform.sizeDelta = new Vector2(distance, 0.0025f);
                    rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
                    rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
                }
            }

            StartCoroutine(WaitForMenu(graphObj, graphMask));
        }

        IEnumerator WaitForMenu(GameObject graphObj, GameObject graphMask) {
            if (endActions is StandardLevelGameplayManager) {
                ResultsViewController resultsController = null;
                do {
                    resultsController = Resources.FindObjectsOfTypeAll<ResultsViewController>().LastOrDefault();
                    yield return new WaitForSeconds(0.1f);
                } while (resultsController == null);

                resultsController.continueButtonPressedEvent += DismissGraph;
                resultsController.restartButtonPressedEvent += DismissGraph;
            } else {
                MissionResultsViewController resultsController = null;
                do {
                    resultsController = Resources.FindObjectsOfTypeAll<MissionResultsViewController>().LastOrDefault();
                    yield return new WaitForSeconds(0.1f);
                } while (resultsController == null);

                resultsController.continueButtonPressedEvent += DismissGraph_Mission;
                resultsController.retryButtonPressedEvent += DismissGraph_Mission;  
            }
            Logger.log.Debug("PerformanceMeter menu created successfully");
            StartCoroutine(GraphAnimation(graphObj, graphMask));
        }

        IEnumerator GraphAnimation(GameObject graphObj, GameObject graphMask) {
            if (PluginConfig.Instance.animationDuration <= 0f)
                yield break;

            float fps = XRDevice.refreshRate;            
            float steps = fps * PluginConfig.Instance.animationDuration;
            Vector3 posDelta = new Vector3(0.5f / steps, 0f, 0f);
            Vector2 sizeDelta = new Vector2(1f / steps, 0f);
            graphMask.GetComponent<RectTransform>().sizeDelta = new Vector2(0.0f, 0.45f);
            graphMask.GetComponent<RectTransform>().localPosition -= posDelta * steps;
            graphObj.GetComponent<RectTransform>().localPosition += posDelta * steps;
            for (int s = 1; s <= steps; s++) {
                if (panel == null)
                    yield break;
                graphMask.GetComponent<RectTransform>().sizeDelta += sizeDelta;
                graphMask.GetComponent<RectTransform>().localPosition += posDelta;
                graphObj.GetComponent<RectTransform>().localPosition -= posDelta;
                yield return new WaitForSeconds(1f / fps);
            }
        }

        void DismissGraph(ResultsViewController vc) {
            if (panel != null) {
                panel.SetActive(false);
                Destroy(panel, 1);
                panel = null;
                scoreController = null;
                objectManager = null;
                comboController = null;
                energyCounter = null;
                rankCounter = null;
                audioController = null;
                endActions = null;
            }
            if (vc != null) {
                vc.continueButtonPressedEvent -= DismissGraph;
                vc.restartButtonPressedEvent -= DismissGraph;
            }
        }

        void DismissGraph_Mission(MissionResultsViewController vc) {
            DismissGraph(null);
            if (vc != null) {
                vc.continueButtonPressedEvent -= DismissGraph_Mission;
                vc.retryButtonPressedEvent -= DismissGraph_Mission;
            }
        }

        public void GetControllers() {
            DismissGraph(null);
            levelOk = false;
            averageHitValue = 0.0f;
            averageHitValueSize = 0;
            secondaryAverageHitValue = 0.0f;
            secondaryAverageHitValueSize = 0;
            energyList.Clear();
            secondaryEnergyList.Clear();
            misses.Clear();
            
            if (PluginConfig.Instance.mode == PluginConfig.MeasurementMode.Energy)
                energyList.Add(new Pair<float, float>(0.0f, 0.5f));
            if (PluginConfig.Instance.secondaryMode == PluginConfig.MeasurementMode.Energy)
                secondaryEnergyList.Add(new Pair<float, float>(0.0f, 0.5f));

            scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().LastOrDefault();
            ComboUIController comboUIController = Resources.FindObjectsOfTypeAll<ComboUIController>().LastOrDefault();
            energyCounter = Resources.FindObjectsOfTypeAll<GameEnergyCounter>().LastOrDefault();
            rankCounter = Resources.FindObjectsOfTypeAll<RelativeScoreAndImmediateRankCounter>().LastOrDefault();
            audioController = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().LastOrDefault();
            endActions = Resources.FindObjectsOfTypeAll<StandardLevelGameplayManager>().LastOrDefault();
            if (endActions == null)
                endActions = Resources.FindObjectsOfTypeAll<MissionLevelGameplayManager>().LastOrDefault();

            if (endActions != null && scoreController != null && energyCounter != null && rankCounter != null && audioController != null && comboUIController != null) {
                objectManager = (BeatmapObjectManager)_beatmapObjectManager.GetValue(scoreController);
                comboController = (ComboController)_comboController.GetValue(comboUIController);
                objectManager.noteWasCutEvent += NoteHit;
                objectManager.noteWasMissedEvent += NoteMiss;
                comboController.comboBreakingEventHappenedEvent += ComboBreak;
                endActions.levelFinishedEvent += LevelFinished;
                endActions.levelFailedEvent += LevelFinished;
                Logger.log.Debug("PerformanceMeter reloaded successfully");
            } else {
                Logger.log.Error("Could not reload PerformanceMeter. This may occur when playing online - if so, disregard this message.");
                scoreController = null;
                objectManager = null;
                comboController = null;
                energyCounter = null;
                rankCounter = null;
                audioController = null;
                endActions = null;
            }
        }

        private class ScoreFinishEventHandler : ICutScoreBufferDidFinishReceiver {
            private PerformanceMeterController controller;
            private NoteData data;
            private bool secondary;
            internal ScoreFinishEventHandler(PerformanceMeterController c, NoteData d, bool s) {controller = c; data = d; secondary = s;}
            public void HandleCutScoreBufferDidFinish(CutScoreBuffer cutScoreBuffer) {
                if (!secondary)
                    controller.RecordHitValue(cutScoreBuffer, data, this);
                else
                    controller.RecordHitValueSecondary(cutScoreBuffer, data, this);
            }
        }

        private void RecordHitValue(CutScoreBuffer score, NoteData data, ScoreFinishEventHandler fn) {
            float newEnergy;
            switch (PluginConfig.Instance.mode) {
                case PluginConfig.MeasurementMode.Energy:
                    newEnergy = energyCounter.energy;
                    break;
                case PluginConfig.MeasurementMode.PercentModified:
                    newEnergy = (float)scoreController.modifiedScore / scoreController.immediateMaxPossibleModifiedScore;
                    break;
                case PluginConfig.MeasurementMode.PercentRaw:
                    newEnergy = rankCounter.relativeScore;
                    break;
                case PluginConfig.MeasurementMode.CutValue:
                    if (score == null)
                        return;

                    newEnergy = score.cutScore / 115.0f;
                    break;
                case PluginConfig.MeasurementMode.AvgCutValue:
                    if (score == null)
                        return;

                    averageHitValue = ((averageHitValue * averageHitValueSize) + score.cutScore / 115.0f) / ++averageHitValueSize;
                    newEnergy = averageHitValue;
                    break;
                default:
                    Logger.log.Error("An invalid mode was specified! PerformanceMeter will not record scores, resulting in a blank graph. Check the readme for the valid modes.");
                    return;
            }

            if (energyList.Count == 0)
                energyList.Add(new Pair<float, float>(0, newEnergy));
            energyList.Add(new Pair<float, float>(data.time, newEnergy));

            if (score != null)
                score.UnregisterDidFinishReceiver(fn);
        }

        private void RecordHitValueSecondary(CutScoreBuffer score, NoteData data, ScoreFinishEventHandler fn) {
            float newEnergy;
            switch (PluginConfig.Instance.secondaryMode) {
                case PluginConfig.MeasurementMode.None:
                    return;
                case PluginConfig.MeasurementMode.Energy:
                    newEnergy = energyCounter.energy;
                    break;
                case PluginConfig.MeasurementMode.PercentModified:
                    newEnergy = (float)scoreController.modifiedScore / scoreController.immediateMaxPossibleModifiedScore;
                    break;
                case PluginConfig.MeasurementMode.PercentRaw:
                    newEnergy = rankCounter.relativeScore;
                    break;
                case PluginConfig.MeasurementMode.CutValue:
                    if (score == null)
                        return;
                    
                    newEnergy = score.cutScore / 115.0f;
                    break;
                case PluginConfig.MeasurementMode.AvgCutValue:
                    if (score == null)
                        return;

                    secondaryAverageHitValue = ((secondaryAverageHitValue * secondaryAverageHitValueSize) + score.cutScore / 115.0f) / ++secondaryAverageHitValueSize;
                    newEnergy = secondaryAverageHitValue;
                    break;
                default:
                    Logger.log.Error("An invalid mode was specified! PerformanceMeter will not record scores, resulting in a blank graph. Check the readme for the valid modes.");
                    return;
            }

            if (secondaryEnergyList.Count == 0)
                secondaryEnergyList.Add(new Pair<float, float>(0, newEnergy));
            secondaryEnergyList.Add(new Pair<float, float>(data.time, newEnergy));
            
            if (score != null)
                score.UnregisterDidFinishReceiver(fn);
        }

        private void NoteHit(NoteController controller, in NoteCutInfo info) {
            PluginConfig.MeasurementSide side = PluginConfig.Instance.side;
            if (side == PluginConfig.MeasurementSide.Both || (side == PluginConfig.MeasurementSide.Left && info.saberType == SaberType.SaberA) || (side == PluginConfig.MeasurementSide.Right && info.saberType == SaberType.SaberB)) {
                if (info.noteData == null) {
                    RecordHitValue(null, controller.noteData, null);
                } else {
                    ScoreFinishEventHandler handler = new ScoreFinishEventHandler(this, controller.noteData, false);
                    CutScoreBuffer buf = new CutScoreBuffer();
                    buf.Init(info);
                    buf.RegisterDidFinishReceiver(handler);
                }
            }
            side = PluginConfig.Instance.secondarySide;
            if (side == PluginConfig.MeasurementSide.Both || (side == PluginConfig.MeasurementSide.Left && info.saberType == SaberType.SaberA) || (side == PluginConfig.MeasurementSide.Right && info.saberType == SaberType.SaberB)) {
                if (info.noteData == null) {
                    RecordHitValueSecondary(null, controller.noteData, null);
                } else {
                    ScoreFinishEventHandler handler = new ScoreFinishEventHandler(this, controller.noteData, true);
                    CutScoreBuffer buf = new CutScoreBuffer();
                    buf.Init(info);
                    buf.RegisterDidFinishReceiver(handler);
                }
            }
        }

        private void NoteMiss(NoteController controller) {
            RecordHitValue(null, controller.noteData, null);
            RecordHitValueSecondary(null, controller.noteData, null);
        }

        private void ComboBreak() {
            misses.Add(audioController.songTime);
        }

        private void LevelFinished() {
            if (objectManager != null && energyCounter != null && rankCounter != null && endActions != null) {
                levelOk = true;
                objectManager.noteWasCutEvent -= NoteHit;
                objectManager.noteWasMissedEvent -= NoteMiss;
                comboController.comboBreakingEventHappenedEvent -= ComboBreak;
                endActions.levelFinishedEvent -= LevelFinished;
                endActions.levelFailedEvent -= LevelFinished;
            }
        }

        #region Monobehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake() {
            // For this particular MonoBehaviour, we only want one instance to exist at any time, so store a reference to it in a static property
            //   and destroy any that are created while one already exists.
            if (instance != null) {
                Logger.log?.Warn($"Instance of {this.GetType().Name} already exists, destroying.");
                DestroyImmediate(this);
                return;
            }
            DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            instance = this;
            Logger.log?.Debug($"{name}: Awake()");
        }
      
        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy() {
            Logger.log?.Debug($"{name}: OnDestroy()");
            instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.
        }
        #endregion
    }
}