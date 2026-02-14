# /init-deep Work Plan (Unity_GAS)

## TL;DR

> **Quick Summary**: Build/update a hierarchical `AGENTS.md` knowledge map in this Unity repo using **update mode**, **max depth 3**, and **first-party-only** scope.
>
> **Deliverables**:
> - Updated root `AGENTS.md` (non-destructive refresh)
> - New/updated child `AGENTS.md` files only where complexity threshold is met
> - Dry-run validation artifacts proving depth/scope/anti-duplication constraints
>
> **Estimated Effort**: Medium
> **Parallel Execution**: YES - 3 waves
> **Critical Path**: Discovery baseline -> scoring/placement -> generation -> validation

---

## Context

### Original Request
Run `/init-deep` planning flow with exhaustive search/analyze behavior and produce hierarchical AGENTS documentation strategy.

### Interview Summary
- Mode: **Update mode** (not create-new)
- Max depth: **3**
- Scope: **First-party only（含目标 NTSD 活跃迁移子树）**
- Exclude from scoring/placement: `.omc/`, `.sisyphus/`, vendor/third-party trees
- Verification strategy: **Dry-run checks**
- Accuracy: **Standard** (no Momus loop requested)

### Repository Findings Used
- Existing root conventions: `AGENTS.md`
- First-party target area:
  - `Assets/GAS/Runtime`, `Assets/GAS/Editor`, `Assets/GAS/General`
  - `Assets/NTSD/Scripts/Animation`, `Assets/NTSD/Scripts/Simulation`（来自 handoff 上下文）
- Known TODO signal in first-party: `Assets/GAS/Runtime/State/Internal/CompatibilityBridge.cs`

### Metis Review (applied)
- Lock explicit scoring threshold before generation.
- Prevent parent/child duplication by requiring root-reference in every child file.
- Define update semantics (preserve existing root intent; child content remains local-only).
- Ensure all checks are scriptable and human-free.

---

## Work Objectives

### Core Objective
Produce a maintainable AGENTS hierarchy that helps future agents navigate EX-GAS code quickly without duplicating root guidance.

### Concrete Deliverables
- `AGENTS.md` refreshed in-place (update-safe)
- Candidate child AGENTS under `Assets/GAS/**` (depth <= 3) where threshold passes
- Validation logs under `.sisyphus/evidence/`

### Definition of Done
- [ ] No child `AGENTS.md` generated outside allowed scope/depth
- [ ] Every child file contains root-reference statement
- [ ] Child files contain only directory-local guidance (no global command duplication)
- [ ] Dry-run report includes scored directories + rationale

### Must Have
- Deterministic scoring and placement policy
- Anti-duplication rules enforced
- Agent-executed verification only

### Must NOT Have (Guardrails)
- No AGENTS generation in `Assets/Plugins/**`, `Packages/**`, `.omc/**`, `.sisyphus/**`
- No scoring using `*.meta` files
- No copy-paste of root-wide coding conventions into child files

### Scope Inclusion Rule (locked)
- Include for scoring/placement:
  - `Assets/GAS/**`
  - `Assets/NTSD/Scripts/Animation/**`
  - `Assets/NTSD/Scripts/Simulation/**`
- Exclude rest of NTSD by default unless future handoff explicitly marks additional active first-party zones.

---

## Verification Strategy (MANDATORY)

> **UNIVERSAL RULE: ZERO HUMAN INTERVENTION**

All verification is executed by agents using CLI/tools. No manual checks.

### Test Decision
- Infrastructure exists: N/A (documentation workflow)
- Automated tests: None
- Framework: N/A

### Agent-Executed QA Scenarios (global)

Scenario: Scope exclusion validation
  Tool: Bash (rg / path checks)
  Preconditions: Generation dry-run output available
  Steps:
    1. List generated/planned AGENTS paths from dry-run artifact.
    2. Assert no path starts with `Assets/Plugins/`, `Assets/NTSD/`, `Packages/`, `.omc/`, `.sisyphus/`.
    3. Save output to `.sisyphus/evidence/init-deep-scope-check.txt`.
  Expected Result: Zero excluded-path hits.
  Failure Indicators: Any excluded prefix appears.
  Evidence: `.sisyphus/evidence/init-deep-scope-check.txt`

Scenario: Depth constraint validation
  Tool: Bash (path segment count)
  Preconditions: Candidate file list exists
  Steps:
    1. Compute depth of each candidate AGENTS path relative to repo root.
    2. Assert max depth <= 3.
    3. Save report to `.sisyphus/evidence/init-deep-depth-check.txt`.
  Expected Result: All candidates satisfy max depth.
  Failure Indicators: Any path depth > 3.
  Evidence: `.sisyphus/evidence/init-deep-depth-check.txt`

