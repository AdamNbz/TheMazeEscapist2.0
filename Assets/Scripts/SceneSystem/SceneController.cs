using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : Singleton<SceneController>
{
    [SerializeField] private Image _sceneTransitionOverlay = null;
    [SerializeField] private float _animationSpeed = 2f;

    public void TransitionToScene(string sceneName, Action onDone = null)
    {
        AudioManager.Instance.PlaySfx("click_to_start", Vector2.zero);
        StartCoroutine(TransitionToSceneCoroutine(sceneName, onDone));
    }

    private IEnumerator TransitionToSceneCoroutine(string sceneName, Action onDone = null)
    {
        _sceneTransitionOverlay.raycastTarget = true;

        float startSize = 2f;
        while (startSize > 0f)
        {
            startSize -= _animationSpeed * Time.deltaTime;
            _sceneTransitionOverlay.materialForRendering.SetFloat("_CircleSize", startSize);
            yield return null;
        }
        startSize = 0f;
        _sceneTransitionOverlay.materialForRendering.SetFloat("_CircleSize", startSize);

        yield return SceneManager.LoadSceneAsync(sceneName);
        onDone?.Invoke();

        while (startSize < 2f)
        {
            startSize += _animationSpeed * Time.deltaTime;
            _sceneTransitionOverlay.materialForRendering.SetFloat("_CircleSize", startSize);
            yield return null;
        }

        _sceneTransitionOverlay.raycastTarget = false;
    }

    private void Start()
    {
        _sceneTransitionOverlay.materialForRendering.SetFloat("_CircleSize", 2f);
        _sceneTransitionOverlay.raycastTarget = false;
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.R))
    //    {
    //        TransitionToScene(SceneManager.GetActiveScene().name);
    //    }
    //}
}
