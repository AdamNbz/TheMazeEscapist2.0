using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument = null;
    private VisualElement _clickToContinue = null;

    private void Start()
    {
        _clickToContinue = _uiDocument.rootVisualElement.Q<Label>("ClickToContinue");
    }

    private void Update()
    {
        _clickToContinue.style.opacity = Mathf.PingPong(Time.time, 1f);
        Debug.Log(_clickToContinue.style.opacity);
    }
}
