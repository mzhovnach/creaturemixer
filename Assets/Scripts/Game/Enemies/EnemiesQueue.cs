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
    private Enemies _enemies;

    public void InitQueue(List<QueueElement> elements)
    {
        _enemies = GameManager.Instance.Game.AEnemies;
        _queue = elements;
        CreateNextEnemies(true);
        if (_queue.Count > 0)
        {
            ShowQueueForce();
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

    private void SetNextEnemy(bool atStart)
    {
        TurnesText.text = _queue[0].Delay.ToString();
        Color nextColor = _enemies.GetEnemyColor(_queue[0].Name);
        if (atStart)
        {
            NextColorImage.color = nextColor;
        } else
        {
            LeanTween.cancel(NextColorImage.gameObject);
            LeanTween.value(NextColorImage.gameObject, NextColorImage.color, nextColor, 0.5f)
                .setOnUpdate((Color val)=>
            {
                NextColorImage.color = val;
            });
        }
    }

    private bool CreateNextEnemies(bool atStart)
    {
        if (_queue.Count == 0) { 
            return false; 
        }
        //TODO намагаємось заспавнити ворогів на поле (Enemies.cs) якщо ділей = 0 і є вільні слоти
        bool added = false;
        bool continueAdding = true;
        // TODO можливо якщо жодного ворога немає - пришвидшуєм чергу, щоб не втикати довго
        if (_enemies.NoEnemiesLeft())
        {
            QueueElement element = _queue[0];
            element.Delay = 0;
            _queue[0] = element;
        }
        // намагаємось додати
        do
        {
            if (_queue.Count == 0) { break; }
            if (_queue[0].Delay <= 0)
            {
                continueAdding = _queue[0].Delay < 0; // якщо < 0 (-1), то продовжуєм додавати (на старті так треба ставити), якщо 0 - стоп, один новий ворог за хід
                if (_enemies.TryToAddEnemy(_queue[0]))
                {
                    // додали вдало
                    added = true;
                    _queue.RemoveAt(0);
                } else
                {
                    // не було слота
                    continueAdding = false;
                }
            } else
            {
                // ще не час додавати
                continueAdding = false;
            }
        } while (continueAdding);

        //
        if (_queue.Count == 0)
        {
            HideQueue(atStart);
        } else
        {
            SetNextEnemy(atStart);
        }
        //

        return added;
    }

    public bool OnTurnWasMade()
    {
        //returnes true if some enemy was added
        if (_queue.Count == 0) {
            return false;
        }
        //
        if (_queue[0].Delay > 0)
        {
            QueueElement element = _queue[0];
            element.Delay -= 1;
            _queue[0] = element;
        }
        return CreateNextEnemies(false);
    }
}
