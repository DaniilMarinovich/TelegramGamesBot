using RandomTGBot.Models;
using System.Collections.Generic;

namespace RandomTGBot.Services
{
    internal interface IPrintUsers
    {
        string Print(List<User> users);
    }
}
