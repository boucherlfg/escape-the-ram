using Bytes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicGrid : MonoBehaviour
{
    [SerializeField]
    private int margin = 5;
    [SerializeField]
    private float spaceBetweenLines = 1.5f;
    [SerializeField]
    private float minSpaceBetweenLines = 0.5f;
    [SerializeField]
    private float maxSpaceBetweenLines = 1.5f;
    [HideInInspector]
    public List<LineRenderer> lines = new();
    private Vector2 offset;
    private Camera _mainCam;

    private float explosionProgress = 0;
    private void Start()
    {
        _mainCam = Camera.main;
        lines = GetComponentsInChildren<LineRenderer>().ToList();
        EventManager.AddEventListener("OnUltimatePullUpdate", HandleOnUltimatePullUpdate);
    }

    private void OnDestroy()
    {
        EventManager.RemoveEventListener("OnUltimatePullUpdate", HandleOnUltimatePullUpdate);
    }

    private void HandleOnUltimatePullUpdate(BytesData data)
    {
        var floatData = data as FloatDataBytes;
        var size = 1 - floatData.FloatValue;
        explosionProgress = floatData.FloatValue;
        Debug.Log(explosionProgress);
        var newScale = minSpaceBetweenLines + (maxSpaceBetweenLines - minSpaceBetweenLines) * size;

        var difference = spaceBetweenLines - newScale;
        // offset = -(AllyMovement.Instance.transform.position * difference);
        // spaceBetweenLines = newScale;
    }

    public void Update()
    {
        _mainCam = _mainCam ? _mainCam : Camera.main;

        Vector2 min = _mainCam.ScreenToWorldPoint(Vector2.zero);
        Vector2 max = _mainCam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        var cameraRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        
        Vector2 center = min + (max - min) / 2;
        Vector2 centerInUnits = RoundToNearestMultiple(center, spaceBetweenLines);

        int linesForWidth = (int)(0.5f + cameraRect.width / spaceBetweenLines) + margin * 2;
        int linesForHeight = (int)(0.5f + cameraRect.height / spaceBetweenLines) + margin * 2;

        var widthHalfDistance = linesForWidth * spaceBetweenLines / 2;
        var heightHalfDistance = linesForHeight * spaceBetweenLines / 2;

        for (int i = 0; i < lines.Count; i++)
        {
            lines[i].SetPositions(new[] { Vector3.zero, Vector3.zero });
        }

        // horizontal
        for (int i = 0; i < linesForWidth; i++)
        {
            float value = i / (float)linesForWidth;
            value = Mathf.Pow(value, 1 + this.explosionProgress/2);
            value *= linesForWidth;
            var widthPosition = value * spaceBetweenLines * Vector2.left;

            lines[i].SetPosition(0, centerInUnits + widthPosition + Vector2.up * heightHalfDistance);
            lines[i].SetPosition(1, centerInUnits + widthPosition + Vector2.down * heightHalfDistance);

            lines[i + linesForWidth].SetPosition(0, centerInUnits - widthPosition + Vector2.up * heightHalfDistance);
            lines[i + linesForWidth].SetPosition(1, centerInUnits - widthPosition + Vector2.down * heightHalfDistance);
        }

        // vertical
        for (int i = 0; i < linesForHeight; i++)
        {
            float value = i / (float)linesForHeight;
            value = Mathf.Pow(value, 1 + this.explosionProgress/2);
            value *= linesForHeight;
            var heightPosition = value * spaceBetweenLines * Vector2.up;

            lines[i + linesForWidth * 2].SetPosition(0, centerInUnits + heightPosition + Vector2.left * widthHalfDistance);
            lines[i + linesForWidth * 2].SetPosition(1,  centerInUnits + heightPosition + Vector2.right * widthHalfDistance);

            lines[i + linesForWidth * 2 + linesForHeight].SetPosition(0, centerInUnits - heightPosition + Vector2.left * widthHalfDistance);
            lines[i + linesForWidth * 2 + linesForHeight].SetPosition(1, centerInUnits - heightPosition + Vector2.right * widthHalfDistance);
        }
    }

    public static float RoundToNearestMultiple(float value, float multiple)
    {
        return (float)(Math.Round(value / multiple) * multiple);
    }
    public static Vector2 RoundToNearestMultiple(Vector2 value, float multiple) 
    {
        return ((Vector2)Vector2Int.RoundToInt(value / multiple)) * multiple;
    }
}
