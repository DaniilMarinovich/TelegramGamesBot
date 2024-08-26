using RandomTGBot.Models;
using System.Collections.Generic;


namespace RandomTGBot.Services
{
    internal class PrintUsers : IPrintUsers
    {
        public string Print(List<User> users)
        {
            string message = "Топ умников:\n\n";

            int i = 0;

            foreach (var user in users)
            {
                ++i;
                if (i == 11)
                {
                    break;
                }

                if (user.Name == "")
                {
                    message += $"{i}. {user.Name} – {user.ScoreRiddles} ⭐️\n";
                }
                else
                {
                    message += $"{i}. {user.Name} (@{user.UserName}) – {user.ScoreRiddles} ⭐️\n";
                }
            }

            return message;
        }
    }
}
