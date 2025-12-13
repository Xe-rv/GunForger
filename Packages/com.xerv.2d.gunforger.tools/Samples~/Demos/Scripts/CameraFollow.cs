using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CameraFollow smoothly offsets the camera toward the mouse position relative to the player,
/// providing look-ahead/displacement using a configurable displacementFactor.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;       // Player transform to follow
    [SerializeField] private float displacementFactor = 0.3f; // How strongly camera follows the mouse offset
    private float zPosition = -10f;                          // Fixed z to place camera in 2D view

    private void Update()
    {
        if (playerTransform != null)
        {
            // Convert mouse to world space and compute displacement based on distance from player
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 camDisplacement = (mousePos - playerTransform.position) * displacementFactor;

            // Compose final camera position while preserving z
            Vector3 finalPosition = playerTransform.position + camDisplacement;
            finalPosition.z = zPosition;
            transform.position = finalPosition;
        }
    }
}
