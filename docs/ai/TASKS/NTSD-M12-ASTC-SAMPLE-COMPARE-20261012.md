# NTSD-M12-ASTC-SAMPLE-COMPARE-20261012

Status: VERIFIED
Change: NTSD-M12-ASTC-SAMPLE-COMPARE-001
Operation: OP-NTSD-M12-ASTC-SAMPLE-COMPARE-20261012

Direct human representative compression experiment only. Scope, validation and rollback: ../CHANGE-RECORDS/NTSD-M12-ASTC-SAMPLE-COMPARE-001.md. No production loader, original PNG/DAT changes, prebuild paging or P1.


> 2026-10-12 FINAL OFFLINE ASTC SAMPLE: NTSD-M12-ASTC-SAMPLE-COMPARE-001 VERIFIED for this six-image offline experiment only. Twelve fixed-profile Arm astcenc 5.6.0 encodes and twelve decodes exit0; actual headers/payloads and 18 viewer image embeddings checked. RGBA 13,958,528B -> ASTC4x4 3,498,624B (-74.94%) / ASTC6x6 1,560,736B (-88.82%), no mips/resize. Pixel-art roles/fine lightning favor 4x4; 6x6 smoke is a candidate only; fire alpha/detail needs caution, not blanket promotion. All 7,722 exact protected paths hash identical. PNG/decoded DAT unchanged; no Unity/Play/P1/production loader/prebuild paging/device/peak-memory acceptance. Original helper/path and viewer syntax failures retained; corrected final viewer static syntax passes, browser file-URL preview rejected with no workaround. Parent M12 OPEN/defaultOFF/H08ON; automation stays paused. Evidence: artifacts/diagnostics/M12-astc-sample-20261012/REPORT.md and artifact-validation.json / protection-after.json.

Validation receipts: 24 codec process exits0; 12 exact ASTC headers/hashes/lengths; 18 embedded images exact; Python parse and Node --check pass; 7,722 before/after hashes same. Tools/Validate-ChangeLedger.ps1 final log is captured separately before delivery. Parent/runtime/device/art acceptance is not promoted. All baseline/repair backups retained. External concurrent cleanup-review untracked additions observed, not authored or touched by this ASTC task.
