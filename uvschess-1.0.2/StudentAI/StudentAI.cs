/////////////////////////////////////////////////////////
// PROGRAM OPTIONS, COMMENT OUT THE ONES YOU DON'T WANT
/////////////////////////////////////////////////////////
//#define PROFILE_CODE // if defined, then profiling will be performed
#define PERFORM_ITERATIVE_DEEPENING // if defined then we will start with a depth of MAX_NUM_PLIES and increase by 1 each time through the loop until time runs out (decision tree info will not be valid since the final best move is always from the last completed search not the one that we stopped in the middle of)
//#define GENERATE_DECISION_TREE // if defined then the decision tree will be available in the GUI if the opponent is a human, otherwise the decision tree will not be generated

// make sure that we don't have incompatible options set
#if (PERFORM_ITERATIVE_DEEPENING && GENERATE_DECISION_TREE)
#error (PERFORM_ITERATIVE_DEEPENING and GENERATE_DECISION_TREE are mutually exclusive!) The decision tree is meaningless with iterative deepening since the iteration that was interrupted when the time ran out is incomplete
#endif
#if (PERFORM_BEAMING && !PERFORM_ITERATIVE_DEEPENING)
#error (If you define PERFORM_BEAMING you must also define PERFORM_ITERATIVE_DEEPENING) Beaming is just a modification to iterative deepening
#endif

using System;
using System.Collections.Generic;
using System.Text;
using UvsChess;
using System.IO;

namespace StudentAI
{
	public class StudentAI : IChessAI
	{
		#region IChessAI Members that are implemented by the Student

		DecisionTree dt = null;
		Random random = null;
		ChessMove lastMove = null;
		ChessMove nextLastMove = null;
		int nGamePlies = 0;
#if PERFORM_BEAMING
        List<EvaluatedMove> beamingMoves = null;
        bool bSelectBeamingCandidates = false;
#endif
		int goalNumPlies = 0;
		int ifSameAs = 0;

		private static readonly short[] KingTable = new short[]
        {
          -30, -40, -40, -50, -50, -40, -40, -30,
          -30, -40, -40, -50, -50, -40, -40, -30,
          -30, -40, -40, -50, -50, -40, -40, -30,
          -30, -40, -40, -50, -50, -40, -40, -30,
          -20, -30, -30, -40, -40, -30, -30, -20,
          -10, -20, -20, -20, -20, -20, -20, -10,
           20,  20,   0,   0,   0,   0,  20,  20,
           20,  30,  10,   0,   0,  10,  30,  20
        };
		private static readonly short[] PawnTable = new short[]
        {
             0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
             5,  5, 10, 27, 27, 10,  5,  5,
             0,  0,  0, 25, 25,  0,  0,  0,
             5, -5,-10,  0,  0,-10, -5,  5,
             5, 10, 10,-25,-25, 10, 10,  5,
             0,  0,  0,  0,  0,  0,  0,  0
        };
		private static readonly short[] KnightTable = new short[]
        {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-20,-30,-30,-20,-40,-50,
        };
		private static readonly short[] BishopTable = new short[]
        {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-40,-10,-10,-40,-10,-20,
        };

		private enum MyAIProfilerTags
		{
			AddAllPossibleMovesKing,
			AddAllPossibleMovesQueen,
			AddAllPossibleMovesBishop,
			AddAllPossibleMovesKnight,
			AddAllPossibleMovesRook,
			AddAllPossibleMovesPawn,
			AddAllPossibleMovesDiagonal,
			AddAllPossibleMovesVertical,
			AddAllPossibleMovesHorizontal,
			IsValidMove,
			GetAllLegalMoves,
			GetAllPossibleMoves,
			Utility,
			TerminalTest,
			MinValue,
			MaxValue,
			IsOpponentPiece,
#if PERFORM_BEAMING
            BeamCandidate,
            GetBeamingMoves,
#endif
			IsKingInCheck
		}

		private const int MAX_NUM_PLIES = 4; // the maximum number of half-plies to search, if PERFORM_ITERATIVE_DEEPENING is defined then we will start with this value and increase by 1 each iteration
		private const int MAX_QUIESCENT_MOVES = 0; // the maximum number of quiescent (non-capture) moves that will be evaluated during quiescent trimming
		public const int QUIESCENT_TRIMMING_PLIES = 0; // when ply = <this value> quiescent trimming will begin, set it to 0 for off
#if PERFORM_BEAMING
        private const int BEAM_N_BEST_MOVES = 3; // the number of top level best moves to beam (only these moves will be explored in iterative deepening)
#endif

		/// <summary>
		/// The name of your AI
		/// </summary>
		public string Name
		{
#if DEBUG

			get { return "BryAndMike2 (Debug)"; }
#else
            get { return "BryAndMike2"; }
#endif
		}

		private void logBoard ( ChessBoard board, ChessColor myColor, ChessMove move, int score, int depth )
		{
#if FALSE
            FileStream fs = null;
            StreamWriter writer = null;
            ChessPiece piece;
            int i, j;

            if ( myColor == ChessColor.Black )
                fs = new FileStream("C:/Users/bryant/Desktop/BlackBoard.txt", FileMode.Append, FileAccess.Write);
            else
                fs = new FileStream("C:/Users/bryant/Desktop/WhiteBoard.txt", FileMode.Append, FileAccess.Write);
            writer = new StreamWriter(fs);
            writer.WriteLine("==============================================");
            writer.WriteLine("depth: {0}", depth);
            for (i = 0; i < ChessBoard.NumberOfRows; i++)
            {
                writer.WriteLine("");
                for (j = 0; j < ChessBoard.NumberOfColumns; j++)
                {
                    piece = board[j, i];
                    switch (piece)
                    {
                        case ChessPiece.BlackBishop:
                            writer.Write("BlackBishop\t");
                            break;
                        case ChessPiece.WhiteBishop:
                            writer.Write("WhiteBishop\t");
                            break;
                        case ChessPiece.BlackKing:
                            writer.Write("BlackKing\t");
                            break;
                        case ChessPiece.WhiteKing:
                            writer.Write("WhiteKing\t");
                            break;
                        case ChessPiece.BlackKnight:
                            writer.Write("BlackKnight\t");
                            break;
                        case ChessPiece.WhiteKnight:
                            writer.Write("WhiteKnight\t");
                            break;
                        case ChessPiece.BlackPawn:
                            writer.Write("BlackPawn\t");
                            break;
                        case ChessPiece.WhitePawn:
                            writer.Write("WhitePawn\t");
                            break;
                        case ChessPiece.BlackQueen:
                            writer.Write("BlackQueen\t");
                            break;
                        case ChessPiece.WhiteQueen:
                            writer.Write("WhiteQueen\t");
                            break;
                        case ChessPiece.BlackRook:
                            writer.Write("BlackRook\t");
                            break;
                        case ChessPiece.WhiteRook:
                            writer.Write("WhiteRook\t");
                            break;
                        case ChessPiece.Empty:
                            writer.Write("Empty\t");
                            break;
                        default:
                            writer.Write("<ERROR>\t");
                            break;
                    }
                }
            }

            writer.WriteLine("");
            if (move == null)
                writer.WriteLine("MOVED: NULL!!!!!");
            else
                writer.WriteLine("MOVED: from {0},{1} to {2},{3}", move.From.X, move.From.Y, move.To.X, move.To.Y);
            writer.Flush();
            writer.Close();
#endif
		}

