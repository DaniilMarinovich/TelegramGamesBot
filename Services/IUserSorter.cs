using System.Collections.Generic;
using RandomTGBot.Models;


namespace RandomTGBot.Services
{
    internal interface IUserSorter
    {
        void Sort(List<User> users);
    }
}
