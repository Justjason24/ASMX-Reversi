using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Web;

namespace OldBones
{
    public class ReversiBoard
    {
        public string CurrentPlayerColor { get; set; }
        public int MoveRow { get; set; }
        public int MoveCol { get; set; }
        public string BoardString { get; set; }
        private char[,] Board { get; set; }
        public List<String> ErrorMessages = new List<String>();


        /// <summary>
        ///     From our front-end we get a comma delimitted string of the board, but NOT inlcuding the current players move.
        ///     We get the current players move in the values MoveRow and MoveCol.
        ///     This method fills in our matrix from that board string and also tacks in the current move after filling in the board string. 
        ///     We also kick back an error if the selected move wasn't eligible
        /// </summary>
        public void FillBoardArray()
        {
            var boardArray = this.BoardString.Split(',').ToArray();
            int boardWidth = Convert.ToInt32(Math.Sqrt(boardArray.Length));

            var boardData = new char[boardWidth, boardWidth];

            int counter = 0;
            for (int i = 0; i < boardWidth; i++)
            {
                for (int j = 0; j < boardWidth; j++)
                {
                    boardData[i, j] = Convert.ToChar(boardArray[counter++]);
                }
            }

            this.Board = boardData;

            // this breaks with my new eligible moves button, worry about it later lol
            //if (this.Board[MoveRow, MoveCol] != 'e')
            //    ErrorMessages.Add("ERROR: Invalid Move");



        }

        public void UpdateBoardPieces() // this either needs a name change or something because I plan on handling finding the eligable moves elsewhere
        {
            this.Board[MoveRow, MoveCol] = Convert.ToChar(CurrentPlayerColor); // tack in current move to the board array

            var coordinatesToChange = new List<Tuple<int, int>>();

            var debugTuple = new Tuple<int, int>(-2, -3);

            coordinatesToChange.AddRange(LookLeft());
            if(coordinatesToChange.Contains(debugTuple))
                Console.WriteLine();
            
            coordinatesToChange.AddRange(LookUp());
            if(coordinatesToChange.Contains(debugTuple))
                Console.WriteLine();

            coordinatesToChange.AddRange(LookRight());
            if(coordinatesToChange.Contains(debugTuple))
                Console.WriteLine( );

            coordinatesToChange.AddRange(LookDown());
            if(coordinatesToChange.Contains(debugTuple))
                Console.WriteLine();

            coordinatesToChange.AddRange(LookTopLeft());
            if(coordinatesToChange.Contains(debugTuple))
                Console.WriteLine();

            coordinatesToChange.AddRange(LookDownRight());
            if (coordinatesToChange.Contains(debugTuple))
                Console.WriteLine();

            coordinatesToChange.AddRange(LookTopRight());
            if (coordinatesToChange.Contains(debugTuple))
                Console.WriteLine();

            coordinatesToChange.AddRange(LookDownLeft());
            if (coordinatesToChange.Contains(debugTuple))
                Console.WriteLine();

            coordinatesToChange = coordinatesToChange.Distinct().ToList();

            foreach(var coordinate in coordinatesToChange)
            {
                //if (Convert.ToChar(CurrentPlayerColor) == 'b')
                //    this.Board[coordinate.Item1, coordinate.Item2] = 'w';
                //else
                //    this.Board[coordinate.Item1, coordinate.Item2] = 'b';

                this.Board[coordinate.Item1, coordinate.Item2] = Convert.ToChar(CurrentPlayerColor);
            }

            Console.WriteLine();
        }

