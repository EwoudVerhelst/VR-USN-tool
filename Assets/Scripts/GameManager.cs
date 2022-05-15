using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public enum Module
{
    None,
    Testing,
    Assessment,
    RightToLeft,
    SequenceRepeating,
    FreeToPlay,
}

public enum Difficulty
{
    Zero,
    One,
    Two,
    Three,
    Four,
}

public enum NeglectType
{
    Left,
    Right,
}

public enum GameState
{
    Pause,
    Play,
}
public class GameManager : MonoBehaviour
{
    /// <summary>
    ///     Controls the main part of the VR application
    /// </summary>
    ///


    // Comunicating outside the applicatoin
    public GameObject communicatorGameObject;
    private Communicator communicator;
    private ExtractMetaInfo extractMetaInfo;

    // alowing recentering
    public GameObject _OVRCameraRig;
    private OVRManager _OVRManager;
    public bool recenterPoseNow = false;

    // For the fixation point
    public GameObject _looker;
    private Looker _lookerScript;

    // Extracting rehabilition info and writing to csv
    public GameObject _rehabilitationTracker;
    private ExtractRehabilitationMeasures _extractRehabilitationMeasures;
    private System.DateTime noteHitTimer = System.DateTime.Now;
    private System.DateTime sequenceTimer = System.DateTime.Now;

    // Alows for pausing the game
    public bool gameIsPaused = false;

    // Allowing not rendering the controllers
    public GameObject leftController;
    public GameObject rightController;

    /*** SETUP ***/
    // Radius of the circle on which the buttons spawn
    public float arcRadius = 1.2f;

    // The button prefab 
    public GameObject buttonPf;

    // List of audioclips for the buttons
    public List<AudioClip> audioClips;
    public AudioClip specialAudio;

    // List of notes texts for the buttons
    private List<string> noteTexts;
    private string specialNoteText = "X";

    // List of colors for the button
    private List<Color> buttonColors = new List<Color>();



    /*** THERAPY VARIABLES ***/

    // current module and previous moudle
    public Module currentModule;
    private Module previousModule;

    // current difficulty level
    public Difficulty currentDifficulty;
    private Difficulty previousDifficulty;

    // Right or left neglect
    public NeglectType currentNeglectType;
    private NeglectType previousNeglectType;


    // dictionary that maps each difficulty level to the number of notes on scene
    private Dictionary<Difficulty, int> rehabDifficultyMap = new Dictionary<Difficulty, int>()
    {
        { Difficulty.Zero, 0},
        { Difficulty.One, 6},
        { Difficulty.Two, 7 },
        { Difficulty.Three, 8 },
        { Difficulty.Four, 9 },
    };

    /*** ASSESSMENT ***/

    /*** RIGHT TO LEFT ***/
    // index of note that needs te be played now 
    private int noteToPlayIndex = -1;
    

    /*** SEQUENCE REPEATING ***/
    // number of notes that currently need to be repeated
    public int numberOfNotesToRepeat = 1;
    // list of notes that currently need to be repeated
    private List<int> notesToRepeatList = new List<int>();
    // queue of notes that are not yet repeated 
    public Queue<int> notesToRepeatQueue = new Queue<int>();
    // number of seconds to wait between each trigger when showing the user which notes to repeat
    public float waitBetweenEachRepeat = 1f;
    // time to wait until replaying of sequence when no input of user
    private float waitForInputTime = 20f;
    private float timeUntilReplay = Mathf.Infinity;

    /*** FREE TO PLAY ***/
    // min en max values of the timer for setting the target active
    private int freeToPlayTimer;
    private int freeToPlayTimerMin = 40;
    private int freeToPlayTimerMax = 60;



    // list of spawn angles
    private int[] assessmentSpawnAngles = { 70, 50, 30, 10, -10, -30, -50, -70 };
    private int[] leftRehabSpawnAngles = { 18, 6, -6, -18, -30, -42, -54, -66, -78};
    private int[] rightRehabSpawnAngles = { -18, -6, 6, 18, 30, 42, 54, 66, 78 };
    private int[] testSpawnAngles1 = { 20, 0, -20 };
    private int[] testSpawnAngles2 = { 40, 20, 0, -20, -40 };
    private int[] testSpawnAngles3 = { 60, 40, 20, 0, -20, -40, -60 };
    private int[] testSpawnAngles4 = { 80, 60, 40, 20, 0, -20, -40, -60, -80};

