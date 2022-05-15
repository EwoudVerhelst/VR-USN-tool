using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public class RandomnessTester : MonoBehaviour
{

    public GameObject _gameController;
    private GameManager _gameManager;

    private List<string[]> NotesToRepeatData = new List<string[]>();

    private int maxIncrement = 1000;


    private Difficulty[] difficulties = new Difficulty[4] { Difficulty.One, Difficulty.Two, Difficulty.Three, Difficulty.Four};
    private int[] numberOfNotesToRepeatList = new int[5] { 1, 2, 3, 4, 5 };

    private bool fileWritten = false;

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = _gameController.GetComponent<GameManager>();

        createHeader();

        StartCoroutine(StartTest());
    }

    IEnumerator StartTest()
    {
        yield return new WaitForSeconds(1);
        Debug.Log("starting randomness test");

        foreach (Difficulty difficulty in difficulties)
        {

            startSequenceRepeating(difficulty);
            yield return new WaitForSeconds(1);
            foreach(int numberOfNotesToRepeat in numberOfNotesToRepeatList)
            {
                _gameManager.SetNumberOfNotesToRepeat(numberOfNotesToRepeat);
                yield return new WaitForSeconds(0.5f);
                for (int i = 0; i < maxIncrement; i++)
                {
                    _gameManager.callCreateNotesToRepeat();
                    addToNotesToRepeatData(_gameManager.getNotesToRepeatList(), numberOfNotesToRepeat, difficulty);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        string filename = "notesToRepeat" + NotesToRepeatData.Count.ToString() + ".csv";
        WriteToCSV(NotesToRepeatData, filename);
    }

    private void startSequenceRepeating(Difficulty difficulty)
    {
        _gameManager.SetCurrentModule(Module.SequenceRepeating);
        _gameManager.SetCurrentDifficulty(difficulty);
    }

    private void createHeader()
    {
        string[] header = new string[3];
        header[0] = "note";
        header[1] = "number of notes to repeat";
        header[2] = "difficulty";
        NotesToRepeatData.Add(header);
    }

    private void addToNotesToRepeatData(List<int> notesToRepeatList, int numberOfNotesToRepeat, Difficulty difficulty)
    {
        string[] rowData = new string[3];

        foreach (int note in notesToRepeatList)
        {
            int noteIndex = _gameManager.GetButtonIndexById(note);
            rowData[0] = noteIndex.ToString();
            rowData[1] = numberOfNotesToRepeat.ToString();
            rowData[2] = difficulty.ToString();
        }

        NotesToRepeatData.Add(rowData);
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
            return Application.dataPath + "/CSV/" + fileName;
        }
        else
        {
            return Application.dataPath + "/" + fileName;
        }

    }
}
