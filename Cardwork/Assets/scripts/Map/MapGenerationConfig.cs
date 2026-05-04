using UnityEngine;

[CreateAssetMenu(menuName = "Map/Map Generation Config")]
public class MapGenerationConfig : ScriptableObject
{
    [Header("Node Weights (sum doesn't matter)")]
    public float weightFight = 60f;
    public float weightCampfire = 20f;
    public float weightChest = 15f;
    public float weightMystery = 5f;

    [Header("Chest")]
    public int chestOptionsMin = 2;
    public int chestOptionsMax = 3;

    [Header("Campfire Heal")]
    [Range(0f, 1f)] public float healMissingHpPercent = 0.35f;
    public int healMinimum = 0;
}