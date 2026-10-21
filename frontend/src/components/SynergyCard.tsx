import React from 'react';
import { SynergyMatch, Card } from '../types';

interface SynergyCardProps {
  match: SynergyMatch;
  onSelectCard: (card: Card) => void;
}

export const SynergyCard: React.FC<SynergyCardProps> = ({ match, onSelectCard }) => {
  const { card, totalScore, contributions } = match;

  const scoreClass = totalScore >= 70 ? 'score-high' : 'score-mid';

  const getReasonClass = (category: string) => {
    switch (category.toLowerCase()) {
      case 'color':
        return 'reason-color';
      case 'tribal':
        return 'reason-tribal';
      case 'cardtype':
        return 'reason-cardtype';
      default:
        return '';
    }
  };

  return (
    <div className="synergy-card">
      <div className="synergy-card-header">
        <img
          src={card.imageUrlArtCrop || card.imageUrlNormal || ''}
          alt={card.name}
          className="synergy-card-art"
          loading="lazy"
        />
        <div className={`synergy-score-badge ${scoreClass}`}>
          <span>⚡</span>
          <span>{Math.round(totalScore)}% Synergy</span>
        </div>
      </div>

      <div className="synergy-card-body">
        <div className="synergy-card-name-row">
          <span className="synergy-card-name">{card.name}</span>
          {card.manaCost && <span className="synergy-card-mana">{card.manaCost}</span>}
        </div>
        <span className="synergy-card-type">{card.typeLine}</span>

        <div className="synergy-reasons-container">
          {contributions.map((c, i) => (
            <div key={i} className={`synergy-reason-pill ${getReasonClass(c.category)}`}>
              <strong>+{c.score}pts:</strong> {c.reason}
            </div>
          ))}
        </div>

        <button
          className="explore-btn"
          onClick={() => onSelectCard(card)}
          title={`Find synergies for ${card.name}`}
        >
          Explore this card →
        </button>
      </div>
    </div>
  );
};
