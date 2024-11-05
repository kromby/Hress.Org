export type NewsEntity = {
  id: number;
  name: string;
  inserted: Date;
  insertedString: string;
  author: any;
  image: { height: number; href: string };
  imageAlign: number;
  content: string;
};
