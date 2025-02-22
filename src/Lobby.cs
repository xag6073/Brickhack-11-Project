
using Godot;

public partial class Lobby : Control
{
	public static Lobby Instance { get; private set; }

	private const int PORT = 7000;
	private const string SERVER_IP = "127.0.0.1";

	private Godot.Label LABEL;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Instance = this;
		this.LABEL = GetNode<Label>("/root/Lobby/ConnectionLabel");  //GetTree().Root.FindChild("connectionLabel", true, false) as Godot.Label;
		//HOST_BUTTON = GetNode<Button>("Button");
		//HOST_BUTTON.Pressed += CreateGame;
		Multiplayer.PeerConnected += OnPlayerConnected;
    Multiplayer.PeerDisconnected += OnPlayerDisconnected;
    //Multiplayer.ConnectedToServer += OnConnectOk;
    //Multiplayer.ConnectionFailed += OnConnectionFail;
    Multiplayer.ServerDisconnected += OnServerDisconnected;
	}

    private void CreateGame()
	{
		GD.Print("Creating game...");
		var peer = new ENetMultiplayerPeer();
		Error	error = peer.CreateServer(PORT, 1);

		if(error != Error.Ok)
		{
			//return error;
			return;
		}

		GD.Print("Created game");
		Multiplayer.MultiplayerPeer = peer;
		LABEL.Text = "Waiting on other player...";
		//return Error.Ok;
	}

	private void JoinGame() 
	{
		var address = SERVER_IP;


		var peer = new ENetMultiplayerPeer();
		Error error = peer.CreateClient(address, PORT);

		if (error != Error.Ok) 
		{
			GD.Print(error);
		}

		Multiplayer.MultiplayerPeer = peer;
		//return Error.Ok;

	}

	[Rpc]
	private void OnPlayerConnected(long id) {
		//if (Multiplayer.IsServer()) 
		//{
			GD.Print("Player connected!!");
			var scene = ResourceLoader.Load<PackedScene>("../Scenes/board.tscn").Instantiate();
			GetTree().Root.AddChild(scene);
			//GetTree().ChangeSceneToFile("../Scenes/board.tscn");
   		//}
			Hide();
	}
	
	[Rpc]
	private void OnPlayerDisconnected(long id)
	{
		//if (Multiplayer.IsServer()) 
		//{
			GD.Print("Player disconnected!!");
			PrintTreePretty();
			//var scene = ResourceLoader.Load<PackedScene>("../Scenes/lobby.tscn").Instantiate();
			//GetTree().Root.AddChild(scene);
			//if  (HasNode("ranks")) 
			//{
				GetNode("ranks").Free();
				Show();
			//}
			Multiplayer.MultiplayerPeer = null;
		//}
	}

	[Rpc]
  private void OnServerDisconnected()
	{
		GD.Print("Server disconnected!!");
		//var scene = ResourceLoader.Load<PackedScene>("../Scenes/lobby.tscn").Instantiate();
		//GetTree().Root.AddChild(scene);
		PrintTreePretty();
		//if  (HasNode("ranks")) 
		//	{
				GetNode("ranks").Free();
				Show();
		//	}
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
