using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

class PencilAttack : MonoBehaviour
{
    public static UnityAction OnPencilAttackTriggered;

    bool canFollow = false;
    float aimingDuration = 2f;
    float lockDuration = 1f;
    float speed = 5f;
    Vector3Int direction;
    Vector3Int initialPosition; // row or column depending on direction

    Tween fadeTween;

    private void Start()
    {
        // For testing purposes, trigger the pencil attack after 5 seconds
        //Invoke(nameof(TriggerPencilAttack), 5f);
    }

    public void Initialise(float aimingDuration, float lockDuration, float speed, Vector3Int direction, Vector3Int initialPosition)
    {
        this.aimingDuration = aimingDuration;
        this.lockDuration = lockDuration;
        this.speed = speed;
        this.direction = direction;
        this.initialPosition = initialPosition;
        SetUpTransform();
    }

    private void SetUpTransform()
    {
        bool isHorizontal = Mathf.Abs(direction.x) > Mathf.Abs(direction.y);
        bool isTopLeft = (isHorizontal && direction.x > 0) || (!isHorizontal && direction.y > 0);

        if (isHorizontal)
        {
            transform.position = new Vector3(isTopLeft ? -10 : 10, GridManager.Instance.CellToWorld(initialPosition).y + 0.5f, 0);
        }
        else
        {
            transform.position = new Vector3(GridManager.Instance.CellToWorld(initialPosition).x + 0.5f, isTopLeft ? -10 : 10, 0);
        }

        if (isHorizontal && direction.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (!isHorizontal && direction.y < 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (!isHorizontal && direction.y > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }

        // Use tween to fade in the pencil over the aiming duration
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr.color;
        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        fadeTween = sr.DOColor(originalColor, aimingDuration).SetLink(gameObject);
    }

    private void Update()
    {
        //Wait for aiming duration before moving
        if (aimingDuration > 0)
        {
            aimingDuration -= Time.deltaTime;
            return;
        }
        //Move for lock duration, then destroy        
        if (lockDuration > 0)
        {
            lockDuration -= Time.deltaTime;
            return;
        }
        transform.position += (Vector3)direction * speed * Time.deltaTime;
        // If out of camera bounds, destroy the pencil
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPos.x < -100 || screenPos.x > Screen.width + 100 || screenPos.y < -100 || screenPos.y > Screen.height + 100)
        {
            if (fadeTween != null && fadeTween.IsActive()) fadeTween.Kill();
            Destroy(gameObject);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player hit by pencil attack!");
            if (fadeTween != null && fadeTween.IsActive()) fadeTween.Kill();
            //Destroy(gameObject);
            var pencilCollider = GetComponent<Collider2D>();
            pencilCollider.enabled = false;
        }
    }

    public void TriggerPencilAttack()
    {
        Debug.Log("Pencil attack triggered!");
        OnPencilAttackTriggered?.Invoke();
    }
}