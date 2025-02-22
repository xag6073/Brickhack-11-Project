extends Node

# Constants
const BOARD_SIZE = 8
const WHITE = 0
const BLACK = 1
const EMPTY = -1

# Piece identifiers (Pawn, Rook, Knight, Bishop, Queen, King)
enum PieceType { PAWN, ROOK, KNIGHT, BISHOP, QUEEN, KING }

# Board as a 2D array of pieces
var board = []

# Store whose turn it is (0 for white, 1 for black)
var turn = WHITE

# Initialize the board
func _ready():
	initialize_board()

# Initialize the chess board with pieces in their starting positions
func initialize_board():
	board.clear()
	
	for row in range(BOARD_SIZE):
		var row_array = []
		for col in range(BOARD_SIZE):
			row_array.append(EMPTY)
		board.append(row_array)
	
	# Place white pieces
	board[0][0] = { 'type': PieceType.ROOK, 'color': WHITE }
	board[0][1] = { 'type': PieceType.KNIGHT, 'color': WHITE }
	board[0][2] = { 'type': PieceType.BISHOP, 'color': WHITE }
	board[0][3] = { 'type': PieceType.QUEEN, 'color': WHITE }
	board[0][4] = { 'type': PieceType.KING, 'color': WHITE }
	board[0][5] = { 'type': PieceType.BISHOP, 'color': WHITE }
	board[0][6] = { 'type': PieceType.KNIGHT, 'color': WHITE }
	board[0][7] = { 'type': PieceType.ROOK, 'color': WHITE }
	for col in range(BOARD_SIZE):
		board[1][col] = { 'type': PieceType.PAWN, 'color': WHITE }
	
	# Place black pieces
	board[7][0] = { 'type': PieceType.ROOK, 'color': BLACK }
	board[7][1] = { 'type': PieceType.KNIGHT, 'color': BLACK }
	board[7][2] = { 'type': PieceType.BISHOP, 'color': BLACK }
	board[7][3] = { 'type': PieceType.QUEEN, 'color': BLACK }
	board[7][4] = { 'type': PieceType.KING, 'color': BLACK }
	board[7][5] = { 'type': PieceType.BISHOP, 'color': BLACK }
	board[7][6] = { 'type': PieceType.KNIGHT, 'color': BLACK }
	board[7][7] = { 'type': PieceType.ROOK, 'color': BLACK }
	for col in range(BOARD_SIZE):
		board[6][col] = { 'type': PieceType.PAWN, 'color': BLACK }

# Move a piece
func move_piece(from_pos: Vector2, to_pos: Vector2) -> bool:
	# Check if positions are within bounds
	if !is_valid_position(from_pos) or !is_valid_position(to_pos):
		return false
	
	# Get the piece at the starting position
	var piece = board[from_pos.x][from_pos.y]
	
	# Ensure the piece exists
	if piece == EMPTY:
		return false
	
	# Ensure it's the correct turn
	if piece.color != turn:
		return false
	
	# Validate move for specific piece type
	if !is_valid_move(piece, from_pos, to_pos):
		return false
	
	# Perform the move (for simplicity, no capturing logic in this example)
	board[to_pos.x][to_pos.y] = piece
	board[from_pos.x][from_pos.y] = EMPTY
	
	# Switch turn
	turn = WHITE if turn == BLACK else BLACK
	
	return true

# Check if a position is valid (within the board limits)
func is_valid_position(pos: Vector2) -> bool:
	return pos.x >= 0 and pos.x < BOARD_SIZE and pos.y >= 0 and pos.y < BOARD_SIZE

