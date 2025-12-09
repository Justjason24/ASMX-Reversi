using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web;
using System.Web.Services;

namespace OldBones
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    //[WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        public WebService1()
        {

            // Add CORS headers for every request
            //HttpContext context = HttpContext.Current;
            //if (context != null)
            //{
            //    //context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            //    context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
            //    context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");

            //    // Handle OPTIONS request
            //    if (context.Request.HttpMethod == "OPTIONS")
            //    {
            //        context.Response.StatusCode = 200;
            //        context.Response.End();
            //    }
            //}

        }


        [WebMethod]
        public string HelloWorld()
        {    
            return "Hello World. I responded from an ASMX application! ";
        }

        [WebMethod]
        public ReversiBoard Debug()
        {
            var debugBoard = new ReversiBoard()
            {
                BoardString = " , , , ,e, , , , ,e, ,e,w,b, , ,e,w,w,w,b, , , , , , ,w,w, , , , , ,e,w,w, , , , , ,e,w,w,b, , , , , ,w,w, ,b, , , , ,e,e, , ,b,",
                CurrentPlayerColor = "b",
                MoveCol = 3,
                MoveRow = 6,
            };

            debugBoard.FillBoardArray();

            return debugBoard; 
        }

        [WebMethod]
        public string AddA(string name)
        {
            HttpContext context = HttpContext.Current;
            return name + "A";
        }

        [WebMethod]
        public bool EnterPerson(Person person)
        {
            if (String.IsNullOrEmpty(person.Name) || person.Age < 0)
                return false;
            return true;
        }

        [WebMethod]
        public ReversiBoard ReversiNextMove(ReversiBoard board)
        {
            board.FillBoardArray();

            if (board.MoveRow > -1 && board.MoveCol > -1)
                board.UpdateBoardPieces();

            board.MarkEligibleMoves();
            board.StringifyBoard();
            board.SetOppositePlayerColor();

            return board;
        }
    }
}
