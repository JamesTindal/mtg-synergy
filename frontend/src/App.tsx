import { useEffect } from 'react';
import { useCardSynergies } from './hooks/useCardSynergies';
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
  const {
    selectedCard,
    synergyData,
    isLoading,
    error,
    selectCard,
    selectByName
  } = useCardSynergies();

  // Load default featured card on initial visit
  useEffect(() => {
    selectByName('Olivia, Crimson Bride');
  }, [selectByName]);

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
        onSelectCard={selectCard}
        selectedCardName={selectedCard?.name}
      />

      <div className="quick-picks">
        <span className="quick-picks-label">Popular in VOW:</span>
        {FEATURED_CARDS.map((name) => (
          <button
            key={name}
            className="quick-pick-pill"
            onClick={() => selectByName(name)}
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
            <p className="state-text">
              Analyzing game mechanics & rule interactions for {selectedCard?.name}...
            </p>
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
                    onSelectCard={selectCard}
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
