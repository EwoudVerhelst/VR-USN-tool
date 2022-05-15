using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public class TesterDataDelay : MonoBehaviour
{
    public GameObject _headRotationTracker;
    private ExtractHeadRotation _extractHeadRotation;


    private List<string[]> frameData = new List<string[]>();
    private List<string[]> publishData = new List<string[]>();

    private int counter = 2000;
    private bool csvWritten = false;

    // Start is called before the first frame update
    void Start()
    {
        _extractHeadRotation = _headRotationTracker.GetComponent<ExtractHeadRotation>();

        addPublishDataHeader(); 
    }

    // Update is called once per frame
    void Update()
    {
        // add head rotation data each frame update
        //addToFrameData(_extractHeadRotation.GetHeadRotation()[1]);

        if (!csvWritten && counter <= 0) {
            csvWritten = true;
            WriteToCSV(publishData, "dataDelayUnity.csv");
        }


    }

    private void OnDestroy()
    {
        // write the frame data
        //WriteToCSV(frameData, "FrameData.csv");

        // write the publish data
        //WriteToCSV(publishData, "dataDelayUnity.csv");
    }

    public void addToFrameData(float headRotation)
    {
        string[] rowDataTemp = new string[3];
        rowDataTemp[0] = System.DateTime.Now.ToString("HH:mm:ss:fff");
        rowDataTemp[1] = headRotation.ToString();
        frameData.Add(rowDataTemp);
    }

    public void addPublishDataHeader()
    {
        string[] rowDataTemp = new string[3];
        rowDataTemp[0] = "id";
        rowDataTemp[1] = "time";
        rowDataTemp[2] = "head rotation";
        publishData.Add(rowDataTemp);
    }

    public void addToPublishData(string message_id, float headRotation)
    {

        string[] rowDataTemp = new string[3];
        rowDataTemp[0] = message_id;
        rowDataTemp[1] = System.DateTime.Now.ToString("HH:mm:ss:fff");
        rowDataTemp[2] = headRotation.ToString();
        publishData.Add(rowDataTemp);

        counter--;
    }   


    public bool StopSendingData()
    {
        if (counter > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }



    void WriteToCSV(List<string[]> rowData, string fileName)
    {
        Debug.Log("Writing " + fileName);
        string[][] output = new string[rowData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));


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
