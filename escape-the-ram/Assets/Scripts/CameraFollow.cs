using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _followedTransform;

    private void Update()
    {
        Vector3 targetPos = Vector3.Lerp(transform.position, _followedTransform.position, Time.deltaTime * 10f);
        targetPos.z = -10f;
        transform.position = targetPos;
    }
}