    // List of buttons currently in the scene
    private List<GameObject> buttonList = new List<GameObject>();



    // Start is called before the first frame update
    void Start()
    {
        // find OVRManager component to be able to recenter pose 
        _OVRManager = _OVRCameraRig.GetComponent<OVRManager>();

        // Find the looker script for the fixation point
        _lookerScript = _looker.GetComponent<Looker>();

        // Find the extractRehabilitationMeasures script
        _extractRehabilitationMeasures = _rehabilitationTracker.GetComponent<ExtractRehabilitationMeasures>();

        // fill the buttonColors and noteTexts list
        createNoteTextsList();
        createButtonColorsList();

        // Initiate the current module and neglect type
        currentModule = Module.None;
        currentDifficulty = Difficulty.Zero;
        currentNeglectType = NeglectType.Left;
        InitiateModule();

        // Trigger the first metaInfoChanged
        communicator = communicatorGameObject.GetComponent<Communicator>();
        extractMetaInfo = communicatorGameObject.GetComponent<ExtractMetaInfo>();
        StartCoroutine(TriggerMetaInfoChanged());

    }



    // Update is called once per frame
    void Update()
    {
        // recenter functionality
        if (recenterPoseNow)
        {
            recenterPoseNow = false;
            recenterPose();
        }

        // Listen for changes in currentModule, difficulty or neglectType
        if (
            previousModule != currentModule 
            || previousDifficulty != currentDifficulty 
            || previousNeglectType != currentNeglectType
            )
        {
            Debug.Log("module, difficulty or neglect type changed: " + "currentModule: " + currentModule + " -- currentDifficulty: " + currentDifficulty + " -- currentNeglectType: " + currentNeglectType);

            // stop all running coroutines
            StopAllCoroutines();
            
            EndModule(previousModule);

            InitiateModule();
        }
        previousModule = currentModule;
        previousDifficulty = currentDifficulty;
        previousNeglectType = currentNeglectType;


        // deals with user not responding in SequenceRepeating module
        if (currentModule == Module.SequenceRepeating)
        {
            timeUntilReplay -= Time.deltaTime;

            // If time is up to respond, replay the note sequence
            if (timeUntilReplay <= 0)
            {
                Debug.Log("waited long enough");

                timeUntilReplay = Mathf.Infinity;
                StartCoroutine(PlayNoteSequence());


            }
        }

    }

    /// <summary>
    /// Inititate the currentModule by spawning the correct buttons
    /// </summary>
    private void InitiateModule()
    {
        switch (currentModule)
        {
            case Module.None:
                _lookerScript.SetTargetActive(false);
                break;
            case Module.Testing:
                InitiateTestingModule();
                break;
            case Module.Assessment:
                StartAssessmentModule();
                break;
            case Module.RightToLeft:
                InitiateRightToLeftModule();
                break;
            case Module.SequenceRepeating:
                InitiateSequenceRepeatingModule();
                break;
            case Module.FreeToPlay:
                InitiateFreeToPlayModule();


                break;
            default:
                break;
        }
    }


    /// <summary>
    /// End the currentModule by destroying the buttons currently in the scene
    /// </summary>
    /// <param name="previousModule">The previous module</param>
    private void EndModule(Module previousModule)
    {

        // Set the target inactive
        _lookerScript.SetTargetActive(false);

        // Destroy the buttons
        foreach (GameObject button in buttonList)
        {
            Destroy(button);

        }
        // clear the button list
        buttonList.Clear();

        // Turn the controllers back on
        TurnControllersOn();

        // Perform specific end functions based on the specific module that is ended
        switch (previousModule)
        {
            case Module.Assessment:
                Debug.Log("end assessment module");
                break;
            case Module.SequenceRepeating:
                Debug.Log("end sequence repeating module");
                EndSequenceRepeatingModule();
                break;
            case Module.RightToLeft:
                Debug.Log("end RightToLeft module");
                break;
            case Module.FreeToPlay:
                Debug.Log("end FreeToPlay module");
               
                break;
            default:
                break;

        }
    }

