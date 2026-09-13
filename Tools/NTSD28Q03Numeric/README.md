# Q03 numeric decoder witness

Run `pwsh -File Tools/NTSD28Q03Numeric/Build-And-Capture.ps1` from the repository. The tool only compiles a workspace executable against unchanged authority sources and reads the shared DAT fixtures. It writes float32 raw hexadecimal bits and effective integer/action values from the real DatParser -> CombatRecordDecoder chain.

Change: NTSD28-Q03-NUMERIC-DECODE-WITNESS-001. Source/input hashes and double-run identity accompany output. This is not the formal EXE or a Unity decoder implementation. Keep signed zero/subnormal bits and invalid-input defaults visible; no float formatting tolerance is used. The evidence informs the Q03 contract and Q05 tests without modifying production behavior.
