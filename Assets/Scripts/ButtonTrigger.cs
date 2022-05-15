using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{

    /// <summary>
    /// Button logic, handles the hitting of a note
    /// </summary>
    ///

    // GameManager
    private GameManager _gameManager;

    // Rehabilitation measures
    private ExtractRehabilitationMeasures _extractRehabilitationMeasures;

    private Animator _buttonAnimator;
    private Renderer _buttonRenderer;
    private AudioSource _buttonAudioSource;
    private ExtractPlayedNotes _extractPlayedNotes;

    // colors for animation
    private Color ogColor = Color.blue;

    // id of the button
    private int buttonId;

    // Time it takes to complete 1 animation cycle of the button
    public float animationTime = 1;


    // bool to determine if this particular button is triggerable (to provide a downtime of the trigger)
    private bool buttonIsTriggerable = false;

    // max heigth for the ball
    private float maxHeight = 2.2f;

    // drag of the ball when comming down
    private float minBallDrag = 38f;
    private float maxBallDrag = 47f;

    // wether or not their is a ball attached to the note
    public GameObject ball;
    private bool isBall = false;
    private Renderer _ballRenderer;

    // height of ball bellow which the note is hittable
    private float ballRestingPos = 1.319f;
    private float hittableRatio = 0.25f;
    private float hittableHeight;
    private float offset = 0.01f;

    // total time the ball lies on the surface in seconds
    private float timeOnSurface = 0;

    // TESTING
    public bool triggerButton = false;


    


    // Start is called before the first frame update
    void Awake()
    {
        // Game manager
        _gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        // Rehabilitation measures
        _extractRehabilitationMeasures = GameObject.FindGameObjectWithTag("RehabilitationTracker").GetComponent<ExtractRehabilitationMeasures>();

        // button animation
        _buttonAnimator = GetComponent<Animator>();
        _buttonRenderer = GetComponent<Renderer>();

        // button sound
        _buttonAudioSource = GetComponent<AudioSource>();

        // to save the played notes when triggered
        _extractPlayedNotes = GameObject.FindGameObjectWithTag("PlayedNotesTracker").GetComponent<ExtractPlayedNotes>();

        // to change the color of the ball
        _ballRenderer = ball.GetComponent<Renderer>();
    }

    private void Start()
    {
        // set button as triggerable 
        this.buttonIsTriggerable = true;

        //setIsBall(true);

        // calculate the hittable height
        hittableHeight = ballRestingPos + ((maxHeight - ballRestingPos) * hittableRatio);

        // rehabilitation measures
        timeOnSurface = 0;


        // TESTING
        //StartCoroutine(HitRandom());

    }

    private void OnDestroy()
    {   
        // Persist the time on surface on destroy ,because otherwise it would not get counted when it would never have been hitt
        persistTimeOnSurfaceInfo();
    }

    // Update is called once per frame
    void Update()
    {


        if (isBall)
        {

            if (transform.GetChild(1).position[1] > maxHeight - 0.1)
            {
                GetComponentInChildren<Rigidbody>().drag = RandomDrag();
                _ballRenderer.material.SetColor("_Color", Color.white);
            }



            // change the color an triggerable of button when ball reaches certain position
            float yPos = transform.GetChild(1).position[1];
            if (yPos > hittableHeight)
            {
                buttonIsTriggerable = false;
            }
            else if ( yPos < hittableHeight && yPos > ballRestingPos + offset && _ballRenderer.material.color != Color.green)
            {
                _ballRenderer.material.SetColor("_Color", Color.green);
                buttonIsTriggerable = true;
            }
            else if (yPos < ballRestingPos + offset && _ballRenderer.material.color != Color.red )
            {
                _ballRenderer.material.SetColor("_Color", Color.red);
                buttonIsTriggerable = true;
            }
            else if (yPos < ballRestingPos + offset)
            {
                timeOnSurface += Time.deltaTime;
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {   
        // if collision with table, don't trigger
        if (other.CompareTag("Table"))
        {
            return;
        }


        if (!buttonIsTriggerable)
        {
            return;
        }


        
        // trigger button animation and sound
        TriggerButton(true, Color.white);

        // Logic based on which module is currently active
        switch (this._gameManager.currentModule)
        {
            case Module.None:
                break;

            case Module.Assessment:
                break;

            case Module.Testing:
                break;

            case Module.RightToLeft:
                _gameManager.HandleTriggerNoteOnRightToLeft(buttonId);
                break;

            case Module.SequenceRepeating:
                _gameManager.HandleTriggerNoteOnSequenceRepeating(buttonId);
                break;

            case Module.FreeToPlay:
                persistTimeOnSurfaceInfo();
                break;

            default:
                Debug.LogError("Button triggered during unknown module");
                break;
        }

        // Save that this particular button was triggered to send to database
        _extractPlayedNotes.AddPLayedNote(this.buttonId);
        
    }

    public void TriggerButton(bool audio, Color triggerColor)
    {
        StartCoroutine(FlipButtonTriggerable());

        // Trigger the animation
        _buttonAnimator.SetTrigger("ButtonPressed");

        // Change color 
        StartCoroutine(FlipColor(triggerColor));

        // Play the sound
        if (audio)
        {
            _buttonAudioSource.Play();
        }

        // Push the ball in the air
        if (isBall)
        {
            pushBall();
        }


    }
     


    // add a vertical force the the sphere so that it jumps into the air
    public void pushBall()
    {
        GetComponentInChildren<Rigidbody>().drag = 0;
        GetComponentInChildren<Rigidbody>().AddForce(calculateImpuls(), ForceMode.Impulse);

    }


    // set the ID of this button, which is its rotation
    // used in Spawnmanager
    public void SetButtonId(int buttonId)
    {
        this.buttonId = buttonId;
    }

    public int GetButtonId()
    {
        return this.buttonId;
    }

    // set the original color of the button
    public void SetOgColor(Color color)
    {
        this.ogColor = color;
        _buttonRenderer.material.SetColor("_Color", color);
    }

    // Change the color of the button
    public void ChangeColor(Color color)
    {
        _buttonRenderer.material.SetColor("_Color", color);
    }


    // Change the color of the note back to the of color 
    public void ResetColor()
    {
        _buttonRenderer.material.SetColor("_Color", ogColor);
    }

    public void StartGlowing()
    {
        _buttonAnimator.SetBool("Glow", true);
    }

    public void StopGlowing()
    {
        _buttonAnimator.SetBool("Glow", false);
    }

    public void setIsBall(bool isBall)
    {
        this.isBall = isBall;
        if (isBall) {
            ball.SetActive(true);
            pushBall();
        }
        else
        {
            ball.SetActive(false);
        }

    }

    // calculate the impuls needed to reach the maximum heigth based on current position of the ball
    public Vector3 calculateImpuls()
    {
        float yPos = transform.GetChild(1).position[1];
        float mass = GetComponentInChildren<Rigidbody>().mass;
        float gravity = -Physics.gravity[1];

        float distanceToTravel = this.maxHeight - yPos;

        // velocity needed based on conservation of momentum 1/2 * m * v^2 = m * g * h
        float velocity = Mathf.Sqrt(2 * gravity * distanceToTravel);
        // impuls = v * m 
        Vector3 impuls = Vector3.up * velocity * mass;


        return impuls;
    }

    private float RandomDrag()
    {

        return Random.Range(minBallDrag, maxBallDrag);
    }

    private void persistTimeOnSurfaceInfo()
    {
        if (timeOnSurface > 0 && isBall)
        {
            int noteIndex = _gameManager.GetButtonIndexById(buttonId);
            _extractRehabilitationMeasures.AddFreeToPlayTimeOnSurfaceInfo(noteIndex, timeOnSurface, _gameManager.GetCurrentDifficulty());
            timeOnSurface = 0;
        }
    }



    // Change the color of the button, wait animaionTime amount of seconds and turn it back 
    IEnumerator FlipColor(Color triggercolor)
    {
        _buttonRenderer.material.SetColor("_Color", triggercolor);
        yield return new WaitForSeconds(animationTime);
        _buttonRenderer.material.SetColor("_Color", ogColor);
    }

    // flips the buttonIsTrigerable varialbe to make sure button cannot be triggered to fast in a row
    IEnumerator FlipButtonTriggerable()
    {
        buttonIsTriggerable = false;
        yield return new WaitForSeconds(0.3f);
        buttonIsTriggerable = true;
    }


    IEnumerator HitRandom()
    {
        for (int i = 0; i< 100; i++) {
            float waitTime = Random.Range(1, 30);
            yield return new WaitForSeconds(waitTime);
            OnTriggerEnter(gameObject.GetComponent<Collider>());
        }


    }

}
