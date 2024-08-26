namespace RandomTGBot.Models
{
    public class DuelContext
    {
        public long ChatId { get; }
        public int MessageId { get; set; }  // Make MessageId writable
        public Duel Duelist1 { get; }
        public Duel Duelist2 { get; }
        public string Name1 { get; }
        public string Name2 { get; }
        public int Round { get; set; }

        public bool IsTurnReady => Duelist1.HasChosenAttackAction && Duelist2.HasChosenAttackAction && Duelist1.HasChosenDefenceAction && Duelist2.HasChosenDefenceAction;

        public DuelContext(long chatId, Duel duelist1, Duel duelist2, int messageId, string name1, string name2, int round)
        {
            ChatId = chatId;
            MessageId = messageId;
            Duelist1 = duelist1;
            Duelist2 = duelist2;
            Name1 = name1;
            Name2 = name2;
            Round = round;
        }

        public void HandleAttack(long userId, string attackPart)
        {
            var duelist = GetDuelist(userId);
            duelist.AttackPart = attackPart;
            duelist.HasChosenAttackAction = true;
        }

        public void HandleDefense(long userId, string defensePart)
        {
            var duelist = GetDuelist(userId);
            duelist.DefensePart = defensePart;
            duelist.HasChosenDefenceAction = true;
        }

        public Duel GetDuelist(long userId)
        {
            return Duelist1.UserGame.Id == userId ? Duelist1 : Duelist2;
        }

        public void ResetActions()
        {
            Duelist1.HasChosenAttackAction = false;
            Duelist1.HasChosenDefenceAction = false;
            Duelist2.HasChosenAttackAction = false;
            Duelist2.HasChosenDefenceAction = false;
        }
    }
}
