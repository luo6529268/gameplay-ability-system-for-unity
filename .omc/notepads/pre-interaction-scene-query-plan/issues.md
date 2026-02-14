## 2026-02-12

- `lsp_diagnostics` repeatedly surfaced pre-existing warnings in touched files (e.g., unused usings / hidden members); these were mostly unrelated to this minimal change set.
- Tool wrapper occasionally aborted parallel calls when empty parameters were sent; direct per-file diagnostics calls are more reliable.
- New `Animation/Services` path failed immediate symbol resolution due to current project compile context; fallback to existing `Animation/Character` area and world-level service exposure avoided blocking progress.
