namespace ExportOrderEntites.Object.Dto
{
    public class ObjectDto
    {
        public Guid GuidId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string? NameEn { get; set; }
        public string? ShortName { get; set; }
        public string? Description { get; set; }
        public ObjectDto() { }
        public ObjectDto(string id, string name)
        {
            if (Guid.TryParse(id, out Guid guidId))
                GuidId = guidId;
            Id = id;
            Name = name;
        }
        public ObjectDto(Guid id, string name)
        {
            GuidId = id;
            Id = id.ToString();
            Name = name;
        }
        public ObjectDto(string name)
        {
            Name = name;
        }
        public ObjectDto(string id, string name, string? shortName = null, string? description = null)
        {
            if (Guid.TryParse(id, out Guid guidId))
                GuidId = guidId;
            Id = id;
            Name = name;
            ShortName = shortName;
            Description = description;
        }
        public ObjectDto(Guid id, string name, string shortName, string? nameEn = null)
        {
            GuidId = id;
            Id = id.ToString();
            Name = name;
            ShortName = shortName;
            NameEn = nameEn;
        }

    }
}
