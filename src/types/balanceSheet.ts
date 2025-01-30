import { UserBasicEntity } from "./userBasicEntity";

export interface Transaction {
  user: UserBasicEntity;
  amount: number;
  id: number;
  name: string;
  inserted: string;
  insertedString: string;
  insertedBy: number;
  deleted: string;
}

export interface BalanceSheet {
  userID: number;
  balance: number;
  transactions: Transaction[];
}
