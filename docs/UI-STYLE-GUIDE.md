# Fanfoot UI Style Guide

## Framework
The UI is a React + TypeScript SPA in `src/Fanfoot.Web/ClientApp`. Use accessible native HTML controls, React Router links, and the reusable styles in `src/styles.css`. Do not add Bootstrap or a component library.

## Brand Colors

| Name | Hex | Usage |
|------|-----|-------|
| Honolulu Blue | `#0076B6` | Primary buttons, links, accents |
| Silver | `#B0B7BC` | Muted text |
| White | `#FFFFFF` | Light surfaces |
| Black | `#000000` | Dark theme basis |

## Themes
Use the CSS variables declared in `styles.css`. Light mode is the white/light-blue Lions palette; dark mode is the black/silver alternate palette.

The app loads the saved preference from `GET /api/me/preferences`, assigns `data-theme="dark"` to `html`, and persists changes through `PUT /api/me/preferences`. New components should consume shared variables such as `--bg`, `--surface`, `--text`, `--muted`, and `--line` rather than hard-coded colors.

## Layout

- Keep the responsive app bar with links to Teams and AI Chat, a theme control, and sign out.
- Keep content within `.page`; use `.card` for grouped content.
- Use semantic headings (`h1` for page title, `h2` for sections) and native tables in `.table-wrap` for tabular data.
- Use `<button>`, `<input>`, `<select>`, and `<label>` with visible labels. Do not replace these with click handlers on generic elements.

## Components

- Primary actions use `.button` or the default blue button style.
- Secondary actions use `.secondary`; compact table actions use `.small`.
- Loading states use `.loading`; errors use `.alert`.
- Chat messages use `.bubble.user` and `.bubble.assistant`, preserving auto-scroll when messages change.
- Tables must remain horizontally scrollable on small screens through `.table-wrap`.

## Responsive Behavior

The app bar, page padding, chat height, and message width have mobile rules at `600px`. Preserve those rules when changing navigation or page layouts. Prefer wrapping controls over horizontal overflow outside tables.

## API Boundary

React code calls same-origin `/api` endpoints only through `src/api.ts`. The client sends cookie credentials and obtains an antiforgery token for unsafe requests. Do not expose Sleeper, FantasyCalc, ESPN, LLM, persistence, or server domain models to the browser.