        public void MarkEligibleMoves()
        {
            // remove eligable moves from previous move so we can recalculate
            for(int i = 0; i < Math.Sqrt(this.Board.Length); i++)
            {
                for(int j = 0; j < Math.Sqrt(this.Board.Length); j++)
                {
                    if (Board[i,j] == 'e')
                    {
                        Board[i, j] = ' ';
                    }
                }
            }



            // Get all positions of opposite colors and add them to a list
            var allOppositePebblesPositions = new List<Tuple<int, int>>();
            var oppositeColor = GetOppositePlayerColor();
            for (int i = 0; i < Math.Sqrt(this.Board.Length); i++)
            {
                for (int j = 0; j < Math.Sqrt(this.Board.Length); j++)
                {
                    if (Board[i, j] == oppositeColor)
                    {
                        allOppositePebblesPositions.Add(new Tuple<int, int>(i, j));
                    }
                }
            }

            // iterate over all opposite player colors and look in all directions for eligable moves. 
            // TODO - optimize this. This could potentially be dozens of pebbles - each pebble calling 8 seek functions

            var debugTuple = new Tuple<int, int>(1, 3);

            var eligibleMoveCoordinates = new List<Tuple<int, int>>();
            foreach (var oppositePebblePosition in allOppositePebblesPositions)
            {
                eligibleMoveCoordinates.Add(LookUpForEligibleMoves(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
                if(eligibleMoveCoordinates.Contains(debugTuple))
                    Console.WriteLine();

                eligibleMoveCoordinates.Add(LookLeftForEligibleMove(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
                if (eligibleMoveCoordinates.Contains(debugTuple))
                    Console.WriteLine();

                eligibleMoveCoordinates.Add(LookDownForEligibleMoves(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
                if (eligibleMoveCoordinates.Contains(debugTuple))
                    Console.WriteLine();

                eligibleMoveCoordinates.Add(LookRightForEligibleMoves(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
                if (eligibleMoveCoordinates.Contains(debugTuple))
                    Console.WriteLine();

                eligibleMoveCoordinates.Add(LookTopLeftForEligibleMoves(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
                if (eligibleMoveCoordinates.Contains(debugTuple))
                    Console.WriteLine();

                eligibleMoveCoordinates.Add(LookDownRightForEligibleMoves(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
                if (eligibleMoveCoordinates.Contains(debugTuple))
                    Console.WriteLine();

                eligibleMoveCoordinates.Add(LookDownLeftForEligibleMoves(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
                if (eligibleMoveCoordinates.Contains(debugTuple))
                    Console.WriteLine();

                eligibleMoveCoordinates.Add(LookTopRightForEligibleMoves(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
                if (eligibleMoveCoordinates.Contains(debugTuple))
                    Console.WriteLine();
            }

            eligibleMoveCoordinates = eligibleMoveCoordinates.Distinct().ToList();

           
            eligibleMoveCoordinates = eligibleMoveCoordinates.Where(x => x.Item1 != -1 && x.Item2 != -1).ToList();

            foreach(var coordinate in eligibleMoveCoordinates)
            {
                this.Board[coordinate.Item1, coordinate.Item2] = 'e';
            }

            Console.WriteLine();
        }

        public void StringifyBoard()
        {
            var boardData = new StringBuilder();

            for(int i = 0; i < Math.Sqrt(this.Board.Length);i++)
            {
                for(int j = 0; j < Math.Sqrt(this.Board.Length); j++)
                {
                    boardData.Append($"{Board[i, j]},");
                }
            }

            this.BoardString = boardData.ToString();
        }


        // TODO
        // I'm going to make 8 methods to look in each direction. Need to factor this later. Easier for now.

        // TODO
        // Also appears at the surface level that there is a bug where when checking for coordinates to 'flip'. 
        // I'm not ensuring that the last peg is the current players color. Not going to be found until we go to 8x8 instead of the testing 4x4 board.

        #region Flipping pebbles after a move was made
        public List<Tuple<int, int>> LookLeft()
        {
            var currentMove = Board[MoveRow, MoveCol];
            var oppositeColor = GetOppositePlayerColor();
            var startingPoint = MoveCol;
            var coordinatesToChange = new List<Tuple<int, int>>();


            if(MoveCol - 1 < 0)
                return new List<Tuple<int, int>>(); 

            if (Board[MoveRow, MoveCol - 1] == ' ' || Board[MoveRow, MoveCol - 1] == 'e')
                return new List<Tuple<int, int>>();

            while(startingPoint > 0)
            {
                startingPoint--;

                if (Board[MoveRow, startingPoint] == oppositeColor)
                {
                    coordinatesToChange.Add(new Tuple<int, int>(MoveRow, startingPoint));
                }
                if (Board[MoveRow, startingPoint] == Convert.ToChar(CurrentPlayerColor))
                {
                    return coordinatesToChange;
                }
                    
            }


            return new List<Tuple<int, int>>();
        }

        public List<Tuple<int, int>> LookUp()
        {
            var currentMove = Board[MoveRow, MoveCol];
            var oppositeColor = GetOppositePlayerColor();
            var startingPoint = MoveRow;
            var coordinatesToChange = new List<Tuple<int, int>>();

            if (MoveRow - 1 < 0)
                return new List<Tuple<int, int>>();

            if (Board[MoveRow - 1, MoveCol] == ' ' || Board[MoveRow - 1, MoveCol] == 'e')
                return new List<Tuple<int, int>>();

            while (startingPoint > 0)
            {
                startingPoint--;

                if (Board[startingPoint, MoveCol] == oppositeColor)
                {
                    coordinatesToChange.Add(new Tuple<int, int>(startingPoint, MoveCol));
                }
                if (Board[startingPoint, MoveCol] == Convert.ToChar(CurrentPlayerColor))
                {
                    return coordinatesToChange;
                }
            }

            return new List<Tuple<int, int>>();
        }

        public List<Tuple<int, int>> LookDown()
        {
            var currentMove = Board[MoveRow, MoveCol];
            var oppositeColor = GetOppositePlayerColor();
            var startingPoint = MoveRow;
            var boardLength = GetBoardSideLength();
            var coordinatesToChange = new List<Tuple<int, int>>();

            if (MoveRow + 1 >= boardLength)
                return new List<Tuple<int, int>>();

            if (Board[MoveRow + 1, MoveCol] == ' ' || Board[MoveRow + 1, MoveCol] == 'e')
                return new List<Tuple<int, int>>();

            while(startingPoint < boardLength - 1)
            {
                startingPoint++;

                if (Board[startingPoint, MoveCol] == oppositeColor)
                {
                    coordinatesToChange.Add(new Tuple<int, int>(startingPoint, MoveCol));
                }
                if (Board[startingPoint, MoveCol] == Convert.ToChar(CurrentPlayerColor))
                {
                    return coordinatesToChange;
                }
            }

            return new List<Tuple<int, int>>();
        }

        public List<Tuple<int, int>> LookRight()
        {
            var currentMove = Board[MoveRow, MoveCol];
            var oppositeColor = GetOppositePlayerColor();
            var startingPoint = MoveCol;
            var boardLength = GetBoardSideLength();
            var coordinatesToChange = new List<Tuple<int, int>>();

            if (MoveCol + 1 >= boardLength)
                return new List<Tuple<int, int>>();

            if (Board[MoveRow, MoveCol + 1] == ' ' || Board[MoveRow, MoveCol + 1] == 'e')
                return new List<Tuple<int, int>>();

            while(startingPoint < boardLength - 1)
            {
                startingPoint++;

                if (Board[MoveRow, startingPoint] == oppositeColor)
                {
                    coordinatesToChange.Add(new Tuple<int, int>(MoveRow, startingPoint));
                }
                if (Board[MoveRow, startingPoint] == Convert.ToChar(CurrentPlayerColor))
                {
                    return coordinatesToChange;
                }

            }

            return new List<Tuple<int, int>>();
        }

        public List<Tuple<int, int>> LookTopLeft()
        {
            // both row and column decrease
            var oppositeColor = GetOppositePlayerColor();
            var coordinatesToChange = new List<Tuple<int, int>>();
            var boardLength = GetBoardSideLength();

            var row = MoveRow;
            var column = MoveCol;

            if (MoveRow - 1 < 0 || MoveCol - 1 < 0)
                return new List<Tuple<int, int>>();

            if (Board[MoveRow - 1, MoveCol - 1] == ' ' || Board[MoveRow - 1, MoveCol - 1] == 'e')
                return new List<Tuple<int, int>>();

            while (row > 0 && column > 0)
            {
                row--;
                column--;

                if (Board[row, column] == oppositeColor)
                {
                    coordinatesToChange.Add(new Tuple<int, int>(row, column));
                }
                if (Board[row, column] == Convert.ToChar(CurrentPlayerColor))
                {
                    return coordinatesToChange;
                }
            }

            return new List<Tuple<int, int>>();
        }

        public List<Tuple<int, int>> LookDownRight()
        {
            var oppositeColor = GetOppositePlayerColor();
            var coordinatesToChange = new List<Tuple<int, int>>();
            var boardLength = GetBoardSideLength();

            // move col and row col both increment when going bottom right
            var row = MoveRow;
            var column = MoveCol;

            if (MoveRow + 1 > boardLength - 1 || MoveCol + 1 > boardLength - 1)
                return new List<Tuple<int, int>>();

            if (Board[MoveRow + 1, MoveCol + 1] == ' ' || Board[MoveRow + 1, MoveCol + 1] == 'e')
                return new List<Tuple<int, int>>();

            while (row < boardLength - 1 && column < boardLength - 1)
            {
                row++;
                column++;

                if (Board[row, column] == oppositeColor)
                {
                    coordinatesToChange.Add(new Tuple<int, int>(row, column));
                }
                if (Board[row, column] == Convert.ToChar(CurrentPlayerColor))
                {
                    return coordinatesToChange;
                }
            }
            return new List<Tuple<int, int>>();

        }

        public List<Tuple<int, int>> LookTopRight()
        {
            // row decreases, column increases
            var oppositeColor = GetOppositePlayerColor();
            var coordinatesToChange = new List<Tuple<int, int>>();
            var boardLength = GetBoardSideLength();

            var row = MoveRow;
            var column = MoveCol;

            if (MoveRow - 1 < 0 || MoveCol + 1 > boardLength - 1)
                return new List<Tuple<int, int>>();

            if (Board[MoveRow - 1, MoveCol + 1] == ' ' || Board[MoveRow - 1, MoveCol + 1] == 'e')
                return new List<Tuple<int, int>>();

            while (row > 0 && column < boardLength - 1)
            {
                row--;
                column++;

                if (Board[row, column] == oppositeColor)
                {
                    coordinatesToChange.Add(new Tuple<int, int>(row, column));
                }
                if (Board[row, column] == Convert.ToChar(CurrentPlayerColor))
                {
                    return coordinatesToChange;
                }
            }
            return new List<Tuple<int, int>>();

        }

        public List<Tuple<int, int>> LookDownLeft()
        {
            // row increases and col decreases
            var oppositeColor = GetOppositePlayerColor();
            var coordinatesToChange = new List<Tuple<int, int>>();
            var boardLength = GetBoardSideLength();

            var row = MoveRow;
            var column = MoveCol;

            if (MoveRow + 1 > boardLength - 1 || MoveCol - 1 < 0)
                return new List<Tuple<int, int>>();

            if (Board[MoveRow + 1, MoveCol - 1] == ' ' || Board[MoveRow + 1, MoveCol - 1] == 'e')
                return new List<Tuple<int, int>>();

            while (row < boardLength - 1 && column > 0)
            {
                row++;
                column--;

                if (Board[row, column] == oppositeColor)
                {
                    coordinatesToChange.Add(new Tuple<int, int>(row, column));
                }
                if (Board[row, column] == Convert.ToChar(CurrentPlayerColor))
                {
                    return coordinatesToChange;
                }
            }

            return new List<Tuple<int, int>>();
        }
        #endregion

        #region Finding elible moves
        public Tuple<int, int> LookRightForEligibleMoves(int row, int column)
        {
            // col increases
            var boardLength = GetBoardSideLength();

            if (column + 1 >= boardLength)
                return new Tuple<int, int>(-1, -1);

            if (Board[row, column + 1] == ' ' || Board[row, column + 1] == 'e')
                return new Tuple<int, int>(-1, -1);

            column++;

            while(column < boardLength)
            {
                if (Board[row, column] == GetOppositePlayerColor())
                    return new Tuple<int, int>(-1, -1); //proven! leave this! (until it isn't and I end up changing it)

                if (Board[row, column] == Convert.ToChar(CurrentPlayerColor))
                {
                    column++;
                    continue;
                }

                if (Board[row, column] == ' ' || Board[row, column] == 'e')
                    return new Tuple<int, int>(row, column);
            }

            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int> LookUpForEligibleMoves(int row, int column)
        {
            // row decreases
            if (row - 1 < 0)
                return new Tuple<int, int>(-1, -1);

            if (Board[row - 1, column] == ' ' || Board[row - 1, column] == 'e')
                return new Tuple<int, int>(-1, -1);

            row--;

            while (row >= 0)
            {

                if (Board[row, column] == GetOppositePlayerColor())
                    return new Tuple<int, int>(-1, -1); //proven! leave this! (until it isn't and I end up changing it)

                if (Board[row, column] == Convert.ToChar(CurrentPlayerColor))
                {
                    row--;
                    continue;
                }

                if (Board[row, column] == ' ' || Board[row, column] == 'e')
                    return new Tuple<int, int>(row, column);


            }

            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int> LookDownForEligibleMoves(int row, int column)
        {
            // row increases
            var boardLength = GetBoardSideLength();

            if (row + 1 >= boardLength)
                return new Tuple<int, int>(-1, -1);

            if (Board[row + 1, column] == ' ' || Board[row + 1, column] == 'e')
                return new Tuple<int, int>(-1, -1);

            row++;

            while (row < boardLength)
            {
                if (Board[row, column] == GetOppositePlayerColor())
                    return new Tuple<int, int>(-1, -1); //proven! leave this! (until it isn't and I end up changing it)

                if (Board[row, column] == Convert.ToChar(CurrentPlayerColor))
                {
                    row++;
                    continue;
                }

                if (Board[row, column] == ' ' || Board[row, column] == 'e')
                    return new Tuple<int, int>(row, column);
            }

            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int> LookLeftForEligibleMove(int row, int column)
        {
            if (column - 1 < 0)
                return new Tuple<int, int>(-1, -1);

            if (Board[row, column - 1] == ' ' || Board[row, column - 1] == 'e')
                return new Tuple<int, int>(-1, -1);

            column--;

            while(column >= 0)
            {

                if (Board[row, column] == GetOppositePlayerColor())
                    return new Tuple<int, int>(-1, -1); //proven! leave this! (until it isn't and I end up changing it)

                if (Board[row, column] == Convert.ToChar(CurrentPlayerColor))
                {
                    column--;
                    continue;
                }

                if (Board[row, column] == ' ' || Board[row, column] == 'e')
                    return new Tuple<int, int>(row, column);


            }
            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int> LookTopLeftForEligibleMoves(int row, int column)
        {
            // row and col decrease
            var boardLength = GetBoardSideLength();

            if (row - 1 < 0 || column - 1 < 0)
                return new Tuple<int, int>(-1, -1);

            if (Board[row - 1, column - 1] == ' ' || Board[row - 1, column - 1] == 'e')
                return new Tuple<int, int>(-1, -1);

            row--;
            column--;

            while (row >= 0 && column >= 0)
            {
                if (Board[row, column] == GetOppositePlayerColor())
                    return new Tuple<int, int>(-1, -1);

                if (Board[row, column] == Convert.ToChar(CurrentPlayerColor))
                {
                    row--;
                    column--;
                    continue;
                }

                if (Board[row, column] == ' ' || Board[row, column] == 'e')
                    return new Tuple<int, int>(row, column);

            }


            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int>LookDownRightForEligibleMoves(int row, int column)
        {
            // row and col increase
            var boardLength = GetBoardSideLength();

            if (row + 1 >= boardLength || column + 1 >= boardLength)
                return new Tuple<int, int>(-1, -1);

            if (Board[row + 1, column + 1] == ' ' || Board[row + 1, column + 1] == 'e')
                return new Tuple<int, int>(-1, -1);

            row++;
            column++;

            while (row < boardLength && column < boardLength)
            {
                if (Board[row, column] == GetOppositePlayerColor())
                    return new Tuple<int, int>(-1, -1);

                if (Board[row, column] == Convert.ToChar(CurrentPlayerColor))
                {
                    row++;
                    column++;
                    continue;
                }

                if (Board[row, column] == ' ' || Board[row, column] == 'e')
                    return new Tuple<int, int>(row, column);

            }

            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int> LookDownLeftForEligibleMoves(int row, int column)
        {
            //row increase, column decreaes for bottom left
            var boardLength = GetBoardSideLength();

            if (row + 1 >= boardLength || column - 1 < 0)
                return new Tuple<int, int>(-1, -1);

            if (Board[row + 1, column - 1] == ' ' || Board[row + 1, column - 1] == 'e')
                return new Tuple<int, int>(-1, -1);

            row++;
            column--;

            while (row < boardLength && column >= 0)
            {
                if (Board[row, column] == GetOppositePlayerColor())
                    return new Tuple<int, int>(-1, -1);

                if (Board[row, column] == Convert.ToChar(CurrentPlayerColor))
                {
                    row++;
                    column--;
                    continue;
                }

                if (Board[row, column] == ' ' || Board[row, column] == 'e')
                    return new Tuple<int, int>(row, column);

            }

            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int> LookTopRightForEligibleMoves(int row, int column)
        {
            // row decreases and col increases
            var boardLength = GetBoardSideLength();

            if (row - 1 < 0 || column + 1 >= boardLength)
                return new Tuple<int, int>(-1, -1);

            if (Board[row - 1, column + 1] == ' ' || Board[row - 1, column + 1] == 'e')
                return new Tuple<int, int>(-1, -1);

            row--;
            column++;

            while (row >= 0 && column < boardLength)
            {
                if (Board[row, column] == GetOppositePlayerColor())
                    return new Tuple<int, int>(-1, -1);

                if (Board[row, column] == Convert.ToChar(CurrentPlayerColor))
                {
                    row--;
                    column++;
                    continue;
                }

                if (Board[row, column] == ' ' || Board[row, column] == 'e')
                    return new Tuple<int, int>(row, column);

            }



            return new Tuple<int, int>(-1, -1);
        }


        #endregion

        public char GetOppositePlayerColor()
        {
            if (CurrentPlayerColor == "w")
                return 'b';
            else
                return 'w';

        }

        public void SetOppositePlayerColor()
        {
            this.CurrentPlayerColor = GetOppositePlayerColor().ToString();
        }

        public double GetBoardSideLength()
        {
            return Math.Sqrt(this.Board.Length);
        }
    }
}