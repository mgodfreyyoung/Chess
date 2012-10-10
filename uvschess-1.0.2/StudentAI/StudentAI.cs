using System;
using System.Collections.Generic;
using System.Text;
using UvsChess;
using System.IO;

namespace StudentAI
{
    public class StudentAI : IChessAI
    {
        //ChessMove myChosenMove = null;
        //ChessFlag myFlag = ChessFlag.Stalemate;
              //myChosenMove = new ChessMove(null, null);
              //myChosenMove.Flag = ChessFlag.Stalemate;
      

        ChessMove lastMove = null;
        ChessMove nextLastMove = null;
        int ifSameAs = 0;
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

        private void logBoard(ChessBoard board, ChessColor myColor, ChessMove move)
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
            writer.WriteLine("");
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
        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor)
        {
            
            //List<ChessMove> allMoves = GetAllMoves(board, myColor);
            //ChessMove myChosenMove = null;
            ChessMove bestMove = MiniMaxDecision(board, myColor, MAX_NUM_PLIES);
            //bestMove.Flag = ChessFlag.Check;
            //myChosenMove = allMoves[bestMove];

           // SetDecisionTree(allMoves, bestMove, board.Clone(), myColor);
  
            return bestMove;
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

            // make sure they didn't try to move one of our pieces
            if (IsOpponentPiece(piece, colorOfPlayerMoving))
                return false; // they tried to move our pieces! cheater!
                //return myFlag;
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
            {
                // TODO <Mike> Write this heuristic function

                //f(p) = 200(K-K')
                //+ 9(Q-Q')
                //+ 5(R-R')
                //+ 3(B-B' + N-N')
                //+ 1(P-P')
                ChessPiece piece = ChessPiece.Empty;

                int score = 0;
                if (myColor == ChessColor.White)
                {
                    for (int i = 0; i < ChessBoard.NumberOfRows; i++)
                        for (int j = 0; j < ChessBoard.NumberOfColumns; j++)
                            if (board[i, j] != ChessPiece.Empty)
                            {
                                piece = board[i, j];
                                if (piece == ChessPiece.BlackBishop || piece == ChessPiece.BlackKnight)
                                    score = score - 3000;
                                else if (piece == ChessPiece.BlackPawn)
                                    score = score - 1000;
                                else if (piece == ChessPiece.BlackRook)
                                    score = score - 5000;
                                else if (piece == ChessPiece.BlackQueen)
                                    score = score - 9000;
                                else if (piece == ChessPiece.BlackKing)
                                    score = score - 900000;
                                else if (piece == ChessPiece.WhiteBishop || piece == ChessPiece.WhiteKnight)
                                    score = score + 3000;
                                else if (piece == ChessPiece.WhitePawn)
                                    score = score + 1000;
                                else if (piece == ChessPiece.WhiteRook)
                                    score = score + 5000;
                                else if (piece == ChessPiece.WhiteQueen)
                                    score = score + 9000;
                                else if (piece == ChessPiece.WhiteKing)
                                    score = score + 900000;

                            }
                }

                else if (myColor == ChessColor.Black)
                {
                    for (int i = 0; i < ChessBoard.NumberOfRows; i++)
                        for (int j = 0; j < ChessBoard.NumberOfColumns; j++)
                            if (board[i, j] != ChessPiece.Empty)
                            {
                                piece = board[i, j];
                                if (piece == ChessPiece.BlackBishop || piece == ChessPiece.BlackBishop)
                                    score = score + 3000;
                                else if (piece == ChessPiece.BlackPawn)
                                    score = score + 1000;
                                else if (piece == ChessPiece.BlackRook)
                                    score = score + 5000;
                                else if (piece == ChessPiece.BlackQueen)
                                    score = score + 9000;
                                else if (piece == ChessPiece.BlackKing)
                                    score = score + 900000;
                                else if (piece == ChessPiece.WhiteBishop || piece == ChessPiece.WhiteBishop)
                                    score = score - 3000;
                                else if (piece == ChessPiece.WhitePawn)
                                    score = score - 1000;
                                else if (piece == ChessPiece.WhiteRook)
                                    score = score - 5000;
                                else if (piece == ChessPiece.WhiteQueen)
                                    score = score - 9000;
                                else if (piece == ChessPiece.WhiteKing)
                                    score = score - 900000;

                            }
                }

               Random random = new Random();
               //int randomNumber = random.Next(0, 100);
               if (score == 0 || ifSameAs == score)
                  score = random.Next(0, 1000);
               ifSameAs = score;
                                                     
               return score;
            }
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
            ChessBoard finalBoard = null;
            ChessBoard checkBoard = null;
            ChessColor opponentColor = (myColor == ChessColor.Black) ? ChessColor.White : ChessColor.Black;
            ChessMove bestMove = null;
            ChessMove checkMove = null;
            int bestValue = System.Int32.MinValue;
            int value;
            int checkValue = System.Int32.MinValue;
            int tmpValue;


            // peek ahead through all possible moves and find the best one
            foreach (ChessMove move in allPossibleMoves)
            {
                tempBoard = boardBeforeMove.Clone();
                tempBoard.MakeMove(move);
                value = MinValue(tempBoard, opponentColor, nPlies - 1);
                if (value > bestValue && nextLastMove != move)
                {
                    finalBoard = tempBoard.Clone();
                    bestValue = value;
                    bestMove = move;
                }
            }
            List<ChessMove> allPossibleMoves2 = GetAllPossibleMoves(ref finalBoard, myColor);
            foreach (ChessMove move in allPossibleMoves2)
            {
                tempBoard = finalBoard.Clone();
                tempBoard.MakeMove(move);
                tmpValue = Utility(tempBoard, myColor);
                if (checkValue < tmpValue)
                {
                    //checkBoard = tempBoard.Clone();
                    checkValue = tmpValue;
                    checkMove = move;
                }
            }    
            //finalBoard.MakeMove(bestMove);
            //checkValue = MaxValue(finalBoard, opponentColor, 1);
            if (checkValue >= 500000)
            {
                //finalBoard.MakeMove(bestMove);
                bestMove.Flag = ChessFlag.Checkmate;
                List<ChessMove> allPossibleMoves3 = GetAllPossibleMoves(ref finalBoard, opponentColor);
                foreach (ChessMove move in allPossibleMoves3)
                {
                    tempBoard = finalBoard.Clone();
                    tempBoard.MakeMove(move);
                    List<ChessMove> allPossibleMoves4 = GetAllPossibleMoves(ref finalBoard, opponentColor);
                    //if (allPossibleMoves4.Find(move))
                    tmpValue = Utility(tempBoard, opponentColor);
                    if (tmpValue >=-500000)
                    {
                        bestMove.Flag = ChessFlag.Check;
                    }
                }   
            }

            //AddAllPossibleMovesToDecisionTree(allPossibleMoves, bestMove, boardBeforeMove.Clone(), myColor);
            // bestMove.Flag = ChessFlag.Checkmate;
            //int test = 1000;
            //check for check here....
            //ChessPiece piece = ChessPiece.Empty;
            //piece = bestMove;
            //bestMove.Flag = ChessFlag.Stalemate;
            //lastMove = bestMove;

            //This loop makes sure stalemate does not take place
            if (nextLastMove == null)
            {
                if (lastMove == null)
                    lastMove = bestMove;
                else
                {
                    nextLastMove = lastMove;
                    lastMove = bestMove;
                }

            }
            else
            {
                nextLastMove = lastMove;
                lastMove = bestMove;
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
            int bestValue = System.Int32.MinValue;
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
                value = MinValue(tempBoard, opponentColor, nPlies - 1);
                //if (value == 0)
                    
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
            //DecisionTree dt = new DecisionTree(currentBoard);                     //come back to this and setup decision tree!!
            //SetDecisionTree(dt);
            if (myColor == ChessColor.Black)
            {
                newY = y + 1;
                if (newY < ChessBoard.NumberOfRows)
                {

                    newX = x + 1;
                    if (newX < ChessBoard.NumberOfColumns && IsOpponentPiece(currentBoard[newX, newY], myColor)) // capture diagonally forward and right
                    {
                        //currentBoard.RawBoard
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

                    if (newY < ChessBoard.NumberOfRows && currentBoard[x, newY] == ChessPiece.Empty && y == 1) // forward 2
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
                    if (newY >= 0 && currentBoard[x, newY] == ChessPiece.Empty && y == 6) // forward 2
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

			// lookup up & left
            newY = y - 1;
            for (newX = x - 1; newX >= 0 && newY >= 0; newX--, newY --)
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
                    break;
				}
			}

			// look up and right
            newY = y - 1;
			for ( newX = x + 1; newX < ChessBoard.NumberOfColumns && newY >= 0; newX++, newY-- )
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
					break;
				}
			}

			// look down and right
            newY = y + 1;
			for ( newX = x + 1; newX < ChessBoard.NumberOfColumns && newY < ChessBoard.NumberOfRows; newX++, newY++ )
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
					break;
				}
			}

			// look down and left
            newY = y + 1;
            for (newX = x - 1; newX >= 0 && newY < ChessBoard.NumberOfRows; newX--, newY++)
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
			for ( int newY = y + 1; newY < ChessBoard.NumberOfRows; newY++ )
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
