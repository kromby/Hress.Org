import { EntityBase } from "./entityBase";

export interface CrewMember {
  name: string;
  role: string;
}

export interface MovieInfo extends EntityBase<number> {
  year: number;
  rated: string;
  released: string; // ISO date string
  dvdReleased?: string; // ISO date string
  age: number;
  runtime: number;
  genre: string[];
  crew: CrewMember[];
  language: string[];
  country: string;
  awards?: string;
  ratings: Record<string, string>; // Dictionary of rating source to rating value
  metascore?: string;
  imdbRating: number;
  imdbVotes: number;
  imdbID?: string;
  boxOffice?: string;
  production?: string;
  website?: string;
}
