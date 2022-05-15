using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

public class RequestReceiver : RunAbleThread
{
    /// <summary>
    ///     Receives requests on a TCP connection 
    ///     Communication with User Interface Controller
    /// </summary>
    ///

    private RequestHandler _requestHandler;
    public RequestReceiver(RequestHandler _requestHandler)
    {
        this._requestHandler = _requestHandler;
    }
    protected override void Run()
    {
        ForceDotNet.Force(); // this line is needed to prevent unity freeze after one use, not sure why yet
        Debug.Log("start dataServer");


        using (var serverSocket = new ResponseSocket())
        {
            serverSocket.Bind("tcp://127.0.0.1:12346");
            serverSocket.Options.ReceiveBuffer = 10;
            

            Debug.Log("NET Request Receiver: Started");

            string message = null;
            bool gotMessage = false;
            while (Running)
            {
                gotMessage = serverSocket.TryReceiveFrameString(out message);
                if (gotMessage)
                {


                    // Parse the received string into a dictionary
                    Dictionary<string, string> requestDict = RequestParser(message);

                    Debug.Log("NET Receiver: Received (" + DictionaryToString(requestDict) + ")");
                    _requestHandler.SetRequest(requestDict);
                    _requestHandler.RequestReceived();

                    // send a reply
                    serverSocket.SendFrame("Request correctly received in unity");
                    
                }

            }


        }


        // parses the received string into a dictionary
        Dictionary<string, string> RequestParser(string message)
        {
            Dictionary<string, string> requestDict = new Dictionary<string, string>();

            string[] splitList = message.Split('/');

            foreach (string pair in splitList)
            {
                string[] splitPair = pair.Split(':');
                requestDict.Add(splitPair[0], splitPair[1]);
            }

            return requestDict;

        }

        // create a string representation of a dictionary 

        string DictionaryToString(Dictionary<string, string> dictionary)
        {
            string toPrint = "";
            foreach(string key in dictionary.Keys){
                toPrint += key + " : " + dictionary[key] + " -- ";
            }

            return toPrint;
        }

       

    }

}
