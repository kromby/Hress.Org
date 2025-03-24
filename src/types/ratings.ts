export interface RatingEntity {
  code: string;
  name: string;
  myRating?: number;
  averageRating?: number;
  numberOfRatings?: number;
}

export interface RatingsResponse {
  ratings: RatingEntity[];
  readonly: boolean;
}
