﻿namespace Ez.Hress.Shared.Entities;

public class VoteEntity : EntityBase<Guid>
{
    public VoteEntity(Guid id, int stepID, string valueID)
    {
        ID = id;
        StepID = stepID;    
        Value = valueID;

        Inserted = DateTime.UtcNow;
    }

    /// <summary>
    /// Unique identifier for a category (e.g. new rule, stallone, hardhead of the year)
    /// </summary>
    public int StepID { get; set; }

    public string Value { get; set; }

    public void Validate()
    {

        if (StepID <= 0)
            throw new ArgumentException("Must be greater than zero.", nameof(StepID));

        if (string.IsNullOrWhiteSpace(Value))
            throw new ArgumentException("Can not be null or empty.", nameof(Value));
    }
}
