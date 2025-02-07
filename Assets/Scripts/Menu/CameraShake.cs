using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 0.1f;

    private Vector3 originalPos;

    private void Awake()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    public IEnumerator Shake()
    {
        Debug.Log("Shaking");
        float elapsed = 0f;
        originalPos = cameraTransform.localPosition;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            // Offset the camera
            cameraTransform.localPosition = new Vector3(x, y, originalPos.z);

            yield return null;
        }

        // Reset back to original position
        cameraTransform.localPosition = originalPos;
    }
}
