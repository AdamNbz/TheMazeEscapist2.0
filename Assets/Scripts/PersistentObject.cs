using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        AudioManager.Instance.PlayBGM("start_bgm");
    }
}
