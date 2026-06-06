# Fantfoot UI Style Guide

## Framework
**MudBlazor v9.5.0** — all UI components come from MudBlazor. Bootstrap is not used.

---

## Brand Colors

| Name | Hex | Usage |
|------|-----|-------|
| Honolulu Blue | `#0076B6` | Primary — buttons, links, accents, borders |
| Silver | `#B0B7BC` | Secondary — muted text, subtle elements |
| White | `#FFFFFF` | Light mode background, dark mode text |
| Black | `#000000` | Dark mode background, AppBar |
| Light Blue | `#4AA3D4` | Lighter blue for dark mode icons |

---

## Themes

### Light Mode — White Away Jersey
Inspired by the Lions' white road uniform.

| Element | Color |
|---------|-------|
| Background | `#FFFFFF` |
| Surface (cards, tables) | `#FFFFFF` |
| AppBar | `#0076B6` |
| Drawer | `#EBF4FB` (light blue tint) |
| Primary text | `#1C1F23` |
| Secondary text | `#5C6470` |
| Table stripe | `#F0F7FC` |
| Table hover | `#D6EEFA` |
| Dividers | `#E2E6EA` |

### Dark Mode — Black Alternate Jersey
Inspired by the Lions' black alternate uniform.

| Element | Color |
|---------|-------|
| Background | `#000000` |
| Surface (cards, tables) | `#000000` |
| AppBar | `#000000` + `2px #0076B6` bottom border |
| Drawer | `#000000` + `2px #0076B6` right border |
| Primary text | `#FFFFFF` |
| Secondary text | `#B0B7BC` (silver) |
| Table stripe | `#111111` |
| Table hover | `#0D2035` (dark blue tint) |
| Dividers | `#2A2A2A` |
| AI chat bubble | `#1E2530` (dark navy) |

---

## Dark Mode Implementation

