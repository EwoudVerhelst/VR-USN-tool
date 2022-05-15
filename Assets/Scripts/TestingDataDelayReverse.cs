using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public class TestingDataDelayReverse : MonoBehaviour
{
    private List<string[]> data = new List<string[]>();

    // Start is called before the first frame update
    void Start()
    {
        createHeader();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        // write the frame data
        WriteToCSV(data, "requestDataDelayUnity.csv");
    }

    public void addDelayData(string id)
    {
        string[] rowDataTemp = new string[2];
        string time = System.DateTime.Now.ToString("HH:mm:ss:fff");
        rowDataTemp[0] = time;
        rowDataTemp[1] = id;
        data.Add(rowDataTemp);
    }

    private void createHeader()
    {
        string[] rowDataTemp = new string[2];
        rowDataTemp[0] = "time";
        rowDataTemp[1] = "id";
        data.Add(rowDataTemp);
    }

    void WriteToCSV(List<string[]> rowData, string fileName)
    {

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
