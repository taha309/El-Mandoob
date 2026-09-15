using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GamePlayManager : MonoBehaviour
{
    private GameObject[] buildings;
    [SerializeField]
    private Player player;

    // For target pointer
    [SerializeField]
    private GameObject pointer;
    private Vector3 pointerPosition;

    [SerializeField]
    private TargetIndicator questPointer;

    // For delivery feature
    [SerializeField]
    private Button recieveButton, deliverButton;

    private GameObject shop, destination;
    private int destinationIndex, shopIndex;
    private bool inProcess = false, carryingOrder = false;

    // For level settings
    private int level;
    public VehicleSpawner[] vehicleSpawners;
    public CountdownTimer timer;
    private int numTotalOrders;
    private bool useTime;

    // El Mandoob economy
    private GameData data;
    private int currentShiftEarnings;

    // For calculating score when game over
    private int numDeliveredOrders = 0;
    private float remainingTime;
    private int totalMinutes, totalSeconds;
    public GameObject deathScreen, gameCompletedSceen, itemDeliveredCanvas,
     scoreDisplay, taskOrdersDisplay, taskTimeDisplay, deliveredOredersDisplay;
    private TextMeshProUGUI tmp, tmp1, tmp2, tmp3;

    // Start is called before the first frame update
    void Start()
    {
        data = SaveSystem.Load();
        currentShiftEarnings = 0;

        level = PlayerPrefs.GetInt("SelectedLevel");
        if (level <= 0)
        {
            level = 1;
            PlayerPrefs.SetInt("SelectedLevel", level);
        }

        Debug.Log("EL MANDOOB SHIFT: " + level);

        // Settings for the level
        useTime = level <= 4;

        foreach (VehicleSpawner vehicleSpawner in vehicleSpawners)
        {
            vehicleSpawner.carSpeed = 3 + level;
            vehicleSpawner.carsPerSpawn = (level - 1) / 2 + 1;
        }

        if (level == 1)
        {
            numTotalOrders = 1;
            totalMinutes = 1;
            totalSeconds = 0;
        }
        else if (level == 2)
        {
            numTotalOrders = 2;
            totalMinutes = 1;
            totalSeconds = 15;
        }
        else if (level == 3)
        {
            numTotalOrders = 3;
            totalMinutes = 1;
            totalSeconds = 30;
        }
        else if (level == 4)
        {
            numTotalOrders = 4;
            totalMinutes = 1;
            totalSeconds = 45;
        }
        else if (level == 5)
        {
            numTotalOrders = 4;
            totalMinutes = 2;
            totalSeconds = 30;
        }
        else if (level == 6)
        {
            numTotalOrders = 5;
            totalMinutes = 2;
            totalSeconds = 45;
        }
        else if (level == 7)
        {
            numTotalOrders = 6;
            totalMinutes = 3;
            totalSeconds = 15;
        }
        else
        {
            numTotalOrders = 7;
            totalMinutes = 3;
            totalSeconds = 45;
        }

        tmp1 = taskOrdersDisplay.GetComponent<TextMeshProUGUI>();
        tmp1.text = ElMandoobArabic.Shape(BuildOrderObjective(numTotalOrders));

        tmp2 = taskTimeDisplay.GetComponent<TextMeshProUGUI>();
        tmp2.text = ElMandoobArabic.Shape(
            "خلّص الشيفت قبل " + totalMinutes.ToString("00") + ":" + totalSeconds.ToString("00"));

        timer.minutesLeft = totalMinutes;
        timer.secondsLeft = totalSeconds;

        recieveButton.gameObject.SetActive(false);
        deliverButton.gameObject.SetActive(false);

        buildings = GameObject.FindGameObjectsWithTag("Buildings");
        shopIndex = Random.Range(0, buildings.Length);

        shop = buildings[shopIndex];

        shop.AddComponent<Shop>();
        shop.tag = "Shop";
        for (int i = 0; i < buildings.Length; i++)
        {
            if (i != shopIndex)
            {
                buildings[i].AddComponent<House>();
                buildings[i].tag = "House";
            }
        }

        updatePointer(shop);
        changeTarget(shop);
        StartCoroutine(GenerateOrder());
    }

    private string BuildOrderObjective(int orderCount)
    {
        if (orderCount == 1)
        {
            return "وصّل طلب واحد";
        }

        if (orderCount == 2)
        {
            return "وصّل طلبين";
        }

        return "وصّل " + orderCount + " طلبات";
    }

    void updatePointer(GameObject building)
    {
        pointerPosition = building.transform.position;
        pointerPosition.y += 2;
        pointer.transform.position = pointerPosition;
    }

    void changeTarget(GameObject building)
    {
        questPointer.Target = building;
    }

    public void receiveButtonClick()
    {
        carryingOrder = true;
        player.GetComponent<Player>().carryingOrder = true;
        recieveButton.gameObject.SetActive(false);
        shop.GetComponent<Shop>().havingOrder = false;
        updatePointer(destination);
        changeTarget(destination);
    }

    public void deliverButtonClick()
    {
        if (!carryingOrder)
        {
            return;
        }

        destination.GetComponent<House>().isDesination = false;
        deliverButton.gameObject.SetActive(false);
        carryingOrder = false;
        player.GetComponent<Player>().carryingOrder = false;
        inProcess = false;
        updatePointer(shop);
        changeTarget(shop);

        numDeliveredOrders += 1;
        AwardDelivery();

        tmp3 = deliveredOredersDisplay.GetComponent<TextMeshProUGUI>();
        tmp3.text = numDeliveredOrders.ToString();

        if (useTime && numDeliveredOrders == numTotalOrders)
        {
            gameCompleted();
        }
    }

    private void AwardDelivery()
    {
        // A simple first economy pass. Customer/tip-specific payouts come in the story milestone.
        int basePay = 35 + (level * 5);
        int safeDeliveryBonus = Mathf.Max(0, player.currentLives - 1) * 5;
        int payout = basePay + safeDeliveryBonus;

        currentShiftEarnings += payout;
        data.money += payout;
        data.reputation += 1;
        data.completedDeliveries += 1;
        SaveSystem.Save(data);

        Debug.Log("EL MANDOOB DELIVERY: +" + payout + " EGP | Balance: " + data.money + " EGP");
    }

    IEnumerator GenerateOrder()
    {
        while (true)
        {
            if (!inProcess)
            {
                inProcess = true;
                shop.GetComponent<Shop>().havingOrder = true;

                destinationIndex = Random.Range(0, buildings.Length);
                while (destinationIndex == shopIndex)
                {
                    destinationIndex = Random.Range(0, buildings.Length);
                }

                destination = buildings[destinationIndex];
                destination.GetComponent<House>().isDesination = true;
            }

            yield return new WaitForSeconds(Random.Range(3, 5));
        }
    }

    public void endGame()
    {
        if (calculateScore() == 0)
        {
            gameOver();
        }
        else
        {
            gameCompleted();
        }
    }

    public void gameOver()
    {
        itemDeliveredCanvas.SetActive(false);
        deathScreen.SetActive(true);
    }

    public void gameCompleted()
    {
        int starsEarned = calculateScore();
        tmp = scoreDisplay.GetComponent<TextMeshProUGUI>();
        tmp.text = ElMandoobArabic.Shape(
            "كسبت " + currentShiftEarnings + " جنيه | التقييم " + starsEarned + "/3");

        itemDeliveredCanvas.SetActive(false);
        gameCompletedSceen.SetActive(true);
    }

    public int calculateScore()
    {
        // Existing star score is preserved for compatibility with the original level flow.
        if (player.currentLives <= 0)
        {
            return 0;
        }

        if (useTime)
        {
            if (numDeliveredOrders < numTotalOrders)
            {
                return 0;
            }

            remainingTime = timer.secondsLeft + timer.minutesLeft * 60;
            return 1 + (int)Mathf.Min((remainingTime / 15), 2);
        }

        if (numDeliveredOrders < numTotalOrders)
        {
            return 0;
        }

        return 1 + (int)Mathf.Min(2, numDeliveredOrders / 2);
    }
}