---

## Execution Strategy

### Parallel Execution Waves

Wave 1 (Start Immediately):
- Task 1 (parallel discovery agents)
- Task 2 (direct structural discovery)

Wave 2 (After Wave 1):
- Task 3 (scoring model + placement)
- Task 4 (root AGENTS update draft)

Wave 3 (After Wave 2):
- Task 5 (child AGENTS generation)
- Task 6 (dedup + dry-run validation)
- Task 7 (final report)

Critical Path: 1 -> 3 -> 5 -> 6 -> 7

### Dependency Matrix

| Task | Depends On | Blocks | Can Parallelize With |
|---|---|---|---|
| 1 | None | 3 | 2 |
| 2 | None | 3,4 | 1 |
| 3 | 1,2 | 5 | 4 |
| 4 | 2 | 7 | 3 |
| 5 | 3 | 6,7 | None |
| 6 | 5 | 7 | None |
| 7 | 4,5,6 | None | None |

---

## TODOs

- [ ] 1. Run exhaustive parallel discovery

  **What to do**:
  - Launch explore/librarian discovery tracks in parallel (code patterns + external best-practice references).
  - Capture findings into a structured placement input set.

  **Must NOT do**:
  - Do not mutate project files during discovery.

  **Recommended Agent Profile**:
  - **Category**: `unspecified-high`
    - Reason: mixed repo + external research synthesis.
  - **Skills**: `git-master`
    - `git-master`: safe repo-aware inspection patterns and traceability.
  - **Skills Evaluated but Omitted**:
    - `frontend-ui-ux`: no UI work.

  **Parallelization**:
  - Can Run In Parallel: YES
  - Parallel Group: Wave 1 (with Task 2)
  - Blocks: 3
  - Blocked By: None

  **References**:
  - `AGENTS.md` - current root conventions baseline.

  **Acceptance Criteria**:
  - [ ] Discovery summary contains: structure patterns, convention deviations, exclusion candidates, and anti-pattern notes.
  - [ ] Evidence saved: `.sisyphus/evidence/task-1-discovery-summary.md`.

- [ ] 2. Build structural baseline and exclusion filters

  **What to do**:
  - Enumerate candidate directories under first-party zones.
  - Apply exclusions: `Assets/Plugins/**`, `Packages/**`, `.omc/**`, `.sisyphus/**`, `*.meta`.
  - Apply targeted include allow-list for NTSD:
    - `Assets/NTSD/Scripts/Animation/**`
    - `Assets/NTSD/Scripts/Simulation/**`

  **Must NOT do**:
  - Do not include Unity temp/index artifacts in scoring input.

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: deterministic filtering and listing.
  - **Skills**: `git-master`
    - `git-master`: reproducible path filtering and repo-safe behavior.

  **Parallelization**:
  - Can Run In Parallel: YES
  - Parallel Group: Wave 1
  - Blocks: 3,4
  - Blocked By: None

  **References**:
  - `AGENTS.md` lines referencing first-party code paths (`Assets/GAS/Runtime`, `Assets/GAS/Editor`, `Assets/GAS/General`).

  **Acceptance Criteria**:
  - [ ] Filtered candidate list saved at `.sisyphus/evidence/task-2-candidates.txt`.
  - [ ] Exclusion check report saved at `.sisyphus/evidence/task-2-exclusions.txt`.

- [ ] 3. Score directories and decide AGENTS placement

  **What to do**:
  - Apply scoring rule:
    - `score = (cs_file_count * 3) + (subdir_count * 2) + (pattern_signal * 1)`
    - Ignore `*.meta`.
  - Placement rule:
    - Root: always
    - Child: score > 15, or score 8-15 with distinct-domain signal
    - Skip otherwise

  **Must NOT do**:
  - No subjective placement without score/rationale.

  **Recommended Agent Profile**:
  - **Category**: `unspecified-low`
    - Reason: constrained analytical pass.
  - **Skills**: `git-master`
    - `git-master`: deterministic trace logging.

  **Parallelization**:
  - Can Run In Parallel: NO
  - Parallel Group: Sequential (Wave 2)
  - Blocks: 5
  - Blocked By: 1,2

  **References**:
  - `.sisyphus/evidence/task-1-discovery-summary.md`
  - `.sisyphus/evidence/task-2-candidates.txt`

  **Acceptance Criteria**:
  - [ ] `AGENTS_LOCATIONS` table produced with `path`, `score`, `reason`.
  - [ ] Saved to `.sisyphus/evidence/task-3-scoring.md`.

