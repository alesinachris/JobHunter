namespace ParserLib
{
    public class Ad
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string SalaryString { get; set; }

        public decimal MinSalary { get; set; }

        public decimal MaxSalary { get; set; }

        public string Company { get; set; }

        public DateTime DateTime { get; set; }

        public override string ToString()
        {
            return $"{Id} - {Title}";
        }
    }
}