    /*
    * ASSESSMENT
    */


    private void StartAssessmentModule()
    {
        for (int i = 0; i < this.assessmentSpawnAngles.Length; i++)
        {
            // spawn the button
            SpawnButton(this.assessmentSpawnAngles[i], buttonColors[i], noteTexts[i], audioClips[i]);

            // Set the ball
            buttonList[i].GetComponent<ButtonTrigger>().setIsBall(true);
        }
    }

    /*
    * TESTING
    */

    /// <summary>
    /// Initiate the Test module by setting the target active and turning on the controllers
    /// </summary>
    private void InitiateTestingModule()
    {
        SetTargetActiveWithDelay();

        // set the controllers active 
        TurnControllersOn();
    }

    /// <summary>
    ///  Give notes for testing purposes
    /// </summary>
    private void StartTestingModule()
    {
        // Turn controllers on
        TurnControllersOn();

        // Spawn the test notes
        int[] testSpawnAngles;
        switch (currentDifficulty)
        {
            case Difficulty.Zero:
                testSpawnAngles = testSpawnAngles1;
                break;
            case Difficulty.One:
                testSpawnAngles = testSpawnAngles2;
                break;
            case Difficulty.Two:
                testSpawnAngles = testSpawnAngles3;
                break;
            case Difficulty.Three:
                testSpawnAngles = testSpawnAngles4;
                break;
            default:
                testSpawnAngles = testSpawnAngles1;
                break;
             
        }

        for (int i = 0; i < testSpawnAngles.Length; i++)
        {
            SpawnButton(testSpawnAngles[i], buttonColors[i], noteTexts[i], audioClips[i]);
            buttonList[i].GetComponent<ButtonTrigger>().setIsBall(true);
        }
    }


    /*
    * REHABILITATION
    */

    /// <summary>
    /// Spawn the notes that should be on the instrument during the rehabilitation modules
    /// </summary>
    private void SpawnRehabilitationNotes(bool setBall = false)
    {

        // lookup number of buttons associated with current difficulty level
        int n_notes = this.rehabDifficultyMap[this.currentDifficulty];



        // Choose the right spawnangles based on type of neglect
        int[] rehabSpawnAngles;
        if (currentNeglectType == NeglectType.Left)
        {
            rehabSpawnAngles = leftRehabSpawnAngles;
        } else if ( currentNeglectType == NeglectType.Right){
            rehabSpawnAngles = rightRehabSpawnAngles;
        }
        else
        {
            Debug.LogError("invallid neglectType");
            return;
        }

        // Spawn the right amount of buttons    
        for (int i = 0; i < n_notes; i++)
        {   
            // if the last note
            if (i == n_notes-1)
            {
                SpawnButton(rehabSpawnAngles[i], buttonColors[i], specialNoteText, specialAudio, true);
            
            } else
            {
                SpawnButton(rehabSpawnAngles[i], buttonColors[i], noteTexts[i], audioClips[i]);
            }
            

            // Set the ball if needed
            buttonList[i].GetComponent<ButtonTrigger>().setIsBall(setBall);

        }
    }

   
    /// <summary>
    /// Set the target active with a small delay
    /// </summary>
    private void SetTargetActiveWithDelay()
    {
        // Stop all coroutines
        StopAllCoroutines();

        // turn of the timer for user to respond
        timeUntilReplay = Mathf.Infinity;

        StartCoroutine(SetTargetActiveWithDelayCoroutine());
    }


    /// <summary>
    /// Called from Looker when the target has been looked at completely
    /// </summary>
    public void TargetIsDone()
    {
        switch (currentModule)
        {
            case Module.None:
                break;
            case Module.Testing:
                StartTestingModule();
                break;
            case Module.Assessment:
                break;
            case Module.RightToLeft:
                StartRightToLeftModule();
                break;
            case Module.SequenceRepeating:
                StartSequenceRepeatingModule();
                break;
            case Module.FreeToPlay:
                StartFreeToPLayModule();
                break;
            default:
                break;
        }
    }



    /*
     * RIGHT TO LEFT
     */

    
    private void InitiateRightToLeftModule()
    {
        // Set the target active and turn of the controllers
        SetTargetActiveWithDelay();

        // Spawn the right amount of notes
        SpawnRehabilitationNotes();
    }

