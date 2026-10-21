export interface Card {
  id: string;
  scryfallId: string;
  name: string;
  manaCost?: string;
  cmc?: number;
  typeLine: string;
  oracleText?: string;
  power?: string;
  toughness?: string;
  colors: string[];
  rarity: string;
  imageUrlNormal?: string;
  imageUrlArtCrop?: string;
}

export interface SynergyContribution {
  ruleName: string;
  score: number;
  reason: string;
  category: string;
}

export interface SynergyMatch {
  card: Card;
  totalScore: number;
  reasons: string[];
  contributions: SynergyContribution[];
}

export interface SynergyResponse {
  sourceCard: Card;
  totalMatches: number;
  synergies: SynergyMatch[];
}
