using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public class TesterHMDaccuracy : MonoBehaviour
{

    public GameObject _headRotationTracker;
    private ExtractHeadRotation _extractHeadRotation;

    public bool startMeasuring = false;

    private List<string[]> rotationData = new List<string[]>();

    private float fps = 5.0f;
    private float secondsPerFrame;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        _extractHeadRotation = _headRotationTracker.GetComponent<ExtractHeadRotation>();

        secondsPerFrame = 1 / fps;
        timer = secondsPerFrame;

        // header
        string[] rowData = new string[2];
        rowData[0] = "time";
        rowData[1] = "hmd rotation";
        rotationData.Add(rowData);

    }

    // Update is called once per frame
    void Update()
    {

/*        if (timer <= 0)
        {*/
        if (startMeasuring)
        {
            string[] rowData = new string[2];
            rowData[0] = System.DateTime.Now.ToString("HH:mm:ss:fff");
            rowData[1] = _extractHeadRotation.GetHeadRotation()[1].ToString();
            rotationData.Add(rowData);
        }


        //timer = secondsPerFrame;
/*        }
        else
        {
            timer -= Time.deltaTime;
        }*/
    }

    private void OnDestroy()
    {
        WriteToCSV(rotationData,"hmd_rotation_data.csv");
    }

    void WriteToCSV(List<string[]> rowData, string fileName)
    {
        Debug.Log("Writing " + fileName);


        int length = rowData.Count;
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();


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