    private void StartRightToLeftModule()
    {
        // Turn the controllers back on
        TurnControllersOn();

        // Set the noteToPlay index to 0 (= most right note)
        noteToPlayIndex = 0;

        // Change the color en glow of the first note to indidcate it needs to be played 
        ButtonTrigger buttonTr = GetButtonById(GetButtonIdByIndex(noteToPlayIndex)).GetComponent<ButtonTrigger>();
        buttonTr.ChangeColor(Color.red);
        buttonTr.StartGlowing();

        // Start timing for extracting rehabilitation measures
        _extractRehabilitationMeasures.addRightToLeftTimingInfo(-1, -1, Difficulty.Zero, currentNeglectType);
        noteHitTimer = System.DateTime.Now; 
    }



    /// <summary>
    /// Handles the logic when a note is triggered during this module, gets called from the buttonTrigger script.
    /// When correct note is hit, the next note lights up .
    /// </summary>
    public void HandleTriggerNoteOnRightToLeft(int buttonId)
    {
        if (noteToPlayIndex == -1)
        {
            return;
        }

        // correct note played
        if (GetButtonIdByIndex(noteToPlayIndex) == buttonId)
        {
            // measure time in seconds it took to hit that note from the start of the sequence
            double deltaTime = (System.DateTime.Now - noteHitTimer).TotalSeconds;
            int noteId = GetButtonIdByIndex(noteToPlayIndex);
            _extractRehabilitationMeasures.addRightToLeftTimingInfo(deltaTime, noteId, currentDifficulty, currentNeglectType);

            // change the collor of button back to its original color and stop the glowing
            ButtonTrigger buttonTr = GetButtonById(buttonId).GetComponent<ButtonTrigger>();
            buttonTr.ResetColor();
            buttonTr.StopGlowing();
            
            // increase the noteToPlayIndex by 1
            if (noteToPlayIndex < rehabDifficultyMap[currentDifficulty]-1)
            {
                noteToPlayIndex++;

            }
            // end of sequence
            else
            {
                noteToPlayIndex = -1;

                // Set the target back active and turn controllers back off
                SetTargetActiveWithDelay();

                return;
            }

            // Change the color of the current note to play to indidicate it needs to be played
            buttonTr = GetButtonById(GetButtonIdByIndex(noteToPlayIndex)).GetComponent<ButtonTrigger>();
            buttonTr.ChangeColor(Color.red);
            buttonTr.StartGlowing();


        }
    }

    

    /*
     * SEQUENCE REPEATING
     */

    private void InitiateSequenceRepeatingModule()
    {
        // Turn off the controllers while needed to look at target
        TurnControllersOff();

        // Set the target active
        SetTargetActiveWithDelay();

        // Spawn the right amount of notes
        SpawnRehabilitationNotes();
    }

    private void StartSequenceRepeatingModule()
    {

        // Inititate new sequence
        CreateNotesToRepeat();
        StartCoroutine(PlayNoteSequence());

        _extractRehabilitationMeasures.AddSequenceRepeatingTimingInfoNewSequence(numberOfNotesToRepeat, currentDifficulty, currentNeglectType);
    }

    private void EndSequenceRepeatingModule()
    {
        // set numberOfNotesToRepeat back to one
        numberOfNotesToRepeat = 1;

        // Clear the note to repeat list and queue
        notesToRepeatList.Clear();
        notesToRepeatQueue.Clear();

        // Stop the ongoing coroutines
        StopCoroutine("PlayNoteSequence");


    }


