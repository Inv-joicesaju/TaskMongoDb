namespace TaskMongoDb.Models
{
    public class TaskModel
    {
        public string Id { get;  set; } = Guid.NewGuid().ToString();
        public string Title { get;  set; }
        public string Description { get;  set; } = string.Empty;
    }
}
    