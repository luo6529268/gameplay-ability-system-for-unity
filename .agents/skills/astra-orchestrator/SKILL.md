---
name: astra-orchestrator
description: Orchestrate NTSD 2.8-Logan Unity battle-alignment work with GPT-6 Astra as the root agent and specialized Astra subagents.
---

# NTSD Astra Orchestrator

Use GPT-6 Astra for the root agent and all specialized roles. The root owns
scope, authority decisions, integration, and final verification. Follow the
repository `AGENTS.md`, `CLAUDE.md`, and `docs/ai/CURRENT-AUTHORITY.md`.

## Default sequence

1. `explorer` maps the smallest relevant Unity and authority path.
2. The root defines one bounded change and its Change Record.
3. `worker` edits only the assigned files when implementation is authorized.
4. `tester` runs focused compile, self-check, trace, or runtime validation.
5. `reviewer` performs an independent read-only review when risk warrants it.

Keep at most two concurrent subagents. Never assign overlapping files to
multiple workers.

## NTSD alignment safeguards

- Use the current NTSD 2.8-Logan formal EXE and its declared playable source
  closure as battle-rule authority. Mark unconfirmed behavior as pending.
- Read `docs/ai/CURRENT-AUTHORITY.md` before authority-sensitive work.
- Preserve the 33 ms normal cadence, pass order, state/lifecycle contracts,
  Unity GUIDs, meta files, asmdef boundaries, package locks, and protected data.
- Keep Unity presentation separate from deterministic battle truth.
- Do not modify `Assets/NTSD/Scripts/Gen/` or `Assets/Plugins/` unless the user
  explicitly authorizes it.
- Before script edits, create or update the required Change Record and Ledger.
- Prefer the narrowest project-local reversible change; do not invent behavior
  to make a test pass.

## Validation gate

The root must inspect the final diff and report each applicable evidence level:

1. compile status;
2. focused self-check or test status;
3. targeted runtime or trace status;
4. formal EXE/source alignment status.

Compilation alone is not proof of alignment. Report actual logs, blocked
prerequisites, first differences, and any manual Play or two-client checks
that were not run. For shutdown, queue, worker, pool, renderer, or cache work,
apply the repository's ordered-shutdown contract and require focused evidence.
