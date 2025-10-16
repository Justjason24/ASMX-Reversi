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

            var x = boardData[2, 1];

            this.Board = boardData;

            // This shouldn't in theory ever occur since validation is also performed on the front-end
            // TODO - make this a method... and probably should do additional validation like looking at adjacent reversi pebbles
            // to determine if the move was valid.
            if (this.Board[MoveCol, MoveRow] != 'e')
                ErrorMessages.Add("ERROR: Invalid Move");

            this.Board[MoveCol, MoveRow] = Convert.ToChar(CurrentPlayerColor); // tack in current move to the board array

        }

        public void UpdateBoardPieces()
        {
            var coordinatesToChange = new List<Tuple<int, int>>();

            coordinatesToChange.AddRange(LookLeft());

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
        public List<Tuple<int, int>> LookLeft()
        {
            var currentMove = Board[MoveCol, MoveRow];
            var oppositeColor = Convert.ToChar(CurrentPlayerColor) == 'w' ? 'b' : 'w';
            var startingPoint = MoveCol;
            var coordinatesToChange = new List<Tuple<int, int>>();

            while(startingPoint >= 0 || startingPoint <= 4)
            {
                startingPoint--;
                if (Board[startingPoint, MoveRow] == oppositeColor)
                {
                    coordinatesToChange.Add(new Tuple<int, int>(startingPoint, MoveRow));
                }
                else
                    break;
            }

            return coordinatesToChange;
        }

        public List<Tuple<int, int>> LookUp()
        {
            var currentMove = Board[MoveCol, MoveRow];
            var oppositeColor = Convert.ToChar(CurrentPlayerColor) == 'w' ? 'b' : 'w';
            var startingPoint = MoveRow;
            var coordinatesToChange = new List<Tuple<int, int>>();

            while (startingPoint >= 0 || startingPoint <= 4)
            {
                startingPoint--; // decrement because going up.
                if (Board[MoveCol, startingPoint] == oppositeColor)
                {
                    coordinatesToChange.Add(new Tuple<int, int>(MoveCol, startingPoint));
                }
                else
                    break;
            }

            return coordinatesToChange;
        }


    }
}