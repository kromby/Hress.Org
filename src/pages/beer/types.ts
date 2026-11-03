export interface Season {
  id: string;
  name: string;
  typeID?: number;
  startDate: string;
  endDate?: string;
  isOpen: boolean;
}

export interface Beer {
  id: string;
  name: string;
  producer: string;
  // The backend omits default values, so a 0% beer arrives without this.
  alcoholPercentage?: number;
  country: string;
  vinbudinUrl?: string;
  registeredBy: number;
  registeredAt: string;
}

export interface BeerSummary {
  beer: Beer;
  // The current user's stars, absent if they haven't rated the beer.
  myRating?: number;
}

export interface Rating {
  seasonID: string;
  beerID: string;
  userID: number;
  stars: number;
  created: string;
  updated: string;
}
