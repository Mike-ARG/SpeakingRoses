namespace SpeakingRoses.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public DateTime DueDate { get; set; }
        public Priority Priority { get; set; }
    }

    public enum Status
    {
        Pending = 0,
        Completed = 1
    }

    public enum Priority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }
}