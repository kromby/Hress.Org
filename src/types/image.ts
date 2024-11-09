export enum ImageContainer {
  Other = 0,
  Hardhead = 1,
  Profile = 2,
  News = 3,
  Album = 4,
  ATVR = 5,
}

export interface ImageEntity {
  id: number;
  name: string;
  photoUrl: string;
  photoThumbUrl?: string;
  content?: string;
  container: ImageContainer;
}
