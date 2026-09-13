# NTSD28 PNG decoder validation

Change: `NTSD28-B11-PNG-WORKER-DECODE-001`. The production decoder is linked directly into the corpus verifier; there is no alternate decoder implementation or Unity behavior stub in that verifier.

```powershell
python Tools/NTSD28PngDecode/Generate-Fixtures.py
python Tools/NTSD28PngDecode/Generate-CorpusManifest.py --root 'J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime/vfs' --output artifacts/diagnostics/NTSD28-B11-PNG-WORKER-DECODE-001/corpus-reference.tsv
dotnet build Tools/NTSD28PngDecode/PngCorpusVerifier.csproj
& 'D:/Unity/HubEditor/2022.3.62f3/Editor/Data/MonoBleedingEdge/bin/mono.exe' Tools/NTSD28PngDecode/bin/Debug/net472/PngCorpusVerifier.exe artifacts/diagnostics/NTSD28-B11-PNG-WORKER-DECODE-001/corpus-reference.tsv
```

Python uses the existing Pillow installation. Generated fixtures live only in `Temp/NTSD28PngDecode/generated`; formal resources and project assets are read-only. The expected fixture pixels are independently constructed; Pillow also checks the generated palette/filter cases. The corpus manifest binds each input hash, dimensions and bottom-up RGBA reference hash.

In the existing Unity Editor, filter EditMode tests to `NTSD.Test.Editor.NTSD28B11PngWorkerDecodeEditorTests`. Tests run the actual BMPLoader thread-pool path, compare known fixture pixels and real PNGs with Unity's main-thread decoder, verify bad-input rejection and load the largest formal sheet. The fixture generator and corpus manifest are explicit prerequisites. These tests create and destroy only their temporary reference textures; they do not load or save scenes.

Supported production formats are the current formal corpus: non-interlaced indexed PNG at 1/2/4/8 bits and RGBA8. Other PNG formats fail explicitly. Format rules follow the [W3C PNG specification](https://www.w3.org/TR/png/). This does not claim general PNG coverage or certify final rendering. Existing BMP behavior and the main-thread loading path remain unchanged.

The raw decoder preserves alpha. The existing sheet processor still applies old color-key/alpha rules; that separate confirmed difference is tracked as P-21 and must not be declared fixed by these tests.
