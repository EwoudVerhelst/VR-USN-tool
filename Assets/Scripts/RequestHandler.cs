using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Handles the request received in DataServer
///     This has to be executed on the main thread to be able to perform certain functions
/// </summary>
///

public class RequestHandler : MonoBehaviour
{
    // alowing recentering
    public GameObject _OVRCameraRig;
    private OVRManager _OVRManager;

    // Game Manager
    public GameObject _gameController;
    private GameManager _gameManager;

    // Communicator
    public GameObject _communicator;
    private ExtractMetaInfo _extractMetaInfo;

    // To reset the rehabilitation measures when a new patient is selected
    public GameObject _rehabilitationTracker;
    private ExtractRehabilitationMeasures _extractRehabilitationMeasures;

    // For objective testing
    public GameObject _objectiveTester;
    private TestingDataDelayReverse _testingDataDelayReverse;

    // to tell when a request needs attention
    private bool requestReceived = false;

    // The actual request in Dictionary form
    private Dictionary<string, string> request;



    // Start is called before the first frame update
    void Start()
    {
        _OVRManager = _OVRCameraRig.GetComponent<OVRManager>();
        _gameManager = _gameController.GetComponent<GameManager>();
        _extractMetaInfo = _communicator.GetComponent<ExtractMetaInfo>();
        _extractRehabilitationMeasures = _rehabilitationTracker.GetComponent<ExtractRehabilitationMeasures>();
        _testingDataDelayReverse = _objectiveTester.GetComponent<TestingDataDelayReverse>();
    }

    // Update is called once per frame
    void Update()
    {
        if (requestReceived)
        {
            requestReceived = false;
            HandleRequest();
        }

    }

    // Handle the request given in request dictionary
    private void HandleRequest()
    {
        switch (this.request["request"])
        {
            case "reset_hmd_view":
                ResetHMDView();
                break;

            case "change_module":
                ChangeModule();
                break;

            case "set_number_of_notes_to_repeat":
                SetNumberOfNotesToRepeat();
                break;

            case "set_current_player":
                SetCurrentPlayer();
                break;

            case "pause_game":
                PauseGame();
                break;

            case "resume_game":
                ResumeGame();
                break;

            case "test_request_delay":
                TestRequestDelay();
                break;

            default:
                Debug.LogError("not a valid request");
                break;
        }

    }



    // Reset the HMD view of the user
    private void ResetHMDView()
    {
        _OVRManager.publicRescenterPose();
    }

    private void ChangeModule()
    {
        // Set new module, difficulty and neglectType in the GameManager
        Module newModule = StringToModule(this.request["module"]);
        Difficulty newDifficulty = StringToDifficulty(this.request["difficulty"]);
        NeglectType newNeglectType = StringToNeglectType(this.request["neglect_type"]);
        _gameManager.SetCurrentModule(newModule);
        _gameManager.SetCurrentDifficulty(newDifficulty);
        _gameManager.SetCurrentNeglectType(newNeglectType);

        // Set the session name in ExtractMetaInfo
        _extractMetaInfo.SetCurrentSessionName(this.request["session_name"]);
    }

    private void SetCurrentPlayer()
    {
        Debug.Log("Set current player to: " + request["current_player"]);
        _extractMetaInfo.SetCurrentPlayer(request["current_player"]);

        // Trigger that the metainfo has changed
        _extractMetaInfo.SetMetaInfoChanged();

        // Reset the values in the CSV
        _extractRehabilitationMeasures.ResetRehabiliationMeasures();
    }

    private void SetNumberOfNotesToRepeat()
    {
        string input = request["number_of_notes_to_repeat"];
        int result;
        try
        {
            result = int.Parse(input);
        }
        catch
        {
            Debug.LogError($"Unable to parse '{input}'");
            return;
        }

        _gameManager.SetNumberOfNotesToRepeat(result);
    }


    // Pause the game
    private void PauseGame()
    {
        _gameManager.PauseGame();
    }

    // Resume the game
    private void ResumeGame()
    {
        _gameManager.ResumeGame();
    }

    // for testing the delay for a command comming from the therapist interface;
    private void TestRequestDelay()
    {
        string id = request["id"];
        _testingDataDelayReverse.addDelayData(id);
    }


    // Set the requestReceived bool to true so that the request can be handled in Update()
    public void RequestReceived()
    {
        this.requestReceived = true;
    }

    // Set the Request dictionary
    public void SetRequest(Dictionary<string, string> request)
    {
        this.request = request;
    }

    private string DictionaryToString(Dictionary<string, string> dictionary)
    {
        string toPrint = "";
        foreach (string key in dictionary.Keys)
        {
            toPrint += key + " : " + dictionary[key] + " -- ";
        }

        return toPrint;
    }

    private Module StringToModule(string stringModule)
    {
        var modules = System.Enum.GetValues(typeof(Module));
        
        foreach(Module module in modules)
        {
            if (string.Equals(module.ToString(), stringModule, System.StringComparison.OrdinalIgnoreCase)) {
                return module;
            }
        }
        Debug.LogError("no corresponding module found");
        return Module.None;
    }

    private Difficulty StringToDifficulty(string stringDifficulty)
    {
        var difficulties = System.Enum.GetValues(typeof(Difficulty));

        foreach (Difficulty difficulty in difficulties)
        {
            if (string.Equals(difficulty.ToString(), stringDifficulty, System.StringComparison.OrdinalIgnoreCase))
            {
                return difficulty;
            }
        }
        Debug.LogError("no corresponding Difficulty found");
        return Difficulty.Zero;
    }

    private NeglectType StringToNeglectType(string stringNeglectType)
    {
        var neglectTypes = System.Enum.GetValues(typeof(NeglectType));

        foreach (NeglectType neglectType in neglectTypes)
        {
            if (string.Equals(neglectType.ToString(), stringNeglectType, System.StringComparison.OrdinalIgnoreCase))
            {
                return neglectType;
            }
        }
        Debug.LogError("no corresponding Difficulty found");
        return NeglectType.Left;
    }


}
