using System.Collections.Generic;
using UnityEngine;

// Castillo del jugador: cada enemigo que entra al trigger reduce 1 punto de Threat.
// El HashSet evita que un enemigo con múltiples colliders (hijos) descuente HP más de una vez.
public class Castle : MonoBehaviour
{
    private GameManager gameManager;

    // IDs de instancia de los enemigos ya procesados; se limpia en LateUpdate cada frame.
    private readonly HashSet<int> _processedThisFrame = new HashSet<int>();

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    private void LateUpdate()
    {
        _processedThisFrame.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        // GetComponentInParent permite que el tag esté en un hijo pero el Enemy esté en la raíz.
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null) return;

        int id = enemy.gameObject.GetInstanceID();
        if (_processedThisFrame.Contains(id)) return;
        _processedThisFrame.Add(id);

        enemy.RemoveEnemy();

        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        gameManager?.UpdateHp(-1);
    }
}