		/// <summary>
		/// Evaluates the chess board and decided which move to make. This is the main method of the AI.
		/// The framework will call this method when it's your turn.
		/// </summary>
		/// <param name="board">Current chess board</param>
		/// <param name="yourColor">Your color</param>
		/// <returns> Returns the best chess move the player has for the given chess board</returns>
		public ChessMove GetNextMove ( ChessBoard board, ChessColor myColor )
		{
#if PROFILE_CODE
            Profiler.TagNames = Enum.GetNames(typeof(MyAIProfilerTags));
            Profiler.MinisProfilerTag = (int)MyAIProfilerTags.MinValue;
            Profiler.MaxsProfilerTag = (int)MyAIProfilerTags.MaxValue;
#endif
			ChessMove bestMove = null;
			ChessMove tmpMove = null;
			bool bDigDeeper = true;
			int depth = MAX_NUM_PLIES;

			if ( random == null )
				random = new Random ( );
#if PERFORM_BEAMING
            bSelectBeamingCandidates = (BEAM_N_BEST_MOVES > 0);
            if (bSelectBeamingCandidates)
                beamingMoves = new List<EvaluatedMove>();
#endif

#if PERFORM_ITERATIVE_DEEPENING
			do
			{
#endif
				goalNumPlies = depth;
				tmpMove = MiniMaxDecision ( board, myColor, depth );
				bDigDeeper = !( IsMyTurnOver ( ) );
				if ( bDigDeeper || bestMove == null ) // don't store the best move of the iteration if our time ran out, it's incomplete
				{
					bestMove = tmpMove;
					depth++;
				}
#if PERFORM_BEAMING
                bSelectBeamingCandidates = false; // we've found the beaming candidates now beam them
#endif

#if PERFORM_ITERATIVE_DEEPENING
			} while ( bDigDeeper );

			this.Log ( "Depth reached = " + Convert.ToString ( depth - 1 ) ); // subtract 1 since the last iteration didn't complete
#endif
#if PROFILE_CODE
            Profiler.SetDepthReachedDuringThisTurn(depth - 1);
#endif
			return bestMove;
		}


		/// <summary>
		/// Validates a move. The framework uses this to validate the opponents move.
		/// </summary>
		/// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
		/// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
		/// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
		/// <returns>Returns true if the move was valid</returns>
		public bool IsValidMove ( ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.IsValidMove);
#endif
			List<ChessMove> allMoves = new List<ChessMove> ( );
			List<ChessMove> allCaptureMoves = null;
			ChessPiece piece;
			bool bIsValid = false; // if we can't verify the move it will be marked as cheating

			// make sure the move is even on the board
			if ( moveToCheck.From.X < 0 || moveToCheck.From.Y < 0 || moveToCheck.From.X >= ChessBoard.NumberOfColumns || moveToCheck.From.Y >= ChessBoard.NumberOfRows ||
				moveToCheck.To.X < 0 || moveToCheck.To.Y < 0 || moveToCheck.To.X >= ChessBoard.NumberOfColumns || moveToCheck.To.Y >= ChessBoard.NumberOfRows )
			{
				return false; // index out of bounds, cheater how dare you try to crash us!
			}

			// get the piece that moved
			piece = boardBeforeMove[moveToCheck.From.X, moveToCheck.From.Y];

			// make sure they didn't try to move one of our pieces
			if ( IsOpponentPiece ( piece, colorOfPlayerMoving ) )
				return false; // they tried to move our pieces! cheater!
			//return myFlag;
			// get all possible moves for the piece in question
			switch ( piece )
			{
				case ChessPiece.BlackBishop:
				case ChessPiece.WhiteBishop:
				AddAllPossibleMovesBishop ( ref allMoves, ref allCaptureMoves, ref boardBeforeMove, colorOfPlayerMoving, moveToCheck.From.X, moveToCheck.From.Y );
				break;
				case ChessPiece.BlackKing:
				case ChessPiece.WhiteKing:
				AddAllPossibleMovesKing ( ref allMoves, ref allCaptureMoves, ref boardBeforeMove, colorOfPlayerMoving, moveToCheck.From.X, moveToCheck.From.Y );
				break;
				case ChessPiece.BlackKnight:
				case ChessPiece.WhiteKnight:
				AddAllPossibleMovesKnight ( ref allMoves, ref allCaptureMoves, ref boardBeforeMove, colorOfPlayerMoving, moveToCheck.From.X, moveToCheck.From.Y );
				break;
				case ChessPiece.BlackPawn:
				case ChessPiece.WhitePawn:
				AddAllPossibleMovesPawn ( ref allMoves, ref allCaptureMoves, ref boardBeforeMove, colorOfPlayerMoving, moveToCheck.From.X, moveToCheck.From.Y );
				break;
				case ChessPiece.BlackQueen:
				case ChessPiece.WhiteQueen:
				AddAllPossibleMovesQueen ( ref allMoves, ref allCaptureMoves, ref boardBeforeMove, colorOfPlayerMoving, moveToCheck.From.X, moveToCheck.From.Y );
				break;
				case ChessPiece.BlackRook:
				case ChessPiece.WhiteRook:
				AddAllPossibleMovesRook ( ref allMoves, ref allCaptureMoves, ref boardBeforeMove, colorOfPlayerMoving, moveToCheck.From.X, moveToCheck.From.Y );
				break;
				default:
				break;
			}

			// see if the move was possible
			foreach ( ChessMove move in allMoves )
			{
				if ( move.To.X == moveToCheck.To.X && move.To.Y == moveToCheck.To.Y &&
					move.From.X == moveToCheck.From.X && move.From.Y == moveToCheck.From.Y )
				{
					ChessBoard tmpBoard = boardBeforeMove.Clone ( );
					tmpBoard.MakeMove ( move );
					if ( !IsKingInCheck ( tmpBoard, colorOfPlayerMoving ) )
					{
						bIsValid = true; // yep, it's a valid move
					}
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
		/// <param name="board">The board to evaluate</param>
		/// <param name="myColor">The color of my pieces</param>
		/// <returns>The utility value of this board</returns>
		private int Utility ( ChessBoard board, ChessColor myColor )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.Utility);
#endif
			ChessPiece piece = ChessPiece.Empty;
			int score = 0;
			if ( myColor == ChessColor.White )
			{
				for ( int i = 0; i < ChessBoard.NumberOfRows; i++ )
					for ( int j = 0; j < ChessBoard.NumberOfColumns; j++ )
						if ( board[i, j] != ChessPiece.Empty )
						{
							piece = board[i, j];
							if ( piece == ChessPiece.BlackBishop )
							{
								byte index = ( byte )( ( ( byte )( ( j * 8 + ( i ) ) + 56 ) ) - ( byte )( ( byte )( ( j * 8 + ( i ) ) / 8 ) * 16 ) );
								score -= BishopTable[index];
								score = score - 325;
							}
							else if ( piece == ChessPiece.BlackKnight )
							{
								byte index = ( byte )( ( ( byte )( ( j * 8 + ( i ) ) + 56 ) ) - ( byte )( ( byte )( ( j * 8 + ( i ) ) / 8 ) * 16 ) );
								score -= KnightTable[index];
								score = score - 320;
							}
							else if ( piece == ChessPiece.BlackPawn )
							{
								byte index = ( byte )( ( ( byte )( ( j * 8 + ( i ) ) + 56 ) ) - ( byte )( ( byte )( ( j * 8 + ( i ) ) / 8 ) * 16 ) );
								score -= PawnTable[index];
								score = score - 100;
							}
							else if ( piece == ChessPiece.BlackRook )
								score = score - 500;
							else if ( piece == ChessPiece.BlackQueen )
								score = score - 975;
							else if ( piece == ChessPiece.BlackKing )
							{
								byte index = ( byte )( ( ( byte )( ( j * 8 + ( i ) ) + 56 ) ) - ( byte )( ( byte )( ( j * 8 + ( i ) ) / 8 ) * 16 ) );
								score -= KingTable[index];
								score = score - 32767;
							}
							else if ( piece == ChessPiece.WhiteBishop )
							{
								score = score + 325;
								score += BishopTable[j * 8 + ( i )];
							}
							else if ( piece == ChessPiece.WhiteKnight )
							{
								score = score + 320;
								score += KnightTable[j * 8 + ( i )];
							}
							else if ( piece == ChessPiece.WhitePawn )
							{
								score = score + 100;
								score += PawnTable[j * 8 + ( i )];
							}
							else if ( piece == ChessPiece.WhiteRook )
								score = score + 500;
							else if ( piece == ChessPiece.WhiteQueen )
								score = score + 975;
							else if ( piece == ChessPiece.WhiteKing )
							{
								score = score + 32767;
								score += KingTable[j * 8 + ( i )];
							}
						}
			}
			else if ( myColor == ChessColor.Black )
			{
				for ( int i = 0; i < ChessBoard.NumberOfRows; i++ )
					for ( int j = 0; j < ChessBoard.NumberOfColumns; j++ )
						if ( board[i, j] != ChessPiece.Empty )
						{
							piece = board[i, j];
							if ( piece == ChessPiece.BlackBishop )
							{
								byte index = ( byte )( ( ( byte )( ( j * 8 + ( i ) ) + 56 ) ) - ( byte )( ( byte )( ( j * 8 + ( i ) ) / 8 ) * 16 ) );
								score += BishopTable[index];
								score = score + 325;
							}
							else if ( piece == ChessPiece.BlackKnight )
							{
								byte index = ( byte )( ( ( byte )( ( j * 8 + ( i ) ) + 56 ) ) - ( byte )( ( byte )( ( j * 8 + ( i ) ) / 8 ) * 16 ) );
								score += KnightTable[index];
								score = score + 320;
							}
							else if ( piece == ChessPiece.BlackPawn )
							{
								byte index = ( byte )( ( ( byte )( ( j * 8 + ( i ) ) + 56 ) ) - ( byte )( ( byte )( ( j * 8 + ( i ) ) / 8 ) * 16 ) );
								score += PawnTable[index];
								score = score + 100;
							}
							else if ( piece == ChessPiece.BlackRook )
							{
								score = score + 500;
							}
							else if ( piece == ChessPiece.BlackQueen )
								score = score + 975;
							else if ( piece == ChessPiece.BlackKing )
							{
								byte index = ( byte )( ( ( byte )( ( j * 8 + ( i ) ) + 56 ) ) - ( byte )( ( byte )( ( j * 8 + ( i ) ) / 8 ) * 16 ) );
								score += KingTable[index];
								score = score + 32767;
							}
							else if ( piece == ChessPiece.WhiteBishop )
							{
								score = score - 325;
								score -= BishopTable[j * 8 + ( i )];
							}
							else if ( piece == ChessPiece.WhiteKnight )
							{
								score = score - 320;
								score -= KnightTable[j * 8 + ( i )];
							}
							else if ( piece == ChessPiece.WhitePawn )
							{
								score = score - 100;
								score -= PawnTable[j * 8 + ( i )];
							}
							else if ( piece == ChessPiece.WhiteRook )
								score = score - 500;
							else if ( piece == ChessPiece.WhiteQueen )
								score = score - 975;
							else if ( piece == ChessPiece.WhiteKing )
							{
								score = score - 32767;
								score -= KingTable[j * 8 + ( i )];
							}
						}
			}

			//               if (score == 0 || ifSameAs == score)
			// {
			//    score += random.Next(0, 1000);
			//}
			//ifSameAs = score;                                             
			return score;
		}