# Validate a move for a specific piece
func is_valid_move(piece: Dictionary, from_pos: Vector2, to_pos: Vector2) -> bool:
	match piece.type:
		PieceType.PAWN:
			return is_valid_pawn_move(piece, from_pos, to_pos)
		PieceType.ROOK:
			return is_valid_rook_move(piece, from_pos, to_pos)
		PieceType.KNIGHT:
			return is_valid_knight_move(piece, from_pos, to_pos)
		PieceType.BISHOP:
			return is_valid_bishop_move(piece, from_pos, to_pos)
		PieceType.QUEEN:
			return is_valid_queen_move(piece, from_pos, to_pos)
		PieceType.KING:
			return is_valid_king_move(piece, from_pos, to_pos)
	return false

# Pawn move validation
func is_valid_pawn_move(piece: Dictionary, from_pos: Vector2, to_pos: Vector2) -> bool:
	var direction = 1 if piece.color == WHITE else -1
	if from_pos.y == to_pos.y and (to_pos.x == from_pos.x + direction or (from_pos.x == 1 and to_pos.x == 3 and piece.color == WHITE) or (from_pos.x == 6 and to_pos.x == 4 and piece.color == BLACK)):
		return board[to_pos.x][to_pos.y] == EMPTY
	if abs(from_pos.y - to_pos.y) == 1 and abs(from_pos.x - to_pos.x) == 1:
		return board[to_pos.x][to_pos.y] != EMPTY and board[to_pos.x][to_pos.y].color != piece.color
	return false

# Rook move validation
func is_valid_rook_move(piece: Dictionary, from_pos: Vector2, to_pos: Vector2) -> bool:
	if from_pos.x != to_pos.x and from_pos.y != to_pos.y:
		return false
	# Check if path is clear
	return is_path_clear(from_pos, to_pos)

# Knight move validation
func is_valid_knight_move(piece: Dictionary, from_pos: Vector2, to_pos: Vector2) -> bool:
	return abs(from_pos.x - to_pos.x) == 2 and abs(from_pos.y - to_pos.y) == 1 or abs(from_pos.y - to_pos.y) == 2 and abs(from_pos.x - to_pos.x) == 1

# Bishop move validation
func is_valid_bishop_move(piece: Dictionary, from_pos: Vector2, to_pos: Vector2) -> bool:
	if abs(from_pos.x - to_pos.x) != abs(from_pos.y - to_pos.y):
		return false
	return is_path_clear(from_pos, to_pos)

# Queen move validation
func is_valid_queen_move(piece: Dictionary, from_pos: Vector2, to_pos: Vector2) -> bool:
	return is_valid_rook_move(piece, from_pos, to_pos) or is_valid_bishop_move(piece, from_pos, to_pos)

# King move validation
func is_valid_king_move(piece: Dictionary, from_pos: Vector2, to_pos: Vector2) -> bool:
	return abs(from_pos.x - to_pos.x) <= 1 and abs(from_pos.y - to_pos.y) <= 1

# Check if path is clear between two positions (for rooks, bishops, queens)
func is_path_clear(from_pos: Vector2, to_pos: Vector2) -> bool:
	var direction = Vector2(to_pos.x - from_pos.x, to_pos.y - from_pos.y).normalized()
	var current_pos = from_pos + direction
	while current_pos != to_pos:
		if board[current_pos.x][current_pos.y] != EMPTY:
			return false
		current_pos += direction
	return true

# Get the board as a string for easy debugging
func get_board_string() -> String:
	var board_str = ""
	for row in board:
		for cell in row:
			if cell == EMPTY:
				board_str += " . "
			else:
				var piece_symbol = get_piece_symbol(cell)
				board_str += piece_symbol + " "
		board_str += "\n"
	return board_str

# Get piece symbol for displaying (for debugging purposes)
func get_piece_symbol(piece: Dictionary) -> String:
	match piece.type:
		PieceType.PAWN:
			return "P"
		PieceType.ROOK:
			return "R"
		PieceType.KNIGHT:
			return "N"
		PieceType.BISHOP:
			return "B"
		PieceType.QUEEN:
			return "Q"
		PieceType.KING:
			return "K"
	return "?"
