using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
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

            Console.WriteLine( );
        }

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

    }
}