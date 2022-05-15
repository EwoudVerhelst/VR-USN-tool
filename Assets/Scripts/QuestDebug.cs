using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestDebug : MonoBehaviour
{

    public static QuestDebug debugInstance;

    bool inMenu;

    Text logText;

    void Awake()
    {
        debugInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (DebugUIBuilder.instance)
        {
            var rt = DebugUIBuilder.instance.AddLabel("Debug");
            logText = rt.GetComponent<Text>();
            DebugUIBuilder.instance.Show();
        }
    }


    // Update is called once per frame
    void Update()
    {
        /**//*DebugUIBuilder.instance.Show();*/
/*        if (OVRInput.GetDown(OVRInput.Button.Two) || OVRInput.GetDown(OVRInput.Button.Start))
        {
            if (inMenu) DebugUIBuilder.instance.Hide();
            else DebugUIBuilder.instance.Show();
            inMenu = !inMenu;
        }*/
    }

    public void Log(string msg)
    {
        logText.text = msg;
    }
}
