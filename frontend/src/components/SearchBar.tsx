import React, { useState, useEffect, useRef } from 'react';
import { Card } from '../types';

interface SearchBarProps {
  onSelectCard: (card: Card) => void;
  selectedCardName?: string;
}

export const SearchBar: React.FC<SearchBarProps> = ({ onSelectCard, selectedCardName }) => {
  const [query, setQuery] = useState(selectedCardName || '');
  const [results, setResults] = useState<Card[]>([]);
  const [isOpen, setIsOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [highlightedIndex, setHighlightedIndex] = useState(-1);
  const wrapperRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (selectedCardName) {
      setQuery(selectedCardName);
    }
  }, [selectedCardName]);

  // Debounced search
  useEffect(() => {
    if (!query.trim() || query.length < 2) {
      setResults([]);
      setIsOpen(false);
      return;
    }

    const handler = setTimeout(async () => {
      setIsLoading(true);
      try {
        const res = await fetch(`/api/cards/search?q=${encodeURIComponent(query)}&set=vow&limit=8`);
        if (res.ok) {
          const data: Card[] = await res.json();
          setResults(data);
          setIsOpen(data.length > 0);
          setHighlightedIndex(-1);
        }
      } catch (err) {
        console.error('Search error:', err);
      } finally {
        setIsLoading(false);
      }
    }, 200);

    return () => clearTimeout(handler);
  }, [query]);

  // Close on outside click
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (wrapperRef.current && !wrapperRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (!isOpen || results.length === 0) return;

    if (e.key === 'ArrowDown') {
      e.preventDefault();
      setHighlightedIndex((prev) => (prev < results.length - 1 ? prev + 1 : 0));
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setHighlightedIndex((prev) => (prev > 0 ? prev - 1 : results.length - 1));
    } else if (e.key === 'Enter') {
      e.preventDefault();
      if (highlightedIndex >= 0 && highlightedIndex < results.length) {
        handleSelect(results[highlightedIndex]);
      }
    } else if (e.key === 'Escape') {
      setIsOpen(false);
    }
  };

  const handleSelect = (card: Card) => {
    setQuery(card.name);
    setIsOpen(false);
    onSelectCard(card);
  };

  const handleClear = () => {
    setQuery('');
    setResults([]);
    setIsOpen(false);
  };

  return (
    <div className="search-container" ref={wrapperRef}>
      <div className="search-input-wrapper">
        <span className="search-icon">{isLoading ? '⏳' : '🔍'}</span>
        <input
          type="text"
          className="search-input"
          placeholder="Search for a Crimson Vow card (e.g. Olivia, Anje, Sorin)..."
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onFocus={() => results.length > 0 && setIsOpen(true)}
          onKeyDown={handleKeyDown}
        />
        {query && (
          <button className="clear-search-btn" onClick={handleClear} title="Clear search">
            ✕
          </button>
        )}
      </div>

      {isOpen && (
        <ul className="search-dropdown">
          {results.map((card, index) => (
            <li
              key={card.id}
              className={`search-item ${index === highlightedIndex ? 'highlighted' : ''}`}
              onClick={() => handleSelect(card)}
              onMouseEnter={() => setHighlightedIndex(index)}
            >
              <img
                src={card.imageUrlArtCrop || card.imageUrlNormal || ''}
                alt={card.name}
                className="search-item-art"
                loading="lazy"
              />
              <div className="search-item-info">
                <span className="search-item-name">{card.name}</span>
                <span className="search-item-meta">{card.typeLine}</span>
              </div>
              {card.manaCost && <span className="search-item-mana">{card.manaCost}</span>}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};
