namespace RandomTGBot.Models
{
    public class User
    {
        public long Id { get; set; }
        public string? UserName { get; set; }
        public int ScoreRiddles { get; set; } = 0;
        public int ScoreMinigames { get; set; } = 0;
        public int Attempts { get; set; } = 0;
        public string? Name { get; set; } = string.Empty;
        public int QuestStage { get; set; } = 0;

        public User(long id, string? userName, int scoreRiddles, int scoreMinigames, int attempts, string? name, int questStage)
        {
            Id = id;
            UserName = userName;
            ScoreRiddles = scoreRiddles;
            ScoreMinigames = scoreMinigames;
            Attempts = attempts;
            Name = name;
            QuestStage = questStage;
        }
    }
}
