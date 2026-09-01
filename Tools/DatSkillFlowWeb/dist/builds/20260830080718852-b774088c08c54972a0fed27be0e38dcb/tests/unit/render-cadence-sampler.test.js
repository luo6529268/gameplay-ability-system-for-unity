// dat-skill-flow-build:20260830080718852-b774088c08c54972a0fed27be0e38dcb
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    NATIVE_LOGIC_TICK_MS,
    renderCadenceLoopDurationMs,
    sampleRenderCadence,
} from "../../src/client/render-cadence-sampler.js";
                                                                        

function tick(
    index        ,
    x        ,
    frame        ,
    options                                                         = {},
)              {
    return {
        tick: index,
        cameraX: options.cameraX ?? index * 4,
        entities: [{
            slot: 0,
            oid: options.oid ?? 2,
            lineageId: options.lineageId ?? "p1",
            frame,
            pic: frame,
            facing: 0,
            x,
            y: 0,
            z: 500,
            xInt: x,
            yInt: 0,
            zInt: 500,
            displayZ: 500,
            renderOffsetX: index * 2,
            link: 0,
            hitStop: 0,
        }],
    };
}

describe("render cadence sampler", () => {
    const trace = [tick(0, 100, 5), tick(1, 112, 6), tick(2, 136, 7)];

    it("keeps 30 Hz as an exact discrete Native snapshot", () => {
        const sample = sampleRenderCadence(trace, NATIVE_LOGIC_TICK_MS + 1, 30);
        assert.equal(sample.sourceTickIndex, 1);
        assert.equal(sample.previousTickIndex, 1);
        assert.equal(sample.interpolationAlpha, 1);
        assert.equal(sample.presentationTick?.entities[0]?.xInt, 112);
        assert.equal(sample.presentationTick?.entities[0]?.frame, 6);
    });

    it("uses quantized delayed 60/120 Hz presentation samples while keeping the current frame discrete", () => {
        // At 59 ms, the 120 Hz presentation clock has reached 58.333… ms,
        // while the 60 Hz clock is still at 50 ms. This proves display-rate
        // quantization without changing the shared 30 Hz trace.
        const at60 = sampleRenderCadence(trace, 59, 60);
        const at120 = sampleRenderCadence(trace, 59, 120);

        assert.equal(at60.sourceTickIndex, 1);
        assert.equal(at120.sourceTickIndex, 1);
        assert.equal(at60.presentationTick?.entities[0]?.frame, 6);
        assert.equal(at120.presentationTick?.entities[0]?.frame, 6);
        assert.ok((at60.presentationTick?.entities[0]?.xInt ?? 0) > 100);
        assert.ok((at120.presentationTick?.entities[0]?.xInt ?? 0) > (at60.presentationTick?.entities[0]?.xInt ?? 0));
        assert.ok((at120.presentationTick?.cameraX ?? 0) > (at60.presentationTick?.cameraX ?? 0));
    });

    it("never blends an entity that was born this tick or whose slot was reused", () => {
        const born = sampleRenderCadence([
            { tick: 0, cameraX: 0, entities: [] },
            tick(1, 250, 30, { oid: 33, lineageId: "clone-1" }),
        ], 58, 120);
        assert.equal(born.presentationTick?.entities[0]?.xInt, 250);

        const reused = sampleRenderCadence([
            tick(0, 100, 5, { oid: 2, lineageId: "p1" }),
            tick(1, 250, 30, { oid: 33, lineageId: "clone-1" }),
        ], 58, 120);
        assert.equal(reused.presentationTick?.entities[0]?.xInt, 250);
        assert.equal(reused.presentationTick?.entities[0]?.oid, 33);
        assert.equal(reused.presentationTick?.entities[0]?.frame, 30);
    });

    it("rejects invalid sampling inputs and keeps loop duration in Native tick units", () => {
        assert.throws(() => sampleRenderCadence(trace, -1, 60), /elapsedMs/);
        assert.throws(() => sampleRenderCadence(trace, 1, 75         ), /Unsupported/);
        assert.equal(renderCadenceLoopDurationMs(trace), 2 * NATIVE_LOGIC_TICK_MS);
        assert.equal(renderCadenceLoopDurationMs([]), NATIVE_LOGIC_TICK_MS);
    });
});
