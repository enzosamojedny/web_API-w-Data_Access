namespace Models
{
    public class User
    {
        public int? ID { get; set; }
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public string Email { get; set; }
        public bool? Deleted { get; set; } = false;
    }
}
