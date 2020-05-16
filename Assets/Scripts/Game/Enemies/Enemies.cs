using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    //TODO
    // клас тримає поточних ворогів на полі
    // вороги в залежності від розмірів займають 1 - 3 слоти
    // якщо немає місця, то новий ворог з черги не з"являється

    public List<EnemySlot> Slots; // для простоти зараз 5 слотів в ряд
    private List<Enemy> _enemies = new List<Enemy>();

    public void ClearEnemiesForce()
    {
        for (int i = _enemies.Count - 1; i >= 0; --i)
        {
            Enemy enemy = _enemies[i];
            FreeSlotsFromEnemy(enemy);
            enemy.HideForce();
        }
        _enemies.Clear();
    }

    public float ClearEnemies()
    {
        float time = 0;
        if (_enemies.Count == 0)
        {
            return time;
        }
        for (int i = _enemies.Count - 1; i >= 0; --i)
        {
            Enemy enemy = _enemies[i];
            FreeSlotsFromEnemy(enemy);
            time = Mathf.Max(enemy.PlayHideAnimation(), time);
        }
        _enemies.Clear();
        return time;
    }

    private void FreeSlotsFromEnemy(Enemy enemy)
    {
        for (int i = 0; i < Slots.Count; ++i)
        {
            Slots[i].RemoveEnemy(enemy);
        }
    }
}
