using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;


class Message
{
    public string message_type = "message";
    public string message_id = "";
    public string current_player = "";
    public string current_module = "";
    public string neglect_type = "";
    public string current_session_name = "";
    public string notes_layout = "";
    public Vector3 head_rotation;
    public List<int> played_notes;  
}


public class DataPublisher : RunAbleThread
{
    /// <summary>
    ///     publish data on a given tcp connection
    /// </summary>
    ///


    private int publishRate;
    private ExtractHeadRotation extractHeadRotation;
    private ExtractPlayedNotes extractPlayedNotes;
    private ExtractMetaInfo extractMetaInfo;
    private TesterDataDelay testerDataDelay;
    
    
    public DataPublisher(int publishRate, ExtractHeadRotation extractHeadRotation, ExtractPlayedNotes extractPlayedNotes, ExtractMetaInfo extractMetaInfo, TesterDataDelay testerDataDelay = null)
    {
        this.extractHeadRotation = extractHeadRotation;
        this.extractPlayedNotes = extractPlayedNotes;
        this.extractMetaInfo = extractMetaInfo;
        if (testerDataDelay)
        {
            this.testerDataDelay = testerDataDelay;
        }

        this.publishRate = publishRate;


    }
    protected override void Run()
    {
        ForceDotNet.Force(); // this line is needed to prevent unity freeze after one use, not sure why yet
        Debug.Log("start publisher");


        
        using (var pubSocket = new PublisherSocket())
        {
            Console.WriteLine("Publisher socket binding...");
            pubSocket.Options.SendHighWatermark = 1000;
            pubSocket.Bind("tcp://127.0.0.1:12345");
            while (Running)
            {
                // OBJECTIVE TESTING
                if (testerDataDelay)
                {
                    if (testerDataDelay.StopSendingData())
                    {
                        return;
                    }
                }


                // Metainfo
                if (extractMetaInfo.GetMetaInfoChanged())
                {
                    string json2 = JsonUtility.ToJson(extractMetaInfo.CreateMetaMessage());
                    Debug.Log("sending: " + json2);
                    pubSocket.SendFrame(json2);
                }

                // If the current module requires data to be published
                if (extractMetaInfo.PublishData())
                {
                    // Message to be send 

                    Message message = new Message();

                    // message id
                    System.Guid guid = System.Guid.NewGuid();
                    message.message_id = guid.ToString();

                    // Head rotation data
                    Vector3 headRotation = extractHeadRotation.GetHeadRotation();
                    message.head_rotation = headRotation;

                    // Played Notes data
                    List<int> playedNotes = extractPlayedNotes.GetPlayedNotes();
                    message.played_notes = playedNotes;

                    // Current player, module and session_name
                    message.current_player = extractMetaInfo.GetCurrentPlayer();
                    message.current_module = extractMetaInfo.GetCurrentModuleString();
                    message.current_session_name = extractMetaInfo.GetCurrentSessionName();

                    // neglect type 
                    message.neglect_type = extractMetaInfo.GetCurrentNeglectType();

                    // notes layout
                    message.notes_layout = extractMetaInfo.CreateCurrentNotesString();

                    // Convert the message to json and send 
                    string json1 = JsonUtility.ToJson(message);
                    Debug.Log("sending: " + json1);
                    pubSocket.SendFrame(json1);


                    // Objective testing
                    ObjectiveTesting(message.message_id, headRotation[1]);

                }
                Thread.Sleep(publishRate);
            }

            Debug.Log("stopping publisher");
        }

    }

    private void ObjectiveTesting(string message_id, float headRotation)
    {
        if (testerDataDelay)
        {
            testerDataDelay.addToPublishData(message_id, headRotation);
        }

    }
}