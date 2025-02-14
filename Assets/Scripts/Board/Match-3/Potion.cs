using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{
    public OrbType potionType;

    public int xIndex;
    public int yIndex;

    public bool isMatched;
    private Vector2 currentPosition;
    private Vector2 targetPosition;

    public bool isMoving;
    public float moveDuration = 0.2f;

    public Animator animator;

    public Potion(int _x, int _y) 
    {
        xIndex = _x;
        yIndex = _y;
    }

    private void Awake()
    {
       animator = GetComponent<Animator>();
    }

    public void SetIndicies(int _x, int _y) 
    {
        xIndex = _x;
        yIndex = _y;
    }

    // MoveToTarget
    public void MoveToTarget(Vector2 _targetPos)
    {
        StartCoroutine(MoveCoroutine(_targetPos));
    }

    // MoveCoroutine
    private IEnumerator MoveCoroutine(Vector2 _targetPos)
    {
        isMoving = true;
        float duration = 0.2f;

        Vector2 startPostion = transform.position;
        float elaspedTime = 0f;
        while (elaspedTime < duration)
        {
            float t = elaspedTime / duration;
            transform.position = Vector2.Lerp(startPostion, _targetPos, t);
            elaspedTime += Time.deltaTime;
            yield return null;  
        }
        transform .position = _targetPos;
        isMoving = false;   
    }
}

public enum OrbType 
{
    Red,
    Blue, 
    Green, 
    Purple,
    White
}