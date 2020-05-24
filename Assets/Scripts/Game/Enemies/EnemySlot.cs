using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO слот що тримає ворога, великі вороги займають одразу кілька слотів, можливо якісь еффекти на слотах (вогонь, наприклад)
//TODO можливо один з паверапів може блокувати слот на кілька ходів
public class EnemySlot : MonoBehaviour
{
    public int Id;
    public BoxCollider2D Collider;
    private Enemy _enemy;

    public bool RemoveEnemy(Enemy enemy)
    {
        if (_enemy == enemy)
        {
            _enemy = null;
            return true;
        }
        return false;
    }

    public void SetEnemy(Enemy enemy)
    {
        _enemy = enemy;
    }

    public bool IsEmpty()
    {
        return _enemy == null;
    }

    public Enemy GetEnemy()
    {
        return _enemy;
    }
}