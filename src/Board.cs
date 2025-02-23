using Godot;
using System;

public class BoardTile {
	public ColorRect square;

	public Piece piece;

	public BoardTile(ColorRect square, Piece piece) 
	{
		this.square = square;
		this.piece = piece;
	}

}

public partial class Board : Node2D
{

	public static Board Instance { get; set; }

	[Signal]
	public delegate void GameOverEventHandler(string message);

	private BoardTile[,] board = new BoardTile[8, 8];

	private string [,] startLayout = {{"br","bh","bb","bq","bk","bb","bh","br"},
																		{"bp","bp","bp","bp","bp","bp","bp","bp"},
																		{"","","","","","","",""},
																		{"","","","","","","",""},
																		{"","","","","","","",""},
																		{"","","","","","","",""},
																		{"wp","wp","wp","wp","wp","wp","wp","wp"},
																		{"wr","wh","wb","wk","wq","wb","wh","wr"}};
  public Piece selectedPiece = null;

	private Boolean tempFlag = true;

    // Called when the node enters the scene tree for the first time.
  public override void _Ready()
	{
		Instance = this;
		//create chess backend
		if (Multiplayer.IsServer()) {
			Rpc("InitializeBoard");
		}
		updateBoard();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,CallLocal = true, TransferChannel = 1)]
	private void InitializeBoard()
    {
			string[] columns = { "a", "b", "c", "d", "e", "f", "g", "h" };
			for (int row = 0; row < 8; row++)
			{
					for (int col = 0; col < 8; col++)
					{
							string cellName = columns[col] + (8 - row).ToString();
							ColorRect square = GetNode<ColorRect>("%" + cellName);
							square.SetMeta("Coords", new int[] {row, col});
							//GD.Print("Square found " + square.ToString());

							//add a button to each cell that can move the selected piece
							Button cellButton = new Button();
							cellButton.Size = square.Size;
							cellButton.Pressed += () => {
                    if (selectedPiece != null) {
												//Rpc("movePiece", square.Name, selectedPiece.name);
												movePiece(square, selectedPiece, true);
												//remove selected piece
												//selectPiece(selectedPiece);
                    }
                };
							cellButton.Flat = true;
							square.AddChild(cellButton);

							//GD.Print("Generating new piece at " + cellName);

							PieceType pieceType = GetPieceType(startLayout[row, col]);
							Piece newPiece = generateNewPiece(pieceType, "piece " + row + "_" + col, new int[] {row, col});
							board[row, col] = new BoardTile(square, newPiece);
					}
			}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (tempFlag) {
			updateBoard();
			tempFlag = false;
		}
	
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,CallLocal = true, TransferChannel = 1)]
	public Piece generateNewPiece(PieceType type, string name, int[] position) {
		if (type == PieceType.NONE) return null;

		Piece newPiece = new Piece(type, name, position,  false);
		Sprite2D pieceNode = new Sprite2D();
		pieceNode.Name = name;
		//pieceNode.UniqueNameInOwner = true;
		Button pieceNodeButton = new Button();

		string texturePath = GetTexturePath(type);
		pieceNode.Texture = GD.Load<Texture2D>(texturePath);
		pieceNodeButton.Size = pieceNode.Texture.GetSize();
		//TEMP
		pieceNodeButton.Position = new Vector2(pieceNodeButton.Position.X - 15, pieceNodeButton.Position.Y - 15);
		
		//make it invisible
		pieceNodeButton.Modulate = new Color(1, 1, 1, 0);
		pieceNodeButton.Pressed += () => selectPiece(newPiece);
		pieceNode.AddChild(pieceNodeButton);

		AddChild(pieceNode);
		newPiece.pieceNode = pieceNode;
		GD.Print("Creating " + type + name);
		return newPiece;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,CallLocal = true, TransferChannel = 1)]
	public void movePiece(ColorRect square, Piece piece, bool update) {
		if(piece.sleep) {
			GD.Print("Can't move a sleeping piece");
			return;
		}

		//ColorRect square = GetNode<ColorRect>("%" + squareName);
    Sprite2D pieceNode = piece.pieceNode;
    if (pieceNode == null) return;
		
    //Vector2 squareCenter = square.GlobalPosition + (square.GetRect().Size / 2);
    Vector2 pieceOffset = pieceNode.Texture.GetSize() / 2;
    //pieceNode.Position = square.Position + pieceOffset;
		//TEMP
		//GD.Print("Moving piece to " + square.GlobalPosition.X + " : " + square.GlobalPosition.Y);
		pieceNode.Position = new Vector2(square.GlobalPosition.X - 80, square.GlobalPosition.Y - 45);

		//update matrix
		int[] coords = (int[])square.GetMeta("Coords");
		//GD.Print(coords[0] + " : " + coords[1]);
		board[piece.position[0],piece.position[1]].piece = null;
		
		//if a piece already exists, remove it
		Piece moveToPiece = board[coords[0],coords[1]].piece;
		if (update && moveToPiece != null) {
   		removePiece(moveToPiece);
		}
		board[coords[0],coords[1]].piece = piece;
		piece.position = coords;
		
		selectPiece(piece);
		//updateBoard();
		
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,CallLocal = true, TransferChannel = 1)]
	public void removePiece(Piece piece) {
		//PrintTreePretty();
		Sprite2D pieceNode = piece.pieceNode;
		pieceNode.QueueFree();
		pieceNode.Hide();
		if (piece.type == PieceType.BKING || piece.type == PieceType.WKING) {
			//end game
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,CallLocal = true, TransferChannel = 1)]
	public void updateBoard() {
		for (int row = 0; row < 8; row++)
			{
					for (int col = 0; col < 8; col++)
					{
						BoardTile tile = board[row, col];
						if(tile == null) {
							GD.Print(row + " : " + col);
						}
						if(tile.piece != null) {
							movePiece(tile.square, tile.piece, false);
						}
					}
			}
	}

	//[Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,CallLocal = true, TransferChannel = 1)]
	public void selectPiece(Piece piece) {
		if (selectedPiece == piece) {
				// Deselect the piece
				selectedPiece = null;
				piece.pieceNode.GetChild<Button>(0).Modulate = new Color(1, 1, 1, 0); // Make button fully transparent
				GD.Print("Deselected piece");
		} else {
				// Select the piece
				if (selectedPiece != null) {
						selectedPiece.pieceNode.GetChild<Button>(0).Modulate = new Color(1, 1, 1, 0); // Make previously selected button fully transparent
				}
				selectedPiece = piece;
				piece.pieceNode.GetChild<Button>(0).Modulate = new Color(1, 1, 1, 0.5f); // Make button semi-transparent to indicate selection
				GD.Print("Selected piece " + piece.pieceNode.Name);
		}
	}

	private string GetTexturePath(PieceType type) {
		switch (type) {
			case PieceType.BPAWN:
					return "../Pieces/Chess - black matte/Pawn.png";
			case PieceType.BBISHOP:
					return "../Pieces/Chess - black matte/Bishop.png";
			case PieceType.BKNIGHT:
					return "../Pieces/Chess - black matte/Knight.png";
			case PieceType.BROOK:
					return "../Pieces/Chess - black matte/Rook.png";
			case PieceType.BKING:
					return "../Pieces/Chess - black matte/King.png";
			case PieceType.BQUEEN:
					return "../Pieces/Chess - black matte/Queen.png";
			case PieceType.WPAWN:
					return "../Pieces/Chess - white matte/Pawn.png";
			case PieceType.WBISHOP:
					return "../Pieces/Chess - white matte/Bishop.png";
			case PieceType.WKNIGHT:
					return "../Pieces/Chess - white matte/Knight.png";
			case PieceType.WROOK:
					return "../Pieces/Chess - white matte/Rook.png";
			case PieceType.WKING:
					return "../Pieces/Chess - white matte/King.png";
			case PieceType.WQUEEN:
					return "../Pieces/Chess - white matte/Queen.png";
			default:
					return "";
		}
  }

	private PieceType GetPieceType(string type) {
		switch (type) {
			case "bp":
            return PieceType.BPAWN;
        case "bb":
            return PieceType.BBISHOP;
        case "bh":
            return PieceType.BKNIGHT;
        case "br":
            return PieceType.BROOK;
        case "bk":
            return PieceType.BKING;
        case "bq":
            return PieceType.BQUEEN;
        case "wp":
            return PieceType.WPAWN;
        case "wb":
            return PieceType.WBISHOP;
        case "wh":
            return PieceType.WKNIGHT;
        case "wr":
            return PieceType.WROOK;
        case "wk":
            return PieceType.WKING;
        case "wq":
            return PieceType.WQUEEN;
				default:
					return PieceType.NONE;
		}
	}

	public Piece getSelectedPiece() {
		return selectedPiece;
	}

	public BoardTile[,] getBoard() {
		return board;
	}
}
