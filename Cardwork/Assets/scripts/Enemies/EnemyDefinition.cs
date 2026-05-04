using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    public string enemyName = "Enemy";
    public Sprite sprite;

    public int minHP = 1;
    public int maxHP = 30;
    
    

    private void OnValidate()
    {
        if(minHP < 1) minHP = 1;
        if(maxHP < minHP) maxHP = minHP;
    }

    [Header("Moves (Serializable)")] public List<EnemyMove> moves = new();
}