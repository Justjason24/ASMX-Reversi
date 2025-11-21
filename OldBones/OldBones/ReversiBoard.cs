using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
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

            // This shouldn't in theory ever occur since validation is also performed on the front-end
            // TODO - make this a method... and probably should do additional validation like looking at adjacent reversi pebbles
            // to determine if the move was valid.
            if (this.Board[MoveCol, MoveRow] != 'e')
                ErrorMessages.Add("ERROR: Invalid Move");

            this.Board[MoveRow, MoveCol] = Convert.ToChar(CurrentPlayerColor); // tack in current move to the board array

        }

        public void UpdateBoardPieces() // this either needs a name change or something because I plan on handling finding the eligable moves elsewhere
        {
            var coordinatesToChange = new List<Tuple<int, int>>();

            coordinatesToChange.AddRange(LookLeft());
            coordinatesToChange.AddRange(LookUp());
            coordinatesToChange.AddRange(LookRight());

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

        public void MarkEligableMoves()
        {
            //TODO - playing first move at [1,0] does not give correct eligable moves

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

            var eligableMoveCoordinates = new List<Tuple<int, int>>();



            //eligableMoveCoordinates.Add(LookRightForEligableMoves());
            //eligableMoveCoordinates.Add(LookDownForEligableMoves());

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
            foreach(var oppositePebblePosition in allOppositePebblesPositions)
            {
                eligableMoveCoordinates.Add(LookUpForEligableMoves(oppositePebblePosition.Item1, oppositePebblePosition.Item2));
            }

            eligableMoveCoordinates = eligableMoveCoordinates.Distinct().ToList();

           
            eligableMoveCoordinates = eligableMoveCoordinates.Where(x => x.Item1 != -1 && x.Item2 != -1).ToList();

            foreach(var coordinate in eligableMoveCoordinates)
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
            var oppositeColor = Convert.ToChar(CurrentPlayerColor) == 'w' ? 'b' : 'w';
            var startingPoint = MoveCol;
            var coordinatesToChange = new List<Tuple<int, int>>();

            if(MoveCol > 0) // dont need to look left if I'm already on the left side of the board. 
            {
                while (startingPoint >= 0 || startingPoint <= 4)
                {
                    startingPoint--;
                    if (Board[startingPoint, MoveRow] == oppositeColor)
                    {
                        coordinatesToChange.Add(new Tuple<int, int>(MoveRow, startingPoint));
                    }
                    else
                        break;
                }
            }


            return coordinatesToChange;
        }

        public List<Tuple<int, int>> LookUp()
        {
            var currentMove = Board[MoveRow, MoveCol];
            var oppositeColor = Convert.ToChar(CurrentPlayerColor) == 'w' ? 'b' : 'w';
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

        public List<Tuple<int, int>> LookRight()
        {

            var currentMove = Board[MoveRow, MoveCol];
            var oppositeColor = Convert.ToChar(CurrentPlayerColor) == 'w' ? 'b' : 'w';
            var startingPoint = MoveCol;
            var coordinatesToChange = new List<Tuple<int, int>>();

            if(MoveCol < 3) // dont need to look right if I'm already on the right side of the board. Also, dont use a magic number. 3 only works for 4x4 board
            {
                while (startingPoint >= 0 && startingPoint <= 4)
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
        public Tuple<int, int> LookRightForEligableMoves()
        {
            var eligableMoveCoordinates = new List<Tuple<int, int>>();

            var currentPieceRow = 1;
            var currentPieceColumn = 1;


            var startingPoint = currentPieceColumn; // this will need to change to use the column value of the pieces we're checking

            // NOTES
            // white just went - so I'm now looking at black pieces
            // a black pebble remains on 1,1. The elgiable piece would be 1,3

            if (Board[currentPieceRow, startingPoint + 1] != Convert.ToChar(CurrentPlayerColor))
            {
                return new Tuple<int, int>(-1, -1);
            }


            while (startingPoint < Math.Sqrt(this.Board.Length))
            {

                if (Board[currentPieceRow, startingPoint] == ' ')
                    return new Tuple<int, int>(currentPieceRow, startingPoint);

                startingPoint++;

            }

            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int> LookDownForEligableMoves()
        {
            var eligableMoveCoordinates = new List<Tuple<int, int>>();

            var currentPieceRow = 1;
            var currentPieceColumn = 1;


            var startingPoint = currentPieceRow; // this will need to change to use the column value of the pieces we're checking

            // NOTES
            // white just went - so I'm now looking at black pieces
            // a black pebble remains on 1,1. The elgiable piece would be 1,3

            if (Board[startingPoint + 1, currentPieceColumn] != Convert.ToChar(CurrentPlayerColor))
            {
                return new Tuple<int, int>(-1, -1);
            }


            while (startingPoint < Math.Sqrt(this.Board.Length))
            {

                if (Board[startingPoint, currentPieceColumn] == ' ')
                    return new Tuple<int, int>(startingPoint, currentPieceColumn);

                startingPoint++;

            }

            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int>LookUpForEligableMoves(int row, int column)
        {
            // I need to decrement the row index.
            // Say I'm starting at 2,2. I will then go to 1,2 and then 0,2

            // make sure that the piece after is opposite color
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


        #endregion

        public char GetOppositePlayerColor()
        {
            if (CurrentPlayerColor == "w")
                return 'b';
            else
                return 'w';

        }

    }
}