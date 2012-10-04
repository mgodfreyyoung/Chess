using System;
using System.Collections.Generic;
using System.Text;
using UvsChess;

namespace StudentAI
{
    public class StudentAI : IChessAI
    {
        #region IChessAI Members that are implemented by the Student

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
            throw (new NotImplementedException());
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
            throw (new NotImplementedException());
        }

		/**
		 * This function determines whether a specified piece belongs to an opponent
		 * <param name="piece">The piece in question
		 * <param name="myColor">The color of my pieces
		 */
		public bool IsOpponentPiece ( ChessPiece piece, ChessColor myColor )
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

		/**
		 * This function discovers all possible moves for a Pawn from the specified location and adds the moves to the list
		 * @author  Bryant
		 * <param name="allMoves">All possible moves this ply
		 * <param name="currentBoard">The current board state
		 * <param name="myColor">The color of our chess pieces
		 * <param name="x">The bishops x location on the board
		 * <param name="y">The bishops y location on the board
		 */
		public void AddAllPossibleMovesPawn ( ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
		{
            int yDir = (myColor == ChessColor.Black) ? 1 : -1; // indicate whether moving forward uses addition or subtraction

			if ( currentBoard[x, y + yDir] == ChessPiece.Empty ) // forward 1
			{
				allMoves.Add ( new ChessMove ( new ChessLocation ( x, y ), new ChessLocation ( x, y + yDir ) ) );
			}
            if (y == (ChessBoard.NumberOfRows - 2) && currentBoard[x, y + yDir] == ChessPiece.Empty && currentBoard[x, y + (yDir * 2)] == ChessPiece.Empty) // forward 2
			{
                allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x, y + (yDir * 2))));
			}
            if (IsOpponentPiece(currentBoard[x - 1, y + yDir], myColor)) // capture diagonally up & left
            {
                allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x - 1, y + yDir)));
            }
            if (IsOpponentPiece(currentBoard[x + 1, y + yDir], myColor)) // capture diagonally up & right
            {
                allMoves.Add(new ChessMove(new ChessLocation(x, y), new ChessLocation(x + 1, y + yDir)));
            }
		}

		/**
		 * This function discovers all possible moves for a Rook from the specified location and adds the moves to the list
		 * @author  Bryant
		 * <param name="allMoves">All possible moves this ply
		 * <param name="currentBoard">The current board state
		 * <param name="myColor">The color of our chess pieces
		 * <param name="x">The bishops x location on the board
		 * <param name="y">The bishops y location on the board
		 */
		public void AddAllPossibleMovesRook ( ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
		{
			AddAllPossibleMovesHorizontal ( ref allMoves, ref currentBoard, myColor, x, y );
			AddAllPossibleMovesVertical ( ref allMoves, ref currentBoard, myColor, x, y );
		}

		/**
		 * This function discovers all possible moves for a Bishop from the specified location and adds the moves to the list
		 * @author  Bryant
		 * <param name="allMoves">All possible moves this ply
		 * <param name="currentBoard">The current board state
		 * <param name="myColor">The color of our chess pieces
		 * <param name="x">The bishops x location on the board
		 * <param name="y">The bishops y location on the board
		 */
		public void AddAllPossibleMovesBishop ( ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
		{
			AddAllPossibleMovesDiagonal ( ref allMoves, ref currentBoard, myColor, x, y );
		}

		/**
		 * This function discovers all possible moves for a Queen from the specified location and adds the moves to the list
		 * @author  Bryant
		 * <param name="allMoves">All possible moves this ply
		 * <param name="currentBoard">The current board state
		 * <param name="myColor">The color of our chess pieces
		 * <param name="x">The bishops x location on the board
		 * <param name="y">The bishops y location on the board
		 */
		public void AddAllPossibleMovesQueen ( ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
		{
			AddAllPossibleMovesDiagonal ( ref allMoves, ref currentBoard, myColor, x, y );
			AddAllPossibleMovesVertical ( ref allMoves, ref currentBoard, myColor, x, y );
			AddAllPossibleMovesHorizontal ( ref allMoves, ref currentBoard, myColor, x, y );
		}

		/**
		 * This function discovers all possible diagonal moves from the specified location and adds the moves to the list
		 * @author  Bryant
		 * <param name="allMoves">All possible moves this ply
		 * <param name="currentBoard">The current board state
		 * <param name="myColor">The color of our chess pieces
		 * <param name="x">The bishops x location on the board
		 * <param name="y">The bishops y location on the board
		 */
		public void AddAllPossibleMovesDiagonal ( ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
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

		/**
		 * This function discovers all possible vertical moves from the specified location and adds the moves to the list
		 * @author  Bryant
		 * <param name="allMoves">All possible moves this ply
		 * <param name="currentBoard">The current board state
		 * <param name="myColor">The color of our chess pieces
		 * <param name="x">The bishops x location on the board
		 * <param name="y">The bishops y location on the board
		 */
		public void AddAllPossibleMovesVertical ( ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
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

		/**
		 * This function discovers all possible horizontal moves from the specified location and adds the moves to the list
		 * @author  Bryant
		 * <param name="allMoves">All possible moves this ply
		 * <param name="currentBoard">The current board state
		 * <param name="myColor">The color of our chess pieces
		 * <param name="x">The bishops x location on the board
		 * <param name="y">The bishops y location on the board
		 */
		public void AddAllPossibleMovesHorizontal ( ref List<ChessMove> allMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
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