    /// <summary>
    /// Handles the logic when a note is triggered during this module, gets called from the buttonTrigger script
    /// </summary>
    public void HandleTriggerNoteOnSequenceRepeating(int buttonId)
    {
        // turn of the timer
        timeUntilReplay = Mathf.Infinity;


        if (notesToRepeatQueue.Count == 0)
        {
            return;
        }

        // add line in csv for this note
        double dTime = (System.DateTime.Now - noteHitTimer).TotalSeconds;
        int notesToRepeat = notesToRepeatList.Count;
        int noteToPlayNext;
        bool correctlyPlayed;
 


        // if the note played is the note that should have been played, remove it from the queue 
        if (notesToRepeatQueue.Peek() == buttonId)
        {
            notesToRepeatQueue.Dequeue();

            // reset the timer for the user to respond
            timeUntilReplay = waitForInputTime;

            // the next note to be played and if it was correctly played
            correctlyPlayed = true;
            try
            {
                noteToPlayNext = notesToRepeatQueue.Peek();
            } catch(System.InvalidOperationException e)
            {
                // if queue is empty, set note to play next to dummy value
                noteToPlayNext = 360;
            }

            

        }
        // else start over
        else
        {
            // if sequence was wrong, set note to play next to dummy value
            correctlyPlayed = false;
            noteToPlayNext = 360;

            SequenceFailed();
        }

        // if queue is empty, sequence is succeeded 
        if (notesToRepeatQueue.Count == 0)
        {
            SequenceSucceeded();
        }

        //write the the rehabilitation measures CSV
        _extractRehabilitationMeasures.AddSequenceRepeatingTimingInfo(dTime, buttonId, noteToPlayNext, correctlyPlayed, notesToRepeat, currentDifficulty, currentNeglectType);


    }

    /// <summary>
    /// Randomly choose numberOfNotesToRepeat notes to repeat
    /// </summary>
    private void CreateNotesToRepeat()
    {
        // pick notes to repeat from list 
        for (int i = 0; i < numberOfNotesToRepeat; i++)
        {
            int randomIndex = Random.Range(0, this.rehabDifficultyMap[this.currentDifficulty]);
            notesToRepeatList.Add(GetButtonIdByIndex(randomIndex));
            notesToRepeatQueue.Enqueue(GetButtonIdByIndex(randomIndex));
        }
    }


    /// <summary>
    /// Set the number of notes to repeat
    /// </summary>
    public void SetNumberOfNotesToRepeat(int nNotes)
    {
        Debug.Log("set number of notes to repeat: " + nNotes);
        numberOfNotesToRepeat = nNotes;
    }

    /// <summary>
    /// The sequence was repeated correctly
    /// </summary>
    public void SequenceSucceeded()
    {

        // tell the Extract rehabilitation script that the sequence succeeded and how long it took
        double dTime = (System.DateTime.Now - sequenceTimer).TotalSeconds;
        int notesToRepeat = this.notesToRepeatList.Count;
        _extractRehabilitationMeasures.AddSequenceRepeatingSuceedingInfo(dTime, true, notesToRepeat, currentDifficulty, currentNeglectType);

        PlaySucceedAnimation();

        // clear the list and queue 
        this.notesToRepeatList.Clear();
        this.notesToRepeatQueue.Clear();

        // initiate next sequence by setting the target active and turning of the controllers
        SetTargetActiveWithDelay();


    }

    /// <summary>
    /// The sequence was wrongly repeated 
    /// </summary>
    public void SequenceFailed()
    {
        // tell the Extract rehabilitation script that the sequence failed and how long it took
        double dTime = (System.DateTime.Now - sequenceTimer).TotalSeconds;
        int notesToRepeat = this.notesToRepeatList.Count;
        _extractRehabilitationMeasures.AddSequenceRepeatingSuceedingInfo(dTime, false, notesToRepeat, currentDifficulty, currentNeglectType);


        PlayResetAnimation();

        // refil the queue
        this.notesToRepeatQueue.Clear();
        foreach (int note in this.notesToRepeatList)
        {
            notesToRepeatQueue.Enqueue(note);
        }

        // Repeat the same sequence 
        StartCoroutine("PlayNoteSequence");

        

    }

