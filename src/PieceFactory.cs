using Godot;
using System;
using System.ComponentModel;

public enum PieceType {
	BPAWN,
	BBISHOP,
	BKNIGHT,
	BROOK, 
	BKING,
	BQUEEN,
	WPAWN,
	WBISHOP,
	WKNIGHT,
	WROOK, 
	WKING,
	WQUEEN,
	NONE
}

public class Piece {
	public PieceType type;

	public Sprite2D pieceNode;

	public string name;

	public int[] position;

	public bool sleep;

	public Piece(PieceType type, string name, int[] position, bool sleep) {
		this.type = type;
		this.name = name;
		this.position = position;
	  this.sleep = sleep;
	}
}

public partial class PieceFactory : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
