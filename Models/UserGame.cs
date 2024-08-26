using System;

namespace RandomTGBot.Models
{
    public class UserGame
    {
        public long Id { get; set; }
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public string? Achievements { get; set; } = String.Empty;
        public int Damage { get; set; } = 10;
        public int MaxHp { get; set; } = 3;
        public int DamageLvl { get; set; } = 1;
        public int HpLvl { get; set; } = 1;
        public int Money { get; set; } = 0;


        public UserGame(long id, int level, int experience, string? achievements, int damage, int maxHp, int damageLvl, int hpLvl, int money)
        {
            Id = id;
            Level = level;
            Experience = experience;
            Achievements = achievements;
            Damage = damage;
            MaxHp = maxHp;
            DamageLvl = damageLvl;
            HpLvl = hpLvl;
            Money = money;
        }
    }
}
