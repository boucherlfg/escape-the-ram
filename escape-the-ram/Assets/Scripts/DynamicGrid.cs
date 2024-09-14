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

        spaceBetweenLines = minSpaceBetweenLines + (maxSpaceBetweenLines - minSpaceBetweenLines) * size;
    }

    private void Update()
    {
        var player = AllyMovement.Instance.transform.position;
        var linesPerSide = lines.Count / 2;
        var center = linesPerSide / 2;
        for (int i = 0; i < linesPerSide; i++)
        {
            lines[i].SetPositions(new Vector3[]
            {
                new(-50 + player.x, -center + i * spaceBetweenLines),
                new(50 + player.x, -center + i * spaceBetweenLines)
            });
            lines[linesPerSide + i].SetPositions(new Vector3[]{
                new(-center + i * spaceBetweenLines, -50 + player.y),
                new(-center + i * spaceBetweenLines, 50 + player.y)
            });
        }
    }
}