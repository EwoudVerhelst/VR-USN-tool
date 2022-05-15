using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtractPlayedNotes : MonoBehaviour
{
    /// <summary>
    ///     extracts the played notes from during the assessment module
    /// </summary>
    ///

    private List<int> playedNotes;


    // Start is called before the first frame update
    void Start()
    {
        playedNotes = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Add note with given ID to the playedNotes list
    public void AddPLayedNote(int noteId)
    {
        this.playedNotes.Add(noteId);
    }
    // Return the playedNotes list
    public List<int> GetPlayedNotes()
    {
        List<int> CopiedLIst = new List<int>(this.playedNotes);
        ClearPLayedNotes();
        return CopiedLIst;
    }

    public void ClearPLayedNotes()
    {
        this.playedNotes.Clear();
    }
}
