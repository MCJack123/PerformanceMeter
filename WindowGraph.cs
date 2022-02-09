/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PerformanceMeter {
    public class WindowGraph : MonoBehaviour {
        public RectTransform    GraphContainer { get; private set; }
        public List<GameObject> DotObjects     { get; private set; }
        public List<GameObject> LinkObjects    { get; private set; }

        public delegate Color ColorMode_Selection(float dotPositionRage);
        private ColorMode_Selection dotColor;

        private void Awake() {
            GraphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
            if (GraphContainer == null) Logger.log.Error("Could not find GraphContainer");

            DotObjects = new List<GameObject>();
            LinkObjects = new List<GameObject>();
        }

        public void ShowGraph(List<Pair<float, float>> valueList, PluginConfig.MeasurementMode mode, float xMaximum, bool colorOverride, Color sideColor, bool isPrimaryMode) {
            if (valueList.Count == 0)
                return;

            var graphWidth = GraphContainer.sizeDelta.x;
            var graphHeight = GraphContainer.sizeDelta.y;
            var xStep = graphWidth / xMaximum;

            setColorMode(mode, isPrimaryMode, colorOverride, sideColor);

            // Unroll the loop once to prevent the 'if' check for all subsequent iterations
            var xPosition = valueList[0].first * xStep;
            var yPosition = valueList[0].second * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition), false);
            GameObject lastCircleGameObject = null;
            GameObject dotConnectionGameObject = null;
            for (var i = 1; i < valueList.Count; i++) {
                lastCircleGameObject = circleGameObject;

                xPosition = valueList[i].first * xStep;
                yPosition = valueList[i].second * graphHeight;
                circleGameObject = CreateCircle(new Vector2(xPosition, yPosition), false);
                DotObjects.Add(circleGameObject);
                dotConnectionGameObject = CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition,
                                                                      circleGameObject.GetComponent<RectTransform>().anchoredPosition,
                                                                      true, graphHeight);
                LinkObjects.Add(dotConnectionGameObject);
            }
        }

        private GameObject CreateCircle(Vector2 anchoredPosition, bool makeDotsVisible) {
            var gameObject = new GameObject("Circle", typeof(Image));
            gameObject.transform.SetParent(GraphContainer, false);
            var image = gameObject.GetComponent<Image>();
            image.enabled = makeDotsVisible;
            var rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(0.02f, 0.02f);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            return gameObject;
        }

        private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB, bool makeLinkVisible, float graphHeight) {
            float dotPositionRange = dotPositionB.y / graphHeight;
            var gameObject = new GameObject("DotConnection", typeof(Image));
            
            gameObject.transform.SetParent(GraphContainer, false);
            var image = gameObject.GetComponent<Image>();

            image.color = dotColor(dotPositionRange);
            image.enabled = makeLinkVisible;
            var rectTransform = gameObject.GetComponent<RectTransform>();
            var dir = (dotPositionB - dotPositionA).normalized;
            var distance = Vector2.Distance(dotPositionA, dotPositionB);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(distance, 0.01f);
            rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
            rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            return gameObject;
        }

        private void setColorMode(PluginConfig.MeasurementMode mode, bool isPrimaryMode, bool colorOverride, Color sideColor) {
            if (colorOverride) {
                if (isPrimaryMode)
                    dotColor = ColorMode_Override;
                else
                    dotColor = ColorMode_SecondaryOverride;
            } else if (sideColor != Color.white) {
                if (isPrimaryMode)
                    dotColor = ColorMode_Side;
                else
                    dotColor = ColorMode_SecondarySide;
            } else {
                switch (mode) {
                    case PluginConfig.MeasurementMode.Energy:
                        dotColor = ColorMode_Energy;
                        break;
                    case PluginConfig.MeasurementMode.PercentModified:
                    case PluginConfig.MeasurementMode.PercentRaw:
                        dotColor = ColorMode_PercentModifedRaw;
                        break;
                    case PluginConfig.MeasurementMode.CutValue:
                    case PluginConfig.MeasurementMode.AvgCutValue:
                        dotColor = ColorMode_CutAvgCut;
                        break;
                }
            }
        }
        
        private Color ColorMode_Override(float notUsed) {
            return PluginConfig.Instance.color;
        }

        private Color ColorMode_SecondaryOverride(float notUsed) {
            return PluginConfig.Instance.secondaryColor;
        }
        private Color ColorMode_Side(float notUsed) {
            return PluginConfig.Instance.sideColor;
        }

        private Color ColorMode_SecondarySide(float notUsed) {
            return PluginConfig.Instance.secondarySideColor;
        }

        private Color ColorMode_Energy(float dotPositionRange) {
            if (dotPositionRange >= 0.5) return Color.green;
            else if (dotPositionRange >= 0.25) return Color.yellow;
            else return Color.red;
        }

        private Color ColorMode_PercentModifedRaw(float dotPositionRange) {
            if (dotPositionRange >= 0.9) return Color.cyan;
            else if (dotPositionRange >= 0.8) return Color.white;
            else if (dotPositionRange >= 0.65) return Color.green;
            else if (dotPositionRange >= 0.5) return Color.yellow;
            else if (dotPositionRange >= 0.35) return new Color(1.0f, 0.5f, 0.0f, 1.0f);
            else return Color.red;
        }

        private Color ColorMode_CutAvgCut(float dotPositionRange) {
            if (dotPositionRange == 1.0) return Color.white;
            else if (dotPositionRange >= 0.87) return Color.green;                 // ~ 101.0/115.0
            else if (dotPositionRange >= 0.78) return Color.yellow;                // ~  90.0/115.0
            else if (dotPositionRange >= 0.69) return new Color(1.0f, 0.6f, 0.0f); // ~  80.0/115.0
            else if (dotPositionRange >= 0.52) return Color.red;                   // ~  60.0/115.0
            else return new Color(0.5f, 0.0f, 0.0f);
        }
    }
}