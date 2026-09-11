---
name: pushgit
description: Commit all pending changes in logical groups and push to origin, using the team prefix convention (# fix, + add, * edit). Use when the user asks to push, commit and push, or "up lên git".
argument-hint: "[path or scope to limit, optional]"
allowed-tools: Bash(git:*)
---

# Push Git Workflow

Stage, commit and push pending changes of this Unity project to `origin`. Split the work into small logical commits instead of one big dump.

If `$ARGUMENTS` names a path, folder or topic, only commit changes that match it and leave everything else untouched.

## 1. Inspect

```
git status --short
git diff --stat
git log --oneline -5
```

- Nothing changed and nothing ahead of `origin` → stop and tell the user.
- Read the diff of modified code files (`git diff -- <file>`) to understand intent. Skip reading binary files, `.unity`, `.prefab` and `.asset` diffs beyond the stat.
- For untracked folders, list their contents (`git status --short -uall <folder>`) before deciding anything.

## 2. Safety checks (before staging)

The GitHub repo is **public**. Do not stage, and report to the user, any of:

- **Large files**: anything > 50 MB warns on GitHub, > 100 MB is rejected. Check with `find <paths> -type f -size +50M`.
- **Paid / third-party Asset Store packages** newly added under `Assets/` (vendor folders like `Assets/Belevich/`). Redistributing them publicly breaks the Asset Store license. Ask the user; the usual fix is adding the folder and its `.meta` to `.gitignore`. Already ignored: `Assets/Belevich/`, `Assets/Plugins/Demigiant/DOTweenPro/`, `Assets/Plugins/Demigiant/DemiLib/` (DOTween Pro is paid). The free `Assets/Plugins/Demigiant/DOTween/` is committed.
- **Secrets**: `.env`, keystores (`*.keystore`, `*.jks`), API keys, `settings.local.json`, credentials.
- Files that `.gitignore` should already cover (`Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`, `.idea/`, `*.csproj`, `*.sln`). If one shows up, fix `.gitignore` instead of committing it.

**Unity `.meta` rule**: always stage an asset together with its `.meta`, and a new folder together with its folder `.meta`. Never commit a `.meta` whose asset is ignored or missing.

## 3. Group into commits

Group by purpose, one commit per group, in this order:

1. `.gitignore` / repo config (`.mcp.json`, `.claude/`)
2. Packages & plugins (`Packages/manifest.json`, `packages-lock.json`, `Assets/Plugins/`)
3. Scripts, grouped per system (e.g. `Assets/Core/Pool/` separate from `Assets/Core/UIManager/`)
4. Scenes & prefabs
5. Art / sprites / audio
6. `ProjectSettings/` changes

Stage each group with explicit paths (`git add <paths>`), never `git add -A` / `git add .`.

**Push after every commit** — commit group 1, push, commit group 2, push, and so on (steps 4 → 5 per group). Do not batch all commits into a single push at the end. If a push fails, stop and fix it before making the next commit.

## 4. Commit message convention

`<prefix> <short English description>`, imperative, ≤ 50 chars, no trailing period.

| Prefix | Meaning | Example |
|--------|---------|---------|
| `+` | Add something new (feature, script, asset, package) | `+ Add generic Pool<T>` |
| `*` | Edit / improve something existing | `* Fade and scale FeatureBase on show/close` |
| `#` | Fix a bug | `# Fix UIManager spawning duplicate popup` |

Do not use Conventional Commits prefixes (`feat:`, `fix:`…). If the user passes a prefix symbol in their request, use it.

## 5. Push (after each commit)

```
git push origin main
```

- Rejected because the remote is ahead → `git pull --rebase --autostash origin main`, resolve conflicts if any (ask the user when a conflict is not obvious), then push again.
- Never `--force`. If a rewrite is truly needed, ask first and use `--force-with-lease`.

## 6. Report

Tell the user, briefly:
- each commit created (hash + message) and whether its push succeeded,
- anything deliberately left out and why (large file, Asset Store package, secret, out of scope).
