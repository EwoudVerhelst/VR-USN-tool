using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class ExtractRehabilitationMeasures : MonoBehaviour
{
    // Game manager
    public GameObject _gameController;
    private GameManager _gameManager;

    // Extract meta info
    public GameObject _communicator;
    private ExtractMetaInfo _extractMetaInfo;

    // persist rate in seconds
    private float persistRate = 60.0f;
    private float countdownTimer;

    // Rehabilitation measures
    private List<string[]> rightToLeftTimingInfo = new List<string[]>();
    private List<string[]> sequenceRepeatingTimingInfo = new List<string[]>();
    private List<string[]> sequenceRepeatingSuceedingInfo = new List<string[]>();
    private List<string[]> FreeToPlayTimeOnSurfaceInfo = new List<string[]>();


    // Start is called before the first frame update
    void Start()
    {
        // Game manager
        _gameManager = _gameController.GetComponent<GameManager>();

        // extract meta info
        _extractMetaInfo = _communicator.GetComponent<ExtractMetaInfo>();

        // add the headers for the csvs 
        CreateHeaders();

        // set the timer for the persisting
        countdownTimer = persistRate;

    }

    // Update is called once per frame
    void Update()
    {
        countdownTimer -= Time.deltaTime;

      
        // persist the CSV data once in a while
        if (countdownTimer <= 0)
        {
            countdownTimer = persistRate;
            Debug.Log("writing to csv");
            WriteAllToCSV();
        }
    }

    private void OnDestroy()
    {
        
        WriteAllToCSV();
    }

    private void CreateHeaders()
    {
        // Right to left 
        string[] rowDataTemp = new string[4];
        rowDataTemp[0] = "delta T";
        rowDataTemp[1] = "note id";
        rowDataTemp[2] = "difficulty";
        rowDataTemp[3] = "neglect type";
        rightToLeftTimingInfo.Add(rowDataTemp);

        // Sequence repeating suceeding info
        rowDataTemp = new string[5];
        rowDataTemp[0] = "delta T";
        rowDataTemp[1] = "suceeded";
        rowDataTemp[2] = "notes to repeat";
        rowDataTemp[3] = "difficulty";
        rowDataTemp[4] = "neglect type";
        sequenceRepeatingSuceedingInfo.Add(rowDataTemp);

        // Sequence repeating timing info
        rowDataTemp = new string[7];
        rowDataTemp[0] = "delta T";
        rowDataTemp[1] = "note played";
        rowDataTemp[2] = "note to play next";
        rowDataTemp[3] = "correctly played?";
        rowDataTemp[4] = "notes to repeat";
        rowDataTemp[5] = "difficulty";
        rowDataTemp[6] = "neglect type";
        sequenceRepeatingTimingInfo.Add(rowDataTemp);

        // Free to play time on surface info
        rowDataTemp = new string[3];
        rowDataTemp[0] = "note index";
        rowDataTemp[1] = "time on surface";
        rowDataTemp[2] = "difficulty";
        FreeToPlayTimeOnSurfaceInfo.Add(rowDataTemp);
    }

    // add info the the RightToLeftTimingInfo list
    // dTime: time in seconds it took to hit next note 
    // the current difficulty of the rehabilitation
    public void addRightToLeftTimingInfo(double dTime, int noteId, Difficulty difficulty, NeglectType neglectType)
    {
        string[] rowDataTemp = new string[4];
        rowDataTemp[0] = dTime.ToString();
        rowDataTemp[1] = noteId.ToString();
        rowDataTemp[2] = difficulty.ToString();
        rowDataTemp[3] = neglectType.ToString();
        rightToLeftTimingInfo.Add(rowDataTemp);
    }

    public void AddSequenceRepeatingSuceedingInfo(double dTime, bool succeeded, int nNotesToRepeat, Difficulty difficulty, NeglectType neglectType)
    {
        string[] rowDataTemp = new string[5];
        rowDataTemp[0] = dTime.ToString();
        rowDataTemp[1] = succeeded.ToString();
        rowDataTemp[2] = nNotesToRepeat.ToString();
        rowDataTemp[3] = difficulty.ToString();
        rowDataTemp[4] = neglectType.ToString();
        sequenceRepeatingSuceedingInfo.Add(rowDataTemp);
    }


    public void AddSequenceRepeatingTimingInfoNewSequence(int nNotesToRepeat, Difficulty difficulty, NeglectType neglectType)
    {
        string[] rowDataTemp = new string[7];
        rowDataTemp[0] = "NEW";
        rowDataTemp[1] = "NEW";
        rowDataTemp[2] = "NEW";
        rowDataTemp[3] = "NEW";
        rowDataTemp[4] = nNotesToRepeat.ToString();
        rowDataTemp[5] = difficulty.ToString();
        rowDataTemp[6] = neglectType.ToString();
        sequenceRepeatingTimingInfo.Add(rowDataTemp);
    }

    public void AddSequenceRepeatingTimingInfoNewRepeat(int noteToPlayNext, int nNotesToRepeat, Difficulty difficulty, NeglectType neglectType)
    {
        string[] rowDataTemp = new string[7];
        rowDataTemp[0] = "0";
        rowDataTemp[1] = "null";
        rowDataTemp[2] = noteToPlayNext.ToString();
        rowDataTemp[3] = "null";
        rowDataTemp[4] = nNotesToRepeat.ToString();
        rowDataTemp[5] = difficulty.ToString();
        rowDataTemp[6] = neglectType.ToString();
        sequenceRepeatingTimingInfo.Add(rowDataTemp);
    }
    public void AddSequenceRepeatingTimingInfo(double dTime, int notePlayed, int noteToPlayNext, bool correct, int nNotesToRepeat, Difficulty difficulty, NeglectType neglectType)
    {
        string[] rowDataTemp = new string[7];
        rowDataTemp[0] = dTime.ToString();
        rowDataTemp[1] = notePlayed.ToString();
        rowDataTemp[2] = noteToPlayNext.ToString();
        rowDataTemp[3] = correct.ToString();
        rowDataTemp[4] = nNotesToRepeat.ToString();
        rowDataTemp[5] = difficulty.ToString();
        rowDataTemp[6] = neglectType.ToString();
        sequenceRepeatingTimingInfo.Add(rowDataTemp);
    }

    public void AddFreeToPlayTimeOnSurfaceInfo(int noteIndex, float timeOnSurface, Difficulty currentDifficulty)
    {
        Debug.Log("AddFreeToPlayTimeOnSurfaceInfo");
        string[] rowDataTemp = new string[3];
        rowDataTemp[0] = noteIndex.ToString();
        rowDataTemp[1] = timeOnSurface.ToString();
        rowDataTemp[2] = currentDifficulty.ToString();
        FreeToPlayTimeOnSurfaceInfo.Add(rowDataTemp);
    }


    public void ResetRehabiliationMeasures()
    {
        WriteAllToCSV();
        rightToLeftTimingInfo.Clear();
        sequenceRepeatingSuceedingInfo.Clear();
        sequenceRepeatingTimingInfo.Clear();
        FreeToPlayTimeOnSurfaceInfo.Clear();
        CreateHeaders();
    }

    private void WriteAllToCSV()
    {
        string filename;

        // Right to left timing info
        filename = CreateFilename("rightToLeftTiming");
        WriteToCSV(rightToLeftTimingInfo, filename);

        // Sequence Repeating suceeding info
        filename = CreateFilename("SequenceRepeatingSuceeding");
        WriteToCSV(sequenceRepeatingSuceedingInfo, filename);

        // Sequence Repeating suceeding info
        filename = CreateFilename("SequenceRepeatingTiming");
        WriteToCSV(sequenceRepeatingTimingInfo, filename);

        // Free to play time on surface info
        filename = CreateFilename("FreeToPlayTimeOnSurface");
        WriteToCSV(FreeToPlayTimeOnSurfaceInfo, filename);

    }

    private void WriteToCSV(List<string[]> rowData, string fileName)
    {

        string[][] output = new string[rowData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            //output[i] = System.Array.Copy(rowData[i], output[i]);

            output[i] = System.Array.ConvertAll(rowData[i], a => (string)a.Clone());
        }


        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));


        string filePath = GetPath(fileName);

        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
    }

    // Following method is used to retrive the relative path as device platform
    private string GetPath(string fileName)
    {
        if (Application.isEditor)
        {
            return Application.dataPath + "/CSV/" + fileName;
        } else
        {
            return Application.dataPath + "/" + fileName;
        }
;
    }

    private string CreateFilename(string filename)
    {
        return _extractMetaInfo.GetCurrentPlayer() + "_" + _extractMetaInfo.GetCurrentSessionName() + '_' + filename + ".csv";
    }
}
