export type NewsEntity = {
  id: number;
  name: string;
  inserted: Date;
  insertedBy?: number;
  insertedString: string;
  author: any;
  image: { id?: number; height: number; href: string };
  imageAlign: number;
  content: string;
  hits?: number;
  public?: boolean;
};
