using System.Collections.Generic;

namespace Kurs_Dzudo.Hardik.Connector.Date;
public class Group
{
    public string Name { get; set; }
    public List<UkhasnikiDao> Participants { get; set; }
    public List<Match> Matches { get; set; }

    public Group(string name, List<UkhasnikiDao> participants)
    {
        Name = name;
        Participants = participants;
        Matches = GenerateMatches();
    }

    private List<Match> GenerateMatches()
    {
        var matches = new List<Match>();

        for (int i = 0; i < Participants.Count; i++)
        {
            for (int j = i + 1; j < Participants.Count; j++)
            {
                matches.Add(new Match(
                    Participants[i],
                    Participants[j]));
            }
        }

        return matches;
    }
}