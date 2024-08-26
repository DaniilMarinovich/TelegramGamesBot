using System.Collections.Generic;
using RandomTGBot.Models;

namespace RandomTGBot.Services
{
    internal class UserSorter : IUserSorter
    {
        public void Sort(List<User> users)
        {
            int n = users.Count;
            int gap = n / 2;
            while (gap > 0)
            {
                for (int i = gap; i < n; i++)
                {
                    User temp = users[i];
                    int j;
                    for (j = i; j >= gap && users[j - gap].ScoreRiddles < temp.ScoreRiddles; j -= gap)
                    {
                        users[j] = users[j - gap];
                    }
                    users[j] = temp;
                }
                gap /= 2;
            }
        }
    }
}
