import { useState, useCallback } from 'react';
import { Card, SynergyResponse } from '../types';
import { cardApi } from '../services/api';

export function useCardSynergies() {
  const [selectedCard, setSelectedCard] = useState<Card | null>(null);
  const [synergyData, setSynergyData] = useState<SynergyResponse | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const selectCard = useCallback(async (card: Card) => {
    setSelectedCard(card);
    setIsLoading(true);
    setError(null);

    try {
      const data = await cardApi.getSynergies(card.id, 20, 30);
      setSynergyData(data);
    } catch (err: any) {
      setError(err.message || 'Error loading synergies');
    } finally {
      setIsLoading(false);
    }
  }, []);

  const selectByName = useCallback(async (name: string) => {
    try {
      const cards = await cardApi.search(name, 'vow', 1);
      if (cards.length > 0) {
        await selectCard(cards[0]);
      }
    } catch (err: any) {
      setError(err.message || 'Error finding card');
    }
  }, [selectCard]);

  return {
    selectedCard,
    synergyData,
    isLoading,
    error,
    selectCard,
    selectByName
  };
}
