using UnityEngine;
using extOSC;

public class BroadcastListener : MonoBehaviour
{
    [Header("OSC Settings")]
    public int port = 9000;
    public string address = "/camera";

    private OSCReceiver receiver;

    // Scene Specific effects  - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    [Header("MR effects, driven by OSC")]
    public SceneObjectManager sceneObjectManager;
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


    void Start()
    {
        // Create and configure the receiver
        receiver = gameObject.AddComponent<OSCReceiver>();
        receiver.LocalPort = port;

        // Bind to address and register callback
        receiver.Bind(address, OnReceiveboardButtonIndex);

        Debug.Log($"[OSC Listener] Listening on port {port} for address '{address}'");
    }

    private void OnReceiveboardButtonIndex(OSCMessage message)
    {
        if (message.ToInt(out int boardButtonIndex))
        {
            if (boardButtonIndex > -1){
                Debug.Log($"[OSC Listener] Received camera index: {boardButtonIndex}");
                sceneObjectManager.ChangeSceneObject(boardButtonIndex);
            }
        }
        else
        {
            Debug.LogWarning("[OSC Listener] Invalid /camera message received.");
        }
    }
}
