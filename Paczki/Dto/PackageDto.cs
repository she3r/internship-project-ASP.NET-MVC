namespace Paczki.Dto
{
    public class PackageDtoWithId
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsOpened { get; set; }
        public string DestinationCity { get; set; }

    }

    public class PackageDtoTest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsOpened { get; set; }
        public string DestinationCity { get; set; }

    }

    public class PackageDtoUpdateOpen
    {
        public int Id { get; set; }
        public bool IsOpened { get; set; }
    }
}
//this.Name = name;
//this.City = city;
//this.TempId = ID;