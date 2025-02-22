@tool
extends ColorRect

@export var dark : bool = false :
	set(v):
		dark = v
		color = Color.DARK_GOLDENROD if dark else Color.GOLD
