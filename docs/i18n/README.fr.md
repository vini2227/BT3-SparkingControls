[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md)

[Español (España)](README.es.md) · [العربية](README.ar.md) · [Bahasa Indonesia](README.id.md) · [Español (Latinoamérica)](README.es-419.md)

[Português (Brasil)](README.pt.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [简体中文](README.zh-Hans.md)

[繁體中文](README.zh-Hant.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [ไทย](README.th.md)

---

# BT3 Sparking Controls — 1.0.6 Beta

## Commandes et guide d’installation

Des commandes et un HUD inspirés de Sparking! ZERO pour Tenkaichi3Decomp. Les animations, dégâts, coûts en Ki, portée, capacités et fenêtres d’action restent ceux de BT3. Le jeu et ses ressources ne sont pas fournis.

## Nouveautés de la version 1.0.6 Beta

- Windows et Linux : les quatre menus restent à gauche dans la vue de chaque joueur, avec une taille stable adaptée au 4:3, 16:9, 16:10 et ultralarge. Windows : contrôles de compatibilité adaptatifs et détection automatique du Decomp original, Char Package et Tag Team, données des personnages personnalisés et jusqu’à quatre manettes locales. Linux 1.0.6 apporte la nouvelle disposition mais conserve sa bibliothèque et son résolveur, testés avec v0.1.37/v0.1.38.1 ; les variantes Windows ne sont pas prises en charge. Avec quatre vues Tag Team, les indications de boost natives restent sans superposition supplémentaire. Char Package désactive lui-même les personnages personnalisés en ligne. Les tests automatisés ne remplacent pas des parties réelles ; Linux natif et le rollback en ligne restent non vérifiés. Aucune garantie pour toutes les versions futures ou contre toute désynchronisation.

[CHANGELOG](../../CHANGELOG.md)

## Téléchargements et démonstration

[**Windows x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.6-beta/BT3-SparkingControls-1.0.6-Beta-Windows.zip) · [**Linux x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.6-beta/BT3-SparkingControls-1.0.6-Beta-Linux-x64.zip) · [**1.0.6 Beta**](https://github.com/vini2227/BT3-SparkingControls/releases/tag/v1.0.6-beta)

[▶ Gameplay demo (recorded for 1.0 Beta) — MP4, 167 MiB](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Gameplay-Demo.mp4) · [TXT](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.6-beta/BT3-SparkingControls-Controls-and-Setup-Guide.txt) · [SHA256](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.6-beta/SHA256SUMS.txt)

## Installation

### Windows x64

Extrayez le ZIP Windows, lancez `BT3-SparkingControls.exe`, cliquez sur **Browse**, sélectionnez `Tenkaichi3Decomp.exe`, choisissez la langue et les options puis **Save and start**. Lancez toujours le jeu avec l’assistant. Le programme Windows intègre ses ressources et peut être partagé seul pour une utilisation normale ; le destinataire doit posséder le jeu. Le ZIP contient aussi les sources et la licence. Aucun modloader, remplacement des fichiers du jeu ou recompilation du jeu n’est nécessaire.

`%LOCALAPPDATA%\BT3-SparkingControls`

### Linux x64

Extrayez tout le ZIP Linux x64 et gardez ensemble le programme géré, `libbt3sparking.so`, les profils et les ressources. Installez Mono avec Windows Forms/System.Web.Extensions, libgdiplus et les polices nécessaires. Utilisez X11 avec composition ou un XWayland fonctionnel. Dans ce dossier, exécutez `bash start.sh`, sélectionnez le jeu **natif Linux** puis Save and start. Le programme .exe utilise Mono, sans Wine/Proton. Wayland natif n’est pas pris en charge. La bibliothèque cible glibc 2.35 ; un autre système peut nécessiter `bash build.sh`. Ne lancez pas une autre copie du jeu séparément.

`$XDG_DATA_HOME/BT3-SparkingControls/linux` · `~/.local/share/BT3-SparkingControls/linux`

`linux-runtime` · `linux-online-v0.1.37` → `linux`

```bash
bash start.sh --check /path/to/Tenkaichi3Decomp
```

## Commandes

□ = Carré ; △ = Triangle ; ○ = Rond ; LS = stick gauche. LS et les flèches désignent le stick, pas la croix directionnelle. La navigation des menus ordinaires reste celle du jeu.

| Touche / combinaison | Action |
| --- | --- |
| `□` | Attaques Rush |
| `△` | Tirs de Ki |
| `X` | Déplacements et dash natifs |
| `R2` | Charger le Ki |
| `L1` | Monter |
| `L2` | Descendre |
| `R2 + L1` | Z Dash vertical vers le haut |
| `R2 + L2` | Z Dash vertical vers le bas |
| `R1` | Garde hors de vos combos compatibles |
| `R2 + □` | Super attaque 1 |
| `R2 + △` | Super attaque 2 |
| `R2 + ○` | Attaque ultime, selon les conditions natives |
| `R2 + D-pad ↑ / ↓` | Compétence 1 |
| `R2 + D-pad ← / →` | Compétence 2 |
| `○` | Posture de contre native |
| `R1 (combo)` | Téléportation avant native pendant les combos compatibles |
| `R1 + LS (combo)` | Téléportation directionnelle native pendant ces combos |
| `R1 + □` | Projection normale |
| `R1 + LS↓ + □` | Projection au sol alternative, stick uniquement |
| `LS↑ + △` | Projection au sol avec la commande originale |
| `R3` | Emergency Blaster Wave, selon les conditions natives |
| `L3` | Rechercher / verrouiller la cible |
| `Select / View` | Changer de caméra |
| `D-pad ↑` | Maintenir : transformations/fusions |
| `D-pad ←` | Maintenir : changement de coéquipier |
| `D-pad ↓ / → (no R2)` | Bloqués en combat pour éviter les commandes accidentelles |

Les attaques peuvent être choisies lorsque R2 est déjà maintenu. Pour charger une attaque spéciale, continuez à maintenir sa touche d’attaque ; relâcher R2 seul ne la relâche pas. Lors d’une demande de boost, □ + direction indiquée transmet la commande d’origine. Le symbole remplace seulement le Triangle et laisse la main visible. Les raccourcis de compétences ne prétendent pas reproduire exactement ceux de ZERO.

○ demande le contre de BT3, prioritaire sur les attaques maintenues pendant les combos compatibles ; ce n’est pas le système complet Perception/Super Perception de ZERO. Les téléportations de R1 gardent leurs coûts et fenêtres natifs, sans garantir une arrivée derrière l’adversaire ou une position immobile. R1 + □ est prioritaire et produit X–relâchement–X, soumis à la portée et aux fenêtres du jeu. Le double X original reste disponible. La projection au sol n’utilise pas la croix ↓ ; LS↑ + ○ ne la déclenche pas. R3 utilise les conditions et le coût en vie de BT3, pas le Revenge Counter de ZERO basé sur les points de compétence. Les anciennes commandes physiques R3 de transformation/fusion sont consommées ; les cartes génèrent les entrées requises.

## Transformations, fusion et changement

Maintenez la croix ↑ puis appuyez sur □/△/X/○ indiqué sur la carte ; relâchez ↑ pour fermer. L1/R1 alternent entre transformations et fusions si les deux existent. Les catégories vides sont ignorées ; une fusion seule s’ouvre directement. Maintenez la croix ← pour changer de coéquipier. Partenaire, forme, jauges, délais et état du combat restent soumis à BT3. Positions fixes, emplacements vides et barres de vie des réserves sont conservés. En ligne, les panneaux locaux restent à gauche même pour le Joueur 2, sans changer le côté logique des entrées.

## HUD et préférences

Maintenez R2 pour les attaques, commandes et coûts. **Centered** équilibre centre et bords ; **Expanded** écarte les panneaux ; **Classic (4:3)** utilise la zone 4:3 d’origine. Le 4:3 natif est préservé ; l’ultralarge utilise des zones sûres. La liste peut être masquée ; son cadre externe est désactivé par défaut pour les nouvelles préférences. Les ressources HD disponibles sont prioritaires, sinon les originales, sans pack de textures fourni. Le lanceur sombre propose boutons arrondis, boule à trois étoiles, aperçu et 16 langues avec arabe RTL ; noms du jeu inchangés. **F6** : attaques ; **F7** : cadre ; **F8** : placement. Cliquez sur un raccourci pour modifier, désactiver ou rétablir la valeur par défaut ; doublons refusés. Ils exigent le focus du jeu et ne se répètent pas au maintien. Le verrou bloque les changements en combat. Chemin, langue, préférences et changements en jeu persistent après redémarrage ou mise à jour. Resélectionnez le chemin si le jeu est déplacé.

## Compatibilité, jeu en ligne et tests

Windows et Linux : les quatre menus restent à gauche dans la vue de chaque joueur, avec une taille stable adaptée au 4:3, 16:9, 16:10 et ultralarge. Windows : contrôles de compatibilité adaptatifs et détection automatique du Decomp original, Char Package et Tag Team, données des personnages personnalisés et jusqu’à quatre manettes locales. Linux 1.0.6 apporte la nouvelle disposition mais conserve sa bibliothèque et son résolveur, testés avec v0.1.37/v0.1.38.1 ; les variantes Windows ne sont pas prises en charge. Avec quatre vues Tag Team, les indications de boost natives restent sans superposition supplémentaire. Char Package désactive lui-même les personnages personnalisés en ligne. Les tests automatisés ne remplacent pas des parties réelles ; Linux natif et le rollback en ligne restent non vérifiés. Aucune garantie pour toutes les versions futures ou contre toute désynchronisation.

## Sources et licence

Sources et licence AGPL-3.0 incluses dans les deux ZIPs. Les polices conservent leurs licences. Aucun jeu, pack de textures ou réglage personnel n’est fourni. Projet de fans non officiel ; les marques appartiennent à leurs titulaires.

- [Windows — BUILD.md](../../desenvolvimento/BUILD.md)
- [Linux — README.md / build.sh](../../linux/README.md)
- [AGPL-3.0](../../LICENSE-AGPL-3.0.txt)
- [English TXT](../Controls-and-Setup-Guide.txt)
