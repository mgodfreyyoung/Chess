using System;
using System.Collections.Generic;
using System.Text;
using UvsChess;

namespace StudentAI
{
    public class StudentAI : IChessAI
    {
        #region IChessAI Members that are implemented by the Student

        public const int MAX_NUM_PLIES = 3;

        /// <summary>
        /// The name of your AI
        /// </summary>
        public string Name
        {
#if DEBUG
            get { return "StudentAI (Debug)"; }
#else
            get { return "StudentAI"; }
#endif
        }

        /// <summary>
        /// Evaluates the chess board and decided which move to make. This is the main method of the AI.
        /// The framework will call this method when it's your turn.
        /// </summary>
        /// <param name="board">Current chess board</param>
        /// <param name="yourColor">Your color</param>
        /// <returns> Returns the best chess move the player has for the given chess board</returns>
        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor)
        {
            return MiniMaxDecision(board, myColor, MAX_NUM_PLIES);
        }

        /// <summary>
        /// Validates a move. The framework uses this to validate the opponents move.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
        /// <returns>Returns true if the move was valid</returns>
        public bool IsValidMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            List<ChessMove> allMoves = new List<ChessMove>();
            ChessPiece piece;
            bool bIsValid = false; // if we can't verify the move it will be marked as cheating

            // make sure the move is even on the board
            if (moveToCheck.From.X < 0 || moveToCheck.From.Y < 0 || moveToCheck.From.X >= ChessBoard.NumberOfColumns || moveToCheck.From.Y >= ChessBoard.NumberOfRows ||
                moveToCheck.To.X < 0 || moveToCheck.To.Y < 0 || moveToCheck.To.X >= ChessBoard.NumberOfColumns || moveToCheck.To.Y >= ChessBoard.NumberOfRows)
            {
                return false; // index out of bounds, cheater how dare you try to crash us!
            }

            // get the piece that moved
            piece = boardBeforeMove[moveToCheck.From.X, moveToCheck.From.Y];

            // get all possible moves for the piece in question
            switch (piece)
            {
                case ChessPiece.BlackBishop:
                case ChessPiece.WhiteBishop:
                    AddAllPossibleMovesBishop(ref allMoves, ref boardBeforeMove, colorOfPlayerMoving, moveToCheck.From.X, moveToCheck.From.Y);
                    break;
                case ChessPiece.BlackKing:
                case ChessPiece.WhiteKing:
                    AddAllPossibleMovesKing(ref allMoves, ref boardBeforeMove, colorOfPlayerMoving, moveToCheck.From.X, moveToCheck.From.Y);
                    break;
                case ChessPiece.BlackKnight:
                case ChessPiece.WhiteKnight:
                    AddAllPossibleMovesKnight(ref allMoves, ref boardBeforeMove, colorOfPlayerMoving, moveToCheck.From.X, moveToCheck.From.Y);
                    break;
                case ChessPiece.BlackPawn:
                case ChessPiece.WhitePawn:
                    AddAllPossibleMovesPawn(ref allMoves, ref boardBeforeMove, colorOfPlayerMoving, moveToCheck.From.X, moveToCheck.From.Y);
                    break;
                case ChessPiece.BlackQueen:
                case ChessPiece.WhiteQueen:
                    AddAllPossibleMovesQueen(ref allMoves, ref boardBeforeMove, colorOfPlayerMoving, moveToCheck.From.X, moveToCheck.From.Y);
                    break;
                case ChessPiece.BlackRook:
                case ChessPiece.WhiteRook:
                    AddAllPossibleMovesRook(ref allMoves, ref boardBeforeMove, colorOfPlayerMoving, moveToCheck.From.X, moveToCheck.From.Y);
                    break;
                default:
                    break;
            }

            // see if the move was possible
            foreach ( ChessMove move in allMoves )
            {
                if (move.To.X == moveToCheck.To.X && move.To.Y == moveToCheck.To.Y)
                {
                    bIsValid = true; // yep, it's a valid move
                    break;
                }
            }

