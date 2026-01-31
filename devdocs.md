Short overview of the card system, file layout, reference logic adopted, and fixes.

---

## File Structure

| Folder | Role |
|--------|------|
| **Assets/Cards** | Data & flow: `Card`, `CardType`, `Ability`, `Deck`, `Hand`. Deck comp and draw/discard; Hand holds `CardVisual`s and fills to max. |
| **Assets/Board** | `BoardManager` (3×5 grid), `BoardSlot`. Placement row 0, advance to row 2, then activate and discard. |
| **Assets/Core** | `GameManager` (singleton, state), `TurnManager` (turn/advance/draw), `InputHandler` (click + keys), `GameSetup`, `GameState`. |
| **Assets/Visuals** | `CardVisual` – binds `Card` data to a 3D object, shows Artwork texture or type color. |
| **Assets/UI** | `UIManager` – health, turn, hand/deck counts, End Turn button, keybind text. |
| **Assets/Entities** | `Player`, `Monster`, `Effects`, `EnemyAI`. |
| **Assets/Arts** | Source art (Red/Green/Blue card textures). |
| **Assets/Resources/Arts** | Same textures for runtime loading; Deck loads sprites from here if not set in inspector. |
| **Assets/Editor** | `SceneSetupEditor` – menu-driven scene setup. |

---

## Card Placement Flow

1. **Hand** – Holds `CardVisual` instances. Player selects a card (click or keys 1–5).
2. **InputHandler** – **E** or click on board: gets column from `BoardManager.GetColumnFromWorldPosition(click)`, then `PlaceCard(selectedCard, column)`.
3. **BoardManager** – 3×5 grid. `PlaceCard(card, column)` puts the card in row 0 at that column. Each turn, cards advance (0→1→2); at row 2 they **activate** (effect runs), then are **discarded** via `Deck.AddToDiscard` and removed from the board.
4. **GameManager** – Connects Hand, Board, Deck; calls `hand.SetPlayerBoard(playerBoard)` and `playerBoard.SetDeck(playerDeck)` so hand size and discard work correctly.

---

## Logic Taken From Reference (CardCounter / DeckManager / Card)

- **Deck**: List-based draw pile; draw from front (`deck[0]`, remove); if empty, reshuffle discard into deck and shuffle. Fixed composition: Heal Potion, Bag of Holding, Bear, Wolf, Wall (with types/abilities). `AddToDiscard` / reshuffle behavior matches reference.
- **Hand**: “Fill” behaviour – draw until hand size = max. **Max hand** = 5 + number of board cards with ability **"max hand increase"** (from `BoardManager.GetCountOfCardsWithAbility("max hand increase")`).
- **Card**: `CardType` (name + color string), `Ability` (name), `score`, optional `Sprite` artwork. `GetCardAbility()` returns ability name or `"none"`. **Execute(player, monster)** by type/ability: attacker → attack, defender → defense, spell + heal → heal; “max hand increase” is passive (no Execute).
- **Board**: When a card is activated (reaches row 2), it is sent to discard via the board’s linked Deck.

---

## Bug Fixes Applied

- **End Turn**: Removed extra logs and ensured `OnEndTurnClicked()` only calls `TurnManager.EndTurn()` when state is `PlayerTurn`. End Turn button wired in `UIManager` without Place Card.
- **Place Card button**: Removed entirely. Placement is via **E** or clicking the board; keybind text above End Turn shows: E – place card, Space – end turn, 1–5 – select card, R – restart.
- **UIManager**: Removed all `placeCardButton` and `OnPlaceCardClicked` references so no CS0103 after removing the button.
- **Card sprites**: Cards were showing colored quads instead of Arts images. Deck now loads Red/Green/Blue from **Resources/Arts** (with Texture2D→Sprite fallback and cache). `CardVisual` uses card `Artwork` texture and Unlit/Texture when available so the sprite image is visible.

---

## What Works How

- **Start**: `GameSetup` or Editor menu creates boards, decks, hand, UI. `GameManager.StartGame()` calls `hand.SetDeck`, `hand.SetPlayerBoard`, `playerBoard.SetDeck`, then `hand.DrawInitialCards()` (fill to max) and `playerBoard.ClearBoard()` etc.
- **Turn**: Player places cards (E or click), then End Turn (button or Space). `TurnManager` advances both boards (row 2 cards activate and go to discard), then enemy plays, then player draws (`DrawTurnCards()`).
- **Cards**: Data in `Card`; display in `CardVisual` (Artwork from Deck’s Red/Green/Blue by type). Board activation calls `Card.Execute` (player board vs monster) or `ExecuteEnemyCard` (enemy board vs player), then discard.
