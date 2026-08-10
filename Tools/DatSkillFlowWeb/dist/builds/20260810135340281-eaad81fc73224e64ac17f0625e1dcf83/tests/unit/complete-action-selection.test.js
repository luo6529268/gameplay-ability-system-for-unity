// dat-skill-flow-build:20260810135340281-eaad81fc73224e64ac17f0625e1dcf83
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { selectCompleteActionIndex } from "../../src/client/complete-action-selection.js";
                                                                    
                                                                                                        

function skill(startFrame        )             {
    return {
        id: `entry:2:${startFrame}`,
        oid: 2,
        startFrame,
        startOccurrence: startFrame,
        label: `frame_${startFrame}`,
        displayName: `F${startFrame}`,
        category: "action",
        group: "其他动作",
        order: startFrame,
        pinned: false,
        hidden: false,
        notes: "",
        segmentFrameCount: 1,
        triggers: [],
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
});