		/// <summary>
		/// Determine if someone has lost
		/// </summary>
		/// <param name="board">The board to examine</param>
		/// <returns>true if someone lost, false otherwise</returns>
		private bool TerminalTest ( ChessBoard board )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.TerminalTest);
#endif
			ChessPiece piece;
			int numKings = 0;

			for ( int i = 0; i < ChessBoard.NumberOfRows; i++ )
			{
				for ( int j = 0; j < ChessBoard.NumberOfColumns; j++ )
				{
					piece = board[i, j];
					if ( piece == ChessPiece.BlackKing || piece == ChessPiece.WhiteKing )
					{
						numKings++;
						if ( numKings >= 2 )
							return false;
					}
				}
			}

			return true; // we didn't find both kings so somebody lost
		}

		/// <summary>
		/// This function looks ahead nPlies moves to determine what the best move is now
		/// </summary>
		/// <param name="boardBeforeMove">The board before we move</param>
		/// <param name="myColor">The color of my pieces</param>
		/// <param name="nPlies">The number of plies to look ahead</param>
		/// <returns>The best move, null if none</returns>
		private ChessMove MiniMaxDecision ( ChessBoard boardBeforeMove, ChessColor myColor, int nPlies )
		{
			List<ChessMove> dontcare = null;
			ChessBoard finalBoard = null;
			ChessColor opponentColor = ( myColor == ChessColor.Black ) ? ChessColor.White : ChessColor.Black;
			ChessMove bestMove = null;
			int value;

#if GENERATE_DECISION_TREE
            // Tell UvsChess about the decision tree object
            dt = new DecisionTree(boardBeforeMove);
            SetDecisionTree(dt);
#endif

			// peek ahead through all possible moves and find the best one
			value = MaxValue ( boardBeforeMove, myColor, Int32.MinValue, Int32.MaxValue, ref bestMove, ref dt, nPlies );
			if ( bestMove == null )
			{
				bestMove = new ChessMove ( null, null );
				bestMove.Flag = ChessFlag.Stalemate;
				return bestMove;
			}
			finalBoard = boardBeforeMove.Clone ( );
			finalBoard.MakeMove ( bestMove );

#if GENERATE_DECISION_TREE
            dt.BestChildMove = bestMove;
#endif
			// see if they are in check / checkmate / stalemate
			List<ChessMove> allPossibleMoves2 = GetAllLegalMoves ( ref finalBoard, ref dontcare, opponentColor );
			if ( IsKingInCheck ( finalBoard, opponentColor ) )
			{
				bestMove.Flag = ChessFlag.Check;
			}
			if ( allPossibleMoves2.Count == 0 )
			{
				if ( bestMove.Flag == ChessFlag.Check )
				{
					bestMove.Flag = ChessFlag.Checkmate;
				}
				else
				{ // opponent is in stalement
				}
			}

			return bestMove;
		}

		/// <summary>
		/// This mutually recursive function tries to find the best move for the opponent
		/// </summary>
		/// <param name="boardBeforeMove">The board before we move</param>
		/// <param name="myColor">The color of my (the opponent) pieces</param>
		/// <param name="nPlies">The number of plies remaining to look ahead</param>
		/// <returns>The utility value of this branch</returns>
		private int MinValue ( ChessBoard boardBeforeMove, ChessColor myColor, int alpha, int beta, ref ChessMove chosenMove, ref DecisionTree dt, int nPlies )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.MinValue);