    /// <summary>
    /// Play the sequence of notes given in the notesToRepeat list
    /// </summary>
    IEnumerator PlayNoteSequence()
    {
        // turn of the timer to respond
        timeUntilReplay = Mathf.Infinity;

        // Turn of the controllers
        TurnControllersOff();

        // wait few seconds for sequence to start 
        yield return new WaitForSeconds(1);



        // Trigger each note
        foreach (int note in notesToRepeatList)
        {
            yield return new WaitForSeconds(waitBetweenEachRepeat);
            GameObject button = GetButtonById(note);
            button.GetComponent<ButtonTrigger>().TriggerButton(true, Color.red);
        }

        // turn controllers back on
        yield return new WaitForSeconds(waitBetweenEachRepeat);
        TurnControllersOn();

        // Set the timer for the user to respond
        timeUntilReplay = waitForInputTime;

        // Start timer for ExtractRehabilitation measures and initiale fir row in csv
        sequenceTimer = System.DateTime.Now;
        noteHitTimer = System.DateTime.Now;
        int notesToRepeat = notesToRepeatList.Count;
        _extractRehabilitationMeasures.AddSequenceRepeatingTimingInfoNewRepeat(notesToRepeatQueue.Peek(), notesToRepeat, currentDifficulty, currentNeglectType);
    }

    /*
     * FREE TO PLAY
     */

    private void InitiateFreeToPlayModule()
    {
        // Turn off the controllers while needed to look at target
        TurnControllersOff();

        // Set the target active
        SetTargetActiveWithDelay();

        // Spawn the right amount of notes
        SpawnRehabilitationNotes(true);
    }

    private void StartFreeToPLayModule()
    {
        // Turn off the controllers while needed to look at target
        TurnControllersOn();

        // Generate random timer delay
        freeToPlayTimer = Random.Range(freeToPlayTimerMin, freeToPlayTimerMax);

        StartCoroutine(TimerForFreeToPlay());
    }

    /// <summary>
    /// Timer to set target once in a while during free to play module
    /// </summary>
    IEnumerator TimerForFreeToPlay()
    {
        yield return new WaitForSeconds(freeToPlayTimer);
        TurnControllersOff();
        SetTargetActiveWithDelay();
    }


    /*
    * GENERAL
    */

    /// <summary>
    /// Spawn the buttons with SpawnAngle and attach the given audioclip to them
    /// </summary>
    private void SpawnButton(int spawnAngle, Color color, string noteText, AudioClip audioClip, bool specialNote = false)
    {

        // Quaternion for the rotation of the button
        Quaternion spawnRot = Quaternion.Euler(-45, spawnAngle, 0);


        // y-Position of the prefab
        float YPos = buttonPf.GetComponent<Transform>().position[1];

        // determine x and z postion of button to spawn
        // the midsection of the body is defined as Zero degrees
        float xPos = (Mathf.Sin(DegreesToRadial(spawnAngle)) * arcRadius);
        float zPos = (Mathf.Cos(DegreesToRadial(spawnAngle)) * arcRadius);

        // Create spawn position vector
        Vector3 spawnPos = new Vector3(xPos, YPos, zPos);

        // instantiate the button
        GameObject button = Instantiate(buttonPf, spawnPos, spawnRot);

        // set the correct id of the button, which is the spawnAngle
        button.GetComponent<ButtonTrigger>().SetButtonId(spawnAngle);

        // Set the correct color for the button
        button.GetComponent<ButtonTrigger>().SetOgColor(color);

        // Add the correct audiosource
        button.GetComponent<AudioSource>().clip = audioClip;
        // if it is a special note, crank up the volume
        if (specialNote)
        {
            button.GetComponent<AudioSource>().volume = 1;
        }

        // Display the correct note text behind the key
        button.GetComponentInChildren<TextMeshPro>().SetText(noteText);

        // Add to the buttonList for later use
        this.buttonList.Add(button);
    }

    /// <summary>
    /// return the Button objecdt given its id
    /// </summary>
    private GameObject GetButtonById(int buttonId)
    {
        foreach (GameObject button in buttonList)
        {
            if (button.GetComponent<ButtonTrigger>().GetButtonId() == buttonId)
            {
                return button;
            }
        }

        Debug.LogError("Button with id: " + buttonId + "not found in buttonlist");
        return null;
    }


    /// <summary>
    /// Returns the buttonId based on it index.
    //  ID corresponds with spawn angle
    /// </summary>
    private int GetButtonIdByIndex(int buttonIndex)
    {
        if (currentModule == Module.Assessment)
        {
            return assessmentSpawnAngles[buttonIndex];
        }
        else
        {
            if (currentNeglectType == NeglectType.Left)
            {
                return leftRehabSpawnAngles[buttonIndex];
            }
            else if (currentNeglectType == NeglectType.Right)
            {
                return rightRehabSpawnAngles[buttonIndex];
            }
            else
            {
                Debug.LogError("invalid neglectType");
                return -1;
            }
        }

    }

