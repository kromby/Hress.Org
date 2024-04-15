using Ez.Hress.Shared.Entities;

namespace Ez.Hress.MajorEvents.Entities;

public class PartyTeam : EntityBase<int>
{
    public PartyTeam(int id, int number, bool isWinner)
    {
        ID = id;
        Number = number;
        IsWinner = isWinner;

        Members = new List<PartyUser>();
        QuizQuestions = new List<PartyQuiz>();
    }

    public int Number { get; set; }

    public string? Wine { get; set; }

    public bool IsWinner { get; set; }

    public IList<PartyUser> Members { get; set; }

    public bool HasQuiz { get; set; }

    public IList<PartyQuiz> QuizQuestions { get; set; }
}
