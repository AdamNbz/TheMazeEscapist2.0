using UnityEngine;

[ExecuteInEditMode]
public class CameraScaling : MonoBehaviour
{
    [SerializeField] float targetedCameraWidth;
    [SerializeField] float targetedCameraHeight;

    void Awake()
    {
        UpdateScaling();
    }

    void Update()
    {
        #if UNITY_EDITOR
        UpdateScaling();
        #endif
    }

    void UpdateScaling()
    {
        var height = Camera.main.orthographicSize * 2;
        var width = height * Camera.main.aspect;

        var standardAspect = targetedCameraWidth / targetedCameraHeight;
        var currentAspect = width / height;

        var ratio = currentAspect < standardAspect ? width / targetedCameraWidth : height / targetedCameraHeight;

        transform.localScale = new Vector3(ratio, ratio, 1);
    }
}
