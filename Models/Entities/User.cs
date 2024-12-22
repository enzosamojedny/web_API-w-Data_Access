namespace Models.Entities
{
    public class User
    {
        public int? ID { get; set; }
        public string Nombre { get; set; }
        public int Edad { get; set; } //tinyInt
        public int DNI { get; set; }
        public string Email { get; set; }
        public bool? Deleted { get; set; } = false;

        public string Password { get; set; }
        public Rol Rol { get; set; }

}
}