    public int GetButtonIndexById(int buttonId)
    {
        if (currentModule == Module.Assessment)
        {
            return System.Array.IndexOf(assessmentSpawnAngles, buttonId);
        }
        else
        {
            if (currentNeglectType == NeglectType.Left)
            {
                return System.Array.IndexOf(leftRehabSpawnAngles, buttonId);
            }
            else
            {
                return System.Array.IndexOf(rightRehabSpawnAngles, buttonId);
            }
        }
    }
    public void PlayResetAnimation()
    {
        foreach (GameObject button in buttonList)
        {
            button.GetComponent<ButtonTrigger>().TriggerButton(false, Color.red);
        }
    }

    public void PlaySucceedAnimation()
    {
        foreach (GameObject button in buttonList)
        {
            button.GetComponent<ButtonTrigger>().TriggerButton(false, Color.green);
        }
    }

    private void recenterPose()
    {
        _OVRManager.publicRescenterPose();
    }

    private void TurnControllersOff()
    {
        leftController.SetActive(false);
        rightController.SetActive(false);
    }

    private void TurnControllersOn()
    {
        leftController.SetActive(true);
        rightController.SetActive(true);
    }

    private float DegreesToRadial(float degree)
    {
        return (degree / 360) * (2 * Mathf.PI);
    }

    private float RadialToDegrees(float radial)
    {
        return (radial + 2 * Mathf.PI) * 360;
    }


    IEnumerator TriggerMetaInfoChanged()
    {
        yield return new WaitForSeconds(1);
        extractMetaInfo.SetMetaInfoChanged();
    }
 
    /// <summary>
    /// fills the noteTexts list with the right strings of notes 
    /// </summary>
    private void createNoteTextsList()
    {
        noteTexts = new List<string>() { "DO", "RE", "MI", "FA", "SOL", "LA", "SI", "DO", "X" };
    }
 
    /// <summary>
    /// fills the noteTexts list with the right color of the button 
    /// </summary>
    private void createButtonColorsList()
    {
        buttonColors = new List<Color>() {
            Color.blue,
            Color.magenta,
            Color.blue,
            Color.magenta,
            Color.blue,
            Color.magenta,
            Color.blue,
            Color.magenta,
            Color.blue,
            Color.magenta,
            Color.blue,
            Color.magenta,
       };

    }


    public Module GetCurrentModule()
    {
        return this.currentModule;
    }
    public void SetCurrentModule(Module newModule)
    {
        this.currentModule = newModule;
    }

    public Difficulty GetCurrentDifficulty()
    {
        return this.currentDifficulty;
    }

    public void SetCurrentDifficulty(Difficulty newDifficulty)
    {
        this.currentDifficulty = newDifficulty;
    }

    public NeglectType GetCurrentNeglectType()
    {
        return currentNeglectType;
    }

    public void SetCurrentNeglectType(NeglectType newNeglectType)
    {
        this.currentNeglectType = newNeglectType;
    }

    public int[] GetAssessmentSpawnAngles()
    {
        return this.assessmentSpawnAngles;
    }

    public void PauseGame()
    {
        // Set the gameIsPaused variable to false
        gameIsPaused = true;

        Time.timeScale = 0;

        // Turn of the controllers
        TurnControllersOff();
    }
    public void ResumeGame()
    {
        // Set the gameIsPaused variable to false
        gameIsPaused = false;

        Time.timeScale = 1;

        // Turn the controllers back on
        TurnControllersOn();
    }

    public bool GameIsPaused()
    {
        return gameIsPaused;
    }

    /// <summary>
    /// Set the target active with a small delay
    /// </summary>
    IEnumerator SetTargetActiveWithDelayCoroutine()
    {
        yield return new WaitForSeconds(waitBetweenEachRepeat);
        _lookerScript.SetTargetActive(true);
        TurnControllersOff();
    }

    /** 
     * Objective tesing 
     */
    public void callCreateNotesToRepeat()
    {
        CreateNotesToRepeat();
    }

    public List<int> getNotesToRepeatList()
    {
        return notesToRepeatList;
    }
}
