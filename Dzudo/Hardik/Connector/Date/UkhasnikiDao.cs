using System;

namespace Kurs_Dzudo.Hardik.Connector.Date
{
    public class UkhasnikiDao
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SecName { get; set; }
        public DateOnly DateSorevnovaniy { get; set; } = default;
        public string Club { get; set; }
        public string Adres { get; set; }
        public decimal Ves { get; set;}
    }
}
