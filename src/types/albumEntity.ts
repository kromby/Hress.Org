import { HrefEntity } from "./hrefEntity";

export interface AlbumEntity {
  id: number;
  name: string;
  description: string;
  date: Date;
  imageCount?: number;
  insertedString: string;
  images: HrefEntity;
}