            return bIsValid;
        }

        /// <summary>
        /// Evaluate the board and determine a utility value for it. This function is
        /// called from MaxValue() and MinValue() to determine the value of the leaf
        /// nodes.
        /// </summary>
        /// <param name="board"></param>The board to evaluate
        /// <param name="myColor"></param>The color of my pieces
        /// <returns></returns>The utility value of this board
        private int Utility(ChessBoard board, ChessColor myColor)
        {
            // TODO <Mike> Write this heuristic function
            return 0;
        }

        /// <summary>
        /// This function looks ahead nPlies moves to determine what the best move is now
        /// </summary>
        /// <param name="boardBeforeMove"></param>The board before we move
        /// <param name="myColor"></param>The color of my pieces
        /// <param name="nPlies"></param>The number of plies to look ahead
        /// <returns>The best move, null if none</returns>
        private ChessMove MiniMaxDecision(ChessBoard boardBeforeMove, ChessColor myColor, int nPlies)
        {
            List<ChessMove> allPossibleMoves = GetAllPossibleMoves(ref boardBeforeMove, myColor);
            ChessBoard tempBoard = null;
            ChessColor opponentColor = (myColor == ChessColor.Black) ? ChessColor.White : ChessColor.Black;
            ChessMove bestMove = null;
            int bestValue = System.Int32.MinValue;
            int value;

            // peek ahead through all possible moves and find the best one
            foreach (ChessMove move in allPossibleMoves)
            {
                tempBoard = boardBeforeMove.Clone();
                tempBoard.MakeMove(move);
                value = MinValue(tempBoard, opponentColor, nPlies - 1);
                if (value > bestValue)
                {
                    bestValue = value;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        /// <summary>
        /// This mutually recursive function tries to find the best move for the opponent
        /// </summary>
        /// <param name="boardBeforeMove"></param>The board before we move
        /// <param name="myColor"></param>The color of my (the opponent) pieces
        /// <param name="nPlies"></param>The number of plies remaining to look ahead
        /// <returns>The utility value of this branch</returns>
        private int MinValue(ChessBoard boardBeforeMove, ChessColor myColor, int nPlies)
        {
            List<ChessMove> allPossibleMoves = GetAllPossibleMoves(ref boardBeforeMove, myColor);
            ChessBoard tempBoard = null;
            ChessColor opponentColor = (myColor == ChessColor.Black) ? ChessColor.White : ChessColor.Black;
            int bestValue = System.Int32.MaxValue;
            int value;

            // assign utility value if we're at the leaf node, this
            // value will bubble up with each branch choosing what
            // it considers to be the "best" value as it's value.
            if (nPlies == 0)
            {
                return Utility(boardBeforeMove, opponentColor);
            }

            // peek ahead through all possible moves and find the best one
            foreach (ChessMove move in allPossibleMoves)
            {
                tempBoard = boardBeforeMove.Clone();
                tempBoard.MakeMove(move);
                value = MaxValue(tempBoard, opponentColor, nPlies - 1);
                if (value < bestValue)
                {
                    bestValue = value;
                }
            }

            return bestValue;
        }

        /// <summary>
        /// This mutually recursive function tries to find the best move for me
        /// </summary>
        /// <param name="boardBeforeMove"></param>The board before we move
        /// <param name="myColor"></param>The color of my pieces
        /// <param name="nPlies"></param>The number of plies remaining to look ahead
        /// <returns>The utility value of this branch</returns>
        public int MaxValue(ChessBoard boardBeforeMove, ChessColor myColor, int nPlies)
        {
            List<ChessMove> allPossibleMoves = GetAllPossibleMoves(ref boardBeforeMove, myColor);
            ChessBoard tempBoard = null;
            ChessColor opponentColor = (myColor == ChessColor.Black) ? ChessColor.White : ChessColor.Black;
            int bestValue = System.Int32.MaxValue;
            int value;

            // assign utility value if we're at the leaf node, this
            // value will bubble up with each branch choosing what
            // it considers to be the "best" value as it's value.
            if (nPlies == 0)
            {
                return Utility(boardBeforeMove, myColor);
            }

            // peek ahead through all possible moves and find the best one
            foreach (ChessMove move in allPossibleMoves)
            {
                tempBoard = boardBeforeMove.Clone();
                tempBoard.MakeMove(move);
                value = MaxValue(tempBoard, opponentColor, nPlies - 1);
                if (value > bestValue)
                {
                    bestValue = value;
                }
            }

            return bestValue;
        }

        /// <summary>
		/// This function determines whether a specified piece belongs to an opponent
        /// <summary>
        /// <param name="piece">The piece in question
		/// <param name="myColor">The color of my pieces
        /// <returns>True if the piece belongs to the opponent, otherwise false</returns>
		private bool IsOpponentPiece ( ChessPiece piece, ChessColor myColor )
		{
			if ( myColor == ChessColor.White )
			{
				return ( piece == ChessPiece.BlackBishop || piece == ChessPiece.BlackKing || piece == ChessPiece.BlackKnight || piece == ChessPiece.BlackPawn || piece == ChessPiece.BlackQueen || piece == ChessPiece.BlackRook );
			}
			else
			{
				return ( piece == ChessPiece.WhiteBishop || piece == ChessPiece.WhiteKing || piece == ChessPiece.WhiteKnight || piece == ChessPiece.WhitePawn || piece == ChessPiece.WhiteQueen || piece == ChessPiece.WhiteRook );
			}
		}

        /// <summary>
		/// This function discovers all possible moves for the piece at the specified location and adds the moves to the list
        /// <summary>
		/// <param name="currentBoard">The current board state
		/// <param name="myColor">The color of the player whos moving
        /// <returns>A list of all possible moves</returns>
        private List<ChessMove> GetAllPossibleMoves(ref ChessBoard currentBoard, ChessColor myColor)
        {
            List<ChessMove> allMoves = new List<ChessMove>();
            ChessPiece piece;

            for (int x = 0; x < ChessBoard.NumberOfColumns; x++)
            {
                for (int y = 0; y < ChessBoard.NumberOfRows; y++)
                {
                    piece = currentBoard[x, y];
                    if (piece != ChessPiece.Empty && !IsOpponentPiece (piece, myColor))
                    {
                        switch (piece)
                        {
                            case ChessPiece.BlackBishop:
                            case ChessPiece.WhiteBishop:
                                AddAllPossibleMovesBishop(ref allMoves, ref currentBoard, myColor, x, y);
                                break;
                            case ChessPiece.BlackKing:
                            case ChessPiece.WhiteKing:
                                AddAllPossibleMovesKing(ref allMoves, ref currentBoard, myColor, x, y);
                                break;
                            case ChessPiece.BlackKnight:
                            case ChessPiece.WhiteKnight:
                                AddAllPossibleMovesKnight(ref allMoves, ref currentBoard, myColor, x, y);
                                break;
                            case ChessPiece.BlackPawn:
                            case ChessPiece.WhitePawn:
                                AddAllPossibleMovesPawn(ref allMoves, ref currentBoard, myColor, x, y);
                                break;
                            case ChessPiece.BlackQueen:
                            case ChessPiece.WhiteQueen:
                                AddAllPossibleMovesQueen(ref allMoves, ref currentBoard, myColor, x, y);
                                break;
                            case ChessPiece.BlackRook:
                            case ChessPiece.WhiteRook:
                                AddAllPossibleMovesRook(ref allMoves, ref currentBoard, myColor, x, y);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            return allMoves;
        }

        /// <summary>
        /// This function discovers all possible moves for a Pawn from the specified location and adds the moves to the list
        /// <summary>
		/// <param name="allMoves">A possibly non-empty list that will have all possible moves appended to it
        /// <param name="currentBoard">The current board state
        /// <param name="myColor">The color of the player whos moving
        /// <param name="x">The bishops x location on the board
        /// <param name="y">The bishops y location on the board
        ///
        private void AddAllPossibleMovesPawn(ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y)
		{
            int newX;
            int newY;

            if (myColor == ChessColor.Black)
            {
                newY = y + 1;
                if (newY < ChessBoard.NumberOfRows)
                {
                    newX = x + 1;
                    if (newX < ChessBoard.NumberOfColumns && IsOpponentPiece(currentBoard[newX, newY], myColor)) // capture diagonally forward and right
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                    newX = x - 1;
                    if (newX >= 0 && IsOpponentPiece(currentBoard[newX, newY], myColor)) // capture diagonally forward and left
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                }
                if ((newY < ChessBoard.NumberOfRows) && currentBoard[x, newY] == ChessPiece.Empty) // forward 1
                {
                    allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, newY)));
                    newY = y + 2;
                    if (newY < ChessBoard.NumberOfRows && currentBoard[x, newY] == ChessPiece.Empty) // forward 2
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, newY)));
                    }
                }
            }
            else
            {
                newY = y - 1;
                if (newY >= 0)
                {
                    newX = x + 1;
                    if (newX < ChessBoard.NumberOfColumns && IsOpponentPiece(currentBoard[newX, newY], myColor)) // capture diagonally forward and right
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                    newX = x - 1;
                    if (newX >= 0 && IsOpponentPiece(currentBoard[newX, newY], myColor)) // capture diagonally forward and left
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                }
                if ((newY >= 0) && currentBoard[x, newY] == ChessPiece.Empty) // forward 1
                {
                    allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, newY)));
                    newY = y - 2;
                    if (newY >= 0 && currentBoard[x, newY] == ChessPiece.Empty) // forward 2
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, newY)));
                    }
                }
            }
		}

        /// <summary>
        /// This function discovers all possible moves for a Rook from the specified location and adds the moves to the list
        /// <summary>
        /// <param name="allMoves">A possibly non-empty list that will have all possible moves appended to it
        /// <param name="currentBoard">The current board state
        /// <param name="myColor">The color of the player whos moving
        /// <param name="x">The bishops x location on the board
        /// <param name="y">The bishops y location on the board
        ///
        private void AddAllPossibleMovesRook(ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y)
		{
			AddAllPossibleMovesHorizontal ( ref allMoves, ref currentBoard, myColor, x, y );
			AddAllPossibleMovesVertical ( ref allMoves, ref currentBoard, myColor, x, y );
		}

        /// <summary>
        /// This function discovers all possible moves for a Bishop from the specified location and adds the moves to the list
        /// <summary>
        /// <param name="allMoves">A possibly non-empty list that will have all possible moves appended to it
        /// <param name="currentBoard">The current board state
        /// <param name="myColor">The color of the player whos moving
        /// <param name="x">The bishops x location on the board
        /// <param name="y">The bishops y location on the board
        ///
        private void AddAllPossibleMovesBishop(ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y)
		{
			AddAllPossibleMovesDiagonal ( ref allMoves, ref currentBoard, myColor, x, y );
		}

        /// <summary>
        /// This function discovers all possible moves for a Queen from the specified location and adds the moves to the list
        /// <summary>
        /// <param name="allMoves">A possibly non-empty list that will have all possible moves appended to it
        /// <param name="currentBoard">The current board state
        /// <param name="myColor">The color of the player whos moving
        /// <param name="x">The bishops x location on the board
        /// <param name="y">The bishops y location on the board
        ///
        private void AddAllPossibleMovesQueen(ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y)
		{
			AddAllPossibleMovesDiagonal ( ref allMoves, ref currentBoard, myColor, x, y );
			AddAllPossibleMovesVertical ( ref allMoves, ref currentBoard, myColor, x, y );
			AddAllPossibleMovesHorizontal ( ref allMoves, ref currentBoard, myColor, x, y );
		}

        /// <summary>
        /// This function discovers all possible moves for a Knight from the specified location and adds the moves to the list
        /// <summary>
        /// <param name="allMoves">A possibly non-empty list that will have all possible moves appended to it
        /// <param name="currentBoard">The current board state
        /// <param name="myColor">The color of the player whos moving
        /// <param name="x">The bishops x location on the board
        /// <param name="y">The bishops y location on the board
        ///
        private void AddAllPossibleMovesKnight(ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y)
        {
            int newX;
            int newY;

            if (myColor == ChessColor.Black)
            {
                // looking forward
                newY = y + 1; // forward 1
                if (newY < ChessBoard.NumberOfRows)
                {
                    // look forward 1 and right 2
                    newX = x + 2;
                    if (newX < ChessBoard.NumberOfColumns && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }

                    // look forward 1 and left 2
                    newX = x - 2;
                    if (newX >= 0 && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                }
                newY = y + 2; // forward 2
                if (newY < ChessBoard.NumberOfRows)
                {
                    // look forward 2 and right 1
                    newX = x + 1;
                    if (newX < ChessBoard.NumberOfColumns && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }

                    // look forward 2 and left 1
                    newX = x - 1;
                    if (newX >= 0 && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                }

                // looking backward now
                newY = y - 1; // backward 1
                if (newY >= 0)
                {
                    // look backward 1 and right 2
                    newX = x + 2;
                    if (newX < ChessBoard.NumberOfColumns && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }

                    // look backward 1 and left 2
                    newX = x - 2;
                    if (newX >= 0 && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                }
                newY = y - 2; // backward 2
                if ( newY >= 0 ) 
                {
                    // look backward 2 and right 1
                    newX = x + 1;
                    if (newX < ChessBoard.NumberOfColumns && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }

                    // look backward 2 and left 1
                    newX = x - 1;
                    if (newX >= 0 && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                }
            }
            else // myColor == ChessColor.White
            {
                // looking forward
                newY = y - 1; // forward 1
                if (newY >= 0)
                {
                    // look forward 1 and right 2
                    newX = x + 2;
                    if (newX < ChessBoard.NumberOfColumns && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }

                    // look forward 1 and left 2
                    newX = x - 2;
                    if (newX >= 0 && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                }
                newY = y - 2; // forward 2
                if (newY >= 0)
                {
                    // look forward 2 and right 1
                    newX = x + 1;
                    if (newX < ChessBoard.NumberOfColumns && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }

                    // look forward 2 and left 1
                    newX = x - 1;
                    if (newX >= 0 && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                }

                // looking backward now
                newY = y + 1; // backward 1
                if (newY < ChessBoard.NumberOfRows)
                {
                    // look backward 1 and right 2
                    newX = x + 2;
                    if (newX < ChessBoard.NumberOfColumns && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }

                    // look backward 1 and left 2
                    newX = x - 2;
                    if (newX >= 0 && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                }
                newY = y + 2; // backward 2
                if (newY < ChessBoard.NumberOfRows)
                {
                    // look backward 2 and right 1
                    newX = x + 1;
                    if (newX < ChessBoard.NumberOfColumns && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }

                    // look backward 2 and left 1
                    newX = x - 1;
                    if (newX >= 0 && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                }
            }
        }

        /// <summary>
        /// This function discovers all possible moves for a King from the specified location and adds the moves to the list
        /// <summary>
        /// <param name="allMoves">A possibly non-empty list that will have all possible moves appended to it
        /// <param name="currentBoard">The current board state
        /// <param name="myColor">The color of the player whos moving
        /// <param name="x">The bishops x location on the board
        /// <param name="y">The bishops y location on the board
        ///
        private void AddAllPossibleMovesKing(ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y)
        {
            int newX = x;
            int newY = y;

            // looking forward 1
            newY = y + 1;
            if (newY < ChessBoard.NumberOfRows)
            {
                // look forward 1
                if (currentBoard[x, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[x, newY], myColor))
                {
                    allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, newY)));
                }
                // look forward 1 and left 1
                newX = x - 1;
                if (newX >= 0 && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                {
                    allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                }
                // look forward 1 and right 1
                newX = x + 1;
                if (newX < ChessBoard.NumberOfColumns && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                {
                    allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                }
            }

            // looking left
            newX = x - 1;
            if (newX >= 0 && (currentBoard[newX, y] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, y], myColor)))
            {
                allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, y)));
            }

            // looking right
            newX = x + 1;
            if (newX < ChessBoard.NumberOfColumns && (currentBoard[newX, y] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, y], myColor)))
            {
                allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, y)));
            }

            // looking backward 1
            newY = y - 1;
            if (newY >= 0)
            {
                // look backward 1
                if (currentBoard[x, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[x, newY], myColor))
                {
                    allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, newY)));
                }
                // look backward 1 and left 1
                newX = x - 1;
                if (newX >= 0 && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                {
                    allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                }
                // look backward 1 and right 1
                newX = x + 1;
                if (newX < ChessBoard.NumberOfColumns && (currentBoard[newX, newY] == ChessPiece.Empty || IsOpponentPiece(currentBoard[newX, newY], myColor)))
                {
                    allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                }
            }
        }

        /// <summary>
        /// This function discovers all possible diagonal moves from the specified location and adds the moves to the list
        /// <summary>
        /// <param name="allMoves">A possibly non-empty list that will have all possible moves appended to it
        /// <param name="currentBoard">The current board state
        /// <param name="myColor">The color of the player whos moving
        /// <param name="x">The bishops x location on the board
        /// <param name="y">The bishops y location on the board
        ///
        private void AddAllPossibleMovesDiagonal(ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y)
		{
			int newX, newY;
			bool bStop = false;

			// lookup up & left
			for ( newX = x - 1; newX >= 0 && !bStop; newX-- )
			{
				for ( newY = y - 1; newY >= 0 && !bStop; newY-- )
				{
					if ( currentBoard[newX, newY] == ChessPiece.Empty )
					{
                        allMoves.Add ( new ChessMove ( new ChessLocation ( x, y ), new ChessLocation ( newX, newY ) ) );
                    }
                    else
                    {
                        if (IsOpponentPiece(currentBoard[newX, newY], myColor))
                        {
                            allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                        }
						bStop = true;
					}
				}
			}

			// look up and right
			bStop = false;
			for ( newX = x + 1; newX < ChessBoard.NumberOfColumns && !bStop; newX++ )
			{
				for ( newY = y - 1; newY >= 0 && !bStop; newY-- )
				{
                    if (currentBoard[newX, newY] == ChessPiece.Empty)
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                    else
					{
                        if (IsOpponentPiece(currentBoard[newX, newY], myColor))
                        {
                            allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                        }
						bStop = true;
					}
				}
			}

			// look down and right
			bStop = false;
			for ( newX = x + 1; newX < ChessBoard.NumberOfColumns && !bStop; newX++ )
			{
				for ( newY = y + 1; newY < ChessBoard.NumberOfRows && !bStop; newY++ )
				{
					if (currentBoard[newX, newY] == ChessPiece.Empty)
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                    else
					{
                        if (IsOpponentPiece(currentBoard[newX, newY], myColor))
                        {
                            allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                        }
						bStop = true;
					}
				}
			}

			// look down and left
			bStop = false;
			for ( newX = x - 1; newX >= 0 && !bStop; newX-- )
			{
				for ( newY = y + 1; newY < ChessBoard.NumberOfRows && !bStop; newY++ )
				{
                    if (currentBoard[newX, newY] == ChessPiece.Empty)
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                    }
                    else
					{
                        if (IsOpponentPiece(currentBoard[newX, newY], myColor))
                        {
                            allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, newY)));
                        }
						bStop = true;
					}
				}
			}
		}

        /// <summary>
        /// This function discovers all possible vertical moves from the specified location and adds the moves to the list
        /// <summary>
        /// <param name="allMoves">A possibly non-empty list that will have all possible moves appended to it
        /// <param name="currentBoard">The current board state
        /// <param name="myColor">The color of the player whos moving
        /// <param name="x">The bishops x location on the board
        /// <param name="y">The bishops y location on the board
        ///
        private void AddAllPossibleMovesVertical(ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y)
		{
			// looking up
			for ( int newY = y - 1; newY >= 0; newY-- )
			{
                if (currentBoard[x, newY] == ChessPiece.Empty)
                {
                    allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, newY)));
                }
                else
				{
                    if (IsOpponentPiece(currentBoard[x, newY], myColor))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, newY)));
                    }
					break;
				}
			}

			// looking down
			for ( int newY = y + 1; newY < ChessBoard.NumberOfRows; newY-- )
			{
                if (currentBoard[x, newY] == ChessPiece.Empty)
                {
                    allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, newY)));
                }
                else
                {
                    if (IsOpponentPiece(currentBoard[x, newY], myColor))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, newY)));
                    }
                    break;
                }
            }
		}

        /// <summary>
        /// This function discovers all possible horizontal moves from the specified location and adds the moves to the list
        /// <summary>
        /// <param name="allMoves">A possibly non-empty list that will have all possible moves appended to it
        /// <param name="currentBoard">The current board state
        /// <param name="myColor">The color of the player whos moving
        /// <param name="x">The bishops x location on the board
        /// <param name="y">The bishops y location on the board
        ///
        private void AddAllPossibleMovesHorizontal(ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y)
		{
			// looking right
			for ( int newX = x + 1; newX < ChessBoard.NumberOfColumns; newX++ )
			{
                if (currentBoard[newX, y] == ChessPiece.Empty)
                {
                    allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, y)));
                }
                else
				{
                    if (IsOpponentPiece(currentBoard[newX, y], myColor))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, y)));
                    }
					break;
				}
			}

			// looking left
			for ( int newX = x - 1; newX >= 0; newX-- )
			{
                if (currentBoard[newX, y] == ChessPiece.Empty)
                {
                    allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, y)));
                }
                else
                {
                    if (IsOpponentPiece(currentBoard[newX, y], myColor))
                    {
                        allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(newX, y)));
                    }
                    break;
                }
            }
		}

        #endregion
















        #region IChessAI Members that should be implemented as automatic properties and should NEVER be touched by students.
        /// <summary>
        /// This will return false when the framework starts running your AI. When the AI's time has run out,
        /// then this method will return true. Once this method returns true, your AI should return a 
        /// move immediately.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        public AIIsMyTurnOverCallback IsMyTurnOver { get; set; }

        /// <summary>
        /// Call this method to print out debug information. The framework subscribes to this event
        /// and will provide a log window for your debug messages.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        public AILoggerCallback Log { get; set; }

        /// <summary>
        /// Call this method to catch profiling information. The framework subscribes to this event
        /// and will print out the profiling stats in your log window.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="key"></param>
        public AIProfiler Profiler { get; set; }

        /// <summary>
        /// Call this method to tell the framework what decision print out debug information. The framework subscribes to this event
        /// and will provide a debug window for your decision tree.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        public AISetDecisionTreeCallback SetDecisionTree { get; set; }
        #endregion
    }
}
