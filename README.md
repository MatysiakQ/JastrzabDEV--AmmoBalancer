# AmmoBalance 3.2.0

Plugin EXILED do SCP: Secret Laboratory. Po spawnie ustawia rezerwę amunicji MTF i Rebelii Chaosu na wartości z konfiguracji, ale tylko dla kalibrów broni, które gracz faktycznie ma w ekwipunku.

## Polski

### Jak to działa

- Po zdarzeniu `Spawned` plugin sprawdza bronie gracza i ustawia rezerwę każdego kalibru na wartość z `max_ammo`.
- Dwie bronie tego samego kalibru to jeden przydział. Magazynki w broniach nie są zmieniane.
- Pomijani są Facility Guard, pozostałe drużyny oraz zmiany klasy z zachowanym ekwipunkiem.
- Cel nie przekracza realnego limitu gracza (np. z pancerzem). Wyłączysz to przez `respect_ammo_limits: false`.
- Amunicja ponad cel jest zabierana od razu.
- To jednorazowy przydział startowy. Plugin nie ogranicza późniejszego zbierania amunicji.

Przy `delay` większym od zera dodawana jest tylko różnica policzona w chwili spawnu. Przykład: gracz startuje ze 100, cel to 200, przed końcem timera wyrzuca 60. Po timerze ma 140 w rezerwie, a nie nowe 200. Oczekujący przydział znika przy kolejnym spawnie gracza, jego wyjściu, restarcie rundy i wyłączeniu pluginu. Broń dodana później przez inny plugin nie dostaje amunicji, a dłuższy `delay` tego nie zmieni.

### Instalacja

Wymagania: SCP: Secret Laboratory z EXILED, serwer x64, .NET Framework 4.8.

1. Pobierz `AmmoBalance.dll` z wydania lub z artefaktu CI.
2. Wrzuć plik do `EXILED/Plugins` i zrestartuj serwer.

Przy aktualizacji usuń starsze DLL-e: `MatysiakAmmoWipe.dll`, `FreshAmmoRules.dll`, `AmmoBalancer.dll`, `MatysiakAmmoBalance.dll` oraz poprzednią `AmmoBalance.dll`. Ma zostać jedna kopia. Sekcja `fresh_ammo` w configu nie jest już używana. Jeśli masz `delay: 1`, plugin nadal będzie czekał sekundę. Ustaw `delay: 0`, żeby przydział był natychmiastowy.

### Konfiguracja

Prefiks configu to `ammo_balance`.

```yaml
ammo_balance:
  is_enabled: true
  debug: false
  delay: 0
  respect_ammo_limits: true
  apply_to_preserved_inventory: false
  excluded_roles:
  - FacilityGuard
  max_ammo:
    Nato9: 200
    Nato556: 200
    Nato762: 200
    Ammo12Gauge: 54
    Ammo44Cal: 48
```

| Opcja | Opis |
|---|---|
| `delay` | Opóźnienie w sekundach, 0-10. Niepoprawna wartość jest zastępowana zerem i zgłaszana w logu. |
| `respect_ammo_limits` | Cel to mniejsza z wartości: z configu albo limit gracza z pancerzem. |
| `apply_to_preserved_inventory` | `true` obejmuje też zmiany klasy bez `AssignInventory`. |
| `excluded_roles` | Pominięte role. `[]` nie wyklucza nikogo, `null` przywraca Facility Guard. |
| `max_ammo` | Docelowa rezerwa przy spawnie. Obsługiwane kalibry: `Nato9`, `Nato556`, `Nato762`, `Ammo12Gauge`, `Ammo44Cal`. Kaliber spoza listy zostaje bez zmian, `0` jest poprawnym celem, `null` wyłącza przydziały. |

### Budowanie i testy

Potrzebny jest .NET SDK 8.

```
dotnet build MatysiakAmmoBalance.csproj -c Release -p:Platform=x64
dotnet run --project tests/AmmoBalance.Tests.csproj -c Release
```

Wynik trafia do `bin/x64/Release/net48/AmmoBalance.dll`. Biblioteki gry i EXILED są brane z `REFERENCES/` (dołączone: Exiled.API i Exiled.Events z wersji 9.15.3) i nie są kopiowane do wyniku. Własny katalog podasz przez `-p:ReferencesPath=/pełna/ścieżka`. Biblioteki gry w `REFERENCES/` (`Assembly-CSharp`, `Assembly-CSharp-firstpass`, `UnityEngine.CoreModule`, `NorthwoodLib`) są zgodne z EXILED 9.15.3. Po każdej aktualizacji gry lub EXILED podmień pliki w `REFERENCES/` (z `SCPSL_Data/Managed` serwera) i zbuduj plugin od nowa.

Testy uruchamiają kod pluginu na atrapach API i zegara, więc nie zastąpią próby na serwerze. Przed wdrożeniem sprawdź zwykłe fale, zmianę klasy, wyjście gracza, reload, pancerze i swoje customowe loadouty.

### Wydania

Wydanie powstaje po wypchnięciu tagu zgodnego z wersją projektu (np. `v3.2.0`) albo po commicie na `master` z `[release]` w opisie. CI buduje plugin, odpala testy i publikuje DLL z sumą SHA-256 oraz treścią z `RELEASE_NOTES.md`. Istniejący tag wskazujący na inny commit nie jest nadpisywany.

## English

AmmoBalance sets starting reserve ammo for MTF and Chaos Insurgency after they spawn, based on the firearms they are actually carrying. Facility Guards, other teams and class changes that keep the inventory are skipped by default. Magazines are not touched.

The target is capped by the player's real ammo limit (armor included) unless `respect_ammo_limits` is `false`, and anything above the target is removed right away. With a positive `delay`, only the deficit measured at spawn is added later: start at 100, target 200, drop 60 before the timer, and you end up with 140, not a fresh 200. Pending grants are cancelled on a new spawn, disconnect, round restart and plugin disable.

This is a one-time starting grant, not a pickup cap. Weapons added later by other plugins get no ammo.

Config keys are the same as in the YAML above. Install only `AmmoBalance.dll` in `EXILED/Plugins` and remove the old DLL names listed in the Polish section. Build and test with the commands above (.NET SDK 8, target .NET Framework 4.8, bundled Exiled.API and Exiled.Events 9.15.3). Releases are created from a version tag or a `[release]` commit on `master`.

## Licencja

MIT, zobacz [LICENSE.txt](LICENSE.txt).
