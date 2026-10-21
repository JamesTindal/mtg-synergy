import React, { useEffect, useRef } from 'react';
import { Card } from '../types';
import { useCardSearch } from '../hooks/useCardSearch';

interface SearchBarProps {
  onSelectCard: (card: Card) => void;
  selectedCardName?: string;
}

export const SearchBar: React.FC<SearchBarProps> = ({ onSelectCard, selectedCardName }) => {
  const {
    query,
    setQuery,
    results,
    isOpen,
    setIsOpen,
    isLoading,
    highlightedIndex,
    setHighlightedIndex,
    clear
  } = useCardSearch(selectedCardName);

  const wrapperRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (selectedCardName) {
      setQuery(selectedCardName);
    }
  }, [selectedCardName, setQuery]);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (wrapperRef.current && !wrapperRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [setIsOpen]);

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
          <button className="clear-search-btn" onClick={clear} title="Clear search">
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