Dark mode is **not** handled by MudBlazor theme variables alone (they don't propagate to `html`/`body` in v9). Instead:

1. **JS function** `fantfoot.setBackground(isDark)` in `App.razor` sets inline styles on `html` and `body`, and toggles the `body.dark-mode` CSS class.
2. **CSS** in `app.css` uses `body.dark-mode` selectors to override typography, nav, and component colors.
3. **Preference** is saved per-user in the `UserPreferences` table (`UserId`, `IsDarkMode`, `UpdatedAt`) and loaded in `MainLayout.OnAfterRenderAsync`.

**Rule:** For any new dark-mode-specific overrides, add a `body.dark-mode` rule in `app.css`. Do not use inline conditional styles for color.

---

## Layout

```
MudLayout
├── MudAppBar (Dense, Elevation 2)
│   ├── Hamburger menu toggle
│   ├── "Fantfoot" title
│   ├── [Spacer]
│   ├── Dark/Light mode toggle icon
│   └── Sign Out button
├── MudDrawer (ClipMode: Always, Elevation 0)
│   └── MudNavMenu
│       ├── MudNavLink — Home
│       └── MudNavLink — AI Chat
└── MudMainContent
    └── MudContainer (MaxWidth: False, pa-4)
        └── [Page content]
```

---

## Typography

Use `MudText` with `Typo.*` for all text. Never use raw `<h1>`–`<h6>` tags in pages.

| Use | Component |
|-----|-----------|
| Page title | `<MudText Typo="Typo.h4">` |
| Section heading | `<MudText Typo="Typo.h5">` |
| Subsection | `<MudText Typo="Typo.h6">` |
| Muted/secondary text | `<MudText Color="Color.Secondary">` |
| Body | `<MudText Typo="Typo.body1">` or `Typo.body2` |

---

## Components

### Tables
Always use `MudTable<T>` with `Striped="true" Hover="true" Dense="true" Elevation="1"`.

```razor
<MudTable T="MyType" Items="@items" Striped="true" Hover="true" Dense="true" Elevation="1">
    <HeaderContent>
        <MudTh>Column</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="Column">@context.Property</MudTd>
    </RowTemplate>
    <NoRecordsContent>
        <MudText Color="Color.Secondary">No records found.</MudText>
    </NoRecordsContent>
</MudTable>
```

### Cards
Use `MudCard` with `Elevation="1"`. Tables inside cards use `Elevation="0"` and `Class="pa-0"` on `MudCardContent`.

```razor
<MudCard Elevation="1" Class="mb-3">
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">Title</MudText>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent Class="pa-0">
        <MudTable Elevation="0" ...>
```

### Buttons
| Use | Component |
|-----|-----------|
| Primary action | `<MudButton Variant="Variant.Filled" Color="Color.Primary">` |
| Secondary/nav | `<MudButton Variant="Variant.Outlined">` |
| Destructive | `<MudButton Variant="Variant.Filled" Color="Color.Error">` |
| Icon action | `<MudIconButton Icon="@Icons.Material.Filled.X" Color="Color.Inherit">` |

### Loading States
```razor
<MudProgressCircular Indeterminate="true" Color="Color.Primary" />
```

### Alerts / Errors
```razor
<MudAlert Severity="Severity.Error" Class="mb-4">Message</MudAlert>
```

### Status Badges
Use `MudChip<T>` with appropriate `Color`:
- `Color.Error` → Out
- `Color.Warning` → Questionable / Doubtful
- `Color.Default` → IR / other

### Dropdowns / Selects
```razor
<MudSelect T="string" @bind-Value="selected" Dense="true" Margin="Margin.Dense" Variant="Variant.Outlined">
    <MudSelectItem T="string" Value="@("")">All</MudSelectItem>
</MudSelect>
```
Use `Label="..."` on `MudMenu` for dropdown menus (not `ActivatorContent`/`ChildContent`).

### Form Inputs
```razor
<MudTextField T="string" @bind-Value="value" Variant="Variant.Outlined" Margin="Margin.Dense" />
```

---

## Chat Bubbles

Defined as CSS classes in `app.css` — do not use inline styles.

| Class | Use |
|-------|-----|
| `.chat-bubble-user` | User message — Honolulu Blue background, white text |
| `.chat-bubble-ai` | AI message — light grey (light) / dark navy `#1E2530` (dark) |

```razor
<div class="@(msg.IsUser ? "chat-bubble-user" : "chat-bubble-ai")">@msg.Content</div>
```

---

## Navigation

Use `MudNavLink` inside `MudNavMenu`. Always specify `Icon` from `Icons.Material.Filled.*`.

```razor
<MudNavMenu>
    <MudNavLink Href="" Match="NavLinkMatch.All" Icon="@Icons.Material.Filled.Home">Home</MudNavLink>
</MudNavMenu>
```

---

## Page Layout Pattern

Every page follows this structure:

```razor
@page "/route"

<PageTitle>Page Name</PageTitle>

@if (data == null)
{
    <MudProgressCircular Indeterminate="true" Color="Color.Primary" />
    return;
}

<!-- Page header: title + actions -->
<div style="display:flex; align-items:flex-start; justify-content:space-between; flex-wrap:wrap; gap:0.5rem; margin-bottom:1.5rem;">
    <div>
        <MudText Typo="Typo.h4">Title</MudText>
        <MudText Typo="Typo.body2" Color="Color.Secondary">Subtitle</MudText>
    </div>
    <div style="display:flex; gap:0.5rem;">
        <!-- Action buttons -->
    </div>
</div>

<!-- Content -->
```

---

## Spacing

MudBlazor utility classes for spacing:
- `Class="mb-2"`, `mb-3`, `mb-4` — margin bottom
- `Class="mt-4"` — margin top
- `Class="pa-4"` — padding all sides
- `Class="pa-0"` — remove padding (for card content wrapping a table)

Use `style="gap:0.5rem;"` or `style="gap:0.75rem;"` for flex gaps since MudBlazor's gap utilities may vary.
