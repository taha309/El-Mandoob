using System.Collections;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] private float duration = 7.5f;
    private bool collected;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collected || !collider.CompareTag("Player"))
        {
            return;
        }

        Player player = collider.GetComponent<Player>();
        if (player == null)
        {
            return;
        }

        collected = true;
        StartCoroutine(PickUp(player));
    }

    private IEnumerator PickUp(Player player)
    {
        player.SetShieldActive(true);

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.enabled = false;

        Collider2D pickupCollider = GetComponent<Collider2D>();
        if (pickupCollider != null) pickupCollider.enabled = false;

        yield return new WaitForSeconds(Mathf.Max(0.1f, duration));

        if (player != null)
        {
            player.SetShieldActive(false);
        }

        Destroy(gameObject);
    }
}
