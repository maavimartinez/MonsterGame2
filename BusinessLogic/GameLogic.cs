using System;
using System.Text.RegularExpressions;
using Entities;

namespace BusinessLogic
{
    public class GameLogic
    {
        private Board Board;

        private Game Game;

        private ActionLogic ActionLogic;

        public GameLogic(Board brd, Game gm)
        {
            Board = brd;
            ActionLogic = new ActionLogic(brd);
            Game = gm;
        }

        private void TranslateCommandToAction(Player player, string cmd){
            string action =  cmd.Substring(0, 2);
            string sndParameter = cmd.Substring(3,cmd.Length);
            try{
                if(action.Equals("MOV"))
                {
                    CheckCorrectMoveFormat(sndParameter);
                    ActionLogic.Move(player, sndParameter);
                }
                else if(action.Equals("ATT"))
                {
                    CheckDefenderForAttackExists(sndParameter);
                    Player defender = GetDefender(sndParameter);
                    ActionLogic.Attack(player, defender);
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void CheckCorrectMoveFormat(string move){
            char row    = move[0];
            char column = move[1];
            bool validRow = Regex.IsMatch(row.ToString(), "[a-h]", RegexOptions.IgnoreCase);
            bool validColumn = Regex.IsMatch(row.ToString(), "[1-8]"); //FIJARSE SI ESTA BIEN
            if(move.Length > 2 || !validRow || !validColumn) throw new Exception("Invalid format");
        }

        private void CheckDefenderForAttackExists(string username){
            int count = 0;
            foreach (Player pl in Game.Players)
                if(pl.User.Username.Equals(username)) count ++;
            if(count==0) throw new Exception("Non existent player");
        }

        private Player GetDefender(string username)
        {
            foreach (Player pl in Game.Players)
            {
                if(pl.User.Username.Equals(username))
                { 
                    return pl;
                }
            }
            return null;
        }

        //end game, show winner
    }

}