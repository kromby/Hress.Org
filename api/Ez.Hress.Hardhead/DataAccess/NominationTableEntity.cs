﻿using Azure;
using Azure.Data.Tables;
using Ez.Hress.Hardhead.Entities;

namespace Ez.Hress.Hardhead.DataAccess;

internal class NominationTableEntity : ITableEntity
{
    public NominationTableEntity()
    {
        PartitionKey = string.Empty;
        RowKey = string.Empty;
        Description = string.Empty;
        NomineeName = string.Empty;
    }

    public NominationTableEntity(Nomination nomination)
    {
        PartitionKey = nomination.TypeID.ToString();
        RowKey = Guid.NewGuid().ToString();

        NomineeID = nomination.Nominee.ID;
        NomineeName = nomination.Nominee.Name ?? string.Empty;
        if (nomination.Nominee.ProfilePhoto != null)
        {
            NomineeImageID = nomination.Nominee.ProfilePhoto.ID > 0 ? nomination.Nominee.ProfilePhoto.ID : null;
        }
        Description = nomination.Description;
        CreatedBy = nomination.InsertedBy;
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public int NomineeID { get; set; }
    public string NomineeName { get; set; }
    public int? NomineeImageID { get; set; }
    public string Description { get; set; }
    public int CreatedBy { get; set; }

    public string? AffectedRule { get; set; }
}