#endif
			List<ChessMove> allCaptureMoves = null;
			List<ChessMove> allPossibleMoves = null;
			ChessBoard tempBoard = null;
			ChessColor opponentColor = ( myColor == ChessColor.Black ) ? ChessColor.White : ChessColor.Black;
			int bestValue = Int32.MaxValue;
			int value;
			ChessMove valueMove = null;

			if ( IsMyTurnOver ( ) )
				return 0;

			// assign utility value if we're at the leaf node, this
			// value will bubble up with each branch choosing what
			// it considers to be the "best" value as it's value.
			if ( nPlies == 0 || TerminalTest ( boardBeforeMove ) )
			{
				return Utility ( boardBeforeMove, opponentColor );
			}

			// generate posible moves
			allCaptureMoves = new List<ChessMove> ( );
			allPossibleMoves = GetAllPossibleMoves ( ref boardBeforeMove, ref allCaptureMoves, myColor );

			// peek ahead through all possible moves and find the best one.
			// we're using move ordering to improve alpha beta pruning so
			// that's why we have to loop multiple times
			foreach ( ChessMove move in allCaptureMoves )
			{
				tempBoard = boardBeforeMove.Clone ( );
				tempBoard.MakeMove ( move );
#if GENERATE_DECISION_TREE
                dt.AddChild(tempBoard, move);
                dt = dt.LastChild;
#endif
				value = MaxValue ( tempBoard, opponentColor, alpha, beta, ref valueMove, ref dt, nPlies - 1 );
#if GENERATE_DECISION_TREE
                dt.EventualMoveValue = Convert.ToString(value);
                dt = dt.Parent;
#endif
				if ( value < bestValue )
				{
					bestValue = value;
					chosenMove = move;
				}
				if ( bestValue <= alpha )
				{
					chosenMove = move;
#if GENERATE_DECISION_TREE
                    dt.BestChildMove = move;
                    dt.EventualMoveValue = Convert.ToString(bestValue);
#endif
					return bestValue; // bail, max will never choose this value
				}
				if ( bestValue < beta )
				{
					beta = bestValue;
				}
			}


			// Quiescent trimming (remove all but the best quiescent moves *the effectiveness of this depends highly on our heuristic for non-capture moves*)
			if ( nPlies <= QUIESCENT_TRIMMING_PLIES )
			{
				List<EvaluatedMove> bestQuiescentMoves = new List<EvaluatedMove> ( );
				int nBest = MAX_QUIESCENT_MOVES - allCaptureMoves.Count;
				int score;

				// find the best quiescent moves
				if ( nBest > 0 )
				{
					foreach ( ChessMove move in allPossibleMoves )
					{
						tempBoard = boardBeforeMove.Clone ( );
						tempBoard.MakeMove ( move );
						score = Utility ( tempBoard, opponentColor );
						if ( bestQuiescentMoves.Count < nBest )
						{
							bestQuiescentMoves.Add ( new EvaluatedMove ( move, score ) );
							bestQuiescentMoves.Sort ( delegate ( EvaluatedMove m1, EvaluatedMove m2 ) { return m1.score.CompareTo ( m2.score ); } );
						}
						else
						{
							if ( score > bestQuiescentMoves[0].score )
							{
								bestQuiescentMoves[0].move = move;
								bestQuiescentMoves[0].score = score;
								bestQuiescentMoves.Sort ( delegate ( EvaluatedMove m1, EvaluatedMove m2 ) { return m1.score.CompareTo ( m2.score ); } );
							}
						}
					}

					// replace allPossibleMoves with the quiescent moves
					allPossibleMoves.Clear ( );
					foreach ( EvaluatedMove em in bestQuiescentMoves )
					{
						allPossibleMoves.Add ( em.move );
					}
				}
			}

			// search quiescent moves
			foreach ( ChessMove move in allPossibleMoves )
			{
				tempBoard = boardBeforeMove.Clone ( );
				tempBoard.MakeMove ( move );
#if GENERATE_DECISION_TREE
                dt.AddChild(tempBoard, move);
                dt = dt.LastChild;
#endif
				value = MaxValue ( tempBoard, opponentColor, alpha, beta, ref valueMove, ref dt, nPlies - 1 );
#if GENERATE_DECISION_TREE
                dt.EventualMoveValue = Convert.ToString(value);
                dt = dt.Parent;
#endif
				if ( value < bestValue )
				{
					bestValue = value;
					chosenMove = move;
				}
				if ( bestValue <= alpha )
				{
					chosenMove = move;
#if GENERATE_DECISION_TREE
                    dt.BestChildMove = move;
                    dt.EventualMoveValue = Convert.ToString(bestValue);
#endif
					return bestValue; // bail, max will never choose this value
				}
				if ( bestValue < beta )
				{
					beta = bestValue;
				}
			}
#if GENERATE_DECISION_TREE
            dt.BestChildMove = chosenMove;
#endif
			return bestValue;
		}

		/// <summary>
		/// This mutually recursive function tries to find the best move for me
		/// </summary>
		/// <param name="boardBeforeMove"></param>The board before we move
		/// <param name="myColor"></param>The color of my pieces
		/// <param name="nPlies"></param>The number of plies remaining to look ahead
		/// <returns>The utility value of this branch</returns>
		public int MaxValue ( ChessBoard boardBeforeMove, ChessColor myColor, int alpha, int beta, ref ChessMove chosenMove, ref DecisionTree dt, int nPlies )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.MaxValue);
            Profiler.SetDepthReachedDuringThisTurn(goalNumPlies - nPlies);
#endif
			List<ChessMove> allCaptureMoves = null;
			List<ChessMove> allPossibleMoves = null;
			ChessBoard tempBoard = null;
			ChessColor opponentColor = ( myColor == ChessColor.Black ) ? ChessColor.White : ChessColor.Black;
			int bestValue = Int32.MinValue;
			int value;
			ChessMove valueMove = null;

			if ( IsMyTurnOver ( ) )
				return 0;

			// assign utility value if we're at the leaf node, this
			// value will bubble up with each branch choosing what
			// it considers to be the "best" value as it's value.
			if ( nPlies == 0 || TerminalTest ( boardBeforeMove ) )
			{
				return Utility ( boardBeforeMove, myColor );
			}

			// calculate all possible moves (the first time through restrict it to legal moves so we don't accidentally cheat)
			allCaptureMoves = new List<ChessMove> ( );
			if ( nPlies == goalNumPlies )
			{
#if PERFORM_BEAMING
                if ((BEAM_N_BEST_MOVES <= 0) || bSelectBeamingCandidates)
                    allPossibleMoves = GetAllLegalMoves(ref boardBeforeMove, ref allCaptureMoves, myColor);
                else
                    allPossibleMoves = GetBeamingMoves();
#else
				allPossibleMoves = GetAllLegalMoves ( ref boardBeforeMove, ref allCaptureMoves, myColor );
#endif
			}
			else
				allPossibleMoves = GetAllPossibleMoves ( ref boardBeforeMove, ref allCaptureMoves, myColor );

			// peek ahead through all possible moves and find the best one
			// we're using move ordering to improve alpha beta pruning so
			// that's why we have to loop multiple times
			foreach ( ChessMove move in allCaptureMoves )
			{
				tempBoard = boardBeforeMove.Clone ( );
				tempBoard.MakeMove ( move );
#if GENERATE_DECISION_TREE
                dt.AddChild(tempBoard, move);
                dt = dt.LastChild;
#endif
				value = MinValue ( tempBoard, opponentColor, alpha, beta, ref valueMove, ref dt, nPlies - 1 );
#if GENERATE_DECISION_TREE
                dt.EventualMoveValue = Convert.ToString(value);
                dt = dt.Parent;
#endif
				if ( value > bestValue )
				{
					bestValue = value;
					chosenMove = move;

#if PERFORM_BEAMING
                    // if we're still building the beaming moves list then see if this move qualifies, if so
                    // then add it to the list
                    if (bSelectBeamingCandidates && (BEAM_N_BEST_MOVES > 0) && (nPlies == goalNumPlies))
                        BeamCandidate(bestValue, chosenMove);
#endif
				}
				if ( bestValue >= beta )
				{
					chosenMove = move;
#if GENERATE_DECISION_TREE
                    dt.BestChildMove = move;
#endif
					return bestValue; // bail, min will never choose this value
				}
				if ( bestValue > alpha )
				{
					alpha = bestValue;
				}
			}

			// Quiescent trimming (remove all but the best quiescent moves *the effectiveness of this depends highly on our heuristic for non-capture moves*)
			if ( nPlies <= QUIESCENT_TRIMMING_PLIES )
			{
				List<EvaluatedMove> bestQuiescentMoves = new List<EvaluatedMove> ( );
				int nBest = MAX_QUIESCENT_MOVES - allCaptureMoves.Count;
				int score;

				// find the best quiescent moves
				if ( nBest > 0 )
				{
					foreach ( ChessMove move in allPossibleMoves )
					{
						tempBoard = boardBeforeMove.Clone ( );
						tempBoard.MakeMove ( move );
						score = Utility ( tempBoard, myColor );
						if ( bestQuiescentMoves.Count < nBest )
						{
							bestQuiescentMoves.Add ( new EvaluatedMove ( move, score ) );
							bestQuiescentMoves.Sort ( delegate ( EvaluatedMove m1, EvaluatedMove m2 ) { return m1.score.CompareTo ( m2.score ); } );
						}
						else
						{
							if ( score > bestQuiescentMoves[0].score )
							{
								bestQuiescentMoves[0].move = move;
								bestQuiescentMoves[0].score = score;
								bestQuiescentMoves.Sort ( delegate ( EvaluatedMove m1, EvaluatedMove m2 ) { return m1.score.CompareTo ( m2.score ); } );
							}
						}
					}

					// replace allPossibleMoves with the quiescent moves
					allPossibleMoves.Clear ( );
					foreach ( EvaluatedMove em in bestQuiescentMoves )
					{
						allPossibleMoves.Add ( em.move );
					}
				}
			}

			// search quiescent moves
			foreach ( ChessMove move in allPossibleMoves )
			{
				tempBoard = boardBeforeMove.Clone ( );
				tempBoard.MakeMove ( move );
#if GENERATE_DECISION_TREE
                dt.AddChild(tempBoard, move);
                dt = dt.LastChild;
#endif
				value = MinValue ( tempBoard, opponentColor, alpha, beta, ref valueMove, ref dt, nPlies - 1 );
#if GENERATE_DECISION_TREE
                dt.EventualMoveValue = Convert.ToString(value);
                dt = dt.Parent;
#endif
				if ( value > bestValue )
				{
					bestValue = value;
					chosenMove = move;

#if PERFORM_BEAMING
                    // if we're still building the beaming moves list then see if this move qualifies, if so
                    // then add it to the list
                    if (bSelectBeamingCandidates && (BEAM_N_BEST_MOVES > 0) && (nPlies == goalNumPlies))
                        BeamCandidate(bestValue, chosenMove);
#endif
				}
				if ( bestValue >= beta )
				{
					chosenMove = move;
#if GENERATE_DECISION_TREE
                    dt.BestChildMove = move;
                    dt.EventualMoveValue = Convert.ToString(bestValue);
#endif
					return bestValue; // bail, min will never choose this value
				}
				if ( bestValue > alpha )
				{
					alpha = bestValue;
				}
			}

