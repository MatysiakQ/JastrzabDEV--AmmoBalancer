# AmmoBalance v3.2.0

## Polski

Poprawki w przydzielaniu amunicji startowej MTF i Rebelii Chaosu oraz w budowaniu pluginu.

### Zmiany

- Przydział jest przypisany do konkretnego spawnu. Zadanie z poprzedniego życia nie zmienia już amunicji po odrodzeniu jako ta sama klasa.
- Oczekujące przydziały są anulowane przy kolejnym spawnie, wyjściu gracza, restarcie rundy, stanie oczekiwania na graczy i wyłączeniu pluginu. Przed wykonaniem plugin jeszcze raz sprawdza połączenie, rolę, stan gracza i konfigurację.
- Domyślnie amunicja jest nadawana od razu po standardowym ekwipunku (`delay: 0`).
- Opóźniony przydział dodaje tylko różnicę policzoną przy spawnie. Start 100, cel 200, wyrzucone 60 daje 140 w rezerwie, a nie nowe 200.
- Nadmiar ponad cel jest zabierany od razu, także przy dodatnim `delay`. Obliczenia nie przepełniają się.
- Limity amunicji gracza, w tym z pancerzem, są domyślnie respektowane (`respect_ammo_limits: true`).
- Zmiany klasy z zachowanym ekwipunkiem są domyślnie pomijane. Włączysz je przez `apply_to_preserved_inventory: true`.
- Konfiguracja jest walidowana: kolekcje `null`, nieobsługiwane kalibry (akceptowane jest tylko pięć kalibrów z gry, `Custom` z nowszego EXILED jest odrzucany) i `delay` poza zakresem 0-10 s.
- Każdy kaliber jest liczony raz, niezależnie od liczby broni. Magazynki i późniejsze zbieranie amunicji nie są ruszane.
- Naprawione ścieżki referencji, platformy budowania oraz nazwa i wersja assembly (`AmmoBalance.dll`, 3.2.0.0).

### Instalacja i aktualizacja

1. Pobierz `AmmoBalance.dll` z załączników wydania.
2. Usuń starsze `MatysiakAmmoWipe.dll`, `FreshAmmoRules.dll`, `AmmoBalancer.dll`, `MatysiakAmmoBalance.dll` i poprzednią `AmmoBalance.dll`.
3. Wrzuć DLL do `EXILED/Plugins` i zrestartuj serwer.
4. Sekcja configu to `ammo_balance`. Istniejące `delay: 1` nadal działa jako opóźnienie, ustaw `delay: 0` dla przydziału natychmiastowego.

Build: .NET Framework 4.8, x64. Plugin sprawdzony z EXILED 9.15.3 (API używane przez plugin jest bez zmian względem 9.12.6). CI buduje plugin i odpala 21 testów na atrapach API, więc zgodność z konkretną wersją SCP:SL i innymi pluginami trzeba sprawdzić na serwerze. Sumę kontrolną DLL znajdziesz w `AmmoBalance.sha256`. Tag `v3.1.0` został bez zmian.

---

## English

Fixes to starting ammo for MTF and Chaos Insurgency, and to the build setup.

### Changes

- Each grant belongs to a single spawn, so a timer from a previous life can no longer change ammo after respawning as the same class.
- Pending grants are cancelled on a new spawn, disconnect, round restart, waiting-for-players and plugin disable. Connection, role identity, player state and config are checked again before applying.
- Ammo is granted immediately after the standard inventory by default (`delay: 0`).
- A delayed grant adds only the deficit measured at spawn: start 100, target 200, drop 60 leaves 140, not a fresh 200.
- Ammo above the target is removed right away, even with a positive `delay`. Arithmetic is overflow-safe.
- Player ammo limits, armor included, are respected by default (`respect_ammo_limits: true`).
- Class changes that keep the inventory are skipped by default. Use `apply_to_preserved_inventory: true` to include them.
- Config validation covers `null` collections, unsupported ammo types (only the five vanilla calibres are accepted, `Custom` from newer EXILED is rejected) and `delay` outside 0-10 s.
- Each calibre is handled once regardless of weapon count. Magazines and later ammo pickups are untouched.
- Fixed reference paths, build platforms and assembly name/version (`AmmoBalance.dll`, 3.2.0.0).

### Install and upgrade

1. Download `AmmoBalance.dll` from the release assets.
2. Remove the old `MatysiakAmmoWipe.dll`, `FreshAmmoRules.dll`, `AmmoBalancer.dll`, `MatysiakAmmoBalance.dll` and any previous `AmmoBalance.dll`.
3. Copy the DLL to `EXILED/Plugins` and restart the server.
4. The config section is `ammo_balance`. An existing `delay: 1` still delays the grant; set `delay: 0` for immediate assignment.

Build: .NET Framework 4.8, x64. Checked against EXILED 9.15.3 (the API the plugin uses is unchanged since 9.12.6). CI builds the plugin and runs 21 tests against API doubles, so compatibility with your SCP:SL version and other plugins still needs a check on a test server. Verify the DLL with `AmmoBalance.sha256`. The `v3.1.0` tag is unchanged.
