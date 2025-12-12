using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float displacementFactor = 0.3f;
    private float zPosition = -10f;

    private void Update()
    {
        if (playerTransform != null)
        {
            // Calculate mouse position in world space then calculate displacement using difference between mouse and player's position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 camDisplacement = (mousePos - playerTransform.position) * displacementFactor;

            // Determine final camera position
            Vector3 finalPosition = playerTransform.position + camDisplacement;
            finalPosition.z = zPosition;
            transform.position = finalPosition;
        }
    }
}
