#!/usr/bin/env python3
"""Audit the fixed eight-role plans and staged image residency without running a game."""
from __future__ import annotations
import argparse
import csv
import hashlib
import importlib.util
import json
import struct
import sys
from collections import Counter
from pathlib import Path

MIB = 1048576
TASK = "NTSD-M12-MATCH-IMAGE-RESIDENCY-20261013"


def read(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def write(path, value):
    with Path(path).open("x", encoding="utf-8", newline="\n") as stream:
        json.dump(value, stream, ensure_ascii=False, indent=2)
        stream.write("\n")


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def normalized(value):
    return str(value).replace("\\", "/").lower()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repository-root", type=Path, default=Path(__file__).resolve().parent.parent)
    parser.add_argument("--selection-root", type=Path, required=True)
    parser.add_argument("--output-root", type=Path, required=True)
    args = parser.parse_args()
    repo = args.repository_root.resolve()
    work = repo / "artifacts/diagnostics/M12-match-image-residency-20261013"
    output = args.output_root.resolve()
    if not output.is_relative_to(work.resolve()) or output.exists():
        raise ValueError("Output must be a fresh directory under the current task diagnostics.")
    output.mkdir(parents=True)
    module_path = repo / "Tools/Audit-NtsdEightRoleMemory.py"
    spec = importlib.util.spec_from_file_location("prior_memory_audit_readonly", module_path)
    prior = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = prior
    spec.loader.exec_module(prior)
    old_work = repo / "artifacts/diagnostics/M12-astc-eight-role-20261013"
    old = read(old_work / "result-10-final/result.json")
    capture_path = Path(old["capture"]["capturePath"])
    capture = read(capture_path)
    objects = prior.capture_object_map(capture)
    selection_path = old_work / "eight-role-audit-worker/result-01/selection.json"
    selection = read(selection_path)
    roots = selection["selectedOids"]
    if roots != [28, 18, 65, 23, 8, 10, 66, 27]:
        raise ValueError("The previously selected eight roles changed; no resampling is allowed.")
    pages_path = repo / old["pagesManifest"]["manifestPath"]
    astc_path = repo / old["astcManifest"]["manifestPath"]
    pages = prior.load_pages_manifest(str(pages_path))
    astc = prior.load_astc_manifest(str(astc_path))
    if not pages["usable"] or not astc["usable"]:
        raise ValueError("The deployed page/ASTC manifests must validate for this audit.")
    inputs = []
    for path in [module_path, capture_path, selection_path, pages_path, astc_path]:
        inputs.append({"path": str(path), "sha256": digest(path)})
    for item in objects.values():
        path = Path(item["datPath"])
        actual = digest(path)
        if actual.lower() != item["datIdentity"]["sha256"].lower():
            raise ValueError("Captured DAT changed: " + str(path))
        inputs.append({"kind": "current-DAT", "oid": item["id"], "path": str(path), "sha256": actual})
    all_ui, ui_errors = prior.ui_instances_for_oids(sorted(objects), objects)
    if ui_errors:
        raise ValueError(str(ui_errors))
    ui_unique = list({item["asset"]["key"]: item["asset"] for item in all_ui}.values())
    all_ui_bytes = sum(item["rgbaBytes"] * 2 for item in ui_unique)
    common_path = old_work / "common-assets-static-budget.json"
    common = read(common_path)
    for item in common["assets"]:
        if digest(repo / item["path"]) != item["sha256"]:
            raise ValueError("Common map/UI source changed: " + item["path"])
    spark_path = repo / common["SPARK"]["path"]
    spark = prior.image_asset(str(spark_path), None, "common_spark")
    outside_images = common["currentCatalogMapCommonResidentUpperBytes"] - spark["rgbaBytes"]
    write(output / "inputs.json", inputs)
    cases = []
    models = {}
    image_rows = []

    def make_case(name, assets, cells, ui_assets, old_bytes=0, common_spark=False):
        images, seen = [], set()
        for asset in assets:
            key = normalized(asset.get("astcSourceKey") or asset.get("sourcePath"))
            compressed = astc["entries"].get(key)
            if compressed and compressed.get("usable"):
                block_key = asset["key"] if asset["kind"] == "derived_page" else normalized(compressed["filesystemPath"])
                if block_key in seen:
                    continue
                seen.add(block_key)
                images.append({"path": compressed["filesystemPath"], "mode": "astc", "width": asset["width"],
                    "height": asset["height"], "fileBytes": compressed["fileBytes"], "retainPixels": False,
                    "readableTexture": False, "source": asset["path"]})
            else:
                images.append({"path": asset["path"], "mode": "png", "width": asset["width"],
                    "height": asset["height"], "fileBytes": asset["bytes"], "retainPixels": False,
                    "readableTexture": False})
        for asset in ui_assets + ([spark] if common_spark else []):
            images.append({"path": asset["path"], "mode": "png", "width": asset["width"], "height": asset["height"],
                "fileBytes": asset["bytes"], "retainPixels": False, "readableTexture": asset is not spark})
        gpu = readable = retained = max_upload = max_clamp_work = 0
        works = []
        for image in images:
            rgba = 4 * image["width"] * image["height"]
            allocation = ((image["width"] + 3) // 4) * ((image["height"] + 3) // 4) * 16 if image["mode"] == "astc" else rgba
            if image["mode"] == "astc":
                if image["fileBytes"] != allocation + 16:
                    raise ValueError("ASTC length mismatch")
                with Path(image["path"]).open("rb") as stream:
                    header = stream.read(16)
                if header[:7] != bytes([0x13, 0xAB, 0xA1, 0x5C, 4, 4, 1]):
                    raise ValueError("ASTC header mismatch")
            gpu += allocation
            readable += rgba if image["readableTexture"] else 0
            max_upload = max(max_upload, allocation)
            works.append(2 * image["fileBytes"] + 65536 if image["mode"] == "astc" else
                2 * rgba + 4 * image["fileBytes"] + 8 * image["width"] + 65536)
            image_rows.append({"plan": name, **image, "gpuBytes": allocation, "readableCpuBytes": rgba if image["readableTexture"] else 0})
        clamped = []
        for cell in cells:
            rgba = 4 * cell["width"] * cell["height"]
            gpu += rgba
            max_upload = max(max_upload, rgba)
            max_clamp_work = max(max_clamp_work, 2 * rgba + 8)
            clamped.append({"width": cell["width"], "height": cell["height"], "retainPixels": False})
        resident = old_bytes + gpu + readable
        loading = resident + retained + max_upload + 4 * max_clamp_work + sum(sorted(works, reverse=True)[:4])
        expected = {"residentBytes": resident, "loadingBytes": loading, "sourceGpuBytes": gpu,
            "readableCpuBytes": readable, "retainedRgbaBytes": retained, "oldPublicationBytes": old_bytes,
            "atlasReservationBytes": 0}
        cases.append({"name": name, "oldPublicationBytes": old_bytes, "atlasBytes": 0,
            "images": images, "clampedCells": clamped, "expected": expected})
        return {**expected, "bodyAllocationCount": len(images) - len(ui_assets) - int(common_spark),
            "imageInputCount": len(images), "clampCellCount": len(clamped), "largest4WorkBytes": sorted(works, reverse=True)[:4],
            "withCurrentMapCommonResidentBytes": resident + outside_images,
            "withCurrentMapCommonLoadingBytes": loading + outside_images}

    models["menu"] = make_case("menu", [], [], ui_unique)
    plans = {"role-only": read(args.selection_root / "role-only.json"), "match": read(args.selection_root / "match.json")}
    independent = {}
    missing = []
    for name, plan in plans.items():
        include = name == "match"
        projection = prior.DependencyClosure(objects, capture).run(roots,
            branch="compatibility" if include else "production", include_scenario=include,
            include_compatibility=include, context_victim_pool=roots, include_optional_209=include and 209 in objects)
        if projection["oids"] != plan["definitionIds"]:
            raise ValueError("Independent closure mismatch for " + name)
        files_checked = 0
        for declaration in plan["bodySourceDeclarations"]:
            files = objects[declaration["definitionId"]]["bmp"]["files"]
            if len(files) != len(declaration["files"]):
                raise ValueError("Runtime declaration count mismatch")
            for captured, runtime in zip(files, declaration["files"]):
                for key in ["width", "height", "row", "col"]:
                    if captured[key] != runtime[key]: raise ValueError("Runtime geometry mismatch: " + key)
                if normalized(captured["path"]) != normalized(runtime["sourcePath"]) or captured["effectiveFirst"] != runtime["startFrame"] or captured["effectiveLast"] != runtime["endFrame"]:
                    raise ValueError("Runtime declaration path/range mismatch")
                files_checked += 1
        assets, declarations, clamps, errors = prior.body_assets_for_declarations(plan["definitionIds"], objects, pages, astc)
        if errors: raise ValueError(str(errors))
        budget = make_case(name + "-from-menu", assets, clamps, [], all_ui_bytes, True)
        budget["plannedDefinitionCount"] = len(plan["definitionIds"])
        budget["uniqueBodySourceOrPageCount"] = len(assets)
        budget["bodyDeclarationCount"] = files_checked
        budget["uiUniqueCount"] = len(ui_unique)
        budget["planFingerprint"] = plan["fingerprint"]
        budget["frameCount"] = sum(len(objects[oid]["frames"]) for oid in plan["definitionIds"])
        budget["rawDatBytesSelected"] = sum(objects[oid]["datIdentity"]["bytes"] for oid in plan["definitionIds"])
        models[name] = budget
        old_edges = {(edge.get("fromOid"), edge["toOid"], edge.get("frameId")) for edge in projection["edges"]}
        new_edges = {(edge["fromId"], edge["toId"], edge["frameId"]) for edge in plan["edges"]}
        independent[name] = {"closureMatch": True, "definitionCount": len(plan["definitionIds"]),
            "sourceDeclarationsMatch": files_checked, "oldEdgeCount": len(projection["edges"]), "newEdgeCount": len(plan["edges"]),
            "oldOnlyNormalizedEdges": sorted(old_edges - new_edges, key=str), "newOnlyNormalizedEdges": sorted(new_edges - old_edges, key=str),
            "oldOnlyEdgeTypes": dict(Counter(edge["type"] for edge in projection["edges"] if (edge.get("fromOid"), edge["toOid"], edge.get("frameId")) in old_edges - new_edges))}
        missing.extend(projection["unknowns"])
        write(output / (name + "-body-resources.json"), {"assets": assets, "declarations": declarations, "clampedCells": clamps})
    full_assets, _, full_clamps, full_errors = prior.body_assets_for_declarations(sorted(objects), objects, pages, astc)
    if full_errors: raise ValueError(str(full_errors))
    models["full-catalogue-cold"] = make_case("full-catalogue-cold", full_assets, full_clamps, ui_unique, common_spark=True)
    models["full-catalogue-from-menu"] = make_case("full-catalogue-from-menu", full_assets, full_clamps, [], all_ui_bytes, True)
    for name, assets, clamps in [("match", read(output / "match-body-resources.json")["assets"], read(output / "match-body-resources.json")["clampedCells"]), ("full-catalogue", full_assets, full_clamps)]:
        steady = models["match" if name == "match" else "full-catalogue-from-menu"]["residentBytes"]
        models[name + "-one-old-one-new"] = make_case(name + "-one-old-one-new", assets, clamps, [], steady, True)
    individuals = []
    for selected in selection["selected"]:
        oid = selected["oid"]
        plan = read(args.selection_root / ("individual-" + str(oid) + ".json"))
        projection = prior.DependencyClosure(objects, capture).run([oid], branch="production", include_scenario=False, include_compatibility=False, context_victim_pool=[oid])
        if projection["oids"] != plan["definitionIds"]: raise ValueError("Per-role closure mismatch: " + str(oid))
        assets, _, clamps, errors = prior.body_assets_for_declarations(plan["definitionIds"], objects, pages, astc)
        if errors: raise ValueError(str(errors))
        budget = make_case("individual-" + str(oid), assets, clamps, [], all_ui_bytes, True)
        individuals.append({"oid": oid, "name": selected["name"], "definitionCount": len(plan["definitionIds"]),
            "bodyUniqueCount": len(assets), "missingDependencies": plan["missingDependencies"], **budget})
    write(output / "independent-closure-comparison.json", independent)
    write(output / "production-budget-cases.json", {"cases": cases})
    write(output / "all-ui-resources.json", ui_unique)
    write(output / "individuals.json", individuals)
    with (output / "image-memory-inputs.csv").open("x", newline="", encoding="utf-8-sig") as stream:
        columns = sorted({key for row in image_rows for key in row})
        writer = csv.DictWriter(stream, fieldnames=columns)
        writer.writeheader()
        writer.writerows(image_rows)
    baseline = models["full-catalogue-cold"]
    comparison = {"residentSavedBytes": baseline["residentBytes"] - models["match"]["residentBytes"],
        "loadingSavedBytes": baseline["loadingBytes"] - models["match"]["loadingBytes"],
        "residentSavedPercent": 100 * (1 - models["match"]["residentBytes"] / baseline["residentBytes"]),
        "loadingSavedPercent": 100 * (1 - models["match"]["loadingBytes"] / baseline["loadingBytes"]),
        "old8uiScenarioResidentBytes": old["sets"]["scenario_static_envelope"]["budgets"]["cold"]["residentBytes"],
        "extraAllUiComparedWithPrior8UiBytes": all_ui_bytes - old["sets"]["scenario_static_envelope"]["budgets"]["cold"]["uiUniquePathShaCombinedBytes"]}
    result = {"taskId": TASK, "status": "STATIC_VERIFIED_WITH_DECLARED_GAPS", "fixedRoots": roots,
        "fullDatCountRetained": len(objects), "allUiUniqueCount": len(ui_unique), "allUiResidentBytes": all_ui_bytes,
        "outsideCurrentMapFootShadowBytes": outside_images, "models": models, "individuals": individuals,
        "comparison": comparison, "unknowns": missing, "productionBudgetCaseCount": len(cases),
        "boundary": "ASTC4x4 + >=4096 max texture; no mipmaps; source identity PNG files are not uploaded when sidecars are used. All330 DAT remain parsed; their managed allocations, Unity/driver/TMP/audio/instances/RT are not included. Current map/foot/shadow are RGBA upper estimates. No phone or whole-process peak. Selected all-frame closure is not an executed-all-moves claim. Stage dynamic paths require configured all-wave roots; fixed8 comparison has no authored stage campaign."}
    write(output / "result.json", result)
    lines = ["# " + TASK + " static memory audit", "", "Fixed roots: " + str(roots), "",
        "All330 DAT and all203 unique UI paths retained. Existing missing OID200 remains explicit.", "",
        "| Model | Image resident MiB | Image loading MiB | With current map/common resident MiB | With current map/common loading MiB |",
        "|---|---:|---:|---:|---:|"]
    for name, value in models.items():
        lines.append("| " + name + " | " + " | ".join(f"{value[key]/MIB:.3f}" for key in ["residentBytes", "loadingBytes", "withCurrentMapCommonResidentBytes", "withCurrentMapCommonLoadingBytes"]) + " |")
    lines += ["", "Menu with-map figures are comparison-only; the new menu does not create battle map/common textures.", "", result["boundary"], "", "Prior closure edge counts may differ because of deduplication/context predicates; normalized differences are preserved in independent-closure-comparison.json."]
    with (output / "REPORT.md").open("x", encoding="utf-8", newline="\n") as stream: stream.write("\n".join(lines) + "\n")
    print(json.dumps({"status": result["status"], "output": str(output), "models": models, "comparison": comparison}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
