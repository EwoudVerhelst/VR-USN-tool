using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorusChanger : MonoBehaviour
{

    private Looker _looker;

    public GameObject _gameController;
    private GameManager _gameManager;

    // time it takes for torus to shrink completely in seconds
    private float shrinkTimeLong = 3f; // for during the other modules
    private float shrinkTimeShort = 1f; // for during the free to play modules

    // size and position of torus when completely shrinked
    private Vector3 shrinkGoalScale = new Vector3(0, 1, 0);
    private Vector3 shrinkgGoalPosition;

    // time it takes for torus to expand completely in seconds;
    private float expandTime = 5f;

    // size and position of torus when completely shrinked
    private Vector3 expandGoalScale = new Vector3(1, 1, 1);
    private Vector3 expandGoalPosition;

    // amount of each size change step
    private Vector3 step_size = new Vector3(0.01f, 0, 0.01f);



    // Start is called before the first frame update
    void Start()
    {
        _looker = GetComponentInParent<Looker>();

        // set shrink and expand goalPosition
        expandGoalPosition = GetComponent<Transform>().position;
        shrinkgGoalPosition = new Vector3(0, expandGoalPosition[1], expandGoalPosition[2]);

        _gameManager = _gameController.GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ResetTorus()
    {
        StopAllCoroutines();
        GetComponent<Transform>().localScale = expandGoalScale;
        GetComponent<Transform>().position = expandGoalPosition;
    }

    public void SetTorusinactive()
    {
        StopAllCoroutines();
    }



    public void shrinkTorus()
    {
        // stop all ongoing coroutines
        StopAllCoroutines();

        // start shrinking the torus
        StartCoroutine(shrinkTorusCoroutine());

    }

    IEnumerator shrinkTorusCoroutine()
    {

        Vector3 currentScale = GetComponent<Transform>().localScale;
        Vector3 scaleToDo = currentScale - shrinkGoalScale;
        Vector3 scaleDone = expandGoalScale - currentScale;

        if (scaleToDo[0] == 0)
        {
            yield return null;
        }

        // number of steps needed for reaching goal Scale
        float nStepsToDo = (int)(scaleToDo[0] / step_size[0]);

        // number of steps already done to reach that goal state
        float nStepsDone = (int)(scaleDone[0] / step_size[0]);

        // fraction of steps still needed to be done
        float stepFraction = nStepsToDo / (nStepsToDo + nStepsDone);

        // time per step needed for this
        float shrinkTime = _gameManager.GetCurrentModule() == Module.FreeToPlay ? shrinkTimeShort : shrinkTimeLong;
        float timeNeeded = shrinkTime * stepFraction;
        float timePerStep = timeNeeded / nStepsToDo;

        // stepsize for x-transform
        Vector3 currentPosition = GetComponent<Transform>().position;
        Vector3 positionTodo = currentPosition - shrinkgGoalPosition;
        Vector3 positionPerStep = positionTodo / nStepsToDo;

        for (int i = 0; i< (int)nStepsToDo; i++)
        {
            // adapt scale
            GetComponent<Transform>().localScale -= step_size;

            // adapt x position
            GetComponent<Transform>().position -= positionPerStep;

            yield return new WaitForSeconds(timePerStep);

        }

        GetComponent<Transform>().localScale = shrinkGoalScale;
        GetComponent<Transform>().position = shrinkgGoalPosition;

        // indicate to the Looker script that the torus has shrunken completely
        _looker.TorusHasShrunk();



    }

    public void expandTorus()
    {
        // stop all ongoing coroutines
        StopAllCoroutines();

        // start expanding the torus
        StartCoroutine(expandTorusCoroutine());
    }

    IEnumerator expandTorusCoroutine()
    {

        Vector3 currentScale = GetComponent<Transform>().localScale;
        Vector3 scaleToDo = expandGoalScale - currentScale;
        Vector3 scaleDone = currentScale - shrinkGoalScale;

        if (scaleToDo[0] == 0)
        {
            yield return null;
        }

        // number of steps needed for reaching goal Scale
        float nStepsToDo = scaleToDo[0] / step_size[0];

        // number of steps already done to reach that goal state
        float nStepsDone = scaleDone[0] / step_size[0];

        // fraction of steps still needed to be done
        float stepFraction = nStepsToDo / (nStepsToDo + nStepsDone);

        // time per step needed for this
        float timeNeeded = expandTime * stepFraction;
        float timePerStep = timeNeeded / nStepsToDo;

        // stepsize for x-transform
        Vector3 currentPosition = GetComponent<Transform>().position;
        Vector3 positionTodo = expandGoalPosition - currentPosition;
        Vector3 positionPerStep = positionTodo /  nStepsToDo;
        for (int i = 0; i < (int)nStepsToDo; i++)
        {
            // adapt scale
            GetComponent<Transform>().localScale += step_size;

            // adapt x position
            GetComponent<Transform>().position += positionPerStep;

            yield return new WaitForSeconds(timePerStep);
        }

        GetComponent<Transform>().localScale = expandGoalScale;
        GetComponent<Transform>().position = expandGoalPosition;

    }
}
