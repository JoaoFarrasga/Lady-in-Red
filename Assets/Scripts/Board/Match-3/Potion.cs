using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{
    public PotionType potionType;

    public int xIndex;
    public int yIndex;

    public bool isMatched;
    private Vector2 currentPosition;
    private Vector2 targetPosition;

    public bool isMoving;
    public float moveDuration = 0.2f;

    public Potion(int _x, int _y) 
    {
        xIndex = _x;
        yIndex = _y;
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
        
        Vector2 startPosition = transform.position;
        float elaspedTime = 0f;

        while (elaspedTime < moveDuration)
        {
            float t = elaspedTime / moveDuration;

            transform.position = Vector2.Lerp(startPosition, _targetPos, t);

            elaspedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = _targetPos;
        isMoving = false;
    }
}

public enum PotionType 
{
    Red,
    Blue, 
    Green, 
    Purple,
    White
}