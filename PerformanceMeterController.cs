/*
 * PerformanceMeterController.cs
 * PerformanceMeter
 *
 * This file defines the main functionality of PerformanceMeter.
 *
 * This code is licensed under the MIT license.
 * Copyright (c) 2021 JackMacWindows.
 */

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
        GameEnergyCounter energyCounter;
        RelativeScoreAndImmediateRankCounter rankCounter;
        AudioTimeSyncController audioController;
        GameObject panel;
        ILevelEndActions endActions;
        bool levelOk = false;

        public void ShowResults() {
            if (!levelOk) return;
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

            bool hasPrimary = PluginConfig.Instance.GetMode(false) != PluginConfig.MeasurementMode.None && energyList.Count > 0;
            bool hasSecondary = PluginConfig.Instance.GetMode(true) != PluginConfig.MeasurementMode.None && secondaryEnergyList.Count > 0;

            float width = 0;
            if (hasPrimary && hasSecondary) {
                if (energyList.Last().first > secondaryEnergyList.Last().first) {
                    width = energyList.Last().first;
                    secondaryEnergyList.Add(new Pair<float, float>(width, secondaryEnergyList.Last().second));
                } else if (secondaryEnergyList.Last().first > energyList.Last().first) {
                    width = secondaryEnergyList.Last().first;
                    energyList.Add(new Pair<float, float>(width, energyList.Last().second));
                } else
                    width = secondaryEnergyList.Last().first;
            } else if (hasPrimary) {
                width = energyList.Last().first;
            } else if (hasSecondary) {
                width = secondaryEnergyList.Last().first;
            } else
                Logger.log.Warn("Both modes are set to None - the graph will be empty!");

            if (width > 0) {
                if (hasPrimary)
                    graphMask.AddComponent<WindowGraph>().ShowGraph(energyList, false, width, PluginConfig.Instance.GetSideColor(false));

                if (hasSecondary)
                    graphMask.AddComponent<WindowGraph>().ShowGraph(secondaryEnergyList, true, width, PluginConfig.Instance.GetSideColor(true));

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
            }
            StartCoroutine(WaitForMenu(graphObj, graphMask));
        }

        IEnumerator WaitForMenu(GameObject graphObj, GameObject graphMask) {
            bool loaded = false;
            if (endActions is StandardLevelGameplayManager) {
                ResultsViewController resultsController = null;
                while (resultsController == null) {
                    resultsController = Resources.FindObjectsOfTypeAll<ResultsViewController>().LastOrDefault();
                    yield return new WaitForSeconds(0.1f);
                }

                yield return new WaitForSeconds(0.1f);
                resultsController.continueButtonPressedEvent += DismissGraph;
                resultsController.restartButtonPressedEvent += DismissGraph;
            } else {
                MissionResultsViewController resultsController = null;
                while (resultsController == null) {
                    resultsController = Resources.FindObjectsOfTypeAll<MissionResultsViewController>().LastOrDefault();
                    yield return new WaitForSeconds(0.1f);
                }

                yield return new WaitForSeconds(0.1f);
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
                Destroy(panel);
                panel = null;
                scoreController = null;
                energyCounter = null;
                rankCounter = null;
                audioController = null;
                endActions = null;
                averageHitValue = 0.0f;
                averageHitValueSize = 0;
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
            if (PluginConfig.Instance.GetMode(false) == PluginConfig.MeasurementMode.Energy) energyList.Add(new Pair<float, float>(0.0f, 0.5f));
            if (PluginConfig.Instance.GetMode(true) == PluginConfig.MeasurementMode.Energy) secondaryEnergyList.Add(new Pair<float, float>(0.0f, 0.5f));

            scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().LastOrDefault();
            energyCounter = Resources.FindObjectsOfTypeAll<GameEnergyCounter>().LastOrDefault();
            rankCounter = Resources.FindObjectsOfTypeAll<RelativeScoreAndImmediateRankCounter>().LastOrDefault();
            audioController = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().LastOrDefault();
            endActions = Resources.FindObjectsOfTypeAll<StandardLevelGameplayManager>().LastOrDefault();
            if (endActions == null)
                endActions = Resources.FindObjectsOfTypeAll<MissionLevelGameplayManager>().LastOrDefault();

            if (scoreController != null && energyCounter != null && rankCounter != null && endActions != null && audioController != null) {
                scoreController.noteWasCutEvent += NoteHit;
                scoreController.noteWasMissedEvent += NoteMiss;
                scoreController.comboBreakingEventHappenedEvent += ComboBreak;
                endActions.levelFinishedEvent += LevelFinished;
                endActions.levelFailedEvent += LevelFinished;
                Logger.log.Debug("PerformanceMeter reloaded successfully");
            } else {
                Logger.log.Error("Could not reload PerformanceMeter. This may occur when playing online - if so, disregard this message.");
                scoreController = null;
                energyCounter = null;
                rankCounter = null;
                audioController = null;
                endActions = null;
                averageHitValue = 0.0f;
                averageHitValueSize = 0;
            }
        }

        private class ScoreFinishEventHandler : ICutScoreBufferDidFinishEvent {
            private PerformanceMeterController controller;
            private NoteData data;
            private bool secondary;
            internal ScoreFinishEventHandler(PerformanceMeterController c, NoteData d, bool s) {controller = c; data = d; secondary = s;}
            public void HandleCutScoreBufferDidFinish(CutScoreBuffer cutScoreBuffer) {
                if (secondary)
                    controller.RecordHitValueSecondary(cutScoreBuffer, data, this);
                else
                    controller.RecordHitValue(cutScoreBuffer, data, this);
            }
        }

        private void RecordHitValue(CutScoreBuffer score, NoteData data, ScoreFinishEventHandler fn) {
            float newEnergy;
            switch (PluginConfig.Instance.GetMode(false)) {
                case PluginConfig.MeasurementMode.None:
                    return;
                case PluginConfig.MeasurementMode.Energy:
                    newEnergy = energyCounter.energy;
                    break;
                case PluginConfig.MeasurementMode.PercentModified:
                    newEnergy = (float)scoreController.prevFrameModifiedScore / (float)scoreController.immediateMaxPossibleRawScore;
                    break;
                case PluginConfig.MeasurementMode.PercentRaw:
                    newEnergy = rankCounter.relativeScore;
                    break;
                case PluginConfig.MeasurementMode.CutValue:
                    if (score == null)
                        return;

                    newEnergy = score.scoreWithMultiplier / 115.0f;
                    break;
                case PluginConfig.MeasurementMode.AvgCutValue:
                    if (score == null)
                        return;

                    if (averageHitValueSize == 0) {
                        averageHitValue = score.scoreWithMultiplier / 115.0f;
                        averageHitValueSize++;
                    } else
                        averageHitValue = ((averageHitValue * averageHitValueSize) + score.scoreWithMultiplier / 115.0f) / ++averageHitValueSize;
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
                score.didFinishEvent.Remove(fn);
        }

        private void RecordHitValueSecondary(CutScoreBuffer score, NoteData data, ScoreFinishEventHandler fn) {
            float newEnergy;
            switch (PluginConfig.Instance.GetMode(true)) {
                case PluginConfig.MeasurementMode.None:
                    return;
                case PluginConfig.MeasurementMode.Energy:
                    newEnergy = energyCounter.energy;
                    break;
                case PluginConfig.MeasurementMode.PercentModified:
                    newEnergy = (float)scoreController.prevFrameModifiedScore / (float)scoreController.immediateMaxPossibleRawScore;
                    break;
                case PluginConfig.MeasurementMode.PercentRaw:
                    newEnergy = rankCounter.relativeScore;
                    break;
                case PluginConfig.MeasurementMode.CutValue:
                    if (score == null)
                        return;
                    
                    newEnergy = score.scoreWithMultiplier / 115.0f;
                    break;
                case PluginConfig.MeasurementMode.AvgCutValue:
                    if (score == null)
                        return;
                    
                    if (secondaryAverageHitValueSize == 0) {
                        secondaryAverageHitValue = score.scoreWithMultiplier / 115.0f;
                        secondaryAverageHitValueSize++;
                    } else
                        secondaryAverageHitValue = ((secondaryAverageHitValue * secondaryAverageHitValueSize) + score.scoreWithMultiplier / 115.0f) / ++secondaryAverageHitValueSize;
                    newEnergy = secondaryAverageHitValue;
                    break;
                default:
                    Logger.log.Error("An invalid mode was specified! PerformanceMeter will not record scores, resulting in a blank graph. Check the readme for the valid modes."); return;
            }

            if (secondaryEnergyList.Count == 0)
                secondaryEnergyList.Add(new Pair<float, float>(0, newEnergy));
            secondaryEnergyList.Add(new Pair<float, float>(data.time, newEnergy));
            
            if (score != null)
                score.didFinishEvent.Remove(fn);
        }

        private void NoteHit(NoteData data, in NoteCutInfo info, int multiplier) {
            PluginConfig.MeasurementSide side = PluginConfig.Instance.GetSide(false);
            if (side == PluginConfig.MeasurementSide.Both || (side == PluginConfig.MeasurementSide.Left && info.saberType == SaberType.SaberA) || (side == PluginConfig.MeasurementSide.Right && info.saberType == SaberType.SaberB)) {
                if (info.swingRatingCounter == null) {
                    RecordHitValue(null, data, null);
                } else {
                    ScoreFinishEventHandler handler = new ScoreFinishEventHandler(this, data, false);
                    CutScoreBuffer buf = new CutScoreBuffer();
                    buf.Init(info, 1);
                    buf.didFinishEvent.Add(handler);
                }
            }
            side = PluginConfig.Instance.GetSide(true);
            if (side == PluginConfig.MeasurementSide.Both || (side == PluginConfig.MeasurementSide.Left && info.saberType == SaberType.SaberA) || (side == PluginConfig.MeasurementSide.Right && info.saberType == SaberType.SaberB)) {
                if (info.swingRatingCounter == null) {
                    RecordHitValueSecondary(null, data, null);
                } else {
                    ScoreFinishEventHandler handler = new ScoreFinishEventHandler(this, data, true);
                    CutScoreBuffer buf = new CutScoreBuffer();
                    buf.Init(info, 1);
                    buf.didFinishEvent.Add(handler);
                }
            }
        }

        private void NoteMiss(NoteData data, int score) {
            RecordHitValue(null, data, null);
        }

        private void ComboBreak() {
            misses.Add(audioController.songTime);
        }

        private void LevelFinished() {
            if (scoreController != null && energyCounter != null && rankCounter != null && endActions != null) {
                levelOk = true;
                scoreController.noteWasCutEvent -= NoteHit;
                scoreController.noteWasMissedEvent -= NoteMiss;
                scoreController.comboBreakingEventHappenedEvent -= ComboBreak;
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