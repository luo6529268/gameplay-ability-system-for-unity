# Gate 0 recovery audit

The pre-scaffold directory contained exactly two files: `README.md` and
`data/cpp-runtime.json`. Neither is executable web application source or project
configuration. The README is retained as historical context only, and the JSON
runtime data is retained as an immutable binary-scale fixture/reference. Neither
file is behavioral authority.

Because no HTML, JavaScript/TypeScript, server, package configuration, or tests
were recoverable, Gate 0 requires a fresh minimal skeleton. The baseline manifest
records every pre-existing file by relative path, byte size, and SHA-256 before
the skeleton is added.
