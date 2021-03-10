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
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IPA.Utilities;
using System.Reflection;

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
        GameObject panel;
        ILevelEndActions endActions;
        bool levelOk = false;
        MethodInfo CutScoreBuffer_Init;

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

            GameObject graphObj = new GameObject("GraphContainer");
            graphObj.AddComponent<RectTransform>().localPosition = new Vector3(0.0f, 0.45f, 2.275f);
            graphObj.GetComponent<RectTransform>().sizeDelta = new Vector2(1.0f, 0.45f);
            graphObj.transform.Rotate(22.5f, 0, 0, Space.World);
            graphObj.transform.SetParent(panel.transform);
            graphObj.transform.name = "GraphContainer";

            float width = 0;
            if (PluginConfig.Instance.GetMode(false) != PluginConfig.MeasurementMode.None && PluginConfig.Instance.GetMode(true) != PluginConfig.MeasurementMode.None && energyList.Count > 0 && secondaryEnergyList.Count > 0) {
                if (energyList[energyList.Count-1].first > secondaryEnergyList[secondaryEnergyList.Count-1].first) {
                    width = energyList[energyList.Count-1].first;
                    secondaryEnergyList.Add(new Pair<float, float>(width, secondaryEnergyList[secondaryEnergyList.Count-1].second));
                } else if (secondaryEnergyList[secondaryEnergyList.Count-1].first > energyList[energyList.Count-1].first) {
                    width = secondaryEnergyList[secondaryEnergyList.Count-1].first;
                    energyList.Add(new Pair<float, float>(width, energyList[energyList.Count-1].second));
                } else width = secondaryEnergyList[secondaryEnergyList.Count-1].first;
            } else if (PluginConfig.Instance.GetMode(false) != PluginConfig.MeasurementMode.None && energyList.Count > 0) width = energyList[energyList.Count-1].first;
            else if (PluginConfig.Instance.GetMode(true) != PluginConfig.MeasurementMode.None && secondaryEnergyList.Count > 0) width = secondaryEnergyList[secondaryEnergyList.Count-1].first;

            if (width > 0) {
                if (PluginConfig.Instance.GetMode(false) != PluginConfig.MeasurementMode.None) {
                    WindowGraph graph = panel.AddComponent<WindowGraph>();
                    PluginConfig.MeasurementSide side = PluginConfig.Instance.GetSide(false);
                    graph.ShowGraph(energyList, false, width, side == PluginConfig.MeasurementSide.Left ? Color.red : (side == PluginConfig.MeasurementSide.Right ? Color.blue : Color.white /* null */));
                }
                if (PluginConfig.Instance.GetMode(true) != PluginConfig.MeasurementMode.None) {
                    WindowGraph graph = panel.AddComponent<WindowGraph>();
                    PluginConfig.MeasurementSide side = PluginConfig.Instance.GetSide(true);
                    graph.ShowGraph(secondaryEnergyList, true, width, side == PluginConfig.MeasurementSide.Left ? Color.red : (side == PluginConfig.MeasurementSide.Right ? Color.blue : Color.white /* null */));
                }

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
            } else Logger.log.Warn("Both modes are set to None - the graph will be empty!");

            StartCoroutine(WaitForMenu());
        }

        IEnumerator WaitForMenu() {
            bool loaded = false;
            if (endActions is StandardLevelGameplayManager) {
                ResultsViewController resultsController = null;
                while (!loaded) {
                    if (resultsController == null) resultsController = Resources.FindObjectsOfTypeAll<ResultsViewController>().LastOrDefault();
                    if (resultsController != null) loaded = true;
                    else yield return new WaitForSeconds(0.1f);
                }

                yield return new WaitForSeconds(0.1f);
                resultsController.continueButtonPressedEvent += DismissGraph;
                resultsController.restartButtonPressedEvent += DismissGraph;
            } else {
                MissionResultsViewController resultsController = null;
                while (!loaded) {
                    if (resultsController == null) resultsController = Resources.FindObjectsOfTypeAll<MissionResultsViewController>().LastOrDefault();
                    if (resultsController != null) loaded = true;
                    else yield return new WaitForSeconds(0.1f);
                }

                yield return new WaitForSeconds(0.1f);
                resultsController.continueButtonPressedEvent += DismissGraph_Mission;
                resultsController.retryButtonPressedEvent += DismissGraph_Mission;  
            }
            Logger.log.Debug("PerformanceMeter menu created successfully");
        }

        void DismissGraph(ResultsViewController vc) {
            if (panel != null) {
                Destroy(panel);
                panel = null;
                scoreController = null;
                energyCounter = null;
                rankCounter = null;
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
            endActions = Resources.FindObjectsOfTypeAll<StandardLevelGameplayManager>().LastOrDefault();
            if (endActions == null) endActions = Resources.FindObjectsOfTypeAll<MissionLevelGameplayManager>().LastOrDefault();

            if (scoreController != null && energyCounter != null && rankCounter != null && endActions != null) {
                scoreController.noteWasCutEvent += NoteHit;
                scoreController.noteWasMissedEvent += NoteMiss;
                endActions.levelFinishedEvent += LevelFinished;
                endActions.levelFailedEvent += LevelFinished;
                Logger.log.Debug("PerformanceMeter reloaded successfully");
            } else {
                Logger.log.Error("Could not reload PerformanceMeter. This may occur when playing online - if so, disregard this message.");
                scoreController = null;
                energyCounter = null;
                rankCounter = null;
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
                if (secondary) controller.RecordHitValueSecondary(cutScoreBuffer, data, this);
                else controller.RecordHitValue(cutScoreBuffer, data, this);
            }
        }

        private void RecordHitValue(CutScoreBuffer score, NoteData data, ScoreFinishEventHandler fn) {
            if (score == null) misses.Add(data.time);
            float newEnergy;
            switch (PluginConfig.Instance.GetMode(false)) {
                case PluginConfig.MeasurementMode.None: return;
                case PluginConfig.MeasurementMode.Energy: newEnergy = energyCounter.energy; break;
                case PluginConfig.MeasurementMode.PercentModified: newEnergy = (float)scoreController.prevFrameModifiedScore / (float)scoreController.immediateMaxPossibleRawScore; break;
                case PluginConfig.MeasurementMode.PercentRaw: newEnergy = rankCounter.relativeScore; break;
                case PluginConfig.MeasurementMode.CutValue: if (score == null) return; newEnergy = score.scoreWithMultiplier / 115.0f; break;
                case PluginConfig.MeasurementMode.AvgCutValue:
                    if (score == null) return;
                    if (averageHitValueSize == 0) { averageHitValue = score.scoreWithMultiplier / 115.0f; averageHitValueSize++; }
                    else averageHitValue = ((averageHitValue * averageHitValueSize) + score.scoreWithMultiplier / 115.0f) / ++averageHitValueSize;
                    newEnergy = averageHitValue;
                    break;
                default: Logger.log.Error("An invalid mode was specified! PerformanceMeter will not record scores, resulting in a blank graph. Check the readme for the valid modes."); return;
            }
            if (energyList.Count == 0) energyList.Add(new Pair<float, float>(0, newEnergy));
            energyList.Add(new Pair<float, float>(data.time, newEnergy));
            if (score != null) score.didFinishEvent.Remove(fn);
        }

        private void RecordHitValueSecondary(CutScoreBuffer score, NoteData data, ScoreFinishEventHandler fn) {
            if (score == null) misses.Add(data.time);
            float newEnergy;
            switch (PluginConfig.Instance.GetMode(true)) {
                case PluginConfig.MeasurementMode.None: return;
                case PluginConfig.MeasurementMode.Energy: newEnergy = energyCounter.energy; break;
                case PluginConfig.MeasurementMode.PercentModified: newEnergy = (float)scoreController.prevFrameModifiedScore / (float)scoreController.immediateMaxPossibleRawScore; break;
                case PluginConfig.MeasurementMode.PercentRaw: newEnergy = rankCounter.relativeScore; break;
                case PluginConfig.MeasurementMode.CutValue: if (score == null) return; newEnergy = score.scoreWithMultiplier / 115.0f; break;
                case PluginConfig.MeasurementMode.AvgCutValue:
                    if (score == null) return;
                    if (secondaryAverageHitValueSize == 0) { secondaryAverageHitValue = score.scoreWithMultiplier / 115.0f; secondaryAverageHitValueSize++; }
                    else secondaryAverageHitValue = ((secondaryAverageHitValue * secondaryAverageHitValueSize) + score.scoreWithMultiplier / 115.0f) / ++secondaryAverageHitValueSize;
                    newEnergy = secondaryAverageHitValue;
                    break;
                default: Logger.log.Error("An invalid mode was specified! PerformanceMeter will not record scores, resulting in a blank graph. Check the readme for the valid modes."); return;
            }
            if (secondaryEnergyList.Count == 0) secondaryEnergyList.Add(new Pair<float, float>(0, newEnergy));
            secondaryEnergyList.Add(new Pair<float, float>(data.time, newEnergy));
            if (score != null) score.didFinishEvent.Remove(fn);
        }

        /*
         * So, you may notice that I do some wonky stuff involving reflection here.
         * This is because of some sort of bug (compiler or otherwise) that causes a
         * CS0570 error, complaining that CutScoreBuffer.Init is not accessible in the
         * current language. Obviously, since Beat Saber is written in C# and is not
         * run through IL2CPP, this is incorrect. The only way I've been able to get
         * CutScoreBuffer.Init working is by dynamically calling the method from the
         * main assembly through reflection. Now normally I'd use BSIPA's ReflectionUtil,
         * but unfortunately this is not possible due to Init returning void, which
         * is not a valid type to add to generic parameters, and there is no version
         * of ReflectionUtil.InvokeMethod that doesn't return a value. Therefore, I
         * have to instead read the method manually (which is cached in OnAwake), and
         * then call it as a MethodInfo variable instead of the normal way as a member
         * of the object.
         * 
         * This workaround is horrible, and I hate to put it in production code, but
         * it doesn't look like there's any better way around this issue. Maybe I'll
         * try submitting an issue to VS/Roslyn, but due to the nature of this issue
         * (one specific function in the code of a copyrighted game), it'll be
         * difficult to provide proper reproduction instructions. Some vague pointers
         * about things I notice are about all I could provide.
         */

        private void NoteHit(NoteData data, in NoteCutInfo info, int multiplier) {
            PluginConfig.MeasurementSide side = PluginConfig.Instance.GetSide(false);
            if (side == PluginConfig.MeasurementSide.Both || (side == PluginConfig.MeasurementSide.Left && info.saberType == SaberType.SaberA) || (side == PluginConfig.MeasurementSide.Right && info.saberType == SaberType.SaberB)) {
                if (info.swingRatingCounter == null) RecordHitValue(null, data, null);
                else {
                    ScoreFinishEventHandler handler = new ScoreFinishEventHandler(this, data, false);
                    CutScoreBuffer buf = new CutScoreBuffer();
                    CutScoreBuffer_Init?.Invoke(buf, new object[2] {info, 1}); //buf.Init(info, 1);
                    buf.didFinishEvent.Add(handler);
                }
            }
            side = PluginConfig.Instance.GetSide(true);
            if (side == PluginConfig.MeasurementSide.Both || (side == PluginConfig.MeasurementSide.Left && info.saberType == SaberType.SaberA) || (side == PluginConfig.MeasurementSide.Right && info.saberType == SaberType.SaberB)) {
                if (info.swingRatingCounter == null) RecordHitValueSecondary(null, data, null);
                else {
                    ScoreFinishEventHandler handler = new ScoreFinishEventHandler(this, data, true);
                    CutScoreBuffer buf = new CutScoreBuffer();
                    CutScoreBuffer_Init?.Invoke(buf, new object[2] {info, 1}); //buf.Init(info, 1);
                    buf.didFinishEvent.Add(handler);
                }
            }
        }

        private void NoteMiss(NoteData data, int score) {
            RecordHitValue(null, data, null);
        }

        private void LevelFinished() {
            if (scoreController != null && energyCounter != null && rankCounter != null && endActions != null) {
                levelOk = true;
                scoreController.noteWasCutEvent -= NoteHit;
                scoreController.noteWasMissedEvent -= NoteMiss;
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
            // See comment above about why we do this
            CutScoreBuffer_Init = typeof(CutScoreBuffer).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (CutScoreBuffer_Init == null) Logger.log.Critical("Could not get Init method! PerformanceMeter will not be able to function.");
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
