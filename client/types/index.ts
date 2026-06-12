export type MatchStatus = 'Scheduled' | 'InProgress' | 'Completed' | 'Cancelled';
export type ThemeMode = 'Light' | 'Dark' | 'System';
export type ArticleCategory = 'News' | 'Tournament' | 'Player' | 'Tips' | 'Featured';
export type SuggestionType = 'Practice' | 'Strategy' | 'Equipment' | 'Tournament';

export interface Player {
  id: string;
  name: string;
  countryCode?: string;
  ranking?: number;
  average: number;
  total180s: number;
  isLocal: boolean;
}

export interface Visit {
  id: string;
  playerId: string;
  playerName: string;
  score: number;
  remainingAfter: number;
  isBust: boolean;
  isCheckout: boolean;
  is180: boolean;
  visitNumber: number;
  createdAt: string;
}

export interface Leg {
  id: string;
  legNumber: number;
  player1Remaining: number;
  player2Remaining: number;
  currentPlayerId?: string;
  winnerId?: string;
  isComplete: boolean;
  player1DartsThrown: number;
  player2DartsThrown: number;
  visits: Visit[];
}

export interface Set {
  id: string;
  setNumber: number;
  player1Legs: number;
  player2Legs: number;
  winnerId?: string;
  isComplete: boolean;
  legs: Leg[];
}

export interface MatchSummary {
  id: string;
  title: string;
  player1Name: string;
  player2Name: string;
  player1Sets: number;
  player2Sets: number;
  status: MatchStatus;
  tournament?: string;
  venue?: string;
  startedAt?: string;
  completedAt?: string;
  winnerName?: string;
}

export interface MatchDetail extends MatchSummary {
  player1: Player;
  player2: Player;
  setsToWin: number;
  legsPerSet: number;
  startingScore: number;
  winnerId?: string;
  currentLegId?: string;
  startingPlayerId?: string;
  sets: Set[];
}

export interface Article {
  id: string;
  title: string;
  summary: string;
  content: string;
  imageUrl?: string;
  category: ArticleCategory;
  isFeatured: boolean;
  author?: string;
  readTimeMinutes: number;
  publishedAt: string;
}

export interface Suggestion {
  id: string;
  title: string;
  description: string;
  type: SuggestionType;
  icon?: string;
  priority: number;
}

export interface Featured {
  id: string;
  title: string;
  subtitle: string;
  imageUrl?: string;
  badge?: string;
  sortOrder: number;
  matchId?: string;
}

export interface HomeFeed {
  recentScores: MatchSummary[];
  articles: Article[];
  featuredArticles: Article[];
  suggestions: Suggestion[];
  featured: Featured[];
}

export interface UserProfile {
  id: string;
  name: string;
  email: string;
  theme: ThemeMode;
  avatarUrl?: string;
  total180s: number;
  matchesPlayed: number;
  matchesWon: number;
  averageScore: number;
}

export interface MatchRecap {
  id: string;
  title: string;
  player1: Player;
  player2: Player;
  player1Sets: number;
  player2Sets: number;
  winnerName?: string;
  tournament?: string;
  venue?: string;
  startedAt?: string;
  completedAt?: string;
  total180s: number;
  totalVisits: number;
  player1Average: number;
  player2Average: number;
  sets: Set[];
}
