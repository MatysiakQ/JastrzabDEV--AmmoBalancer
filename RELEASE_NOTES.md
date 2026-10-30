# AmmoBalance v3.2.0

## 🇵🇱 Polski

Wydanie poprawia przydzielanie amunicji startowej dla MTF i Rebelii Chaosu, obsługę zmian roli oraz sposób budowania pluginu.

### Poprawki

- Przydział jest przypisany do konkretnego spawnu. Zadanie poprzedniego życia nie zmienia już zapasów po odrodzeniu jako ta sama klasa.
- Oczekujące zadania są anulowane przy kolejnym spawnie, rozłączeniu gracza, restarcie rundy, oczekiwaniu na graczy i wyłączeniu pluginu. Callback ponownie sprawdza połączenie, rolę, stan gracza i konfigurację.
- Domyślny przydział działa natychmiast po nadaniu standardowego ekwipunku (`delay: 0`).
- Opcjonalne opóźnienie dodaje tylko różnicę wyliczoną przy spawnie. Przykład: początkowe 100, cel 200, wyrzucone 60 → 140 w rezerwie i 60 na ziemi. Nie jest odnawiana pełna rezerwa 200.
- Nadmiar amunicji jest usuwany od razu, również przy dodatnim opóźnieniu. Obliczenia są zabezpieczone przed przepełnieniem.
- Limity amunicji gracza, w tym pancerz, są domyślnie respektowane (`respect_ammo_limits: true`). Można jawnie wyłączyć to ograniczenie.
- Zmiany klasy z zachowaniem ekwipunku są domyślnie pomijane. Opcja `apply_to_preserved_inventory: true` pozwala je objąć przydziałem.
- Dodano walidację kolekcji `null`, nieobsługiwanych kalibrów i wartości opóźnienia spoza skończonego zakresu 0–10 sekund.
- Każdy kaliber jest obsługiwany raz, niezależnie od liczby posiadanych broni. Nie są zmieniane magazynki broni ani limity późniejszego zbierania amunicji.
- Naprawiono ścieżki referencji, platformy budowania oraz nazwę i wersję assembly. Wynik to `AmmoBalance.dll`, wersja 3.2.0.0.

### Instalacja i aktualizacja

1. Pobierz `AmmoBalance.dll` z plików tego wydania.
2. Usuń poprzednie kopie `MatysiakAmmoWipe.dll`, `FreshAmmoRules.dll`, `AmmoBalancer.dll` i `MatysiakAmmoBalance.dll`. Zastąp poprzednią `AmmoBalance.dll`; pozostaw tylko jedną kopię pluginu.
3. Wgraj DLL do katalogu `EXILED/Plugins` i zrestartuj serwer.
4. Używana sekcja konfiguracji to `ammo_balance`. Istniejące `delay: 1` pozostaje opóźnieniem; ustaw `delay: 0`, aby używać przydziału natychmiastowego.

Build: .NET Framework 4.8, x64, dołączone referencje EXILED 9.12.6. Kompilacja produkcyjna i 21 scenariuszy regresyjnych są uruchamiane w CI. Testy regresyjne używają źródeł pluginu z zastępczym API i zegarem; zgodność z docelowym SCP:SL i pozostałymi pluginami trzeba sprawdzić w grze.

Plik `AmmoBalance.sha256` umożliwia sprawdzenie sumy kontrolnej DLL. Tag tego wydania wskazuje commit, z którego powstał załączony build. Historyczny tag `v3.1.0` pozostaje bez zmian.

---

## 🇬🇧 English

This release fixes starting reserve ammo allocation for MTF and Chaos Insurgency, role-change handling, and reproducible plugin builds.

### Fixes

- Each grant belongs to one spawn. A callback from a previous life can no longer modify ammo after respawning as the same class.
- Pending grants are canceled on another spawn, disconnect, round restart, waiting-for-players state, and plugin disable. Callbacks recheck connection, role identity, player state, and configuration.
- Standard starting ammo is assigned immediately after the inventory is provided (`delay: 0` by default).
- An optional delay adds only the original spawn-time deficit. Example: starting with 100, targeting 200, and dropping 60 leaves 140 in reserve and 60 on the ground instead of restoring a fresh reserve of 200.
- Excess ammo is trimmed immediately even when a delay is configured. Arithmetic is protected against overflow.
- Player ammo limits, including armor, are respected by default (`respect_ammo_limits: true`). Deliberate limit overrides remain available.
- Class changes preserving inventory are skipped by default. Set `apply_to_preserved_inventory: true` to include them.
- Null collections, unsupported ammo types, and non-finite or out-of-range delays are validated. The supported delay range is 0–10 seconds.
- Each calibre is processed once regardless of weapon count. Weapon magazines and subsequent ammo pickup limits are not modified.
- Fixed reference paths, build platforms, and assembly naming/versioning. The output is `AmmoBalance.dll`, assembly version 3.2.0.0.

### Installation and upgrade

1. Download `AmmoBalance.dll` from this release's assets.
2. Remove older `MatysiakAmmoWipe.dll`, `FreshAmmoRules.dll`, `AmmoBalancer.dll`, and `MatysiakAmmoBalance.dll` copies. Replace any previous `AmmoBalance.dll`; retain only one plugin copy.
3. Copy the DLL to `EXILED/Plugins` and restart the server.
4. The configuration section is `ammo_balance`. Existing `delay: 1` settings retain the optional delay; use `delay: 0` for immediate assignment.

Build target: .NET Framework 4.8, x64, bundled EXILED 9.12.6 references. CI runs the production build and 21 regression scenarios. Regression tests execute the plugin sources with API and scheduler doubles; compatibility with the target SCP:SL version and other plugins still requires in-game testing.

Use `AmmoBalance.sha256` to verify the DLL. This release's tag identifies the commit used to build its attached binary. The historical `v3.1.0` tag is preserved.
