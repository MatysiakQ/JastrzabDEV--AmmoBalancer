# AmmoBalance 3.2.0

Starting reserve ammo for MTF and Chaos Insurgency in SCP: Secret Laboratory.

## Polski

Plugin rozpoznaje kalibry broni w ekwipunku po spawnie i przydziela rezerwę z konfiguracji. Domyślnie pomija Facility Guard, inne drużyny i zmiany roli z zachowaniem ekwipunku. Dwie bronie tego samego kalibru oznaczają jeden przydział. Magazynki broni nie są zmieniane.

### Zmiany w 3.2.0

- Domyślny przydział jest natychmiastowy: EXILED nadaje standardowy ekwipunek przed `Spawned`.
- Opcjonalne opóźnienie jest przypisane do konkretnej instancji roli. Nowy spawn, wyjście, restart rundy i wyłączenie pluginu anulują oczekujący przydział.
- Opóźniony przydział dodaje wyłącznie różnicę ustaloną przy spawnie. Przykład: start 100, cel 200, wyrzucono 60 przed timerem → 140 w rezerwie i 60 na ziemi. Nie następuje odnowienie pełnych 200.
- Nadmiar ponad cel jest usuwany od razu, również przy dodatnim `delay`.
- Limity gry i pancerza są domyślnie respektowane. Można jawnie wyłączyć to w konfiguracji.
- Walidacja obsługuje kolekcje `null`, nieprawidłowe kalibry i opóźnienia.
- Poprawiono ścieżki bibliotek, nazwę DLL i wersję assembly. CI buduje plugin i wykonuje testy; wydanie z tagu powstaje z tego samego commitu.

### Wymagania i instalacja

- SCP: Secret Laboratory z EXILED. Dołączone referencje mają wersję **EXILED 9.12.6**; kompilacja korzysta z rzeczywistych bibliotek w `REFERENCES/`.
- .NET Framework 4.8, serwer x64.
- Testy bez serwera nie potwierdzają zgodności wszystkich pluginów ani każdej wersji SCP:SL. Sprawdź używaną wersję na serwerze testowym.

Pobierz `AmmoBalance.dll` z wydania odpowiadającego wybranej wersji albo z artefaktu CI **AmmoBalance-3.2.0** po udanym buildzie. Skopiuj tylko tę DLL do `EXILED/Plugins` i zrestartuj serwer. Konfiguracja ma prefiks `ammo_balance`.

**Aktualizacja:** usuń poprzednie kopie `MatysiakAmmoWipe.dll`, `FreshAmmoRules.dll`, `AmmoBalancer.dll` i `MatysiakAmmoBalance.dll`. Zastąp też poprzednie `AmmoBalance.dll`; pozostaw jedną kopię pluginu. Stara sekcja `fresh_ammo` nie jest używana. Istniejący `delay: 1` pozostaje opóźnieniem — zmień go na `0`, aby korzystać z natychmiastowego przydziału.

### Konfiguracja

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

| Opcja | Zachowanie |
|---|---|
| `delay` | Skończona liczba 0–10 sekund; błędna wartość daje ostrzeżenie i jest zastępowana przez 0. |
| `respect_ammo_limits` | Cel to minimum z konfiguracji pluginu i rzeczywistego limitu gracza, uwzględniającego pancerz. `false` pozwala celowo przekroczyć limit gry. |
| `apply_to_preserved_inventory` | `false` pomija zmianę klasy bez flagi `AssignInventory`; `true` dopuszcza zmianę istniejących zapasów. |
| `excluded_roles` | `[]` nie wyklucza ról. `null` przywraca wykluczenie Facility Guard i daje ostrzeżenie. |
| `max_ammo` | Docelowa rezerwa przy spawnie. Brak kalibru oznacza brak zmiany; `{}` jest pustym słownikiem, `null` daje pusty słownik z ostrzeżeniem. Zero to prawidłowy cel. |

To przydział startowy; plugin nie wprowadza stałego limitu zbierania amunicji. Przy dodatnim opóźnieniu zapamiętuje kalibry obecne w chwili `Spawned`, a przed wykonaniem sprawdza, czy gracz nadal ma broń danego kalibru. Nie przydziela kolejnej amunicji za broń dodaną później przez inny plugin. Późny custom loadout wymaga uzgodnienia kolejności z jego autorem — samo zwiększanie opóźnienia tego nie rozwiązuje.

### Budowanie i testy

```bash
git clone https://github.com/MatysiakQ/JastrzabDEV--AmmoBalancer.git
cd JastrzabDEV--AmmoBalancer
dotnet build MatysiakAmmoBalance.csproj -c Release -p:Platform=x64
dotnet run --project tests/AmmoBalance.Tests.csproj -c Release
```

Wymagany jest SDK .NET 8. Pakiet reference assemblies umożliwia kompilację `net48` również na Linuxie. DLL powstaje w `bin/x64/Release/net48/AmmoBalance.dll`; biblioteki gry i EXILED nie są kopiowane do katalogu wynikowego. Możesz podać własny zgodny zestaw bibliotek przez `-p:ReferencesPath=/pełna/ścieżka`.

Testy uruchamiają rzeczywiste źródła pluginu z zastępczym API i zegarem; produkcyjny build osobno sprawdza zgodność z dołączonymi DLL. Przed wdrożeniem sprawdź w grze standardowe fale, zmianę klasy na tę samą, rozłączenie, reload, pancerze i używane custom loadouty.

### Wydania

Po zatwierdzeniu zmian utwórz tag zgodny z wersją projektu, np. `v3.2.0`, i wyślij go do GitHub. Workflow sprawdza wersję tagu, buduje plugin, wykonuje testy i publikuje DLL wraz z SHA-256. Historyczny tag `v3.1.0` nie odpowiada w pełni nazwom w jego DLL; nie jest przepinany.

## English

AmmoBalance assigns starting reserve ammo based on the firearms actually present at `Spawned`. Facility Guards, other teams, and role changes preserving inventory are skipped by default. Magazines are untouched.

Version 3.2.0 defaults to immediate assignment and respects the player's real ammo limit, including armor. Optional delayed grants belong to one role instance and are canceled on another spawn, disconnect, round reset or plugin disable. They add only the original spawn-time deficit: starting with 100, targeting 200 and dropping 60 during the delay leaves 140 in reserve, not a fresh 200. Excess starting ammo is trimmed immediately.

The YAML above uses the same keys in both languages. Set `respect_ammo_limits: false` to deliberately bypass game limits, or `apply_to_preserved_inventory: true` to also affect class changes that preserve inventory. Invalid delays reset to zero. Null exclusions restore the default Guard exclusion; null ammo mappings disable grants with a warning. Empty collections are supported.

A positive delay snapshots the original weapon calibres; removed calibres are skipped at execution, and newly added custom-loadout calibres receive no extra grant. This is a one-time starting allocation, not a continuous pickup limit. Late custom loadouts need explicit integration.

Build and test using the commands above (SDK .NET 8). The production build targets .NET Framework 4.8 and uses the bundled EXILED 9.12.6 references. Server compatibility must be checked with the actual game/plugins. Install only `AmmoBalance.dll`; remove all historical DLL names listed above and retain one copy. Existing `delay: 1` configurations keep the optional delay; set it to zero to use immediate assignment.

Tag releases must match the project version. CI builds/tests the tagged source and publishes the DLL plus its SHA-256. The historical v3.1.0 tag is preserved.

## License

[MIT — LICENSE.txt](LICENSE.txt)
