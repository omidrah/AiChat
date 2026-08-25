---
name: AiChat
description: Private, self-hosted Persian AI chat with live streaming
colors:
  royal-iris: "#6366f1"
  royal-iris-strong: "#4f46e5"
  royal-iris-soft: "#eef2ff"
  orchid-violet: "#8b5cf6"
  focus-ring: "#a5b4fc"
  app-bg: "#f4f5f9"
  surface: "#ffffff"
  surface-2: "#f8fafc"
  surface-3: "#f1f5f9"
  border: "#e5e7eb"
  border-strong: "#d1d5db"
  text: "#0f172a"
  muted: "#64748b"
  muted-2: "#94a3b8"
  neutral-soft: "#e5e7eb"
  on-primary: "#ffffff"
  success: "#10b981"
  danger: "#ef4444"
  warning: "#f59e0b"
  warning-strong: "#d97706"
  grad-success: "linear-gradient(135deg, #10b981 0%, #059669 100%)"
  sidebar-bg: "#1f2430"
  sidebar-text: "#ffffff"
  sidebar-muted: "#9e9eb4"
  code-bg: "#f1f5f9"
  pre-bg: "#111827"
typography:
  display:
    fontFamily: "Vazirmatn, Tahoma, 'Segoe UI', sans-serif"
    fontSize: "20px"
    fontWeight: 800
  title:
    fontFamily: "Vazirmatn, Tahoma, sans-serif"
    fontSize: "14px"
    fontWeight: 600
  body:
    fontFamily: "Vazirmatn, Tahoma, sans-serif"
    fontSize: "15px"
    fontWeight: 400
    lineHeight: 2
  label:
    fontFamily: "Vazirmatn, Tahoma, sans-serif"
    fontSize: "12px"
    fontWeight: 500
rounded:
  xs: "8px"
  sm: "12px"
  md: "16px"
  lg: "24px"
  pill: "999px"
spacing:
  xs: "4px"
  sm: "8px"
  md: "12px"
  lg: "16px"
  xl: "24px"
components:
  button-primary:
    backgroundColor: "{colors.royal-iris}"
    textColor: "#ffffff"
    rounded: "{rounded.sm}"
    padding: "0 14px"
    height: "44px"
  button-primary-hover:
    backgroundColor: "{colors.royal-iris-strong}"
  message-user:
    backgroundColor: "{colors.royal-iris}"
    textColor: "#ffffff"
    rounded: "18px"
  message-ai:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.text}"
    rounded: "18px"
  composer:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.text}"
    rounded: "{rounded.lg}"
    padding: "10px"
  status-pill:
    backgroundColor: "{colors.surface-3}"
    textColor: "{colors.text}"
    rounded: "{rounded.pill}"
---

# Design System: AiChat

## Overview

**Creative North Star: "The Night Desk"**

