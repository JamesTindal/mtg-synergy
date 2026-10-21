import { useState, useEffect } from 'react';
import { Card, SynergyResponse } from './types';
import { SearchBar } from './components/SearchBar';
import { CardSpotlight } from './components/CardSpotlight';
import { SynergyCard } from './components/SynergyCard';

const FEATURED_CARDS = [
  'Olivia, Crimson Bride',
  'Anje, Maid of Dishonor',
  'Sorin the Mirthless',
  'Torens, Fist of the Angels',
  'Thalia, Guardian of Thraben',
  'Hullbreaker Horror'
];

export function App() {
  const [selectedCard, setSelectedCard] = useState<Card | null>(null);
  const [synergyData, setSynergyData] = useState<SynergyResponse | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchCardSynergies = async (card: Card) => {
    setSelectedCard(card);
    setIsLoading(true);
    setError(null);

    try {
      const res = await fetch(`/api/cards/${card.id}/synergies?minScore=20&limit=30`);
      if (!res.ok) throw new Error('Failed to load card synergies.');
      const data: SynergyResponse = await res.json();
      setSynergyData(data);
    } catch (err: any) {
      setError(err.message || 'Error fetching synergies');
    } finally {
      setIsLoading(false);
    }
  };

  const handleQuickPick = async (name: string) => {
    try {
      const res = await fetch(`/api/cards/search?q=${encodeURIComponent(name)}&set=vow&limit=1`);
      if (res.ok) {
        const cards: Card[] = await res.json();
        if (cards.length > 0) {
          fetchCardSynergies(cards[0]);
        }
      }
    } catch (err) {
      console.error(err);
    }
  };

  // Load a default card on initial visit (e.g. Olivia, Crimson Bride)
  useEffect(() => {
    handleQuickPick('Olivia, Crimson Bride');
  }, []);

  return (
    <div className="app-container">
      <header className="app-header">
        <h1 className="app-title">🔮 MTG Synergy Engine</h1>
        <p className="app-subtitle">
          Game mechanics synergy finder
          <span className="set-badge">Crimson Vow (VOW)</span>
        </p>
      </header>

      <SearchBar
        onSelectCard={fetchCardSynergies}
        selectedCardName={selectedCard?.name}
      />

      <div className="quick-picks">
        <span className="quick-picks-label">Popular in VOW:</span>
        {FEATURED_CARDS.map((name) => (
          <button
            key={name}
            className="quick-pick-pill"
            onClick={() => handleQuickPick(name)}
          >
            {name}
          </button>
        ))}
      </div>

      <main style={{ marginTop: '2.5rem' }}>
        {selectedCard && <CardSpotlight card={selectedCard} />}

        {isLoading && (
          <div className="state-container">
            <div className="state-icon">⏳</div>
            <p className="state-text">Analyzing game mechanics & rule interactions for {selectedCard?.name}...</p>
          </div>
        )}

        {error && (
          <div className="state-container" style={{ color: '#ff8787' }}>
            <div className="state-icon">⚠️</div>
            <p className="state-text">{error}</p>
          </div>
        )}

        {!isLoading && !error && synergyData && (
          <section>
            <div className="synergies-section-title">
              <h3 className="synergies-heading">Recommended Synergies</h3>
              <span className="synergies-count">
                Found {synergyData.totalMatches} synergistic cards
              </span>
            </div>

            {synergyData.synergies.length === 0 ? (
              <div className="state-container">
                <div className="state-icon">🃏</div>
                <p className="state-text">No strong synergies found for this card in Crimson Vow yet.</p>
              </div>
            ) : (
              <div className="synergies-grid">
                {synergyData.synergies.map((match) => (
                  <SynergyCard
                    key={match.card.id}
                    match={match}
                    onSelectCard={fetchCardSynergies}
                  />
                ))}
              </div>
            )}
          </section>
        )}
      </main>
    </div>
  );
}

export default App;
