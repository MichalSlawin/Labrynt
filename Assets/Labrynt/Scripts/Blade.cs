using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blade : MonoBehaviour
{
    public float moveDistanceZ = 0;
    public float moveDistanceX = 0;
    public float moveDistanceY = 0;
    public float moveTime;
    public float waitTime;

    private float timeCounter;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool moving;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.localPosition;
        targetPosition = new Vector3(startPosition.x + moveDistanceX, startPosition.y + moveDistanceY, startPosition.z + moveDistanceZ);

        StartCoroutine(Move(waitTime));
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            timeCounter += Time.deltaTime / moveTime;
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, timeCounter);

            if (transform.localPosition == targetPosition)
            {
                moving = false;
                targetPosition = startPosition;
                startPosition = transform.localPosition;

                StartCoroutine(Move(waitTime));
            }
        }
    }

    private IEnumerator Move(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        timeCounter = 0f;
        moving = true;
    }
}
