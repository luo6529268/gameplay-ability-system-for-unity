// dat-skill-flow-build:20260807171127777-9d9c52a036ec4d959ef690122f5f51df
import { describe, it } from "node:test";
import assert from "node:assert/strict";

import { enrichNativePreview, objectKind } from "../../src/server/native-preview-trace.js";
             
                      
                             
                                                  

const resources                                      = [
    { oid: 2, type: 0, name: "Naruto", spriteRanges: [], frames: [
        { frameId: 300, occurrence: 0, label: "", pic: 0, state: 1, wait: 1, next: 0 }         ,
        { frameId: 0, occurrence: 0, label: "", pic: 0, state: 0, wait: 1, next: 0 }         ,
    ] },
    { oid: 33, type: 0, name: "Naruto Clone", spriteRanges: [], frames: [] },
    { oid: 204, type: 1, name: "Wind", spriteRanges: [], frames: [] },
];

function entity(slot        , oid        , frame        , yInt        , velocityY        ) {
    return {
        slot, oid, frame, pic: 0, facing: 0, x: 0, y: yInt, z: 0,
        xInt: 0, yInt, zInt: 0, velocity: { x: 0, y: velocityY, z: 0 },
        renderOffsetX: 0, frameDelay: 0, team: 1, target: -1, holder: -1, link: 0, ai: false,
        objectType: null                 , kind: "unknown"         , lineageId: "unclassified",
        firstSeenTick: 0, lastSeenTick: 0, resourceAvailable: false,
    };
}

function preview()                    {
    return {
        metadata: {
            runtime: "ntsd_cpp", tickDriver: "SimulationTickDriver", renderer: "none",
            seed: 1, startFrame: 300, ticksRequested: 3,
            stage: { index: 0, name: "Stage", width: 1000, zMin: 0, zMax: 500 },
            initial: { p1: { x: 0, y: 0, z: 0 }, p2: { x: 0, y: 0, z: 0 } },
        },
        ticks: [
            { tick: 0, cameraX: 0, cameraVelocity: 0, background: { width: 1000, zMin: 0, zMax: 500, boundLeft: 0, boundRight: 1000 }, entities: [entity(0, 2, 300, 0, 0), entity(50, 204, 0, -10, 1)] },
            { tick: 1, cameraX: 0, cameraVelocity: 0, background: { width: 1000, zMin: 0, zMax: 500, boundLeft: 0, boundRight: 1000 }, entities: [entity(0, 2, 0, 0, 0), entity(50, 204, 0, 0, 0), entity(51, 33, 0, 0, 0)] },
            { tick: 2, cameraX: 0, cameraVelocity: 0, background: { width: 1000, zMin: 0, zMax: 500, boundLeft: 0, boundRight: 1000 }, entities: [entity(0, 2, 0, 0, 0), entity(51, 33, 0, 0, 0), entity(50, 216, 0, 0, 0)] },
        ],
        resources,
        trace: {
            rootSkillEndedTick: null, progressEndTick: null, playbackEndTick: 0, status: "timeout",
            pendingProjectiles: [], entities: [], events: [],
        },
    };
}

describe("native preview trace classification", () => {
    it("maps raw DAT object types without treating unknown OIDs as projectiles", () => {
        assert.equal(objectKind({ slot: 0, oid: 2 }, 0, resources[0], 2), "root");
        assert.equal(objectKind({ slot: 51, oid: 33 }, 0, resources[1], 2), "clone");
        assert.equal(objectKind({ slot: 50, oid: 204 }, 1, resources[2], 2), "projectile");
        assert.equal(objectKind({ slot: 50, oid: 999 }, null, undefined, 2), "unknown");
    });

    it("records root completion, clone success, projectile landing, and slot reuse separately", () => {
        const result = enrichNativePreview(preview(), resources, new Map([
            [2, 0], [33, 0], [204, 1], [216, 2],
        ]), 2);
        assert.equal(result.trace.rootSkillEndedTick, 1);
        assert.equal(result.trace.progressEndTick, 1);
        assert.equal(result.trace.playbackEndTick, 2);
        assert.equal(result.trace.status, "persistent");
        assert.equal(result.trace.pendingProjectiles.length, 1);
        assert.equal(result.trace.entities.find((item) => item.oid === 33)?.completion, "spawned");
        assert.equal(result.trace.entities.find((item) => item.oid === 204)?.completion, "landed");
        assert.equal(result.trace.entities.filter((item) => item.slot === 50).length, 2);
        assert.equal(result.trace.events.filter((item) => item.kind === "spawn").length, 4);
    });
});
