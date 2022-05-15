using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Looker : MonoBehaviour
{
    // Game manager
    public GameObject _gameController;
    private GameManager _gameManager;

    public GameObject _headRotationTracker;
    private ExtractHeadRotation _extractHeadRotation;

    // To shrink and expand the torus
    public GameObject target;
    private TorusChanger _torusChanger;

    // is the patient currently looking at the target
    private bool isLooking = false;

    // target head rotation
    private Vector3 fixationPoint = new Vector3(10, 0, 0);

    // marge
    private float margeX = 10;
    private float margeY = 5;


    // Start is called before the first frame update
    void Start()
    {
        // Game manager
        _gameManager = _gameController.GetComponent<GameManager>();

        // ExtractHeadRotation script
        _extractHeadRotation = _headRotationTracker.GetComponent<ExtractHeadRotation>();

        _torusChanger = target.GetComponent<TorusChanger>();

    }

    // Update is called once per frame
    void Update()
    {

        // If user has just entered the target
        if (LookingAtTarget() && isLooking == false && target.activeSelf)
        {
            isLooking = true;
            _torusChanger.shrinkTorus();

        // If user has just left the target
        } else if (!LookingAtTarget() && isLooking == true && target.activeSelf)
        {
            isLooking = false;
            _torusChanger.expandTorus();
        }


    }

    public void SetTargetActive(bool active)
    {

        target.SetActive(active);

        if (active)
        {
            target.SetActive(active);
            _torusChanger.ResetTorus();
        }
        else
        {
            if (_torusChanger)
            {
                _torusChanger.SetTorusinactive();
                target.SetActive(active);
            }
        }


    }

    private bool LookingAtTarget()
    {
        
        if (_extractHeadRotation.GetHeadRotation()[1] < fixationPoint[1] + margeY 
            && _extractHeadRotation.GetHeadRotation()[1] > fixationPoint[1] - margeY
            && _extractHeadRotation.GetHeadRotation()[0] < fixationPoint[0] + margeX
            && _extractHeadRotation.GetHeadRotation()[0] > fixationPoint[0] - margeX
            )
        {
            return true;
        }
        return false;
        
    }

    public void TorusHasShrunk()
    {
        SetTargetActive(false);
        _gameManager.TargetIsDone();
        isLooking = false;

    }

}
