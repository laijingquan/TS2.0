using UnityEngine;
using DarkRift;
using System.Collections.Generic;

public class Client : MonoBehaviour
{
	#region Initialization
	public GameObject TestFab;
	public GameObject CustomTestObject;

	private void Start ()
	{
		Time.fixedDeltaTime = TargetSimulationRate;
		DarkRiftAPI.onData += HandleonData;
	}
	#endregion

	#region Communication
	public GUIStyle Style;
	private bool isStarted;
	private string _IP = "127.0.0.1";
	//GUI Stuff for connecting
	private void OnGUI ()
	{
		GUILayout.BeginArea (new Rect(Screen.width / 2 - 100, 0, 200,150));
		//If the player hasn't connected yet, allow the player to connect
		if (!DarkRiftAPI.isConnected ) {
			_IP = GUILayout.TextField (_IP);
			if (GUILayout.Button ("Connect")) {
				DarkRiftAPI.Connect (_IP);
				//Connect to the DarkRift server hosted on the machine with _IP IP Address
			}
		}
		if (!DoVisualize) {
			Texture2D tex = new Texture2D (1, 1);
			tex.SetPixel (1, 1, new Color32 (255, 255, 255, 255));
			GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), tex);
			GUILayout.Label ("Catching up", Style);
		}
		GUILayout.Label ("Step Count: " + StepCount.ToString (), Style);
		GUILayout.EndArea();
	}

	/// <summary>
	/// Handles data received from server.data转换成byte[]即可
	/// </summary>
	void HandleonData (byte tag, ushort frameCount, object data)
	{
		switch ((NetworkTag)tag) {
		//Meta data includes connection, room initialization, and subject.
		//In the current implementation, the server automatically puts a player in a room upon connecting
		//So there is no need for NetworkTag.Meta out of the box.
		case NetworkTag.Meta:

			break;

		//Game data includes sending input to the server and receiving frames from the server
		case NetworkTag.Game:
			//Checking if we already have the frame to prevent duplicate frames
			break;
		}
	}
	/// <summary>
	/// Sends a command to the server, to be distributed to everyone in the player's room.
	/// </summary>
	public static void SendCommand (byte[] data)
	{
		//Be sure to convert the list to an array since DarkRift can't send lists
		DarkRiftAPI.SendMessageToServer ((byte)NetworkTag.Game,(ushort)0, data);
	}
	//Needed to prevent some nasty bugs
	private void OnApplicationQuit ()
	{
		DarkRiftAPI.Disconnect ();
	}
	#endregion

	#region Simulation
	/// <summary>
	/// The target simulation rate. In ideal networking conditions, simulation will run at this rate. Make sure you change this value on the server if you change it on the client.
	/// </summary>
	public const float TargetSimulationRate = .1f;
	/// <summary>
	/// Frames are stored here. When saving a game, this is what you would serialize and replay.
	/// </summary>
	//public Dictionary<ushort,Frame> Frames = new Dictionary<ushort, Frame> ();
	/// <summary>
	/// Represents how many simulation frames have passed; given the same input and step, all clients will have the same game state.
	/// </summary>
	[HideInInspector]
	public static ushort StepCount{
		get{
			return _StepCount;
		}
	}
	private static ushort _StepCount = 0;
	private ushort NextStepCount = 0;
	private int ForeSight = 0;
	private bool DoVisualize = true;

	private void FixedUpdate ()
	{
		DarkRiftAPI.Recieve ();
	}
	#endregion

	#region Visualization: Communicating with Unity's rendering
	private void Update ()
	{
	}
	#endregion
}
