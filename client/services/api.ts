import { API_BASE_URL } from '../constants/config';
import type {
  Article,
  HomeFeed,
  MatchDetail,
  MatchRecap,
  MatchStatus,
  MatchSummary,
  Player,
  ThemeMode,
  UserProfile,
} from '../types';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
    ...options,
  });

  if (!response.ok) {
    const error = await response.text();
    throw new Error(error || `Request failed: ${response.status}`);
  }

  if (response.status === 204) return undefined as T;
  return response.json();
}

export const api = {
  getHomeFeed: () => request<HomeFeed>('/home'),
  getMatches: (status?: MatchStatus) =>
    request<MatchSummary[]>(status ? `/matches?status=${status}` : '/matches'),
  getMatch: (id: string) => request<MatchDetail>(`/matches/${id}`),
  getMatchRecap: (id: string) => request<MatchRecap>(`/matches/${id}/recap`),
  createMatch: (data: {
    title: string;
    player1Id: string;
    player2Id: string;
    setsToWin?: number;
    legsPerSet?: number;
    startingScore?: number;
    venue?: string;
    tournament?: string;
    startingPlayerId?: string;
  }) => request<MatchDetail>('/matches', { method: 'POST', body: JSON.stringify(data) }),
  updateMatch: (id: string, data: { title?: string; venue?: string; tournament?: string; status?: MatchStatus }) =>
    request<MatchDetail>(`/matches/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  startMatch: (id: string) => request<MatchDetail>(`/matches/${id}/start`, { method: 'POST' }),
  recordVisit: (id: string, playerId: string, score: number) =>
    request<MatchDetail>(`/matches/${id}/visit`, {
      method: 'POST',
      body: JSON.stringify({ playerId, score }),
    }),
  deleteMatch: (id: string) => request<void>(`/matches/${id}`, { method: 'DELETE' }),
  getPlayers: (localOnly?: boolean) =>
    request<Player[]>(localOnly ? '/players?localOnly=true' : '/players'),
  createPlayer: (name: string, countryCode?: string) =>
    request<Player>('/players', { method: 'POST', body: JSON.stringify({ name, countryCode }) }),
  getProfile: () => request<UserProfile>('/profile'),
  updateProfile: (data: { name?: string; email?: string; theme?: ThemeMode; avatarUrl?: string }) =>
    request<UserProfile>('/profile', { method: 'PUT', body: JSON.stringify(data) }),
  getArticles: (featured?: boolean) =>
    request<Article[]>(featured ? '/articles?featured=true' : '/articles'),
  getArticle: (id: string) => request<Article>(`/articles/${id}`),
};
