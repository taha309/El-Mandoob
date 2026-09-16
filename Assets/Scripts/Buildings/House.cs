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
    private bool wasDestination;
    private string previousCustomer;

    private void LateUpdate()
    {
        if (isDesination)
        {
            string labelText = "توصيل لـ " + customerName;
            if (!wasDestination || worldLabel == null || previousCustomer != customerName)
            {
                worldLabel = ElMandoobWorldLabel.Attach(
                    gameObject,
                    labelText,
                    "ElMandoobDestinationSign",
                    new Color(0.42f, 0.20f, 0.07f, 0.94f));
                previousCustomer = customerName;
            }
        }
        else if (wasDestination && worldLabel != null)
        {
            Destroy(worldLabel.gameObject);
            worldLabel = null;
        }

        wasDestination = isDesination;
    }
}
