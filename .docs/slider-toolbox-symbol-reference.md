# Slider Toolbox Symbol Reference

This document lists the text values accepted by the `Symbol` and `Sequence` inputs in Slider Toolbox symbol groups.

For a more direct visual lookup table with rendered glyph previews, see [`slider_symbols/README.md`](slider_symbols/README.md).

## Basic Usage

- `Symbol`: generates one symbol for the selected symbol group.
- `Sequence`: generates multiple symbol sliders. Enter tokens separated by spaces, for example `A B C alpha beta gamma`.
- If `Sequence` is empty, Slider Toolbox uses the normal single-frame / animation behavior.
- If `Sequence` is not empty, each token becomes one slider. Time spacing uses `Frame duration` and `Frame gap`; canvas placement uses `Columns`, `Spacing X`, and `Spacing Y`.
- In sequence mode, `Frames` is ignored and the generated count is the number of sequence tokens. End-frame parameter values are also ignored; every symbol uses the start-side parameter values plus its grid offset.
- For grouped letter styles, enter a normal ASCII letter such as `A`, `B`, `x`, or `z`.
- For named symbol groups, enter one of the aliases below.
- Direct Unicode input is also accepted when the font contains the glyph.

## Letter Style Groups

These groups accept `A` through `Z` or `a` through `z`, depending on the selected group.

| Toolbox group | Input examples | Notes |
| --- | --- | --- |
| Latin uppercase | `A B C ... Z` | Plain Latin uppercase letters. |
| Latin lowercase | `a b c ... z` | Plain Latin lowercase letters. |
| Italic uppercase | `A B C ... Z` | Mathematical italic uppercase. |
| Italic lowercase | `a b c ... z` | Mathematical italic lowercase. `h` maps to Planck-style italic h. |
| Bold uppercase | `A B C ... Z` | Mathematical bold uppercase. |
| Bold lowercase | `a b c ... z` | Mathematical bold lowercase. |
| Bold italic uppercase | `A B C ... Z` | Mathematical bold italic uppercase. |
| Bold italic lowercase | `a b c ... z` | Mathematical bold italic lowercase. |
| Script uppercase | `A B C ... Z` | Mathematical script uppercase. |
| Script lowercase | `a b c ... z` | Mathematical script lowercase. |
| Fraktur uppercase | `A B C ... Z` | Mathematical fraktur uppercase. |
| Fraktur lowercase | `a b c ... z` | Mathematical fraktur lowercase. |
| Sans-serif uppercase | `A B C ... Z` | Mathematical sans-serif uppercase. |
| Sans-serif lowercase | `a b c ... z` | Mathematical sans-serif lowercase. |
| Sans-serif bold uppercase | `A B C ... Z` | Mathematical sans-serif bold uppercase. |
| Sans-serif bold lowercase | `a b c ... z` | Mathematical sans-serif bold lowercase. |
| Monospace uppercase | `A B C ... Z` | Mathematical monospace uppercase. |
| Monospace lowercase | `a b c ... z` | Mathematical monospace lowercase. |
| Blackboard letters | `A B C ... Z`, `a b c ... z` | Blackboard uppercase and lowercase letters. Common uppercase aliases include `N`, `Z`, `Q`, `R`, `C`. |

## Greek Letters

Greek groups accept the letter names below. The uppercase group maps names to uppercase Greek; the lowercase group maps names to lowercase Greek.

| Input token | Uppercase output | Lowercase output |
| --- | --- | --- |
| `alpha` | Alpha | alpha |
| `beta` | Beta | beta |
| `gamma` | Gamma | gamma |
| `delta` | Delta | delta |
| `epsilon` | Epsilon | epsilon |
| `zeta` | Zeta | zeta |
| `eta` | Eta | eta |
| `theta` | Theta | theta |
| `iota` | Iota | iota |
| `kappa` | Kappa | kappa |
| `lambda` | Lambda | lambda |
| `mu` | Mu | mu |
| `nu` | Nu | nu |
| `xi` | Xi | xi |
| `omicron` | Omicron | omicron |
| `pi` | Pi | pi |
| `rho` | Rho | rho |
| `sigma` | Sigma | sigma |
| `finalsigma` | - | final sigma |
| `tau` | Tau | tau |
| `upsilon` | Upsilon | upsilon |
| `phi` | Phi | phi |
| `varphi` | - | variant phi |
| `chi` | Chi | chi |
| `psi` | Psi | psi |
| `omega` | Omega | omega |

## Hebrew Letters

The Hebrew group accepts these aliases:

`aleph beth bet gimel gimmel daleth dalet he vav zayin chet tet yod kaf lamed mem nun samekh ayin pe tsadi qof resh shin tav`

`aleph`, `beth`, `gimel`, and `daleth` use the common mathematical letter codepoints.

## Named Math Letters

| Input token | Meaning |
| --- | --- |
| `ell` | Script small l. |
| `hbar`, `hslash` | Planck constant over two pi. |
| `planck` | Planck constant h. |
| `weierstrass`, `wp` | Weierstrass p. |
| `real`, `re` | Real part symbol. |
| `imaginary`, `imag`, `im` | Imaginary part symbol. |
| `angstrom` | Angstrom sign. |
| `kelvin` | Kelvin sign. |
| `ohm` | Ohm sign. |
| `mho` | Mho sign. |
| `prime` | Prime. |
| `doubleprime` | Double prime. |
| `tripleprime` | Triple prime. |
| `degree` | Degree sign. |

## Calculus

`int integral iint iiint oint sum sigma prod product coprod partial nabla grad sqrt infty infinity`

## Operators

`plus minus times div pm mp cdot star circ bullet oplus otimes wedge vee angle parallel perp`

## Relations

`eq neq ne approx sim simeq cong equiv le ge ll gg propto models vdash dashv`

## Set / Logic

`in notin ni empty emptyset union cup intersection cap subset superset subseteq superseteq forall exists nexists and or not therefore because`

## Arrows

`left up to right down leftright mapsto doubleleft doubleright doubleleftright hookleft hookright uparrow downarrow ne se sw nw`

## Delimiters

`leftparen rightparen leftbracket rightbracket leftbrace rightbrace angle leftangle rightangle floor leftfloor rightfloor ceil leftceil rightceil vertical vert doublevertical norm`

## Sequence Examples

```
A B C D E F G
```

```
alpha beta gamma delta epsilon
```

```
forall x in R exists y in C
```

```
int partial nabla sqrt infinity
```

Use a symbol group that matches the intended tokens. For example, choose `Greek lowercase` for `alpha beta gamma`, `Blackboard letters` for `R C Q`, or `Set / logic` for `forall exists subset`.