#if GENERATE_DECISION_TREE
            dt.BestChildMove = chosenMove;
#endif

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
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.IsOpponentPiece);
#endif
			if ( myColor == ChessColor.White )
			{
				return ( piece == ChessPiece.BlackBishop || piece == ChessPiece.BlackKing || piece == ChessPiece.BlackKnight || piece == ChessPiece.BlackPawn || piece == ChessPiece.BlackQueen || piece == ChessPiece.BlackRook );
			}
			else
			{
				return ( piece == ChessPiece.WhiteBishop || piece == ChessPiece.WhiteKing || piece == ChessPiece.WhiteKnight || piece == ChessPiece.WhitePawn || piece == ChessPiece.WhiteQueen || piece == ChessPiece.WhiteRook );
			}
		}

#if PERFORM_BEAMING
        /// <summary>
        /// See if the specified move is one of the best so far, if so then we'll add it to the
        /// list of moves to beam search
        /// </summary>
        /// <param name="value">The final board value associated with the move</param>
        /// <param name="move">The move to check</param>
        private void BeamCandidate(int value, ChessMove move)
        {
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.BeamCandidate);
#endif
            if (beamingMoves.Count < BEAM_N_BEST_MOVES)
                beamingMoves.Add(new EvaluatedMove(move, value));
            else if (value > beamingMoves[0].score)
            {
                beamingMoves[0] = new EvaluatedMove(move, value);
                beamingMoves.Sort(delegate(EvaluatedMove m1, EvaluatedMove m2) { return m1.score.CompareTo(m2.score); });
            }
        }

        /// <summary>
        /// Retrieve a list of all of the moves that will be beam searched
        /// </summary>
        /// <returns>A list of all of the moves to beam search</returns>
        private List<ChessMove> GetBeamingMoves()
        {
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.GetBeamingMoves);
#endif
            if (beamingMoves == null)
                throw new Exception("Beaming moves is null!");
            List<ChessMove> moves = new List<ChessMove>();
            foreach (EvaluatedMove em in beamingMoves)
            {
                moves.Add(em.move);
            }
            return moves;
        }
#endif

		/// <summary>
		/// Determine whether the specified players king is in check
		/// </summary>
		/// <param name="board"></param>The current board state
		/// <param name="myColor"></param>The color of the player in question
		/// <returns>true if player myColor's king is in check, otherwise false</returns>
		private bool IsKingInCheck ( ChessBoard board, ChessColor myColor )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.IsKingInCheck);
#endif
			ChessColor opponentColor = ( myColor == ChessColor.White ) ? ChessColor.Black : ChessColor.White;
			List<ChessMove> examineMoves = new List<ChessMove> ( );
			List<ChessMove> allMoves = new List<ChessMove> ( );
			List<ChessMove> allCaptureMoves = new List<ChessMove> ( );
			List<ChessMove> allCaptureMoves2 = null;
			ChessPiece piece;
			ChessLocation myKingLoc = null;
			bool bContinue = true;

			// find my king
			for ( int i = 0; i < ChessBoard.NumberOfRows && bContinue; i++ )
			{
				for ( int j = 0; j < ChessBoard.NumberOfColumns; j++ )
				{
					piece = board[i, j];
					if ( piece == ChessPiece.BlackKing && myColor == ChessColor.Black || piece == ChessPiece.WhiteKing && myColor == ChessColor.White )
					{
						myKingLoc = new ChessLocation ( i, j );
						bContinue = false;
						break;
					}
				}
			}

			if ( myKingLoc == null )
				return true; // well if the king doesn't exist then I guess it's most useful to say it's in check even tho it means something else has gone drastically wrong

			// find all pieces that could attack my king's location
			// WARNING: it might look wrong to use "myColor" but it's not! This is an optimization;
			// We're looking at all queen and knight moves from our kings location, we find any opponent
			// pieces in any of those moves and then see if those pieces can attack my king. If they can
			// then he's in check (or checkmate). This optimization prevents us from having to check all
			// possible moves for all pieces.
			AddAllPossibleMovesQueen ( ref examineMoves, ref allCaptureMoves, ref board, myColor, myKingLoc.X, myKingLoc.Y );
			foreach ( ChessMove move in allCaptureMoves )
			{
				piece = board[move.To.X, move.To.Y];
				if ( piece != ChessPiece.Empty )
				{
					allMoves.Clear ( );
					switch ( piece )
					{
						case ChessPiece.BlackBishop:
						case ChessPiece.WhiteBishop:
						AddAllPossibleMovesBishop ( ref allMoves, ref allCaptureMoves2, ref board, opponentColor, move.To.X, move.To.Y );
						break;
						case ChessPiece.BlackKing:
						case ChessPiece.WhiteKing:
						AddAllPossibleMovesKing ( ref allMoves, ref allCaptureMoves2, ref board, opponentColor, move.To.X, move.To.Y );
						break;
						case ChessPiece.BlackKnight:
						case ChessPiece.WhiteKnight:
						AddAllPossibleMovesKnight ( ref allMoves, ref allCaptureMoves2, ref board, opponentColor, move.To.X, move.To.Y );
						break;
						case ChessPiece.BlackPawn:
						case ChessPiece.WhitePawn:
						AddAllPossibleMovesPawn ( ref allMoves, ref allCaptureMoves2, ref board, opponentColor, move.To.X, move.To.Y );
						break;
						case ChessPiece.BlackQueen:
						case ChessPiece.WhiteQueen:
						AddAllPossibleMovesQueen ( ref allMoves, ref allCaptureMoves2, ref board, opponentColor, move.To.X, move.To.Y );
						break;
						case ChessPiece.BlackRook:
						case ChessPiece.WhiteRook:
						AddAllPossibleMovesRook ( ref allMoves, ref allCaptureMoves2, ref board, opponentColor, move.To.X, move.To.Y );
						break;
						default:
						break;
					}
					foreach ( ChessMove opMove in allMoves )
					{
						if ( opMove.To.X == myKingLoc.X && opMove.To.Y == myKingLoc.Y )
							return true;
					}
				}
			}
			examineMoves.Clear ( );
			allCaptureMoves.Clear ( );
			AddAllPossibleMovesKnight ( ref examineMoves, ref allCaptureMoves, ref board, myColor, myKingLoc.X, myKingLoc.Y );
			foreach ( ChessMove move in allCaptureMoves )
			{
				piece = board[move.To.X, move.To.Y];
				if ( piece != ChessPiece.Empty )
				{
					allMoves.Clear ( );
					switch ( piece )
					{
						case ChessPiece.BlackBishop:
						case ChessPiece.WhiteBishop:
						AddAllPossibleMovesBishop ( ref allMoves, ref allCaptureMoves2, ref board, opponentColor, move.To.X, move.To.Y );
						break;
						case ChessPiece.BlackKing:
						case ChessPiece.WhiteKing:
						AddAllPossibleMovesKing ( ref allMoves, ref allCaptureMoves2, ref board, opponentColor, move.To.X, move.To.Y );
						break;
						case ChessPiece.BlackKnight:
						case ChessPiece.WhiteKnight:
						AddAllPossibleMovesKnight ( ref allMoves, ref allCaptureMoves2, ref board, opponentColor, move.To.X, move.To.Y );
						break;
						case ChessPiece.BlackPawn:
						case ChessPiece.WhitePawn:
						AddAllPossibleMovesPawn ( ref allMoves, ref allCaptureMoves2, ref board, opponentColor, move.To.X, move.To.Y );
						break;
						case ChessPiece.BlackQueen:
						case ChessPiece.WhiteQueen:
						AddAllPossibleMovesQueen ( ref allMoves, ref allCaptureMoves2, ref board, opponentColor, move.To.X, move.To.Y );
						break;
						case ChessPiece.BlackRook:
						case ChessPiece.WhiteRook:
						AddAllPossibleMovesRook ( ref allMoves, ref allCaptureMoves2, ref board, opponentColor, move.To.X, move.To.Y );
						break;
						default:
						break;
					}
					foreach ( ChessMove opMove in allMoves )
					{
						if ( opMove.To.X == myKingLoc.X && opMove.To.Y == myKingLoc.Y )
							return true;
					}
				}
			}

			return false; // nope, my king is not in check
		}

		/// <summary>
		/// This function discovers all legal moves for the piece at the specified location and adds the moves to the list
		/// <summary>
		/// <param name="currentBoard">The current board state
		/// <param name="allCaptureMoves">The buffer to receive all capture moves</param>
		/// <param name="myColor">The color of the player whos moving
		/// <returns>A list of all legal moves</returns>
		private List<ChessMove> GetAllLegalMoves ( ref ChessBoard currentBoard, ref List<ChessMove> allCaptureMoves, ChessColor myColor )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.GetAllLegalMoves);
