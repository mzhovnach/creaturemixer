using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlyingPopup : MonoBehaviour
{
    [SerializeField]
    Text _text;

    [SerializeField]
    GameObject _innerObject;

    [SerializeField]
    CanvasGroup _canvas;

    [SerializeField]
    float _destroyDelay = 0;

    public void Init(string text)
    {
        if (_text)
        {
            _text.text = text;
        }
        if (_innerObject)
        {
            if (_canvas)
            {
                _canvas.alpha = 0;
                LeanTween.value(_canvas.gameObject, 0.0f, 1.0f, _destroyDelay / 3.0f)
                    .setEase(LeanTweenType.easeInOutSine)
                    .setOnUpdate((float val)=>
                    {
                        _canvas.alpha = val;
                    });
                LeanTween.delayedCall(_destroyDelay / 3.0f * 2.0f, ()=>
                {
                    LeanTween.value(_canvas.gameObject, 1.0f, 0.0f, _destroyDelay / 3.0f)
                        .setEase(LeanTweenType.easeInSine)
                        .setOnUpdate((float val) =>
                        {
                            _canvas.alpha = val;
                        });
                });
            }
            // fly via code
            LeanTween.moveLocalY(_innerObject, 150, _destroyDelay)
                .setEase(LeanTweenType.easeInOutSine);
        } else
        {
            // should fly via animation
        }
        GameObject.Destroy(gameObject, _destroyDelay + 0.01f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
