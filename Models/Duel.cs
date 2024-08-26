namespace RandomTGBot.Models
{
    public class Duel
    {
        public UserGame UserGame { get; }
        public int Damage { get; private set; }
        public int Hp { get; set; }
        public string AttackPart { get; set; }
        public string DefensePart { get; set; }
        public bool HasChosenAttackAction { get; set; }
        public bool HasChosenDefenceAction { get; set; }

        public Duel(UserGame userGame)
        {
            UserGame = userGame;
            Damage = userGame.Damage;
            Hp = userGame.MaxHp;
            HasChosenAttackAction = false;
            HasChosenDefenceAction = false;
        }
    }

}
