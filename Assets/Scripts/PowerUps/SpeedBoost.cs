using System.Collections;
using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    [SerializeField] private float multiplier = 2f;
    [SerializeField] private float duration = 5f;
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
        float safeMultiplier = Mathf.Max(1f, multiplier);
        player.AddSpeedBoost(safeMultiplier);

        ElMandoobHUD hud = FindObjectOfType<ElMandoobHUD>();
        if (hud != null)
        {
            hud.ShowMessage("دفعة سرعة! استغلها قبل ما تخلص.", 2.5f);
        }

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.enabled = false;

        Collider2D pickupCollider = GetComponent<Collider2D>();
        if (pickupCollider != null) pickupCollider.enabled = false;

        yield return new WaitForSeconds(Mathf.Max(0.1f, duration));

        if (player != null)
        {
            player.RemoveSpeedBoost(safeMultiplier);
        }

        Destroy(gameObject);
    }
}
