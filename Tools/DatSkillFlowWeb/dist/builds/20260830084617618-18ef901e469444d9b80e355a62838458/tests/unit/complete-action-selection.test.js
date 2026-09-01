// dat-skill-flow-build:20260830084617618-18ef901e469444d9b80e355a62838458
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    buildInternalStageChain,
    nextDistanceToFrame,
    selectCompleteActionIndex,
} from "../../src/client/complete-action-selection.js";
                                                                    
                                                                                                        

function skill(startFrame        )             {
    return {
        id: `entry:2:${startFrame}`,
        oid: 2,
        startFrame,
        startOccurrence: startFrame,
        label: `frame_${startFrame}`,
        displayName: `F${startFrame}`,
        category: "input",
        group: "其他动作",
        order: startFrame,
        pinned: false,
        hidden: false,
        notes: "",
        segmentFrameCount: 1,
        actionRole: "root",
        triggers: [],
        routes: [],
        parentStartFrames: [],
        internalStages: [],
    };
}

function chain(...occurrences          )                 {
    const nodes                       = occurrences.map((occurrence) => ({
        id: `frame:${occurrence}:${occurrence}`,
        kind: "frame",
        frameId: occurrence,
        occurrence,
    }));
    const edges                  = nodes.slice(0, -1).map((node, index) => ({
        id: `${node.id}:next`,
        from: node.id,
        key: "next",
        rawTarget: nodes[index + 1] .frameId,
        resolution: "frame",
        to: nodes[index + 1] .id,
    }));
    return {
        startFrame: occurrences[0] ,
        startNodeId: nodes[0] .id,
        nodes,
        edges,
        cycles: [],
    };
}

describe("complete action selection", () => {
    it("chooses the earliest real next-chain entry instead of an isolated target frame", () => {
        const skills = [skill(210), skill(211), skill(212)];
        const graphs = [chain(210, 211, 212), chain(211, 212), chain(212)];

        assert.equal(selectCompleteActionIndex(skills, 2, 212, (_, index) => graphs[index]), 0);
    });

    it("keeps the current complete action when the target belongs to it", () => {
        const skills = [skill(263), skill(280)];
        const graphs = [chain(263, 264, 283, 284), chain(280, 284)];

        assert.equal(selectCompleteActionIndex(skills, 2, 284, (_, index) => graphs[index], 1), 1);
    });

    it("returns minus one when no action contains the target", () => {
        const skills = [skill(100)];

        assert.equal(selectCompleteActionIndex(skills, 2, 999, () => chain(100, 101)), -1);
    });

    it("orders nested internal hit stages from the real parent next-chain", () => {
        const parent = {
            ...skill(271),
            actionRole: "root"         ,
        };
        const first = {
            ...skill(355),
            actionRole: "internal"         ,
            routes: [{
                key: "hit_a"         ,
                sourceFrame: 271,
                sourceOccurrence: 271,
                sourceLabel: "clone",
                sourceState: 3,
                sourceKind: "action"         ,
            }],
            parentStartFrames: [271],
        };
        const second = {
            ...skill(356),
            actionRole: "internal"         ,
            routes: [{
                key: "hit_d"         ,
                sourceFrame: 355,
                sourceOccurrence: 355,
                sourceLabel: "clone_hell",
                sourceState: 3,
                sourceKind: "action"         ,
            }],
            parentStartFrames: [271],
        };
        const graphs = new Map([
            [271, chain(271, 272, 273, 274)],
            [355, chain(355, 357, 358)],
            [356, chain(356, 359)],
        ]);

        assert.deepEqual(buildInternalStageChain(
            parent,
            second,
            [parent, first, second],
            (entry, sourceFrame) => nextDistanceToFrame(graphs.get(entry.startFrame), sourceFrame) >= 0,
        )?.map((entry) => entry.startFrame), [355, 356]);
    });

    it("rejects a cyclic internal hit dependency", () => {
        const parent = skill(271);
        const first = {
            ...skill(355),
            actionRole: "internal"         ,
            routes: [{
                key: "hit_a"         ,
                sourceFrame: 356,
                sourceOccurrence: 356,
                sourceLabel: "cycle_b",
                sourceState: 3,
                sourceKind: "action"         ,
            }],
            parentStartFrames: [271],
        };
        const second = {
            ...skill(356),
            actionRole: "internal"         ,
            routes: [{
                key: "hit_d"         ,
                sourceFrame: 355,
                sourceOccurrence: 355,
                sourceLabel: "cycle_a",
                sourceState: 3,
                sourceKind: "action"         ,
            }],
            parentStartFrames: [271],
        };

        assert.equal(buildInternalStageChain(
            parent,
            first,
            [parent, first, second],
            (entry, sourceFrame) => entry.startFrame === sourceFrame,
        ), undefined);
    });
});
