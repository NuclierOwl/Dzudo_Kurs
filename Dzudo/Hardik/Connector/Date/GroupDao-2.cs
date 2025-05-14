using Kurs_Dzudo.Hardik.Connector.Date;
using System.Collections.Generic;

namespace Dzudo.Hardik.Connector.Date
{
    public class GroupDao_2
    {
        public int Id { get; set; }
        public int? TatamiId { get; set; }
        public string AgeCategory { get; set; }
        public string WeightCategory { get; set; }
        public char Gender { get; set; }
        public bool IsCompleted { get; set; } = false;


        public Tatami Tatami { get; set; }
        public List<UkhasnikiDao> Participants { get; set; } = new List<UkhasnikiDao>();
        public List<Match> Matches { get; set; } = new List<Match>();

        public GroupDao_2(string ageCategory, string weightCategory, char gender)
        {
            AgeCategory = ageCategory;
            WeightCategory = weightCategory;
            Gender = gender;
        }
    }
}
