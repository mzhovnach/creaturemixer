using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemiesQueue : MonoBehaviour
{
    //TODO
    // Клас слідкує за чергою ворогів, відраховує ходи до появи нового

    public Text TurnesText;                 // через скільки ходів з"явиться ворог
    public Image NextColorImage;            // колір наступного ворога
    public CanvasGroup AGroup;              // ховати панель через альфу коли вже немає ворогів в черзі
    private List<QueueElement>  _queue;     // черга

    public void InitQueue(List<QueueElement> elements)
    {
        _queue = elements;
        if (_queue.Count == 0)
        {
            HideQueue(true);
        } else
        {
            if (!CreateNextEnemies(true))
            {
                SetNextEnemyAtStart();
            }
        }
    }

    private void HideQueue(bool force)
    {
        LeanTween.cancel(AGroup.gameObject);
        if (force)
        {
            AGroup.alpha = 0;
        } else
        {
            LeanTween.value(AGroup.gameObject, AGroup.alpha, 0, 0.5f)
                .setEaseInOutSine();
        }
    }

    private void ShowQueueForce()
    {
        LeanTween.cancel(AGroup.gameObject);
        AGroup.alpha = 1;
    }

    private void SetNextEnemyAtStart()
    {
        TurnesText.text = _queue[0].Delay.ToString();
        //TODO color
    }

    private bool CreateNextEnemies(bool atStart)
    {
        //TODO намагаємось заспавнити ворогів на поле (Enemies.cs) якщо ділей = 0 і є вільні слоти
        //...
        if (_queue.Count == 0)
        {
            HideQueue(atStart);
        }
        return false;
    }
}