#endif
			List<ChessMove> allMoves = GetAllPossibleMoves ( ref currentBoard, ref allCaptureMoves, myColor );
			ChessMove move = null;
			ChessBoard tmpBoard = null;

			// remove illegal moves (moves that would put you in check)
			for ( int i = allMoves.Count - 1; i >= 0; i-- )
			{
				move = allMoves[i];
				tmpBoard = currentBoard.Clone ( );
				tmpBoard.MakeMove ( move );
				if ( IsKingInCheck ( tmpBoard, myColor ) )
				{
					allMoves.Remove ( move );
				}
			}
			if ( allCaptureMoves != null )
			{
				for ( int i = allCaptureMoves.Count - 1; i >= 0; i-- )
				{
					move = allCaptureMoves[i];
					tmpBoard = currentBoard.Clone ( );
					tmpBoard.MakeMove ( move );
					if ( IsKingInCheck ( tmpBoard, myColor ) )
					{
						allCaptureMoves.Remove ( move );
					}
				}
			}

			return allMoves;
		}

		/// <summary>
		/// This function discovers all possible moves for the piece at the specified location and adds the moves to the list. If allCaptureMoves is not
		/// null then then any moves resulting in a capture will be placed in that list instead of the returned list.
		/// <summary>
		/// <param name="currentBoard">The current board state
		/// <param name="allCaptureMoves">The list to receive any capture moves, may be null. If not null then any moves resulting in a capture are placed in this list instead of the returned list</param>
		/// <param name="myColor">The color of the player whos moving
		/// <returns>A list of all possible moves</returns>
		private List<ChessMove> GetAllPossibleMoves ( ref ChessBoard currentBoard, ref List<ChessMove> allCaptureMoves, ChessColor myColor )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.GetAllPossibleMoves);