- [ ] 4. Update root AGENTS.md (non-destructive)

  **What to do**:
  - Update root with concise hierarchy-aware sections only where needed.
  - Keep existing high-value conventions intact.

  **Must NOT do**:
  - Do not remove Unity/EX-GAS core setup guidance.

  **Recommended Agent Profile**:
  - **Category**: `writing`
    - Reason: concise documentation editing.
  - **Skills**: `git-master`
    - `git-master`: controlled diff and safe update pattern.

  **Parallelization**:
  - Can Run In Parallel: YES
  - Parallel Group: Wave 2 (with Task 3)
  - Blocks: 7
  - Blocked By: 2

  **References**:
  - `AGENTS.md` existing structure and repo commands.

  **Acceptance Criteria**:
  - [ ] Root AGENTS remains concise and still valid for Unity project onboarding.
  - [ ] No third-party scope creep added.

- [ ] 5. Generate/update child AGENTS.md files from placement list

  **What to do**:
  - For each approved child path, generate concise local AGENTS content.
  - Mandatory first line in each child: `See root AGENTS.md for global conventions.`
  - Keep each child 30-80 lines.

  **Must NOT do**:
  - No duplication of root-wide command sections.

  **Recommended Agent Profile**:
  - **Category**: `writing`
    - Reason: repeated concise per-directory docs.
  - **Skills**: `git-master`
    - `git-master`: safe multi-file update discipline.

  **Parallelization**:
  - Can Run In Parallel: YES
  - Parallel Group: Wave 3
  - Blocks: 6,7
  - Blocked By: 3

  **References**:
  - `.sisyphus/evidence/task-3-scoring.md`
  - `AGENTS.md` (for inherit-only, not copy).

  **Acceptance Criteria**:
  - [ ] Child files exist only at approved paths.
  - [ ] Each child has root-reference line.
  - [ ] Evidence list: `.sisyphus/evidence/task-5-generated-files.txt`.

- [ ] 6. Run dry-run validation and dedup checks

  **What to do**:
  - Validate excluded-path absence, depth <= 3, root-reference presence, and duplicate-section avoidance.
  - Produce machine-checkable logs.

  **Must NOT do**:
  - No manual inspection-only signoff.

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: scripted validation pass.
  - **Skills**: `git-master`
    - `git-master`: predictable verification workflow.

  **Parallelization**:
  - Can Run In Parallel: NO
  - Parallel Group: Sequential (Wave 3)
  - Blocks: 7
  - Blocked By: 5

  **References**:
  - `.sisyphus/evidence/task-5-generated-files.txt`
  - exclusion policy from this plan.

  **Acceptance Criteria**:
  - [ ] `.sisyphus/evidence/init-deep-scope-check.txt` contains zero violations.
  - [ ] `.sisyphus/evidence/init-deep-depth-check.txt` contains zero violations.
  - [ ] `.sisyphus/evidence/init-deep-root-ref-check.txt` shows all child files passing.

- [ ] 7. Produce completion summary

  **What to do**:
  - Emit final `=== init-deep Complete ===` summary with analyzed dirs, created/updated counts, and hierarchy tree.

  **Must NOT do**:
  - No unsupported claims without evidence file references.

  **Recommended Agent Profile**:
  - **Category**: `writing`
    - Reason: final report formatting.
  - **Skills**: `git-master`
    - `git-master`: ties report to actual repo diff state.

  **Parallelization**:
  - Can Run In Parallel: NO
  - Parallel Group: Final sequential
  - Blocks: None
  - Blocked By: 4,5,6

  **Acceptance Criteria**:
  - [ ] Final report includes evidence paths from Tasks 1-6.
  - [ ] Final report explicitly states mode=update, depth=3, scope=first-party-only + targeted NTSD active subtrees.

---

## Commit Strategy

| After Task | Message | Files | Verification |
|---|---|---|---|
| 4-5-6 batch | `docs(agents): refresh hierarchical AGENTS guidance for GAS` | `AGENTS.md`, `Assets/GAS/**/AGENTS.md` | scope/depth/root-ref checks |

---

## Success Criteria

### Verification Commands (Executor-run)
```bash
# 1) list AGENTS files
rg --files -g "**/AGENTS.md"

# 2) assert no excluded-path AGENTS
rg -n "AGENTS.md" .sisyphus/evidence/task-5-generated-files.txt

# 3) assert child root-reference presence
rg -n "See root AGENTS.md for global conventions" Assets/GAS/**/AGENTS.md
```

### Final Checklist
- [ ] All Must Have items present
- [ ] All Must NOT Have items absent
- [ ] Evidence artifacts exist under `.sisyphus/evidence/`
- [ ] Report matches actual generated hierarchy
