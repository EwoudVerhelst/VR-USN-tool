using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtractHeadRotation : MonoBehaviour
{
    /// <summary>
    ///     Extract the rotation of the head mounted display
    /// </summary>
    ///

    public GameObject _OVRCameraRig;
    public GameObject _CenterEyeAncher;

    private OVRManager _OVRManager;

    private Vector3 headRotation;

    // Start is called before the first frame update
    void Start()
    {
        _OVRManager = _OVRCameraRig.GetComponent<OVRManager>();
    }

    // Update is called once per frame
    void Update()
    {
        headRotation = ExtractRotation();
    }

    // Extract the head rotation as a Quaternion from the CenterEyeAncher game object en turn it into euler coordinate sytem
    private Vector3 ExtractRotation()
    {
        Quaternion qRotation = _CenterEyeAncher.transform.rotation;
        Vector3 r = qRotation.eulerAngles;

        // adapt rotation in order to make x,y,z axis Zero degrees
        // z-axis
        if (r.y <= 360 && r.y >= 180)
        {
            r.y -= 360;
        }
        // x-axis
        r.x =  360 - r.x;
        if (r.x <= 360 && r.x >= 180)
        {
            r.x -= 360;
        }
        // y-axis
        r.z = 360 - r.z;
        if (r.z <= 360 && r.z >= 180)
        {
            r.z -= 360;
        }

        return r;
    }

    public Vector3 GetHeadRotation()
    {
        return this.headRotation;
    }
}
