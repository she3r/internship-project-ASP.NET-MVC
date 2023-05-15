namespace Paczki.Dto
{
    public class DeliveryDto
    {
        public string Name { get; set; }
        public decimal Weight { get; set; }
    }

    public class DeliveryDtoWithId
    {
        public string Name { get; set; }
        public decimal Weight { get; set; }
        public int Id { get; set; }
    }
}
