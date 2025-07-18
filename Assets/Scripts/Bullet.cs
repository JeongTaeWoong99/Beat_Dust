using UnityEditorInternal;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public string monsterTag = "Monster"; // �浹�� ������ ����� �±�

    public Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Shoot(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed; // ������ ����ȭ�Ͽ� �ӵ� ����
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(monsterTag))
        {
            Debug.Log(other.name + " ���Ϳ� �浹!");
            Destroy(gameObject); // �浹 �� �Ѿ� ����
        }
    }
}