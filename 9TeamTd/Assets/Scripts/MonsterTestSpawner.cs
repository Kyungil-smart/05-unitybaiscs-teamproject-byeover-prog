using UnityEngine;
using UnityEngine.Serialization;

public sealed class MonsterTestSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridSystem gridSystem;
    
    [SerializeField] private MonsterAgent monsterPrefab;

    [Header("Test Spawn")]
    [SerializeField] private KeyCode spawnKey = KeyCode.Space;
    
    [SerializeField, Min(1)] private int spawnCountPerPress = 1;

    [SerializeField] private float spawnHeight = 0.5f;

    private void Awake()
    {
        if (gridSystem == null)
            gridSystem = FindObjectOfType<GridSystem>();

        if (gridSystem == null)
        {
            Debug.LogError("[MonsterTestSpawner] GridSystem not found in scene.");
            enabled = false;
            return;
        }

        if (monsterPrefab == null)
        {
            Debug.LogError("[MonsterTestSpawner] Monster prefab is not assigned.");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(spawnKey))
            return;

        for (int i = 0; i < spawnCountPerPress; i++)
        {
            if (!gridSystem.TryGetRandomSpawnCell(out Cell spawnCell))
            {
                Debug.LogWarning("[MonsterTestSpawner] No reachable edge spawn cell.");
                return;
            }

            Vector3 pos = gridSystem.CellToWorld(spawnCell, y: spawnHeight);
            MonsterAgent agent = Instantiate(monsterPrefab, pos, Quaternion.identity);
            agent.name = $"Monster_{spawnCell.X}_{spawnCell.Y}";
        }
    }
}