#endif
			List<ChessMove> allMoves = new List<ChessMove> ( );
			ChessPiece piece;

			for ( int x = 0; x < ChessBoard.NumberOfColumns; x++ )
			{
				for ( int y = 0; y < ChessBoard.NumberOfRows; y++ )
				{
					piece = currentBoard[x, y];
					if ( piece != ChessPiece.Empty && !IsOpponentPiece ( piece, myColor ) )
					{
						switch ( piece )
						{
							case ChessPiece.BlackBishop:
							case ChessPiece.WhiteBishop:
							AddAllPossibleMovesBishop ( ref allMoves, ref allCaptureMoves, ref currentBoard, myColor, x, y );
							break;
							case ChessPiece.BlackKing:
							case ChessPiece.WhiteKing:
							AddAllPossibleMovesKing ( ref allMoves, ref allCaptureMoves, ref currentBoard, myColor, x, y );
							break;
							case ChessPiece.BlackKnight:
							case ChessPiece.WhiteKnight:
							AddAllPossibleMovesKnight ( ref allMoves, ref allCaptureMoves, ref currentBoard, myColor, x, y );
							break;
							case ChessPiece.BlackPawn:
							case ChessPiece.WhitePawn:
							AddAllPossibleMovesPawn ( ref allMoves, ref allCaptureMoves, ref currentBoard, myColor, x, y );
							break;
							case ChessPiece.BlackQueen:
							case ChessPiece.WhiteQueen:
							AddAllPossibleMovesQueen ( ref allMoves, ref allCaptureMoves, ref currentBoard, myColor, x, y );
							break;
							case ChessPiece.BlackRook:
							case ChessPiece.WhiteRook:
							AddAllPossibleMovesRook ( ref allMoves, ref allCaptureMoves, ref currentBoard, myColor, x, y );
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
		/// <param name="allCaptureMoves">The list to receive any capture moves, may be null. If not null then any moves resulting in a capture are placed in this list instead of the returned list</param>
		/// <param name="currentBoard">The current board state
		/// <param name="myColor">The color of the player whos moving
		/// <param name="x">The bishops x location on the board
		/// <param name="y">The bishops y location on the board
		///
		private void AddAllPossibleMovesPawn ( ref List<ChessMove> allMoves, ref List<ChessMove> allCaptureMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.AddAllPossibleMovesPawn);
#endif
			ChessLocation from = new ChessLocation ( x, y );
			int newX;
			int newY;
			//DecisionTree dt = new DecisionTree(currentBoard);                     //come back to this and setup decision tree!!
			//SetDecisionTree(dt);
			if ( myColor == ChessColor.Black )
			{
				newY = y + 1;
				if ( newY < ChessBoard.NumberOfRows )
				{

					newX = x + 1;
					if ( newX < ChessBoard.NumberOfColumns && IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) // capture diagonally forward and right
					{
						if ( allCaptureMoves == null )
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
					newX = x - 1;
					if ( newX >= 0 && IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) // capture diagonally forward and left
					{
						if ( allCaptureMoves == null )
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
				}
				if ( ( newY < ChessBoard.NumberOfRows ) && currentBoard[x, newY] == ChessPiece.Empty ) // forward 1
				{
					allMoves.Add ( new ChessMove ( from, new ChessLocation ( x, newY ) ) );
					newY = y + 2;

					if ( newY < ChessBoard.NumberOfRows && currentBoard[x, newY] == ChessPiece.Empty && y == 1 ) // forward 2
					{
						allMoves.Add ( new ChessMove ( from, new ChessLocation ( x, newY ) ) );
					}
				}
			}
			else
			{
				newY = y - 1;
				if ( newY >= 0 )
				{
					newX = x + 1;
					if ( newX < ChessBoard.NumberOfColumns && IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) // capture diagonally forward and right
					{
						if ( allCaptureMoves == null )
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
					newX = x - 1;
					if ( newX >= 0 && IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) // capture diagonally forward and left
					{
						if ( allCaptureMoves == null )
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
				}
				if ( ( newY >= 0 ) && currentBoard[x, newY] == ChessPiece.Empty ) // forward 1
				{
					allMoves.Add ( new ChessMove ( from, new ChessLocation ( x, newY ) ) );
					newY = y - 2;
					if ( newY >= 0 && currentBoard[x, newY] == ChessPiece.Empty && y == 6 ) // forward 2
					{
						allMoves.Add ( new ChessMove ( from, new ChessLocation ( x, newY ) ) );
					}
				}
			}
		}

		/// <summary>
		/// This function discovers all possible moves for a Rook from the specified location and adds the moves to the list
		/// <summary>
		/// <param name="allMoves">A possibly non-empty list that will have all possible moves appended to it
		/// <param name="allCaptureMoves">The list to receive any capture moves, may be null. If not null then any moves resulting in a capture are placed in this list instead of the returned list</param>
		/// <param name="currentBoard">The current board state
		/// <param name="myColor">The color of the player whos moving
		/// <param name="x">The bishops x location on the board
		/// <param name="y">The bishops y location on the board
		///
		private void AddAllPossibleMovesRook ( ref List<ChessMove> allMoves, ref List<ChessMove> allCaptureMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.AddAllPossibleMovesRook);
#endif
			AddAllPossibleMovesHorizontal ( ref allMoves, ref allCaptureMoves, ref currentBoard, myColor, x, y );
			AddAllPossibleMovesVertical ( ref allMoves, ref allCaptureMoves, ref currentBoard, myColor, x, y );
		}

		/// <summary>
		/// This function discovers all possible moves for a Bishop from the specified location and adds the moves to the list
		/// <summary>
		/// <param name="allMoves">A possibly non-empty list that will have all possible moves appended to it
		/// <param name="allCaptureMoves">The list to receive any capture moves, may be null. If not null then any moves resulting in a capture are placed in this list instead of the returned list</param>
		/// <param name="currentBoard">The current board state
		/// <param name="myColor">The color of the player whos moving
		/// <param name="x">The bishops x location on the board
		/// <param name="y">The bishops y location on the board
		///
		private void AddAllPossibleMovesBishop ( ref List<ChessMove> allMoves, ref List<ChessMove> allCaptureMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.AddAllPossibleMovesBishop);
#endif
			AddAllPossibleMovesDiagonal ( ref allMoves, ref allCaptureMoves, ref currentBoard, myColor, x, y );
		}

		/// <summary>
		/// This function discovers all possible moves for a Queen from the specified location and adds the moves to the list
		/// <summary>
		/// <param name="allMoves">A possibly non-empty list that will have all possible moves appended to it
		/// <param name="allCaptureMoves">The list to receive any capture moves, may be null. If not null then any moves resulting in a capture are placed in this list instead of the returned list</param>
		/// <param name="currentBoard">The current board state
		/// <param name="myColor">The color of the player whos moving
		/// <param name="x">The bishops x location on the board
		/// <param name="y">The bishops y location on the board
		///
		private void AddAllPossibleMovesQueen ( ref List<ChessMove> allMoves, ref List<ChessMove> allCaptureMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.AddAllPossibleMovesQueen);
#endif
			AddAllPossibleMovesDiagonal ( ref allMoves, ref allCaptureMoves, ref currentBoard, myColor, x, y );
			AddAllPossibleMovesVertical ( ref allMoves, ref allCaptureMoves, ref currentBoard, myColor, x, y );
			AddAllPossibleMovesHorizontal ( ref allMoves, ref allCaptureMoves, ref currentBoard, myColor, x, y );
		}

		/// <summary>
		/// This function discovers all possible moves for a Knight from the specified location and adds the moves to the list
		/// <summary>
		/// <param name="allMoves">A possibly non-empty list that will have all possible moves appended to it
		/// <param name="allCaptureMoves">The list to receive any capture moves, may be null. If not null then any moves resulting in a capture are placed in this list instead of the returned list</param>
		/// <param name="currentBoard">The current board state
		/// <param name="myColor">The color of the player whos moving
		/// <param name="x">The bishops x location on the board
		/// <param name="y">The bishops y location on the board
		///
		private void AddAllPossibleMovesKnight ( ref List<ChessMove> allMoves, ref List<ChessMove> allCaptureMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.AddAllPossibleMovesKnight);
#endif
			ChessLocation from = new ChessLocation ( x, y );
			int newX;
			int newY;
			bool bIsOpponent = false;

			if ( myColor == ChessColor.Black )
			{
				// looking forward
				newY = y + 1; // forward 1
				if ( newY < ChessBoard.NumberOfRows )
				{
					// look forward 1 and right 2
					newX = x + 2;
					if ( newX < ChessBoard.NumberOfColumns && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}

					// look forward 1 and left 2
					newX = x - 2;
					if ( newX >= 0 && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
				}
				newY = y + 2; // forward 2
				if ( newY < ChessBoard.NumberOfRows )
				{
					// look forward 2 and right 1
					newX = x + 1;
					if ( newX < ChessBoard.NumberOfColumns && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}

					// look forward 2 and left 1
					newX = x - 1;
					if ( newX >= 0 && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
				}

				// looking backward now
				newY = y - 1; // backward 1
				if ( newY >= 0 )
				{
					// look backward 1 and right 2
					newX = x + 2;
					if ( newX < ChessBoard.NumberOfColumns && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}

					// look backward 1 and left 2
					newX = x - 2;
					if ( newX >= 0 && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
				}
				newY = y - 2; // backward 2
				if ( newY >= 0 )
				{
					// look backward 2 and right 1
					newX = x + 1;
					if ( newX < ChessBoard.NumberOfColumns && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}

					// look backward 2 and left 1
					newX = x - 1;
					if ( newX >= 0 && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
				}
			}
			else // myColor == ChessColor.White
			{
				// looking forward
				newY = y - 1; // forward 1
				if ( newY >= 0 )
				{
					// look forward 1 and right 2
					newX = x + 2;
					if ( newX < ChessBoard.NumberOfColumns && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}

					// look forward 1 and left 2
					newX = x - 2;
					if ( newX >= 0 && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
				}
				newY = y - 2; // forward 2
				if ( newY >= 0 )
				{
					// look forward 2 and right 1
					newX = x + 1;
					if ( newX < ChessBoard.NumberOfColumns && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}

					// look forward 2 and left 1
					newX = x - 1;
					if ( newX >= 0 && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
				}

				// looking backward now
				newY = y + 1; // backward 1
				if ( newY < ChessBoard.NumberOfRows )
				{
					// look backward 1 and right 2
					newX = x + 2;
					if ( newX < ChessBoard.NumberOfColumns && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}

					// look backward 1 and left 2
					newX = x - 2;
					if ( newX >= 0 && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
				}
				newY = y + 2; // backward 2
				if ( newY < ChessBoard.NumberOfRows )
				{
					// look backward 2 and right 1
					newX = x + 1;
					if ( newX < ChessBoard.NumberOfColumns && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}

					// look backward 2 and left 1
					newX = x - 1;
					if ( newX >= 0 && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || ( currentBoard[newX, newY] == ChessPiece.Empty ) ) )
					{
						if ( bIsOpponent && allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
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
		private void AddAllPossibleMovesKing ( ref List<ChessMove> allMoves, ref List<ChessMove> allCaptureMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.AddAllPossibleMovesKing);
#endif
			ChessLocation from = new ChessLocation ( x, y );
			int newX = x;
			int newY = y;
			bool bIsOpponent = false;

			// looking forward 1
			newY = y + 1;
			if ( newY < ChessBoard.NumberOfRows )
			{
				// look forward 1
				if ( ( bIsOpponent = IsOpponentPiece ( currentBoard[x, newY], myColor ) ) || currentBoard[x, newY] == ChessPiece.Empty )
				{
					if ( bIsOpponent && allCaptureMoves != null )
						allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( x, newY ) ) );
					else
						allMoves.Add ( new ChessMove ( from, new ChessLocation ( x, newY ) ) );
				}
				// look forward 1 and left 1
				newX = x - 1;
				if ( newX >= 0 && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || currentBoard[newX, newY] == ChessPiece.Empty ) )
				{
					if ( bIsOpponent && allCaptureMoves != null )
						allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					else
						allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
				}
				// look forward 1 and right 1
				newX = x + 1;
				if ( newX < ChessBoard.NumberOfColumns && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || currentBoard[newX, newY] == ChessPiece.Empty ) )
				{
					if ( bIsOpponent && allCaptureMoves != null )
						allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					else
						allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
				}
			}

			// looking left
			newX = x - 1;
			if ( newX >= 0 && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, y], myColor ) ) || currentBoard[newX, y] == ChessPiece.Empty ) )
			{
				if ( bIsOpponent && allCaptureMoves != null )
					allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, y ) ) );
				else
					allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, y ) ) );
			}

			// looking right
			newX = x + 1;
			if ( newX < ChessBoard.NumberOfColumns && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, y], myColor ) ) || currentBoard[newX, y] == ChessPiece.Empty ) )
			{
				if ( bIsOpponent && allCaptureMoves != null )
					allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, y ) ) );
				else
					allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, y ) ) );
			}

			// looking backward 1
			newY = y - 1;
			if ( newY >= 0 )
			{
				// look backward 1
				if ( ( bIsOpponent = IsOpponentPiece ( currentBoard[x, newY], myColor ) ) || currentBoard[x, newY] == ChessPiece.Empty )
				{
					if ( bIsOpponent && allCaptureMoves != null )
						allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( x, newY ) ) );
					else
						allMoves.Add ( new ChessMove ( from, new ChessLocation ( x, newY ) ) );
				}
				// look backward 1 and left 1
				newX = x - 1;
				if ( newX >= 0 && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || currentBoard[newX, newY] == ChessPiece.Empty ) )
				{
					if ( bIsOpponent && allCaptureMoves != null )
						allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					else
						allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
				}
				// look backward 1 and right 1
				newX = x + 1;
				if ( newX < ChessBoard.NumberOfColumns && ( ( bIsOpponent = IsOpponentPiece ( currentBoard[newX, newY], myColor ) ) || currentBoard[newX, newY] == ChessPiece.Empty ) )
				{
					if ( bIsOpponent && allCaptureMoves != null )
						allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					else
						allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
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
		private void AddAllPossibleMovesDiagonal ( ref List<ChessMove> allMoves, ref List<ChessMove> allCaptureMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.AddAllPossibleMovesDiagonal);
#endif
			ChessLocation from = new ChessLocation ( x, y );
			int newX, newY;

			// lookup up & left
			newY = y - 1;
			for ( newX = x - 1; newX >= 0 && newY >= 0; newX--, newY-- )
			{
				if ( currentBoard[newX, newY] == ChessPiece.Empty )
				{
					allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
				}
				else
				{
					if ( IsOpponentPiece ( currentBoard[newX, newY], myColor ) )
					{
						if ( allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
					break;
				}
			}

			// look up and right
			newY = y - 1;
			for ( newX = x + 1; newX < ChessBoard.NumberOfColumns && newY >= 0; newX++, newY-- )
			{
				if ( currentBoard[newX, newY] == ChessPiece.Empty )
				{
					allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
				}
				else
				{
					if ( IsOpponentPiece ( currentBoard[newX, newY], myColor ) )
					{
						if ( allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
					break;
				}
			}

			// look down and right
			newY = y + 1;
			for ( newX = x + 1; newX < ChessBoard.NumberOfColumns && newY < ChessBoard.NumberOfRows; newX++, newY++ )
			{
				if ( currentBoard[newX, newY] == ChessPiece.Empty )
				{
					allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
				}
				else
				{
					if ( IsOpponentPiece ( currentBoard[newX, newY], myColor ) )
					{
						if ( allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
					break;
				}
			}

			// look down and left
			newY = y + 1;
			for ( newX = x - 1; newX >= 0 && newY < ChessBoard.NumberOfRows; newX--, newY++ )
			{
				if ( currentBoard[newX, newY] == ChessPiece.Empty )
				{
					allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
				}
				else
				{
					if ( IsOpponentPiece ( currentBoard[newX, newY], myColor ) )
					{
						if ( allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, newY ) ) );
					}
					break;
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
		private void AddAllPossibleMovesVertical ( ref List<ChessMove> allMoves, ref List<ChessMove> allCaptureMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.AddAllPossibleMovesVertical);
#endif
			ChessLocation from = new ChessLocation ( x, y );
			// looking up
			for ( int newY = y - 1; newY >= 0; newY-- )
			{
				if ( currentBoard[x, newY] == ChessPiece.Empty )
				{
					allMoves.Add ( new ChessMove ( from, new ChessLocation ( x, newY ) ) );
				}
				else
				{
					if ( IsOpponentPiece ( currentBoard[x, newY], myColor ) )
					{
						if ( allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( x, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( x, newY ) ) );
					}
					break;
				}
			}

			// looking down
			for ( int newY = y + 1; newY < ChessBoard.NumberOfRows; newY++ )
			{
				if ( currentBoard[x, newY] == ChessPiece.Empty )
				{
					allMoves.Add ( new ChessMove ( from, new ChessLocation ( x, newY ) ) );
				}
				else
				{
					if ( IsOpponentPiece ( currentBoard[x, newY], myColor ) )
					{
						if ( allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( x, newY ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( x, newY ) ) );
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
		private void AddAllPossibleMovesHorizontal ( ref List<ChessMove> allMoves, ref List<ChessMove> allCaptureMoves, ref ChessBoard currentBoard, ChessColor myColor, int x, int y )
		{
#if PROFILE_CODE
            Profiler.IncrementTagCount((int)MyAIProfilerTags.AddAllPossibleMovesHorizontal);
#endif
			ChessLocation from = new ChessLocation ( x, y );
			// looking right
			for ( int newX = x + 1; newX < ChessBoard.NumberOfColumns; newX++ )
			{
				if ( currentBoard[newX, y] == ChessPiece.Empty )
				{
					allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, y ) ) );
				}
				else
				{
					if ( IsOpponentPiece ( currentBoard[newX, y], myColor ) )
					{
						if ( allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, y ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, y ) ) );
					}
					break;
				}
			}

			// looking left
			for ( int newX = x - 1; newX >= 0; newX-- )
			{
				if ( currentBoard[newX, y] == ChessPiece.Empty )
				{
					allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, y ) ) );
				}
				else
				{
					if ( IsOpponentPiece ( currentBoard[newX, y], myColor ) )
					{
						if ( allCaptureMoves != null )
							allCaptureMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, y ) ) );
						else
							allMoves.Add ( new ChessMove ( from, new ChessLocation ( newX, y ) ) );
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

	public class EvaluatedMove
	{
		public ChessMove move;
		public int score;

		/// <summary>
		/// Construct an evaluated move
		/// </summary>
		/// <param name="m">The move that has been evaluated</param>
		/// <param name="s">The score associated with the move</param>
		public EvaluatedMove ( ChessMove m, int s )
		{
			move = m;
			score = s;
		}
	}
}
