export interface ProfilePhoto {
  id: number;
  href: string | undefined | null;
}

export interface TransactionUser {
  username: string;
  name: string;
  href: string;
  profilePhoto: ProfilePhoto;
  id: number;
}

export interface Transaction {
  user: TransactionUser;
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
