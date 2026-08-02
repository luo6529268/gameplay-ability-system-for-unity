# Build manifest pinning

- Each build writes an immutable `builds/<buildId>/build-manifest.json`; the mutable `dist/build-manifest.json` is only a published selection point.
- `loadVerifiedBuildManifest` returns the build-local manifest path after requiring exact content equality with the atomically published current manifest.
- Startup must verify and inject the static configuration plus runtime helper descriptor before listening; otherwise a lazy first request can mix backend build A with client build B.
- `WindowsReplaceFilePublisher` defaults to its own build-local manifest and caches verification, while callers can inject `{ path, manifestPath, buildId }` for an already pinned runtime asset.
- The native safe-file helper is a second runtime asset. CLI derives its `size` and `sha256` from the same pinned manifest, creates one `PowerShellWindowsSafeFileClient`, and injects that one instance into both `WorkspaceRegistry` and `SafeSaveService`.
- Current-pointer reads retry only `ENOENT`, `EBUSY`, `EPERM`, and `EACCES` with a bounded 2 ms backoff. The build-local pinned manifest and parsing/integrity failures do not retry.
