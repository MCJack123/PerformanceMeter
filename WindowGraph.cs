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
    public class WindowGraph : MonoBehaviour
    {
        public RectTransform    GraphContainer { get; private set; }
        public List<GameObject> DotObjects     { get; private set; }
        public List<GameObject> LinkObjects    { get; private set; }
        public List<GameObject> LabelXObjects  { get; private set; }
        public List<GameObject> LabelYObjects  { get; private set; }
        public List<GameObject> DashXObjects   { get; private set; }
        public List<GameObject> DashYObjects   { get; private set; }

        private void Awake()
        {
            GraphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
            if (GraphContainer == null) Logger.log.Error("Could not find GraphContainer");

            DotObjects = new List<GameObject>();
            LinkObjects = new List<GameObject>();
            LabelXObjects = new List<GameObject>();
            LabelYObjects = new List<GameObject>();
            DashXObjects = new List<GameObject>();
            DashYObjects = new List<GameObject>();
        }

        public void ShowGraph(List<float> valueList)
        {
            if (DotObjects != null)
            {
                foreach (var go in DotObjects)
                    Destroy(go);
                DotObjects.Clear();
            }
            if (LinkObjects != null)
            {
                foreach (var go in LinkObjects)
                    Destroy(go);
                LinkObjects.Clear();
            }
            if (LabelXObjects != null)
            {
                foreach (var go in LabelXObjects)
                    Destroy(go);
                LabelXObjects.Clear();
            }
            if (LabelYObjects != null)
            {
                foreach (var go in LabelYObjects)
                    Destroy(go);
                LabelYObjects.Clear();
            }
            if (DashXObjects != null)
            {
                foreach (var go in DashXObjects)
                    Destroy(go);
                DashXObjects.Clear();
            }
            if (DashYObjects != null)
            {
                foreach (var go in DashYObjects)
                    Destroy(go);
                DashYObjects.Clear();
            }

            var graphWidth = GraphContainer.sizeDelta.x;
            var graphHeight = GraphContainer.sizeDelta.y;

            var yMaximum = 1.0f;
            var yMinimum = 0.0f;

            var xSize = graphWidth / (valueList.Count + 1);
            var xIndex = 0;

            GameObject lastCircleGameObject = null;
            for (var i = 0; i < valueList.Count; i++)
            {
                var xPosition = xSize + xIndex * xSize;
                var yPosition = (valueList[i] - yMinimum) / (yMaximum - yMinimum) * graphHeight;
                var circleGameObject = CreateCircle(new Vector2(xPosition, yPosition), false);
                DotObjects.Add(circleGameObject);
                if (lastCircleGameObject != null)
                {
                    var dotConnectionGameObject = CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition,
                                                                      circleGameObject.GetComponent<RectTransform>().anchoredPosition,
                                                                      true, graphHeight);
                    LinkObjects.Add(dotConnectionGameObject);
                }
                lastCircleGameObject = circleGameObject;

                xIndex++;
            }
        }

        private GameObject CreateCircle(Vector2 anchoredPosition, bool makeDotsVisible)
        {
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

        private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB, bool makeLinkVisible, float graphHeight)
        {
            var gameObject = new GameObject("DotConnection", typeof(Image));
            gameObject.transform.SetParent(GraphContainer, false);
            var image = gameObject.GetComponent<Image>();
            switch (PluginConfig.Instance.GetMode()) {
                case PluginConfig.MeasurementMode.Energy:
                    if (dotPositionB.y / graphHeight <= 1.0 && dotPositionB.y / graphHeight >= 0.5) image.color = Color.green;
                    else if (dotPositionB.y / graphHeight < 0.5 && dotPositionB.y / graphHeight >= 0.25) image.color = Color.yellow;
                    else if (dotPositionB.y / graphHeight < 0.25 && dotPositionB.y / graphHeight >= 0) image.color = Color.red;
                    else image.color = Color.white;
                    break;
                case PluginConfig.MeasurementMode.PercentModified:
                case PluginConfig.MeasurementMode.PercentRaw:
                    if (dotPositionB.y / graphHeight < 0.9 && dotPositionB.y / graphHeight >= 0.8) image.color = Color.white;
                    else if (dotPositionB.y / graphHeight < 0.8 && dotPositionB.y / graphHeight >= 0.65) image.color = Color.green;
                    else if (dotPositionB.y / graphHeight < 0.65 && dotPositionB.y / graphHeight >= 0.5) image.color = Color.yellow;
                    else if (dotPositionB.y / graphHeight < 0.5 && dotPositionB.y / graphHeight >= 0.35) image.color = new Color(1.0f, 0.5f, 0.0f, 1.0f);
                    else if (dotPositionB.y / graphHeight < 0.35) image.color = Color.red;
                    else image.color = Color.cyan;
                    break;
                case PluginConfig.MeasurementMode.CutValue:
                case PluginConfig.MeasurementMode.AvgCutValue:
                    if (dotPositionB.y / graphHeight == 1.0) image.color = Color.white;
                    else if (dotPositionB.y / graphHeight < 1.0 && dotPositionB.y / graphHeight >= 101.0/115.0) image.color = Color.green;
                    else if (dotPositionB.y / graphHeight < 101.0/115.0 && dotPositionB.y / graphHeight >= 90.0/115.0) image.color = Color.yellow;
                    else if (dotPositionB.y / graphHeight < 90.0/115.0 && dotPositionB.y / graphHeight >= 80.0/115.0) image.color = new Color(1.0f, 0.6f, 0.0f);
                    else if (dotPositionB.y / graphHeight < 80.0/115.0 && dotPositionB.y / graphHeight >= 60.0/115.0) image.color = Color.red;
                    else if (dotPositionB.y / graphHeight < 60.0/115.0 && dotPositionB.y / graphHeight >= 0.0) image.color = new Color(0.5f, 0.0f, 0.0f);
                    else image.color = Color.cyan;
                    break;
            }
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
    }
}