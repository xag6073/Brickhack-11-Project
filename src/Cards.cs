using Godot;
using System;

public partial class Cards : Node2D
{

	private Sprite2D selectedCard = null;

	private Sprite2D sleep;

	private Sprite2D swap;

	private Button cardButton;

	private Node2D board;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		board = GetNode<Node2D>("/root/board");
		cardButton = GetNode<Button>("%UseCard");
		sleep = GetNode<Sprite2D>("%Sleep");
		swap = GetNode<Sprite2D>("%Swap");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (selectedCard == null) {
			cardButton.Disabled = true;	
		} else {
			cardButton.Disabled = false;
		}
	}

	public void selectCard(Sprite2D card) {
		if (selectedCard == card) {
				// Deselect the piece
				selectedCard = null;
				card.GetChild<Button>(0).Modulate = new Color(1, 1, 1, 0); // Make button fully transparent
				GD.Print("Deselected piece");
		} else {
				// Select the piece
				if (selectedCard != null) {
					selectedCard.GetChild<Button>(0).Modulate = new Color(1, 1, 1, 0); // Make previously selected button fully transparent
				}
				selectedCard = card;
				card.GetChild<Button>(0).Modulate = new Color(1, 1, 1, 0.5f); // Make button semi-transparent to indicate selection
				GD.Print("Selected card " + card.Name);
		}
	}

	public void selectSleep() {
		if (selectedCard == sleep) {
				// Deselect the piece
				selectedCard = null;
				sleep.GetChild<Button>(0).Modulate = new Color(1, 1, 1, 0); // Make button fully transparent
				GD.Print("Deselected card");
		} else {
				// Select the piece
				if (selectedCard != null) {
						selectedCard.GetChild<Button>(0).Modulate = new Color(1, 1, 1, 0); // Make previously selected button fully transparent
				}
				selectedCard = sleep;
				sleep.GetChild<Button>(0).Modulate = new Color(1, 1, 1, 0.5f); // Make button semi-transparent to indicate selection
				GD.Print("Selected card " + sleep.Name);
		}
	}

	public void selectSwap() {
		if (selectedCard == swap) {
				// Deselect the piece
				selectedCard = null;
				swap.GetChild<Button>(0).Modulate = new Color(1, 1, 1, 0); // Make button fully transparent
				GD.Print("Deselected card");
		} else {
				// Select the piece
				if (selectedCard != null) {
					  selectedCard.GetChild<Button>(0).Modulate = new Color(1, 1, 1, 0); // Make previously selected button fully transparent
				}
				selectedCard = swap;
				swap.GetChild<Button>(0).Modulate = new Color(1, 1, 1, 0.5f); // Make button semi-transparent to indicate selection
				GD.Print("Selected card " + swap.Name);
		}
	}

	public void useCard() {
		GD.Print("Using " + selectedCard.Name);
		Piece piece = Board.Instance.getSelectedPiece();
		if(piece == null) {
			GD.Print("Must select a piece!");
			return;
		}

		if(selectedCard.Name == "Sleep") {
			piece.sleep = true;
		} else if(selectedCard.Name == "Swap") {
			BoardTile [,] board = Board.Instance.getBoard();

			RandomNumberGenerator rng = new RandomNumberGenerator();
			int row = rng.RandiRange(0,5);
			int col = rng.RandiRange(0,7);

			//run until we get a black piece
			while(board[row,col].piece == null || (int)board[row,col].piece.type > 5) {
				row = rng.RandiRange(0,5);
			  col = rng.RandiRange(0,7);
			}
			GD.Print("Found " + board[row,col].piece);
			
			//then swap, TODO update matrix
			Piece temp = board[row,col].piece;
			board[row,col].piece = piece;
			board[piece.position[0],piece.position[1]].piece = temp;

			temp.position = piece.position;
			piece.position[0] = row;
			piece.position[1] = col;
			Board.Instance.updateBoard();
		}

		selectedCard.QueueFree();
		selectedCard.Hide();
		selectedCard = null;
	}
}
