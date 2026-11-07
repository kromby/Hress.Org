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
  // e.g. "Lager"; absent if not entered.
  style?: string;
  description?: string;
  // The cover photo in the hress.org image store, absent if it has none.
  imageID?: number;
  registeredBy: number;
  registeredAt: string;
}

export interface BeerSummary {
  beer: Beer;
  // The current user's stars, absent if they haven't rated the beer.
  myRating?: number;
  // How many members have rated it and how many chat messages there are.
  // The backend omits zeros, so absent means 0.
  ratingCount?: number;
  messageCount?: number;
}

export interface Rating {
  seasonID: string;
  beerID: string;
  userID: number;
  stars: number;
  created: string;
  updated: string;
}

// A message in a beer's chat. The text is plain text: render it as text, never as HTML.
export interface ChatMessage {
  id: number;
  userID: number;
  username: string;
  // The author's profile photo, absent if they have none.
  authorImageID?: number;
  text: string;
  created: string;
  // When the message was last edited, absent if never.
  edited?: string;
}
