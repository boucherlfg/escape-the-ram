using Bytes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicGrid : MonoBehaviour
{
    [SerializeField]
    private float spaceBetweenLines = 1.5f;
    [SerializeField]
    private float minSpaceBetweenLines = 0.5f;
    [SerializeField]
    private float maxSpaceBetweenLines = 1.5f;
    private List<LineRenderer> lines = new();
    private Vector3 offset;

    private void Start()
    {
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

        var newScale = minSpaceBetweenLines + (maxSpaceBetweenLines - minSpaceBetweenLines) * size;

        var difference = spaceBetweenLines - newScale;
        offset = -(AllyMovement.Instance.transform.position * difference);
        spaceBetweenLines = newScale;
    }

    private void Update()
    {
        var aspect = Screen.width / Screen.height;
        var player = AllyMovement.Instance.transform.position;
        var linesPerSide = lines.Count / 2;
        var center = linesPerSide / 2;
        for (int i = 0; i < linesPerSide; i++)
        {
            lines[i].SetPositions(new Vector3[]
            {
                offset + new Vector3(-50 + player.x, (-center + i + (int)player.y) * spaceBetweenLines),
                offset + new Vector3(50 + player.x, (-center + i + (int)player.y) * spaceBetweenLines)
            });
            lines[linesPerSide + i].SetPositions(new Vector3[]{
                offset + new Vector3((-center + i + (int)(player.x * aspect)) * spaceBetweenLines, -50 + player.y),
                offset + new Vector3((-center + i + (int)(player.x * aspect)) * spaceBetweenLines, 50 + player.y)
            });
        }
    }
}