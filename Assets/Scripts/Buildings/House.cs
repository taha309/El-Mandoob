using UnityEngine;

public class House : MonoBehaviour
{
    [HideInInspector]
    public bool isDesination = false;

    [HideInInspector]
    public string customerName = "الزبون";

    [HideInInspector]
    public string address = "العنوان";

    private ElMandoobWorldLabel worldLabel;
    private bool wasVisible;
    private string previousCustomer;
    private Player player;

    private void Start()
    {
        player = FindObjectOfType<Player>();
    }

    private void LateUpdate()
    {
        bool shouldShow = isDesination && player != null && player.carryingOrder;

        if (shouldShow)
        {
            string labelText = "توصيل لـ " + customerName;
            if (!wasVisible || worldLabel == null || previousCustomer != customerName)
            {
                worldLabel = ElMandoobWorldLabel.Attach(
                    gameObject,
                    labelText,
                    "ElMandoobDestinationSign",
                    new Color(0.42f, 0.20f, 0.07f, 0.94f));
                previousCustomer = customerName;
            }
        }
        else if (wasVisible && worldLabel != null)
        {
            Destroy(worldLabel.gameObject);
            worldLabel = null;
        }

        wasVisible = shouldShow;
    }
}
