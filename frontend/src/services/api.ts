import { Card, SynergyResponse } from '../types';

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
    this.name = 'ApiError';
  }
}

export const cardApi = {
  async search(query: string, setCode: string = 'vow', limit: number = 8): Promise<Card[]> {
    if (!query.trim() || query.length < 2) return [];

    const res = await fetch(`/api/cards/search?q=${encodeURIComponent(query)}&set=${encodeURIComponent(setCode)}&limit=${limit}`);
    if (!res.ok) {
      throw new ApiError(res.status, 'Failed to search cards');
    }
    return res.json();
  },

  async getById(id: string): Promise<Card> {
    const res = await fetch(`/api/cards/${id}`);
    if (!res.ok) {
      throw new ApiError(res.status, `Failed to fetch card with ID ${id}`);
    }
    return res.json();
  },

  async getSynergies(id: string, minScore: number = 20, limit: number = 30): Promise<SynergyResponse> {
    const res = await fetch(`/api/cards/${id}/synergies?minScore=${minScore}&limit=${limit}`);
    if (!res.ok) {
      throw new ApiError(res.status, 'Failed to fetch card synergies');
    }
    return res.json();
  }
};
