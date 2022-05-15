using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public class TesterFrameRate : MonoBehaviour
{
    public GameObject _gameController;
    private GameManager _gameManager;

    private List<float> frameRateData = new List<float>();

    private int counter = 1000;
    private bool csvWritten1 = false;

    private List<int> framesPerSecondData = new List<int>();
    private int secondCounter = 180;
    private int frameCounter = 0;
    private float timer = 1.0f;
    private bool csvWritten2 = false;

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = _gameController.GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {

        /*// stores time it took for each frame
        if (!csvWritten1 && counter >= 0)
        {
            frameRateData.Add(Time.deltaTime);
            counter--;
        }
        else if (!csvWritten1 && counter < 0)
        {
            csvWritten1 = true;
            WriteToCSV1(frameRateData, "framerateData.csv");
        }*/

        // counts number of frames per second
        if (timer < 0 && !csvWritten2)
        {
            framesPerSecondData.Add(frameCounter);
            frameCounter = 0;
            timer = 1.0f;
            secondCounter--;
        }
        else if (timer > 0 && !csvWritten2)
        {
            timer -= Time.deltaTime;
            frameCounter++;
        }

        if (secondCounter <= 0 && !csvWritten2)
        {
            csvWritten2 = true;
            WriteToCSV2(framesPerSecondData, "framesPerSecondData.csv");
        }
    }


    void WriteToCSV1(List<float> rowData, string fileName)
    {
        Debug.Log("Writing " + fileName);


        int length = rowData.Count;
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("time per frame");

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, rowData[index]));


        string filePath = getPath(fileName);

        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
    }

    void WriteToCSV2(List<int> rowData, string fileName)
    {
        Debug.Log("Writing " + fileName);


        int length = rowData.Count;
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("frames per second");

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, rowData[index]));


        string filePath = getPath(fileName);

        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
    }

    // Following method is used to retrive the relative path as device platform
    private string getPath(string fileName)
    {

        if (Application.isEditor)
        {
            return Application.dataPath + "/CSV/objective/" + fileName;
        }
        else
        {
            return Application.dataPath + "/" + fileName;
        }

    }
}
