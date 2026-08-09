// dat-skill-flow-build:20260808055420174-a6f8cc25e67848ac89ea47f52447d7f7
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    drawPreviewCanvas,
    effectivePreviewPic,
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
                    renderOffsetX: 0,
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
});
