using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OldBones
{
    public class ReversiBoard
    {
        public string CurrentPlayerColor { get; set; }
        public int MoveRow { get; set; }
        public int MoveCol { get; set; }
        //public char[][] Board {  get; set; } BAD IDEA with how XML handles 2D arrays. 
        public string BoardString { get; set; }

        public int BoardWidth { get; set; } = 4; // this can change

        public ReversiBoard(ReversiBoard board)
        {
            
        }

    }
}