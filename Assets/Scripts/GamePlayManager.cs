using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GamePlayManager : MonoBehaviour
{
    private GameObject[] buildings;

    [SerializeField] private Player player;

    [SerializeField] private GameObject pointer;
    private Vector3 pointerPosition;
    [SerializeField] private TargetIndicator questPointer;

    [SerializeField] private Button recieveButton, deliverButton;

    private GameObject shop, destination;
    private int destinationIndex, shopIndex;
    private bool inProcess;
    private bool carryingOrder;

    private int level;
    public VehicleSpawner[] vehicleSpawners;
    public CountdownTimer timer;
    private int numTotalOrders;
    private bool useTime;

    private GameData data;
    private ElMandoobOrder currentOrder;
    private ElMandoobHUD hud;
    private int currentShiftEarnings;
    private int currentShiftTips;
    private bool shiftResolved;

    private int numDeliveredOrders;
    private int totalMinutes, totalSeconds;

    public GameObject deathScreen, gameCompletedSceen, itemDeliveredCanvas,
        scoreDisplay, taskOrdersDisplay, taskTimeDisplay, deliveredOredersDisplay;

    private TextMeshProUGUI tmp, tmp1, tmp2, tmp3;

    void Start()
    {
        Time.timeScale = 1f;

        data = SaveSystem.Load();
        currentShiftEarnings = 0;
        currentShiftTips = 0;
        shiftResolved = false;
        numDeliveredOrders = 0;

        level = PlayerPrefs.GetInt("SelectedLevel", 1);
        if (level <= 0)
        {
            level = 1;
            PlayerPrefs.SetInt("SelectedLevel", level);
            PlayerPrefs.Save();
        }
        level = Mathf.Clamp(level, 1, 8);

        Debug.Log("EL MANDOOB SHIFT: " + level + " | " + ElMandoobContent.GetShiftArea(level));

        ConfigureLevel();
        ConfigureTaskUI();
        ConfigureDeliveryButtons();
        ConfigureBuildings();

        hud = ElMandoobHUD.Create(data, level);

        if (deliveredOredersDisplay != null)
        {
            tmp3 = deliveredOredersDisplay.GetComponent<TextMeshProUGUI>();
            if (tmp3 != null)
            {
                tmp3.text = "0";
            }
        }

        if (shop != null && destination == null)
        {
            UpdatePointer(shop);
            ChangeTarget(shop);
            StartCoroutine(GenerateOrder());
        }
    }

    private void ConfigureLevel()
    {
        // Shifts 1-4 end immediately once the required deliveries are done.
        // Shifts 5-8 run to the clock: meet the minimum, then push for extra-rating deliveries.
        useTime = level <= 4;

        if (vehicleSpawners != null)
        {
            foreach (VehicleSpawner vehicleSpawner in vehicleSpawners)
            {
                if (vehicleSpawner == null)
                {
                    continue;
                }

                // Keep later traffic challenging without making it faster than a fully upgraded rider.
                vehicleSpawner.carSpeed = 3.2f + (level - 1) * 0.6f;
                vehicleSpawner.carsPerSpawn = 1 + (level - 1) / 3;
            }
        }

        switch (level)
        {
            case 1:
                numTotalOrders = 1;
                totalMinutes = 1;
                totalSeconds = 0;
                break;
            case 2:
                numTotalOrders = 2;
                totalMinutes = 1;
                totalSeconds = 15;
                break;
            case 3:
                numTotalOrders = 3;
                totalMinutes = 1;
                totalSeconds = 30;
                break;
            case 4:
                numTotalOrders = 4;
                totalMinutes = 1;
                totalSeconds = 45;
                break;
            case 5:
                numTotalOrders = 4;
                totalMinutes = 2;
                totalSeconds = 30;
                break;
            case 6:
                numTotalOrders = 5;
                totalMinutes = 2;
                totalSeconds = 45;
                break;
            case 7:
                numTotalOrders = 6;
                totalMinutes = 3;
                totalSeconds = 15;
                break;
            default:
                numTotalOrders = 7;
                totalMinutes = 3;
                totalSeconds = 45;
                break;
        }

        if (timer != null)
        {
            timer.SetTime(totalMinutes, totalSeconds);
        }
    }

    private void ConfigureTaskUI()
    {
        if (taskOrdersDisplay != null)
        {
            tmp1 = taskOrdersDisplay.GetComponent<TextMeshProUGUI>();
            string objective = BuildOrderObjective(numTotalOrders);
            if (!useTime)
            {
                objective += " على الأقل";
            }

            ElMandoobBootstrap.ApplyArabicText(
                tmp1,
                objective + " في " + ElMandoobContent.GetShiftArea(level));
        }

        if (taskTimeDisplay != null)
        {
            tmp2 = taskTimeDisplay.GetComponent<TextMeshProUGUI>();
            string timeText = totalMinutes.ToString("00") + ":" + totalSeconds.ToString("00");
            string instruction = useTime
                ? "خلّص المطلوب قبل " + timeText
                : "الوقت " + timeText + " - بعد الحد الأدنى كل طلبين زيادة يرفعوا التقييم";
            ElMandoobBootstrap.ApplyArabicText(tmp2, instruction);
        }
    }

    private void ConfigureDeliveryButtons()
    {
        if (recieveButton != null)
        {
            recieveButton.gameObject.SetActive(false);
        }

        if (deliverButton != null)
        {
            deliverButton.gameObject.SetActive(false);
        }
    }

    private void ConfigureBuildings()
    {
        buildings = GameObject.FindGameObjectsWithTag("Buildings");
        if (buildings == null || buildings.Length < 2)
        {
            Debug.LogError("El Mandoob needs at least two objects tagged Buildings in the gameplay scene.");
            return;
        }

        shopIndex = Random.Range(0, buildings.Length);
        shop = buildings[shopIndex];

        Shop shopComponent = shop.GetComponent<Shop>();
        if (shopComponent == null)
        {
            shopComponent = shop.AddComponent<Shop>();
        }
        shopComponent.displayName = ElMandoobContent.GetShiftBusiness(level);
        shop.tag = "Shop";

        for (int i = 0; i < buildings.Length; i++)
        {
            if (i == shopIndex)
            {
                continue;
            }

            House house = buildings[i].GetComponent<House>();
            if (house == null)
            {
                house = buildings[i].AddComponent<House>();
            }
            buildings[i].tag = "House";
        }
    }

    private string BuildOrderObjective(int orderCount)
    {
        if (orderCount == 1) return "وصّل طلب واحد";
        if (orderCount == 2) return "وصّل طلبين";
        return "وصّل " + orderCount + " طلبات";
    }

    private void UpdatePointer(GameObject building)
    {
        if (building == null || pointer == null)
        {
            return;
        }

        pointerPosition = building.transform.position;
        pointerPosition.y += 2f;
        pointer.transform.position = pointerPosition;
        pointer.SetActive(true);
    }

    private void ChangeTarget(GameObject building)
    {
        if (questPointer != null)
        {
            questPointer.Target = building;
        }
    }

    public void receiveButtonClick()
    {
        if (shiftResolved || currentOrder == null || shop == null || destination == null)
        {
            return;
        }

        carryingOrder = true;
        if (player != null)
        {
            player.carryingOrder = true;
        }

        if (recieveButton != null)
        {
            recieveButton.gameObject.SetActive(false);
        }

        Shop shopComponent = shop.GetComponent<Shop>();
        if (shopComponent != null)
        {
            shopComponent.havingOrder = false;
        }

        UpdatePointer(destination);
        ChangeTarget(destination);

        if (hud != null)
        {
            hud.SetOrder(currentOrder, true);
            hud.ShowMessage(currentOrder.customerMessage);
        }
    }

    public void deliverButtonClick()
    {
        if (shiftResolved || !carryingOrder || currentOrder == null)
        {
            return;
        }

        ElMandoobOrder deliveredOrder = currentOrder;

        House destinationHouse = destination != null ? destination.GetComponent<House>() : null;
        if (destinationHouse != null)
        {
            destinationHouse.isDesination = false;
        }

        if (deliverButton != null)
        {
            deliverButton.gameObject.SetActive(false);
        }

        carryingOrder = false;
        if (player != null)
        {
            player.carryingOrder = false;
        }

        inProcess = false;
        numDeliveredOrders++;

        AwardDelivery(deliveredOrder);
        ElMandoobContent.ApplyCompletedOrder(data, deliveredOrder);
        SaveSystem.Save(data);

        if (deliveredOredersDisplay != null)
        {
            tmp3 = deliveredOredersDisplay.GetComponent<TextMeshProUGUI>();
            if (tmp3 != null)
            {
                tmp3.text = numDeliveredOrders.ToString();
            }
        }

        if (hud != null)
        {
            hud.RefreshStats(data);
            hud.ShowMessage(deliveredOrder.deliveredMessage);
            hud.SetWaitingForOrder();
        }

        currentOrder = null;

        if (deliveredOrder.storyOrder && deliveredOrder.storyStage == 5)
        {
            StartCoroutine(ShowStoryFinishedAfterDelay());
        }

        if (useTime && numDeliveredOrders >= numTotalOrders)
        {
            gameCompleted();
            return;
        }

        // Defensive fallback: late shifts normally end when their timer expires.
        if (!useTime && timer == null && numDeliveredOrders >= numTotalOrders)
        {
            gameCompleted();
            return;
        }

        UpdatePointer(shop);
        ChangeTarget(shop);
    }

    private void AwardDelivery(ElMandoobOrder order)
    {
        if (data == null || order == null)
        {
            return;
        }

        int safeDeliveryBonus = 0;
        if (player != null)
        {
            safeDeliveryBonus = Mathf.Max(0, player.currentLives - 1) * 5;
        }

        int payout = order.TotalPay + safeDeliveryBonus;

        currentShiftEarnings += payout;
        currentShiftTips += order.tip;

        data.money += payout;
        data.totalTips += order.tip;
        data.reputation += Mathf.Max(1, order.reputationReward);
        data.completedDeliveries++;

        Debug.Log(
            "EL MANDOOB DELIVERY: +" + payout + " EGP | Balance: " + data.money +
            " EGP | Customer: " + order.customerName + " | Business: " + order.businessName);
    }

    private IEnumerator GenerateOrder()
    {
        while (!shiftResolved)
        {
            if (!inProcess && shop != null && buildings != null && buildings.Length > 1)
            {
                inProcess = true;
                currentOrder = ElMandoobContent.CreateOrder(data, level);

                Shop shopComponent = shop.GetComponent<Shop>();
                if (shopComponent != null)
                {
                    shopComponent.havingOrder = true;
                    shopComponent.displayName = currentOrder.businessName;
                }

                destinationIndex = Random.Range(0, buildings.Length);
                while (destinationIndex == shopIndex)
                {
                    destinationIndex = Random.Range(0, buildings.Length);
                }

                destination = buildings[destinationIndex];
                House house = destination.GetComponent<House>();
                if (house != null)
                {
                    house.isDesination = true;
                    house.customerName = currentOrder.customerName;
                    house.address = currentOrder.address;
                }

                UpdatePointer(shop);
                ChangeTarget(shop);

                if (hud != null)
                {
                    hud.SetOrder(currentOrder, false);

                    string message = currentOrder.pickupMessage;
                    if (data.completedDeliveries == 0 && numDeliveredOrders == 0)
                    {
                        message = "أول شيفت ليك كمندوب. اتبع السهم للمحل، استلم الطلب، وبعدها وصّله للعنوان. " +
                                  currentOrder.pickupMessage;
                    }

                    hud.ShowMessage(message, data.completedDeliveries == 0 ? 6f : 4.5f);
                }
            }

            yield return new WaitForSeconds(Random.Range(3f, 5f));
        }
    }

    private IEnumerator ShowStoryFinishedAfterDelay()
    {
        yield return new WaitForSecondsRealtime(5f);
        if (hud != null && !shiftResolved)
        {
            hud.ShowStoryFinished();
        }
    }

    public void endGame()
    {
        if (shiftResolved)
        {
            return;
        }

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
        if (shiftResolved)
        {
            return;
        }

        shiftResolved = true;
        StopGameplayForResult();

        if (itemDeliveredCanvas != null)
        {
            itemDeliveredCanvas.SetActive(false);
        }
        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
        }
    }

    public void gameCompleted()
    {
        if (shiftResolved)
        {
            return;
        }

        shiftResolved = true;
        int starsEarned = calculateScore();

        data.completedShifts++;
        data.levelUnlocked = Mathf.Max(data.levelUnlocked, Mathf.Min(8, level + 1));
        if (data.stars != null && level >= 1 && level <= data.stars.Length)
        {
            data.stars[level - 1] = Mathf.Max(data.stars[level - 1], starsEarned);
        }
        SaveSystem.Save(data);

        StopGameplayForResult();

        if (scoreDisplay != null)
        {
            tmp = scoreDisplay.GetComponent<TextMeshProUGUI>();
            ElMandoobBootstrap.ApplyArabicText(
                tmp,
                "كسبت " + currentShiftEarnings + " جنيه" +
                (currentShiftTips > 0 ? " منهم " + currentShiftTips + " بقشيش" : "") +
                " | التقييم " + starsEarned + "/3");
        }

        if (itemDeliveredCanvas != null)
        {
            itemDeliveredCanvas.SetActive(false);
        }
        if (gameCompletedSceen != null)
        {
            gameCompletedSceen.SetActive(true);
        }
    }

    private void StopGameplayForResult()
    {
        if (timer != null)
        {
            timer.counting = false;
        }

        if (player != null)
        {
            player.enabled = false;
        }

        if (recieveButton != null)
        {
            recieveButton.gameObject.SetActive(false);
        }
        if (deliverButton != null)
        {
            deliverButton.gameObject.SetActive(false);
        }

        if (pointer != null)
        {
            pointer.SetActive(false);
        }
        if (questPointer != null)
        {
            questPointer.Target = null;
        }

        if (hud != null)
        {
            hud.HideForResult();
        }
    }

    public int calculateScore()
    {
        if (player != null && player.currentLives <= 0)
        {
            return 0;
        }

        if (numDeliveredOrders < numTotalOrders)
        {
            return 0;
        }

        if (useTime)
        {
            float remainingTime = timer != null
                ? timer.secondsLeft + timer.minutesLeft * 60
                : 0f;

            return 1 + (int)Mathf.Min(remainingTime / 15f, 2f);
        }

        int extraDeliveries = Mathf.Max(0, numDeliveredOrders - numTotalOrders);
        return 1 + Mathf.Min(2, extraDeliveries / 2);
    }
}
