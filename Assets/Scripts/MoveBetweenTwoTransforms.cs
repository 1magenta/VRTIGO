using UnityEngine;
using System.Collections;

public class MoveBetweenTwoTransforms : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform pointC;
    public Transform pointD;
    public Transform pointE;
    public Transform pointF;
    public Transform pointG;

    public GameObject Sphere;

    public float speed = 1.0f;
    public float fastSpeed = 3.0f; // Speed between B and E
    private int currentPoint = 0;

    public int counter = 0; // To count the number of completed movements
    public string phase;

    public StartSystem startMenu;

    private bool isMoving = false; // Prevents multiple calls to movement logic

    void OnEnable()
    {
        if (Sphere == null)
        {
            Debug.LogError("Sphere is not assigned!");
            return;
        }

        Sphere.transform.position = pointA.position;
        StartCoroutine(InitialWaitSequence());
    }

    IEnumerator InitialWaitSequence()
    {
        phase = "RecordLeftPoint";
        yield return MoveToPoint(pointE, speed, 5f);
        phase = "RecordRightPoint";
        yield return MoveToPoint(pointB, speed, 5f);
        currentPoint = 0;
        phase = "NormalRecord";
        yield return MoveToPoint(pointA, speed, 2f);
        StartCoroutine(MovementLoop());
    }

    IEnumerator MoveToPoint(Transform targetTransform, float moveSpeed, float delay)
    {
        isMoving = true;
        while (Vector3.Distance(Sphere.transform.position, targetTransform.position) > 0.01f)
        {
            Sphere.transform.position = Vector3.MoveTowards(Sphere.transform.position, targetTransform.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(delay);
        isMoving = false;
    }

    IEnumerator MovementLoop()
    {
        while (true)
        {
            if (phase != "NormalRecord") yield break;

            yield return MoveToNextPoint();
            if (CheckEndCondition()) yield break;
        }
    }

    IEnumerator MoveToNextPoint()
    {
        if (isMoving) yield break;

        Transform target = GetTargetTransform();
        float moveSpeed = speed; // Faster speed for B to E

        yield return MoveToPoint(target, moveSpeed, 0);

        currentPoint = (currentPoint + 1) % 9; // Cycle through points

        if (currentPoint == 0) counter++;
    }

    Transform GetTargetTransform()
    {
        return currentPoint switch
        {
            0 => pointB,
            1 => pointC,
            2 => pointD,
            3 => pointB,
            4 => pointE,
            5 => pointF,
            6 => pointG,
            7 => pointE,
            8 => pointA,
            _ => pointA,
        };
    }

    bool CheckEndCondition()
    {
        if (startMenu.recording && counter > 2 || !startMenu.recording && counter == 1)
        {
            Sphere.transform.position = pointA.position;
            counter = 0;
            this.enabled = false;
            return true;
        }
        return false;
    }
}
