<div align="center">

# 🔫 AmmoBalance

**Weapon-based starting ammo for MTF and Chaos Insurgency in SCP: Secret Laboratory. / Amunicja dopasowana do broni dla MTF i Rebelii Chaosu.**

[![Release](https://img.shields.io/badge/Release-v3.1.0-blue.svg?style=flat-square)](#)
[![EXILED](https://img.shields.io/badge/EXILED-v9.12.6-yellow.svg?style=flat-square)](#)
[![.NET](https://img.shields.io/badge/.NET%20Framework-4.8-purple.svg?style=flat-square)](#)
[![License](https://img.shields.io/badge/License-MIT-green.svg?style=flat-square)](LICENSE)

</div>

<hr>

## 📑 Spis treści / Table of contents
- [🇵🇱 Polski](#-polski)
- [🇬🇧 English](#-english)

---

# 🇵🇱 Polski

> **Uwaga:** plugin wcześniej nazywał się `MatysiakAmmoWipe` / `FreshAmmoRules`. Od v3.1.0 nazywa się **AmmoBalance** i ma zupełnie inną logikę niż starsze wersje (patrz sekcja *Aktualizacja*).

## 📖 O projekcie

**AmmoBalance** daje graczom z **MTF** i **Rebelii Chaosu** maksymalną ilość amunicji zaraz po zrespieniu. Plugin najpierw **sprawdza, jaką broń gracz ma w ekwipunku**, a potem przydziela amunicję pasującą do tej broni. Dzięki temu amunicja zawsze zgadza się z uzbrojeniem, niezależnie od tego, jaki loadout daje dana rola w aktualnej wersji gry.

## ✨ Jak to działa

1. Gracz respi się (event `Spawned`). Plugin sprawdza, czy jest w drużynie **MTF (Foundation Forces)** lub **Chaosu**. Inne drużyny są pomijane.
2. Role z listy `excluded_roles` (domyślnie **Facility Guard**) są pomijane.
3. Po opóźnieniu `delay` (domyślnie 1 s, żeby gra zdążyła nadać domyślny ekwipunek) plugin przechodzi po wszystkich **broniach palnych** w ekwipunku.
4. Dla każdej broni odczytuje jej **typ amunicji** i ustawia ilość tej amunicji na wartość z `max_ammo`. Gracz z dwiema broniami na różną amunicję dostanie obie.
5. Jeśli gracz wyszedł, zginął albo zmienił rolę w czasie opóźnienia, nic się nie dzieje.

### ⚠️ Warto wiedzieć
- Plugin **ustawia** ilość amunicji (nie dodaje do istniejącej) i działa tylko dla amunicji, która jest w `max_ammo`.
- Gracz bez broni palnej nie dostanie żadnej amunicji.
- Event `Spawned` odpala się także przy innych spawnach niż fala respawnu (start rundy, force-class, **SCP uciekające jako Chaos** w pluginie ScpEscapeSystem). Każdy gracz MTF/Chaosu z bronią dostanie więc amunicję.
- Wartości w `max_ammo` ustaw zgodnie z limitami amunicji na swoim serwerze. Jeśli serwer ma niższy limit, gra może przyciąć wartość.

## 📋 Wymagania

- Serwer **SCP: Secret Laboratory** z **EXILED 9.x** (plugin budowany pod **9.12.6**, .NET Framework 4.8).
- Nie są potrzebne żadne dodatkowe pluginy.

## 📥 Instalacja

1. Zainstaluj EXILED na serwerze, jeśli jeszcze go nie masz.
2. Pobierz `AmmoBalance.dll` z zakładki **Releases**.
3. Skopiuj plik do folderu pluginów:
   - Windows: `%AppData%\EXILED\Plugins`
   - Linux: `~/.config/EXILED/Plugins`
4. Zrestartuj serwer. Przy pierwszym starcie plugin wygeneruje config.
5. Edytuj sekcję `ammo_balance` w `EXILED/Configs/<PORT>-config.yml` (albo w `EXILED/Configs/Plugins/ammo_balance/`, jeśli masz włączone osobne configi) i zrestartuj serwer.

### 🔄 Aktualizacja ze starszych wersji (`MatysiakAmmoWipe` / `FreshAmmoRules`)
1. **Usuń stary plik DLL** (`MatysiakAmmoWipe.dll` lub `FreshAmmoRules.dll`) z `EXILED/Plugins`. Jeśli zostaną oba, ustawią amunicję równolegle.
2. Wgraj `AmmoBalance.dll`.
3. **Usuń stary config** (sekcję `fresh_ammo`). Format się zmienił (`RoleAmmoLimits` → `max_ammo`), a prefix to teraz `ammo_balance`.

## 🛠️ Konfiguracja

```yaml
ammo_balance:
  # Czy plugin jest włączony?
  is_enabled: true

  # Logi debugowania w konsoli
  debug: false

  # Opóźnienie (s) po spawnie, żeby gra zdążyła nadać domyślny ekwipunek
  delay: 1

  # Role pomijane przez plugin
  excluded_roles:
  - FacilityGuard

  # Ilość amunicji ustawiana dla danego typu (zgodnie z limitami serwera)
  max_ammo:
    Nato9: 200
    Nato556: 200
    Nato762: 200
    Ammo12Gauge: 54
    Ammo44Cal: 48
```

| Opcja | Opis |
| :--- | :--- |
| `delay` | Czas oczekiwania po spawnie. Jeśli coś nadpisuje amunicję po pluginie, zwiększ tę wartość. |
| `excluded_roles` | Lista ról (`RoleTypeId`), które plugin ignoruje. Usuń `FacilityGuard`, jeśli ochrona też ma dostawać amunicję. |
| `max_ammo` | Słownik `typ amunicji: ilość`. Typ, którego tu nie ma, nie będzie ustawiany. |

## 🧪 Debugowanie

Ustaw `debug: true`. Przy spawnie MTF/Chaosu w logach pojawią się linie w stylu:
```
Nick (NtfSergeant): GunFRMG0 -> Nato556 = 200
```
Jeśli amunicja jest zła albo jej brak, zgłoś Issue i dołącz logi oraz wersje EXILED i SCP:SL.

## 🔨 Kompilacja ze źródeł

1. Sklonuj repozytorium:
   ```bash
   git clone https://github.com/TwojaNazwaUzytkownika/AmmoBalance.git
   ```
2. Otwórz projekt w **Visual Studio 2022** (target: **.NET Framework 4.8**).
3. Dodaj referencje do EXILED (pakiet NuGet `ExMod.Exiled` lub pliki `Exiled.API.dll` i `Exiled.Events.dll`) oraz bibliotek gry z `SCPSL_Data/Managed`.
4. Ustaw platformę **Any CPU** albo **x64** (nie ARM64). Ostrzeżenia MSB3270 przy Any CPU są normalne.
5. Zbuduj w trybie `Release`. Plik znajdziesz w `bin/Release/net48/AmmoBalance.dll`.

## 🐛 Zgłaszanie błędów

Znalazłeś błąd lub masz pomysł? Otwórz zgłoszenie w zakładce **Issues**. Pull Requesty są mile widziane.

---

# 🇬🇧 English

> **Note:** this plugin used to be called `MatysiakAmmoWipe` / `FreshAmmoRules`. Since v3.1.0 it is **AmmoBalance** and its logic is completely different from older versions (see *Updating*).

## 📖 About

**AmmoBalance** gives **MTF** and **Chaos Insurgency** players the maximum amount of ammo right after they spawn. The plugin first **detects which weapons the player carries** and then assigns the ammo that matches those weapons. The ammo always fits the loadout, regardless of what the current game version gives a given role.

## ✨ How it works

1. A player spawns (`Spawned` event). The plugin checks whether they are on the **MTF (Foundation Forces)** or **Chaos** team. Other teams are skipped.
2. Roles in `excluded_roles` (default: **Facility Guard**) are skipped.
3. After the `delay` (default 1 s, so the game can finish giving the default inventory) the plugin goes through all **firearms** in the inventory.
4. For each weapon it reads the **ammo type** and sets that ammo to the value from `max_ammo`. A player with two weapons using different ammo gets both.
5. If the player left, died or changed role during the delay, nothing happens.

### ⚠️ Good to know
- The plugin **sets** the ammo amount (it doesn't add to the existing amount) and only handles ammo types present in `max_ammo`.
- A player without a firearm gets no ammo.
- The `Spawned` event also fires for spawns other than respawn waves (round start, force-class, **escaping SCPs becoming Chaos** in ScpEscapeSystem). Every MTF/Chaos player with a firearm gets ammo.
- Set `max_ammo` according to the ammo limits on your server. If the server has a lower limit, the game may clamp the value.

## 📋 Requirements

- An **SCP: Secret Laboratory** server with **EXILED 9.x** (built against **9.12.6**, .NET Framework 4.8).
- No additional plugins are required.

## 📥 Installation

1. Install EXILED on your server if you haven't already.
2. Download `AmmoBalance.dll` from the **Releases** tab.
3. Copy the file into the plugins folder:
   - Windows: `%AppData%\EXILED\Plugins`
   - Linux: `~/.config/EXILED/Plugins`
4. Restart the server. The plugin generates its config on first start.
5. Edit the `ammo_balance` section in `EXILED/Configs/<PORT>-config.yml` (or in `EXILED/Configs/Plugins/ammo_balance/` if separate configs are enabled) and restart the server.

### 🔄 Updating from older versions (`MatysiakAmmoWipe` / `FreshAmmoRules`)
1. **Remove the old DLL** (`MatysiakAmmoWipe.dll` or `FreshAmmoRules.dll`) from `EXILED/Plugins`. If both stay, they will set ammo in parallel.
2. Install `AmmoBalance.dll`.
3. **Delete the old config** (the `fresh_ammo` section). The format changed (`RoleAmmoLimits` → `max_ammo`) and the prefix is now `ammo_balance`.

## 🛠️ Configuration

```yaml
ammo_balance:
  is_enabled: true
  debug: false

  # Delay (s) after spawn, so the game can finish giving the default inventory
  delay: 1

  # Roles skipped by the plugin
  excluded_roles:
  - FacilityGuard

  # Ammo amount set per ammo type (match your server's limits)
  max_ammo:
    Nato9: 200
    Nato556: 200
    Nato762: 200
    Ammo12Gauge: 54
    Ammo44Cal: 48
```

| Option | Description |
| :--- | :--- |
| `delay` | Wait time after spawn. If something overwrites the ammo after the plugin, increase it. |
| `excluded_roles` | List of roles (`RoleTypeId`) the plugin ignores. Remove `FacilityGuard` if guards should get ammo too. |
| `max_ammo` | `ammo type: amount` dictionary. Types missing here are not set. |

## 🧪 Debugging

Set `debug: true`. When an MTF/Chaos player spawns, the logs will show lines like:
```
Nick (NtfSergeant): GunFRMG0 -> Nato556 = 200
```
If the ammo is wrong or missing, open an Issue with the logs and your EXILED and SCP:SL versions.

## 🔨 Building from source

1. Clone the repository:
   ```bash
   git clone https://github.com/TwojaNazwaUzytkownika/AmmoBalance.git
   ```
2. Open the project in **Visual Studio 2022** (target: **.NET Framework 4.8**).
3. Add references to EXILED (the `ExMod.Exiled` NuGet package or `Exiled.API.dll` and `Exiled.Events.dll`) and to the game libraries from `SCPSL_Data/Managed`.
4. Set the platform to **Any CPU** or **x64** (not ARM64). MSB3270 warnings with Any CPU are normal.
5. Build in `Release` mode. The file is at `bin/Release/net48/AmmoBalance.dll`.

## 🐛 Reporting bugs

Found a bug or have an idea? Open an issue in the **Issues** tab. Pull Requests are welcome.

---

## 📜 License / Licencja

This project is licensed under **MIT**. See the `LICENSE` file. / Projekt jest objęty licencją **MIT**, szczegóły w pliku `LICENSE`.

<div align="center">
  Created with ❤️ by <b>Matysiak</b>.
</div>