AiChat is a private, self-hosted Persian AI assistant — a calm desk lamp over dark wood, with one glowing accent lighting the work. The interface pairs a vivid indigo-violet energy in everything that acts (buttons, active items, the assistant's voice) with refined, restrained surfaces everywhere else. Light mode is the bright canvas: near-white surfaces (`#f4f5f9` background), crisp slate text, and the signature always-dark navy sidebar anchoring the right edge like the desk itself. Dark mode inverts the canvas but keeps the same accent and the same sidebar — the glow is what carries through both.

Density is workmanlike and spacious: generous bubble padding, airy message rhythm (`line-height: 2`), and a composer that reads as a single calm object. Motion is quiet — 150–250ms ease transitions, hover lifts of 1px, a pulse only where status demands attention. The system is proudly RTL; right-alignment and mirrored radii are invariants, not settings.

**Key Characteristics:**
- Vivid indigo-violet gradients reserved for primary actions and the active state — the "glow" is the product's energy source
- Refined, restrained surfaces: tonal layering over borders, soft shadows only where elements float
- Always-dark navy sidebar in both themes — the Night Desk's dark wood
- Persian RTL-first layout with Vazirmatn as the only voice
- Streaming AI as the centerpiece: status pills, thinking indicator, and instant cancel

## Colors

A two-accent system: Royal Iris carries action and identity, Orchid Violet is its gradient partner. Everything else is neutral slate. The palette is tuned for both themes — the light set is canonical in tokens; dark equivalents live under `body.dark`.

### Primary
- **Royal Iris** (#6366f1): the primary action color — buttons, links, active states, avatars. Never decorative; it marks what responds.
- **Royal Iris Strong** (#4f46e5): hover/pressed deepening of primary.
- **Royal Iris Soft** (#eef2ff; dark `#2a2f4a`): hover washes behind icon buttons, selected-tint backgrounds.
- **Focus Ring** (#a5b4fc): the keyboard-focus outline everywhere — the one color that exists solely for accessibility.

### Secondary
- **Orchid Violet** (#8b5cf6): the second stop of every primary gradient (`linear-gradient(135deg, #6366f1, #8b5cf6)`). Never used alone; it only completes the glow.

### Neutral
- **Canvas** (`app-bg` #f4f5f9; dark `#0f1117`): the page background.
- **Surface** (#ffffff; dark `#1b1f2a`): cards, chat bubbles, composer, topbar.
- **Surface 2 / 3** (#f8fafc / #f1f5f9; dark `#222736` / `#2a3040`): tonal steps for hover washes and secondary backgrounds.
- **Border** (#e5e7eb; dark `#2e3546`): hairlines between surfaces. **Border Strong** (#d1d5db) for emphasized edges.
- **Text** (#0f172a; dark `#e7eaf0`) and **Muted** (#64748b; dark `#9aa3b5`): the complete type ramp. **Muted 2** (#94a3b8; dark `#64748b`) is the quieter tier — field icons, placeholders, disabled text. **Neutral Soft** (#e5e7eb; dark `#2a3040`) fills ghost/neutral buttons (cancel).
- **On Primary** (`on-primary` #ffffff with `on-primary-soft`/`on-primary-muted` white-alpha steps): text and fills that sit on gradients — the brand panel, login submit, buttons over color. Never used on neutral surfaces.
- **Sidebar** (`sidebar-bg` #1f2430 gradient to #181c26; dark deepens to #141821→#0e1118): always dark, with white text, muted `#9e9eb4`, and white-alpha hover/border (0.06–0.08).

### Semantic
- **Success** (#10b981), **Danger** (#ef4444), **Warning** (#f59e0b): status and destructive actions. **Warning Strong** (#d97706) deepens warning hovers (edit button). **Success Gradient** (`grad-success`, #10b981→#059669) is the one allowed non-primary gradient — the add-user create action, paired with its own green glow (`--glow-success`). Danger is the only semantic color that ever becomes a solid button fill.

### Named Rules
**The Glow Rule.** The indigo-violet gradient is reserved for three things: primary buttons, the active conversation, and the user's own messages/avatar. If an element isn't one of those, it uses neutral surfaces. Rarity is the point.

**The Night Desk Rule.** The sidebar is always dark, in both themes, no exceptions. The content canvas carries the theme; the sidebar is the constant.

## Typography

**Display Font:** Vazirmatn (with Tahoma, 'Segoe UI' fallbacks)
**Body Font:** Vazirmatn
**Label/Mono Font:** Vazirmatn for UI; Consolas/Monaco for code blocks

**Character:** A single Persian sans voice — warm, geometric, humanist — carrying both display and body. No secondary face: hierarchy comes from weight (400/500/700/800) and size, never from a second family. Code is the lone exception, and it flips to LTR with its own dark slab (`pre-bg` #111827).

### Hierarchy
- **Display** (800, 20px, default): the sidebar brand wordmark. The single gradient-text element in the product (brand exception).
- **Title** (600, 14px, 1.5): conversation names, section-level labels, active nav.
- **Body** (400, 15px, 2): chat messages — generous line height for reading comfort; long-form content lives in 65–75ch max-width bubbles.
- **Label** (500, 12px, 1): message metadata (model name), timestamps, small captions. The 10px welcome text ("خوش آمدید،") is the floor.

### Named Rules
**The One Voice Rule.** Every visible string is Vazirmatn — enforced globally, buttons and inputs included. No system fonts sneak into UI text; code blocks are the only permitted exception.

**The RTL Mirror Rule.** Text aligns right; measure and radius mirror accordingly. Code blocks and numbers flip to LTR internally but never break the surrounding RTL flow.

## Layout

The shell is a fixed right-edge sidebar (380px, collapsible to 70px icon rail) with a 64px topbar and a fluid chat canvas. The chat column centers on a 900px max-width measure for message bubbles and the composer; everything beyond that is breathing room.

- **Spacing rhythm:** 4 / 8 / 12 / 16 / 24px scale (`--spacing-*`). Groups sit at 12–16px; separation between distinct blocks at 16–24px.
- **Message rhythm:** 22px vertical gap between bubbles; 32px/24px outer padding with a 150px bottom cushion so the composer never crowds the last message.
- **RTL:** `direction: rtl` on html and every surface; flex rows flow right-to-left; the composer's send button sits at the far left as the flow's terminus.
- **Responsive:** no fixed breakpoints — the sidebar collapses via a toggle (380→70px), and the chat measure is fluid (`min(100%, 900px)`).

## Elevation & Depth

Depth is **glow-driven**, per the design direction: the indigo-violet gradient and its colored shadow carry elevation for anything that acts. Neutral, floating elements (composer, delete popup, scroll-back button) use the ambient shadow trio — `--shadow-sm` (1px, barely-there), `--shadow` (10px 30px, cards/floating), `--shadow-lg` (24px 60px, overlays).

### Shadow Vocabulary
- **Ambient low** (`0 1px 2px rgba(15,23,42,.06)`): resting cards and bubbles — just enough to lift off the canvas.
- **Ambient** (`0 10px 30px rgba(15,23,42,.08)`): the composer, popups, floating controls.
- **Ambient high** (`0 24px 60px rgba(15,23,42,.18)`): the delete confirmation popup.
- **Glow** (`0 6px 16px rgba(99,102,241,.28–.45)`): primary buttons and the active conversation item. Hover escalates the glow, never the blur.
- **Glow primary** (`0 10px 24px rgba(99,102,241,.32)`, hover `.4`): the login submit's full-width primary glow.
- **Glow success** (`0 8px 18px rgba(16,185,129,.25)`): the add-user button's green glow.

### Named Rules
**The Glow-Not-Shadow Rule.** Active and primary elements elevate with the colored indigo glow. Neutral shadows are for neutral floats. The two never trade places.

## Shapes

Form language is generous-but-disciplined: 12px for controls and list rows, 16px for cards, 18px for chat bubbles, 24px for the composer, and full pills (999px) only for small status badges and tiny controls.

- **Bubbles:** 18px radius with the near-side corner (top-right, RTL-leading) cut to 6px — the classic chat tail hint, mirrored for RTL.
- **Buttons:** 12px with full-width block behavior in the sidebar; circular (46px) only for the send/stop pair in the composer.
- **Avatars:** always circles, 34px (sidebar) or 42px (chat).
- **Focus:** 2px `--focus-ring` outline, offset 2px — on every interactive element, never suppressed.
- **Borders:** hairlines only (`--border`), 1px, never decorative color.

## Components

### Buttons
- **Shape:** 12px radius, 44px height for primary; no border on primary.
- **Primary:** `--grad-primary` background, white text, 700 weight, full-width in the sidebar. Hover: `brightness(1.08)`, lift 1px, glow escalates to `0 10px 22px rgba(99,102,241,.45)`.
- **Danger:** solid `--danger` (#ef4444) — only for destructive actions (delete confirm, logout hover). Ghost state: transparent with `rgba(239,68,68,.5)` border and `#f87171` text, filling solid red on hover.
- **Icon actions:** 28px, transparent, `--sidebar-muted` icons that turn white on hover — the sidebar's quiet utility layer.
- **Focus:** 2px `--focus-ring` outline, offset 2px, on every variant.

### Message Bubbles
- **User:** `--grad-primary` background, white text, 18px radius with 6px top-right corner. The glow marks the user's own voice.
- **AI:** `--surface` background, `--text` color, 1px `--border`, same 18px radius / 6px corner.
- **Meta:** model name in 12px label style, dimmed; copy button fades in on hover (opacity 0.45→1, 1px lift), with a check-state confirmation.

### Composer
- **Style:** single floating object — `--surface`, 1px `--border`, 24px radius, `--shadow`, 10px internal padding.
- **Focus:** the container's border shifts to `--focus-ring` with a soft indigo glow (`0 8px 28px rgba(99,102,241,.14)`) — `:focus-within` on the whole bar.
- **Send/Stop:** 46px circles; send is `--grad-primary` with a paper-plane icon, stop is solid `--danger`; they swap on sending state. Textarea autogrows to 220px max.

### Model Selector
- **Style:** `--surface` background, 1px `--border`, 14px radius, 46px height, 14px text. Disabled state: 0.7 opacity. Sits inside the composer as the only persistent form control besides the textarea.

### Status Pill
- **Style:** pill (999px), 6px 12px padding, 0.8rem bold text with an 8px dot.
- **States:** online — green tint (`#ecfdf5` bg, `#10b981` dot); offline — red tint (`#fef2f2`, `#ef4444` dot); reconnecting — amber tint with a pulsing dot (1s alternate opacity).

### Conversation List Item
- **Structure:** a real `<ul>`/`<li>` list — each row is a flex container (12px radius, 6px/4px padding, `--sidebar-hover` wash on hover). The row itself is not the click target.
- **Open button:** a dedicated full-width `<button class="open-btn">` (transparent, 10px radius, icon + title) carries the open action — genuine button semantics, native Enter/Space, 2px `--focus-ring` outline (offset −2px). Disabled while the row is being renamed.
- **Active:** the full indigo-violet gradient background with white text and the glow shadow — the Night Desk's lit drawer. Icon and actions shift to `rgba(255,255,255,.9)`.
- **Actions:** white-alpha icon buttons (28px, 8px radius) appearing on hover — rename/delete. During rename, ok/cancel replace them and an inline input takes the width (1px `--accent` border, `--focus-ring` on focus).
- **States:** loading — centered spinner with "در حال بارگذاری…"; empty — a `--sidebar-muted` message plus a ghost create button (`--sidebar-border` + `--sidebar-hover` fill, 10px radius).
- **Delete popup:** an `alertdialog` anchored `top: calc(100% + 4px)` below the row, `--danger` confirm / white-alpha cancel, Escape dismisses from the row.
- **Keyboard:** Enter/Space are native button behavior on the open button; Escape on the row dismisses the delete popup.

### Avatar
- **Style:** circle, `--grad-primary` background, white initials (first two characters, uppercase), 700 weight, glow shadow. AI avatars use `--surface-3` instead — the assistant is quiet gray, the user is the glow.

## Do's and Don'ts

### Do:
- **Do** pull every color, radius, and shadow from the `:root` tokens in `styles.css` — hardcoded hex is drift.
- **Do** reserve the indigo-violet gradient for primary actions, the active conversation, and the user's own voice.
- **Do** keep the sidebar dark in both themes; it is the product's constant.
- **Do** set `direction: rtl` on every new surface and mirror radii and alignment accordingly.
- **Do** use Vazirmatn for all UI text, and flip code blocks to LTR with `--pre-bg`.
- **Do** show a visible 2px `--focus-ring` outline on every interactive element.
- **Do** use Font Awesome for all icons — the system has no emoji or unicode-glyph placeholders.

### Don't:
- **Don't** use gradient text outside the sidebar brand wordmark — the logo is the single exception.
- **Don't** add gradient backgrounds to anything that isn't primary/active/user-voice; neutrality is how the glow stays meaningful.
- **Don't** introduce a second font family for UI; hierarchy is weight and size, not a new face.
- **Don't** use neutral shadows on active elements or glow on neutral floats — they serve different elevation jobs.
- **Don't** suppress or recolor focus outlines; accessibility tokens are not decorative.
- **Don't** ship English/LTR UI copy — the product speaks Persian, RTL, end to end.
