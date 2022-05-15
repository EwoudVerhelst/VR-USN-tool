using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;

public class Communicator : MonoBehaviour
{
    private DataPublisher _dataPublisher;
    private RequestReceiver _requestReceiver;

    private RequestHandler _requestHandler;
    

    public bool publishData = false;
    public bool receiveRequests = false;

    private int publishRate = 2000;
    public GameObject _headRotationTracker;
    public GameObject _playedNotesTracker;

    public GameObject _objectiveTester;
    void Start()
    {   
        // get the ExtractHeadRotation and ExtractPlayedNotes components and construct the DataPublisher
        ExtractHeadRotation _extractHeadRotationScript = _headRotationTracker.GetComponent<ExtractHeadRotation>();
        ExtractPlayedNotes _extractPlayedNotesScript = _playedNotesTracker.GetComponent<ExtractPlayedNotes>();
        ExtractMetaInfo _extractMetaInfo = GetComponent<ExtractMetaInfo>();

        // Objective Testing
        TesterDataDelay _testerDataDelay = null;
        if (_objectiveTester)
        {
            _testerDataDelay = _objectiveTester.GetComponent<TesterDataDelay>();
        }

        _dataPublisher = new DataPublisher(publishRate, _extractHeadRotationScript, _extractPlayedNotesScript, _extractMetaInfo, _testerDataDelay);

        // Construct the DataServer for receiving requests from the User Interface Controller
        _requestHandler = GetComponent<RequestHandler>();
        _requestReceiver = new RequestReceiver(_requestHandler);
     
        if (publishData)
        {
            _dataPublisher.Start();
        }

        if (receiveRequests)
        {
            _requestReceiver.Start();
        }

        //Ojective testing

    }

    void Update()
    {

    }

    private void OnDestroy()
    {
        if (_dataPublisher.Running)
        {
            _dataPublisher.Stop();
        }
        if (_requestReceiver.Running)
        {
            _requestReceiver.Stop();
        }

        Debug.Log("NetMQConfig cleanup");
        NetMQConfig.Cleanup(); // this line is needed to prevent unity freeze after one use, not sure why yet
    }
}