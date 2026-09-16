using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // Legacy progression kept from Delivery Boy while El Mandoob is converted.
    public int healths;
    public int speed;
    public int levelUnlocked;
    public int[] stars = new int[8];

    public int audios;
    public float volume;

    // El Mandoob progression.
    public int money;
    public int reputation;
    public int completedDeliveries;
    public int completedShifts;
    public int totalTips;
    public int storyStage;

    public GameData()
    {
        healths = 1;
        speed = 5;
        levelUnlocked = 1;
        audios = 1;
        volume = 0f;

        money = 0;
        reputation = 0;
        completedDeliveries = 0;
        completedShifts = 0;
        totalTips = 0;
        storyStage = 0;

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i] = 0;
        }
    }
}
