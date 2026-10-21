import React from 'react';
import { Card } from '../types';

interface CardSpotlightProps {
  card: Card;
}

export const CardSpotlight: React.FC<CardSpotlightProps> = ({ card }) => {
  return (
    <div className="spotlight-card">
      <div className="spotlight-image-wrap">
        <img
          src={card.imageUrlNormal || card.imageUrlArtCrop || ''}
          alt={card.name}
          className="spotlight-image"
        />
      </div>
      <div className="spotlight-details">
        <div className="spotlight-header">
          <h2 className="spotlight-name">{card.name}</h2>
          {card.manaCost && <span className="spotlight-mana">{card.manaCost}</span>}
        </div>
        <div className="spotlight-type">{card.typeLine}</div>
        {card.oracleText && (
          <div className="spotlight-oracle">{card.oracleText}</div>
        )}
        {(card.power || card.toughness) && (
          <div className="spotlight-stats">
            {card.power} / {card.toughness}
          </div>
        )}
      </div>
    </div>
  );
};
