using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument = null;
    private VisualElement _clickToContinue = null;

    private void Start()
    {
        _clickToContinue = _uiDocument.rootVisualElement.Q<Label>("ClickToContinue");

        var clickToStart = new Clickable(() =>
        {
            Debug.Log("change scene");
            SceneController.Instance.TransitionToScene("TestGrid");
        });
        var clickOverlay = _uiDocument.rootVisualElement.Q<VisualElement>("ClickOverlay");
        clickOverlay.AddManipulator(clickToStart);
    }

    private void Update()
    {
        _clickToContinue.style.opacity = Mathf.PingPong(Time.time, 1f);
        Debug.Log(_clickToContinue.style.opacity);
    }
}
