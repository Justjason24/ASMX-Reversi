using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;

namespace OldBones
{
    public class Helper
    {
        public static char[,] FillBoardArray(string boardString)
        {
            var boardArray = boardString.Split(',').ToArray();
            int boardWidth = Convert.ToInt32(Math.Sqrt(boardArray.Length));

            var boardData = new char[boardWidth, boardWidth];

            int counter = 0;
            for(int i = 0; i < boardWidth; i++)
            {
                for(int j = 0; j < boardWidth; j++)
                {
                    boardData[i, j] = Convert.ToChar(boardArray[counter++]);
                }
            }
            

            return boardData;
        }
    }
}