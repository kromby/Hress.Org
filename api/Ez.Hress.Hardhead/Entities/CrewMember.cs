using Ez.Hress.Shared.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ez.Hress.Hardhead.Entities;

public class CrewMember: EntityBase<int>
{
    public CrewMember()
    {
        Name = string.Empty;    
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    [JsonConverter(typeof(StringEnumConverter))]
    public Role Role { get; set; } 

    public int MovieCounter { get; set; }
}

public enum Role
{
    Director,
    Writer,
    Actor
}
