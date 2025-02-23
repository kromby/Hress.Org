export interface HrefEntity {
  ID: number;
  Href: string;
}

export interface HardheadUser {
  ID: number;
  Name: string;
  Description?: string;
  Username?: string;
  ProfilePhoto?: HrefEntity;
  Href: string;
  Inserted: string;
  InsertedString: string;
  InsertedBy: number;
  Updated?: string;
  UpdatedBy?: number;
  Deleted?: string;
  Attended: number;
}
