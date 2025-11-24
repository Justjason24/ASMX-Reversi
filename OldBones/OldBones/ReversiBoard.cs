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

            if (this.Board[MoveRow, MoveCol] != 'e')
                ErrorMessages.Add("ERROR: Invalid Move");

            this.Board[MoveRow, MoveCol] = Convert.ToChar(CurrentPlayerColor); // tack in current move to the board array

        }

        public void UpdateBoardPieces() // this either needs a name change or something because I plan on handling finding the eligable moves elsewhere
        {
            var coordinatesToChange = new List<Tuple<int, int>>();

            coordinatesToChange.AddRange(LookLeft());
            coordinatesToChange.AddRange(LookUp());
            coordinatesToChange.AddRange(LookRight());
            coordinatesToChange.AddRange(LookDown());

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
            var eligibleMoveCoordinates = new List<Tuple<int, int>>();
            foreach (var oppositePebblePosition in allOppositePebblesPositions)
            {
                eligibleMoveCoordinates.Add(LookUpForEligibleMoves(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
                eligibleMoveCoordinates.Add(LookLeftForEligibleMove(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
                eligibleMoveCoordinates.Add(LookDownForEligibleMoves(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
                eligibleMoveCoordinates.Add(LookRightForEligibleMoves(oppositePebblePosition.Item1, oppositePebblePosition.Item2));

                eligibleMoveCoordinates.Add(LookTopLeftForEligibleMoves(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
                eligibleMoveCoordinates.Add(LookDownRightForEligibleMoves(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
                eligibleMoveCoordinates.Add(LookDownLeftForEligibleMoves(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
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

            //if(MoveCol > 0) // dont need to look left if I'm already on the left side of the board. 
            //{
            //    while (startingPoint >= 0 || startingPoint <= 4)
            //    {
            //        startingPoint--;
            //        if (Board[MoveRow, startingPoint] == oppositeColor)
            //        {
            //            coordinatesToChange.Add(new Tuple<int, int>(MoveRow, startingPoint));
            //        }
            //        if (Board[MoveRow, startingPoint] == Convert.ToChar(CurrentPlayerColor))
            //        {
            //            return coordinatesToChange;
            //        }
                    
            //    }
            //}

            if(MoveCol - 1 < 0)
                return coordinatesToChange; // maybe return new List<Tuple<int, in>> for clarity that it's empty

            if (Board[MoveRow, MoveCol - 1] == ' ' || Board[MoveRow, MoveCol - 1] == 'e')
                return coordinatesToChange;

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

            if(MoveRow > 0)
            {
                while (startingPoint >= 0 || startingPoint <= 4)
                {
                    startingPoint--; // decrement because going up.
                    if (Board[startingPoint, MoveCol] == oppositeColor)
                    {
                        coordinatesToChange.Add(new Tuple<int, int>(startingPoint, MoveCol));
                    }
                    else
                        break;
                }
            }


            return coordinatesToChange;
        }

        public List<Tuple<int, int>> LookDown()
        {
            var currentMove = Board[MoveRow, MoveCol];
            var oppositeColor = GetOppositePlayerColor();
            var startingPoint = MoveRow;
            var boardLength = GetBoardSideLength();
            var coordinatesToChange = new List<Tuple<int, int>>();

            if(MoveRow < boardLength - 1)
            {
                while(startingPoint >= 0 && startingPoint < boardLength)
                {
                    startingPoint++;
                    if (Board[startingPoint, MoveCol] == oppositeColor)
                    {
                        coordinatesToChange.Add(new Tuple<int, int>(startingPoint, MoveCol));
                    }
                    else
                        break;
                }
            }

            return coordinatesToChange;
        }

        public List<Tuple<int, int>> LookRight()
        {
            var currentMove = Board[MoveRow, MoveCol];
            var oppositeColor = GetOppositePlayerColor();
            var startingPoint = MoveCol;
            var boardLength = GetBoardSideLength();
            var coordinatesToChange = new List<Tuple<int, int>>();

            if(MoveCol < boardLength - 1) 
            {
                while (startingPoint >= 0 && startingPoint <= boardLength)
                {
                    startingPoint++;
                    if (Board[MoveRow, startingPoint] == oppositeColor)
                    {
                        coordinatesToChange.Add(new Tuple<int, int>(MoveRow, startingPoint));
                    }
                    else
                        break;
                }
            }
            return coordinatesToChange;
        }
        #endregion

        #region Finding elible moves
        public Tuple<int, int> LookRightForEligibleMoves(int row, int column)
        {
            var boardLength = Math.Sqrt(this.Board.Length);

            // NOTES: We check if the square we're about to check is in bound. Since I'm about to do a comparison I need to check so I don't get an OOB error.
            if(column + 1 >= boardLength)
                return new Tuple<int, int>(-1, -1);

            // NOTES: At this point, all of the current's player colors pebbles have been set. Therefore, we are now looking at eligible moves for the opposite colors.
            // NOTES cont: That is why we see if the piece next to the opposite pebble is the current color so we can mark the space past it eligible.
            if (Board[row, column + 1] != Convert.ToChar(CurrentPlayerColor))
            {
                return new Tuple<int, int>(-1, -1);
            }

            while(column < boardLength)
            {
                if (Board[row, column] == ' ')
                    return new Tuple<int, int>(row, column);

                column++;
            }


            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int> LookUpForEligibleMoves(int row, int column)
        {
            if (row - 1 < 0)
                return new Tuple<int, int>(-1, -1);

            if (Board[row - 1, column] != Convert.ToChar(CurrentPlayerColor))
            {
                return new Tuple<int, int>(-1, -1);
            }

            while(row >= 0)
            {
                if (Board[row, column] == ' ')
                    return new Tuple<int, int>(row, column);

                row--;
            }

            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int> LookDownForEligibleMoves(int row, int column)
        {
            var boardLength = Math.Sqrt(this.Board.Length); 

            if(row + 1 >= boardLength)
                return new Tuple<int, int>(-1, -1);

            if (Board[row + 1, column] != Convert.ToChar(CurrentPlayerColor))
            {
                return new Tuple<int, int>(-1, -1);
            }

            while(row < boardLength)
            {
                if (Board[row, column] == ' ')
                    return new Tuple<int, int>(row, column);

                row++;
            }

            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int> LookLeftForEligibleMove(int row, int column)
        {
            if(column - 1 < 0)
                return new Tuple<int, int>(-1, -1);

            if (Board[row, column - 1] != Convert.ToChar(CurrentPlayerColor))
            {
                return new Tuple<int, int>(-1, -1);
            }

            while (column >= 0)
            {
                if (Board[row, column] == ' ')
                    return new Tuple<int, int>(row, column);

                column--;
            }

            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int> LookTopLeftForEligibleMoves(int row, int column)
        {
            // Need to make sure we're within bound so we don't access an invalid coordinate in the Board matrix
            if (row - 1 < 0 || column - 1 < 0)
                return new Tuple<int, int>(-1, -1);

            if (Board[row - 1, column - 1] != Convert.ToChar(CurrentPlayerColor))
            {
                return new Tuple<int, int>(-1, -1);
            }

            while (row >= 0 && column >= 0)
            {
                if (Board[row, column] == ' ')
                    return new Tuple<int, int>(row, column);

                row--;
                column--;
            }

            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int>LookDownRightForEligibleMoves(int row, int column)
        {
            var boardLength = GetBoardSideLength();

            if (row + 1 >= boardLength || column + 1 >= boardLength)
                return new Tuple<int, int>(-1, -1);

            if (Board[row + 1, column + 1] != Convert.ToChar(CurrentPlayerColor))
            {
                return new Tuple<int, int>(-1, -1);
            }

            while(row < boardLength && column < boardLength)
            {
                if (Board[row, column] == ' ')
                    return new Tuple<int, int>(row, column);

                row++;
                column++;
                   
            }

            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int> LookDownLeftForEligibleMoves(int row, int column)
        {
            //row increase, column decreaes for bottom left
            var boardLength = GetBoardSideLength();

            if (row + 1 >= boardLength || column - 1 < 0)
                return new Tuple<int, int>(-1, -1);

            if (Board[row + 1, column - 1] != Convert.ToChar(CurrentPlayerColor))
                return new Tuple<int, int>(-1, -1);

            while(row < boardLength && column >= 0)
            {
                if (Board[row, column] == ' ')
                    return new Tuple<int, int>(row, column);

                row++;
                column--;
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

        public double GetBoardSideLength()
        {
            return Math.Sqrt(this.Board.Length);
        }
    }
}