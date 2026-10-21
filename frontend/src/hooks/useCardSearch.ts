import { useState, useEffect } from 'react';
import { Card } from '../types';
import { cardApi } from '../services/api';

export function useCardSearch(initialQuery: string = '') {
  const [query, setQuery] = useState(initialQuery);
  const [results, setResults] = useState<Card[]>([]);
  const [isOpen, setIsOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [highlightedIndex, setHighlightedIndex] = useState(-1);

  useEffect(() => {
    if (!query.trim() || query.length < 2) {
      setResults([]);
      setIsOpen(false);
      return;
    }

    const timer = setTimeout(async () => {
      setIsLoading(true);
      try {
        const cards = await cardApi.search(query, 'vow', 8);
        setResults(cards);
        setIsOpen(cards.length > 0);
        setHighlightedIndex(-1);
      } catch (err) {
        console.error('Search error:', err);
      } finally {
        setIsLoading(false);
      }
    }, 200);

    return () => clearTimeout(timer);
  }, [query]);

  const clear = () => {
    setQuery('');
    setResults([]);
    setIsOpen(false);
  };

  return {
    query,
    setQuery,
    results,
    isOpen,
    setIsOpen,
    isLoading,
    highlightedIndex,
    setHighlightedIndex,
    clear
  };
}
