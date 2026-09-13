# Q05 numeric decoder source-linked runner

Build with `dotnet build Tools/NTSD28NumericDecoderTests`.
Run the built DLL with one input TSV path. Each row is `id<TAB>base64-UTF8-value` (no header).
Standard output contains float32 bits, strict int32 and first int32. The tool writes no files.
Compare with Q03 AuthorityNumericWitness, using identical effective field text; DAT lexer normalization must be accounted for separately.
No packages or Unity Editor dependency. This checks the same decoder source, not Converter integration or runtime alignment.
