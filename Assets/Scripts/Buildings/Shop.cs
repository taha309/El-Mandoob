using UnityEngine;

public class Shop : MonoBehaviour
{
    [HideInInspector]
    public bool havingOrder = false;

    [HideInInspector]
    public string displayName = "محل";

    private ElMandoobWorldLabel worldLabel;
    private bool wasActive;
    private string previousName;

    private void LateUpdate()
    {
        if (havingOrder)
        {
            if (!wasActive || worldLabel == null || previousName != displayName)
            {
                worldLabel = ElMandoobWorldLabel.Attach(
                    gameObject,
                    displayName,
                    "ElMandoobPickupSign",
                    new Color(0.08f, 0.31f, 0.20f, 0.94f));
                previousName = displayName;
            }
        }
        else if (wasActive && worldLabel != null)
        {
            Destroy(worldLabel.gameObject);
            worldLabel = null;
        }

        wasActive = havingOrder;
    }
}
