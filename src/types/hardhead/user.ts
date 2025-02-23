import { HrefEntity } from "../hrefEntity";

export interface HardheadUser {
  id: number;
  name: string;
  description?: string;
  username?: string;
  profilePhoto?: HrefEntity;
  href: string;
  inserted: string;
  insertedString: string;
  insertedBy: number;
  updated?: string;
  updatedBy?: number;
  deleted?: string;
  attended: number;
}
