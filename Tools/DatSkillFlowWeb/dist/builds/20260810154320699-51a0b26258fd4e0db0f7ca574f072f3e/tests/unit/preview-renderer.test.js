// dat-skill-flow-build:20260810154320699-51a0b26258fd4e0db0f7ca574f072f3e
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    drawPreviewCanvas,
    effectivePreviewPic,
    preloadPreviewObjectAssets,
    previewObjectAssetIds,
    sortPreviewEntities,
    spriteSheetColumnCount,
    stageParallaxOffset,
} from "../../src/client/preview-renderer.js";
                                                                

describe("preview renderer sprite ranges", () => {
    it("uses the Native row field for horizontal sprite-sheet layout", () => {
        assert.equal(spriteSheetColumnCount({ row: 6, col: 4 }), 6);
        assert.equal(spriteSheetColumnCount({ row: 7, col: 10 }), 7);
    });

    it("does not infer a horizontal layout from col when row is absent", () => {
        assert.equal(spriteSheetColumnCount({ col: 10 }), 0);
    });

    it("prefers the Native effective render pic over the DAT frame pic", () => {
        assert.equal(effectivePreviewPic({ renderPic: 140, pic: 12, frame: 300, oid: 2, slot: 0, x: 0, y: 0, z: 0 }, { pic: 12 }         ), 140);
        assert.equal(effectivePreviewPic({ pic: 12, frame: 300, oid: 2, slot: 0, x: 0, y: 0, z: 0 }, { pic: 12 }         ), 12);
    });

    it("sorts scene entities by Native z_int while preserving equal-z slot order", () => {
        const entities = [
            { slot: 5, oid: 33, frame: 1, x: 0, y: 0, z: 0, zInt: 500 },
            { slot: 2, oid: 204, frame: 1, x: 0, y: 0, z: 0, zInt: 450 },
            { slot: 1, oid: 121, frame: 1, x: 0, y: 0, z: 0, zInt: 500 },
        ];
        assert.deepEqual(sortPreviewEntities(entities).map((entity) => entity.slot), [2, 5, 1]);
    });

    it("uses Native stage parallax projection", () => {
        assert.equal(stageParallaxOffset(960, 794, 950, 0), 0);
        assert.equal(stageParallaxOffset(960, 794, 950, 166), -156);
        assert.equal(stageParallaxOffset(800, 794, 950, 12), -312);
    });

    it("draws the F271 OID 205 spawn from its auxiliary resource into the visible Canvas", () => {
        const drawCalls              = [];
        const context = {
            canvas: { width: 794, height: 550 },
            clearRect() {}, fillRect() {}, fillText() {}, save() {}, restore() {},
            setLineDash() {}, beginPath() {}, moveTo() {}, lineTo() {}, stroke() {}, strokeRect() {},
            drawImage(...args           ) { drawCalls.push(args); },
            strokeStyle: "", fillStyle: "", lineWidth: 1,
        }                                       ;
        const canvas = {
            width: 794,
            height: 550,
            getContext: () => context,
        }                                ;
        const keyedSheet = { width: 800, height: 161 }                     ;
        const image = {
            complete: true,
            naturalWidth: 800,
            naturalHeight: 161,
            width: 800,
            height: 161,
        }                    ;

        drawPreviewCanvas({
            canvas,
            project: {
                frames: [],
                ranges: [],
                assets: new Map(),
                previewObjects: [{
                    oid: 205,
                    frames: [{
                        frameId: 99,
                        occurrence: 0,
                        pic: 68,
                        state: 9997,
                        centerx: 40,
                        centery: 850,
                    }         ],
                    ranges: [{ frameLo: 68, frameHi: 137, assetId: "oid205-timer", w: 79, h: 79, row: 10, col: 2 }],
                }],
            },
            tick: {
                tick: 17,
                cameraX: 0,
                entities: [{
                    slot: 51,
                    oid: 205,
                    frame: 99,
                    pic: 68,
                    renderPic: 68,
                    facing: 0,
                    x: 322,
                    y: 587,
                    z: 501,
                    xInt: 322,
                    yInt: 587,
                    zInt: 501,
                    displayZ: 501,
                    renderOffsetX: 0,
                    frameDelay: 0,
                    hitStop: 0,
                    link: 0,
                }],
            },
            runtimeFrame: undefined,
            images: new Map([["oid205-timer", image]]),
            colorKeyImages: new Map([["oid205-timer", keyedSheet]]),
            visibleOverlays: new Set(),
            requestRender() {},
        });

        assert.deepEqual(drawCalls, [[keyedSheet, 0, 0, 79, 79, 282, 238, 79, 79]]);
    });

    it("draws the real OID 33 clone reached by the F271 type-3 Native chain", () => {
        const drawCalls              = [];
        const context = {
            canvas: { width: 794, height: 550 },
            clearRect() {}, fillRect() {}, fillText() {}, save() {}, restore() {},
            setLineDash() {}, beginPath() {}, moveTo() {}, lineTo() {}, stroke() {}, strokeRect() {},
            drawImage(...args           ) { drawCalls.push(args); },
            strokeStyle: "", fillStyle: "", lineWidth: 1,
        }                                       ;
        const keyedSheet = { width: 800, height: 560 }                     ;
        const image = {
            complete: true,
            naturalWidth: 800,
            naturalHeight: 560,
            width: 800,
            height: 560,
        }                    ;

        drawPreviewCanvas({
            canvas: { width: 794, height: 550, getContext: () => context }                                ,
            project: {
                frames: [],
                ranges: [],
                assets: new Map(),
                previewObjects: [{
                    oid: 33,
                    frames: [{ frameId: 212, occurrence: 0, pic: 62, state: 4, centerx: 39, centery: 79 }         ],
                    ranges: [{ frameLo: 0, frameHi: 69, assetId: "oid33-naruto-0", w: 79, h: 79, row: 10, col: 7 }],
                }],
            },
            tick: {
                tick: 39,
                cameraX: 0,
                entities: [{
                    slot: 54,
                    oid: 33,
                    frame: 212,
                    pic: 62,
                    renderPic: 62,
                    facing: 0,
                    x: 284,
                    y: -3,
                    z: 486,
                    xInt: 284,
                    yInt: -3,
                    zInt: 486,
                    displayZ: 486,
                    renderOffsetX: 0,
                    frameDelay: 0,
                    hitStop: 0,
                    link: 0,
                }],
            },
            runtimeFrame: undefined,
            images: new Map([["oid33-naruto-0", image]]),
            colorKeyImages: new Map([["oid33-naruto-0", keyedSheet]]),
            visibleOverlays: new Set(),
            requestRender() {},
        });

        assert.deepEqual(drawCalls, [[keyedSheet, 160, 480, 79, 79, 245, 404, 79, 79]]);
    });

    it("draws the F265 OID 33 clone with the Native sprite rect and destination", () => {
        const drawCalls              = [];
        const context = {
            canvas: { width: 794, height: 550 },
            clearRect() {}, fillRect() {}, fillText() {}, save() {}, restore() {},
            setLineDash() {}, beginPath() {}, moveTo() {}, lineTo() {}, stroke() {}, strokeRect() {},
            drawImage(...args           ) { drawCalls.push(args); },
            strokeStyle: "", fillStyle: "", lineWidth: 1,
        }                                       ;
        const keyedSheet = { width: 800, height: 560 }                     ;
        const image = {
            complete: true,
            naturalWidth: 800,
            naturalHeight: 560,
            width: 800,
            height: 560,
        }                    ;

        drawPreviewCanvas({
            canvas: { width: 794, height: 550, getContext: () => context }                                ,
            project: {
                frames: [],
                ranges: [],
                assets: new Map(),
                previewObjects: [{
                    oid: 33,
                    frames: [{ frameId: 252, occurrence: 0, pic: 125, state: 15, centerx: 39, centery: 73 }         ],
                    ranges: [{ frameLo: 70, frameHi: 139, assetId: "oid33-naruto-2", w: 79, h: 79, row: 10, col: 7 }],
                }],
            },
            tick: {
                tick: 24,
                cameraX: 0,
                entities: [{
                    slot: 50, oid: 33, frame: 252, pic: 125, renderPic: 125, facing: 0,
                    x: 331, y: -99, z: 501, xInt: 331, yInt: -99, zInt: 501, displayZ: 501,
                    renderOffsetX: 0, frameDelay: 0, hitStop: 0, link: 0,
                }],
            },
            runtimeFrame: undefined,
            images: new Map([["oid33-naruto-2", image]]),
            colorKeyImages: new Map([["oid33-naruto-2", keyedSheet]]),
            visibleOverlays: new Set(),
            requestRender() {},
        });

        assert.deepEqual(drawCalls, [[keyedSheet, 400, 400, 79, 79, 292, 329, 79, 79]]);
    });

    it("keeps Native invisible and missing-sprite entities silent instead of drawing debug boxes", () => {
        const drawCalls              = [];
        const strokeRects              = [];
        const context = {
            canvas: { width: 794, height: 550 },
            clearRect() {}, fillRect() {}, fillText() {}, save() {}, restore() {},
            setLineDash() {}, beginPath() {}, moveTo() {}, lineTo() {}, stroke() {},
            strokeRect(...args           ) { strokeRects.push(args); },
            drawImage(...args           ) { drawCalls.push(args); },
            strokeStyle: "", fillStyle: "", lineWidth: 1,
        }                                       ;

        drawPreviewCanvas({
            canvas: { width: 794, height: 550, getContext: () => context }                                ,
            project: {
                frames: [], ranges: [], assets: new Map(),
                previewObjects: [{
                    oid: 33,
                    frames: [{ frameId: 251, occurrence: 0, pic: 999, state: 15, centerx: 39, centery: 73 }         ],
                    ranges: [{ frameLo: 70, frameHi: 139, assetId: "unused", w: 79, h: 79, row: 10, col: 7 }],
                }],
            },
            tick: {
                tick: 21,
                cameraX: 0,
                entities: [
                    { slot: 50, oid: 33, frame: 251, pic: 999, x: 331, y: -63, z: 501, link: 0 },
                    { slot: 51, oid: 999, frame: 0, pic: 0, x: 340, y: 0, z: 502, link: 0 },
                ],
            },
            runtimeFrame: undefined,
            images: new Map(),
            colorKeyImages: new Map(),
            visibleOverlays: new Set(),
            requestRender() {},
        });

        assert.deepEqual(drawCalls, []);
        assert.deepEqual(strokeRects, []);
    });

    it("uses Native display_z, render jitter, state-9997 clamping, and hit-stop visibility", () => {
        const drawCalls              = [];
        const context = {
            canvas: { width: 794, height: 550 },
            clearRect() {}, fillRect() {}, fillText() {}, save() {}, restore() {},
            setLineDash() {}, beginPath() {}, moveTo() {}, lineTo() {}, stroke() {}, strokeRect() {},
            drawImage(...args           ) { drawCalls.push(args); },
            strokeStyle: "", fillStyle: "", lineWidth: 1,
        }                                       ;
        const keyedSheet = { width: 800, height: 161 }                     ;
        const image = {
            complete: true, naturalWidth: 800, naturalHeight: 161, width: 800, height: 161,
        }                    ;
        const render = (hitStop        )       => {
            drawPreviewCanvas({
                canvas: { width: 794, height: 550, getContext: () => context }                                ,
                project: {
                    frames: [], ranges: [], assets: new Map(),
                    previewObjects: [{
                        oid: 205,
                        frames: [{ frameId: 99, occurrence: 0, pic: 68, state: 9997, centerx: 40, centery: 850 }         ],
                        ranges: [{ frameLo: 68, frameHi: 137, assetId: "timer", w: 79, h: 79, row: 10, col: 2 }],
                    }],
                },
                tick: {
                    tick: 2,
                    cameraX: 0,
                    entities: [{
                        slot: 50, oid: 205, frame: 99, pic: 68, facing: 0,
                        x: -10, y: 587, z: 901, xInt: -10, yInt: 587, zInt: 901, displayZ: 300,
                        renderOffsetX: 0, frameDelay: -1, hitStop, link: 0,
                    }],
                },
                runtimeFrame: undefined,
                images: new Map([["timer", image]]),
                colorKeyImages: new Map([["timer", keyedSheet]]),
                visibleOverlays: new Set(),
                requestRender() {},
            });
        };

        render(0);
        render(2);

        assert.deepEqual(drawCalls, [[keyedSheet, 0, 0, 79, 79, 0, 37, 79, 79]]);
    });

    it("loads the one-tick F271 OID 205 sprite before preview playback starts", async () => {
        const project = {
            frames: [],
            ranges: [],
            assets: new Map                (),
            previewObjects: [{
                oid: 205,
                frames: [],
                ranges: [
                    { frameLo: 0, frameHi: 50, assetId: "oid205-amaterasu" },
                    { frameLo: 68, frameHi: 137, assetId: "oid205-timer" },
                    { frameLo: 68, frameHi: 137, assetId: "oid205-timer" },
                ],
            }],
        };
        const loaded           = [];
        const images = new Map                          ();
        const createImage = ()                   => {
            const listeners = new Map                         ();
            const image = {
                complete: false,
                addEventListener(type        , listener            ) {
                    const registered = listeners.get(type) ?? new Set            ();
                    registered.add(listener);
                    listeners.set(type, registered);
                },
                removeEventListener(type        , listener            ) {
                    listeners.get(type)?.delete(listener);
                },
                set src(value        ) {
                    loaded.push(value);
                    queueMicrotask(() => {
                        (image                         ).complete = true;
                        for (const listener of listeners.get("load") ?? []) listener();
                    });
                },
            }                               ;
            return image;
        };
        let renderRequests = 0;

        assert.deepEqual(previewObjectAssetIds(project), ["oid205-amaterasu", "oid205-timer"]);
        await preloadPreviewObjectAssets(project, images, () => { renderRequests += 1; }, createImage);

        assert.deepEqual(loaded, [
            "/api/assets/oid205-amaterasu",
            "/api/assets/oid205-timer",
        ]);
        assert.equal(images.size, 2);
        assert.equal(renderRequests, 2);
    });
});
