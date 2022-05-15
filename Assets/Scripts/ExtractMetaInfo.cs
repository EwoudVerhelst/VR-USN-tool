using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaMessage
{
    public string message_type = "meta_message";
    public string current_player = "";
    public string current_notes = "";
    public string current_module = "";
    public string current_difficulty = "";
    public bool active_game = true;
}

public class ExtractMetaInfo : MonoBehaviour
{
    // Boolean to indicate if metainfo has changed 
    private bool metaInfoChanged;

    // Game Manager
    public GameObject _gameController;
    private GameManager _gameManager;

    // The current active player
    private string currentPlayer = "Ewoud Verhelst";
    // The current session name
    private string currentSessionName = "";


    // Start is called before the first frame update
    void Start()
    {
        _gameManager = _gameController.GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Return the bool that indicates wether or not the metadata has changed
    public bool GetMetaInfoChanged()
    {
        bool changed = this.metaInfoChanged;

        // When this function gets called, put the metaInfoChanged variable back to false
        this.metaInfoChanged = false;

        return changed;
    }

    public void SetMetaInfoChanged()
    {
        this.metaInfoChanged = true;
    }

    public MetaMessage CreateMetaMessage()
    {

        string currentPlayer = this.currentPlayer;
        string currentNotes = CreateCurrentNotesString();
        string currentModule = _gameManager.GetCurrentModule().ToString();
        string currentDifficulty = _gameManager.GetCurrentDifficulty().ToString();

        MetaMessage metaMessage = new MetaMessage()
        {
            current_player = currentPlayer,
            current_notes = currentNotes,
            current_module = currentModule,
            current_difficulty = currentDifficulty
        };

        return metaMessage;
    }

    // turn the current assessmentSpawnAngles into a string to send
    // TODO: adapt to send the notes for rehabilitatoin too
    public string CreateCurrentNotesString()
    {
        int[] assessmentSpawnAngles = _gameManager.GetAssessmentSpawnAngles();
        string currentNotesString = string.Join(",", assessmentSpawnAngles);
        return currentNotesString;

    }

    public string GetCurrentPlayer()
    {
        return this.currentPlayer;
    }
    public void SetCurrentPlayer(string currentPlayer)
    {
        this.currentPlayer = currentPlayer;
    }

    public string GetCurrentSessionName()
    {
        return this.currentSessionName;
    }

    public void SetCurrentSessionName(string currentSessionName)
    {
        Debug.Log("set current session name to: " + currentSessionName);
        this.currentSessionName = currentSessionName;
    }

    public string GetCurrentModuleString()
    {
        return _gameManager.GetCurrentModule().ToString();
    }

    public string GetCurrentNeglectType()
    {
        return _gameManager.GetCurrentNeglectType().ToString();
    }

    // Determines wether or not  data should be published 
    public bool PublishData()
    {
        // Publish Head rotation and notes played data when the current module is assessment or rehabilitation
        if (_gameManager.GetCurrentModule() == Module.None || _gameManager.GetCurrentModule() == Module.Testing)
            return false;

        // If the game is paused, return false
        if (_gameManager.GameIsPaused())
        {
            return false;
        }

        return true;

    }
}
