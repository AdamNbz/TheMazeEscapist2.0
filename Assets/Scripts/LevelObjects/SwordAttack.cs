using UnityEngine;
using UnityEngine.Events;

public class SwordAttack : MonoBehaviour
{
    public Vector2 directionToBoss;
    [SerializeField] float speed = 5f;
    public static UnityAction<Vector3> OnSwordAttacked;


    // Update is called once per frame
    void Start()
    {
        // translate rotation to vector2
        directionToBoss = transform.rotation * Vector2.up;
    }

    void Update()
    {
        gameObject.transform.Translate(directionToBoss * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Boss"))
        {
            Debug.Log("Sword attack triggered!");
            OnSwordAttacked?.Invoke(transform.position);
            Destroy(gameObject);
        }
    }
}
