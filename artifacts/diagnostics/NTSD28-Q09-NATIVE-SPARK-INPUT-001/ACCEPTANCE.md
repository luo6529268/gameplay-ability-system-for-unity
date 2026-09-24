# NTSD28-Q09-NATIVE-SPARK-INPUT-001 acceptance

Status: VERIFIED for immutable input capture only. BATCH-05/Q09, R14/R17 and the total alignment goal remain open.

The formal runtime candidate now captures `data/resource.dat` index 43, `data/system.dat` spark_w/spark_h and the selected PNG SHA as one immutable SPARK input. Both DAT files and the PNG contribute to V4 `VisualFingerprint` and `SourceCacheKey`, with a recapture at `AssertInputsCurrent`. On the formal root the selected file is `sprite/UI/SPARK.png`, dimensions are 99x79, and SHA-256 is `15D8843E0CE87FF63F46DFF7170D30C23BAEA0F2799434B26717AADFD5EC881B`. The candidate still contains 906 distinct file/head/small character images. No DAT or PNG values were modified.

Original Unity Editor compilation reported 0 Console errors. Candidate class job `27f49f7658644e7cb13abf648d2dc993`: 8/8 PASS. Final mutation test job `53f3aafaae7e4d6f9f57546f69d6d864`: 1/1 PASS after adding index-43 selection change. Staged/formal identity job `4325246fb0434cb3a36a1af3ec5f33f7`: 1/1 PASS. An earlier incorrect namespace filter yielded 0/0 and provides no validation. The tests cover system dimension, PNG content, resource selection and partial-input rejection; historical minimal source fixtures still pass.

Battle Scene SHA-256: `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39`. Menu Scene SHA-256: `3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228`. Both match the prior baselines. Ledger validator passed with 785 Records and 21 current code diff files covered; `git -c core.safecrlf=false diff --check` passed. No computer-use or second Unity project was used.

Remaining Q09 dependency: use this input in formal SPARK PNG decode and black-key publication, map raw 0..99 IDs to the 10x10 100x80 grid and drawable/bounds gates, update both legacy SparkRenderer and central HitRecord commands without moving C01 logic lifecycle, then validate same-seed 30/60/120 outputs and original-EXE pixels. Existing old SPARK.bmp and its owners are retained until a separate verified retirement decision.
