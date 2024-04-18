using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Hardhead.Entities;

public class Nomination : EntityBase<string>
{
    public Nomination(int typeID, int nomineeID, string description)
    {
        TypeID = typeID;
        Nominee = new UserBasicEntity() { ID = nomineeID };
        Description = description;
    }

    public int TypeID { get; set; }       
    
    public UserBasicEntity Nominee { get; set; }        

    public new string Description { get; set; }

    public string? AffectedRule { get; set; }

    public void Validate()
    {
        if (string.IsNullOrEmpty(Description))
            throw new ArgumentException("Description is required", nameof(Description));

        if (TypeID <= 0)
            throw new ArgumentException("TypeID must be larger then zero", nameof(TypeID));

        if (InsertedBy <= 0)
            throw new ArgumentException("CreatedBy must be larger then zero", nameof(InsertedBy));

        if (Nominee == null || Nominee.ID <= 0)
            throw new ArgumentNullException(nameof(Nominee));
    }
}
