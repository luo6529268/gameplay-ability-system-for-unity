// dat-skill-flow-build:20260808014955742-8de78aa9f9e44b1ca8ac1816c8f498ca
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { LosslessDatDocument } from "../../src/model/dat-document.js";

const hostileKeys = ["__proto__", "constructor", "toString", "unknown_key"]         ;

function assertExactDto(value        , expectedKeys                   )       {
    assert.equal(Object.getPrototypeOf(value), Object.prototype);
    assert.deepEqual(Object.keys(value).sort(), [...expectedKeys].sort());
    for (const key of hostileKeys) assert.equal(Object.hasOwn(value, key), false, `${key} must not be projected`);
    assert.equal(value.constructor, Object);
    assert.equal(value.toString, Object.prototype.toString);
}

function prototypeSensitiveDat()         {
    return Buffer.from([
        "name: Safe Hero\n",
        "walking_speed: 4\n",
        "walking_speed: 5\n",
        "__proto__: 101\n",
        "constructor: 102\n",
        "toString: 103\n",
        "unknown_key: 104\n",
        "<frame> 7 guarded\n",
        "pic: 1 pic: 2 state: 3\n",
        "__proto__: 201 constructor: 202 toString: 203 unknown_key: 204\n",
        "itr:\n",
        " kind: 1 kind: 2 x: 3 catchingact: 4 5 caughtact: 6 7\n",
        " __proto__: 301 constructor: 302 toString: 303 unknown_key: 304\n",
        "itr_end:\n",
        "bdy:\n",
        " x: 3 h: 4 h: 5\n",
        " __proto__: 351 constructor: 352 toString: 353 unknown_key: 354\n",
        "bdy_end:\n",
        "opoint:\n",
        " kind: 1 oid: 200 oid: 201\n",
        " __proto__: 401 constructor: 402 toString: 403 unknown_key: 404\n",
        "opoint_end:\n",
        "wpoint:\n",
        " kind: 1 weaponact: 10 weaponact: 11\n",
        " __proto__: 501 constructor: 502 toString: 503 unknown_key: 504\n",
        "wpoint_end:\n",
        "bpoint:\n",
        " x: 8 x: 9\n",
        " __proto__: 601 constructor: 602 toString: 603 unknown_key: 604\n",
        "bpoint_end:\n",
        "cpoint:\n",
        " kind: 1 injury: 2 cover: 3 fronthurtact: 70 backhurtact: 71\n",
        " __proto__: 701 constructor: 702 toString: 703 unknown_key: 704\n",
        "cpoint_end:\n",
        "<frame_end>\n",
    ].join(""), "ascii");
}

describe("Gate4B2 DatProjection prototype-sensitive keys", () => {
    it("projects only explicit top, frame, itr, cpoint, and simple-block DTO fields", () => {
        const document = LosslessDatDocument.fromPlaintext(prototypeSensitiveDat());
        const projection = document.projection;
        const frame = projection.getFrame(7);
        assert.ok(frame);

        assertExactDto(projection.top, [
            "name", "head", "small", "weapon_hit_sound", "weapon_drop_sound", "weapon_broken_sound",
            "weapon_hp", "weapon_drop_hurt", "walking_frame_rate", "walking_speed", "walking_speedz",
            "running_frame_rate", "running_speed", "running_speedz", "heavy_walking_speed",
            "heavy_walking_speedz", "heavy_running_speed", "heavy_running_speedz", "jump_height",
            "jump_distance", "jump_distancez", "dash_height", "dash_distance", "dash_distancez",
            "rowing_height", "rowing_distance",
        ]);
        assertExactDto(frame, [
            "frameId", "occurrence", "label", "pic", "state", "wait", "next", "dvx", "dvy", "dvz",
            "centerx", "centery", "hit_Fa", "hit_Fj", "hit_Ua", "hit_Uj", "hit_Da", "hit_Dj",
            "hit_ja", "hit_a", "hit_d", "hit_j", "mp", "vaction", "sound", "itrs", "bdys",
            "opoints", "wpoints", "bpoints", "cpoints",
        ]);
        assertExactDto(frame.itrs[0] , [
            "kind", "x", "y", "w", "h", "dvx", "dvy", "fall", "bdefend", "injury", "arest",
            "vrest", "effect", "attacking", "catchingact", "catchingact2", "caughtact", "caughtact2",
            "respond", "pickingact", "pickedact", "throwvx", "throwvy", "zwidth", "throwvz", "throwinjury",
        ]);
        assertExactDto(frame.bdys[0] , ["x", "y", "w", "h"]);
        assertExactDto(frame.opoints[0] , ["kind", "x", "y", "action", "dvx", "dvy", "oid", "facing"]);
        assertExactDto(frame.wpoints[0] , ["kind", "x", "y", "attacking", "cover", "weaponact", "dvx", "dvy", "dvz"]);
        assertExactDto(frame.bpoints[0] , ["x", "y"]);
        assertExactDto(frame.cpoints[0] , [
            "kind", "x", "y", "injury", "cover", "vaction", "aaction", "jaction", "daction",
            "taction", "throwvx", "throwvy", "throwvz", "throwinjury", "hurtable", "decrease",
            "dircontrol", "fronthurtact", "backhurtact",
        ]);
    });

    it("keeps legal duplicate-last/alias semantics while preserving hostile CST bytes losslessly", () => {
        const source = prototypeSensitiveDat();
        const document = LosslessDatDocument.fromPlaintext(source);
        const frame = document.projection.getFrame(7);

        assert.equal(document.projection.top.walking_speed, 5);
        assert.equal(document.projection.top.name, "Safe Hero");
        assert.equal(frame?.pic, 2);
        assert.deepEqual([frame?.itrs[0]?.kind, frame?.itrs[0]?.catchingact, frame?.itrs[0]?.catchingact2], [2, 4, 5]);
        assert.deepEqual([frame?.itrs[0]?.caughtact, frame?.itrs[0]?.caughtact2], [6, 7]);
        assert.equal(frame?.bdys[0]?.h, 5);
        assert.equal(frame?.opoints[0]?.oid, 201);
        assert.equal(frame?.wpoints[0]?.weaponact, 11);
        assert.equal(frame?.bpoints[0]?.x, 9);
        assert.deepEqual(
            [frame?.cpoints[0]?.fronthurtact, frame?.cpoints[0]?.injury, frame?.cpoints[0]?.backhurtact, frame?.cpoints[0]?.cover],
            [70, 70, 71, 71],
        );
        assert.equal(document.cst.topFields.some((field) => field.key === "__proto__"), true);
        assert.equal(document.cst.frames[0]?.blocks.some((block) => block.fields.some((field) => hostileKeys.includes(field.key                              ))), true);
        assert.deepEqual(document.emitPlaintext(), source);
        assert.deepEqual(document.diagnostics, []);
    });

    it("resets itr action secondary values when a duplicate last occurrence omits them", () => {
        const source = Buffer.from([
            "<frame> 1 duplicate-actions\n",
            "itr:\n",
            " catchingact: 4 5 catchingact: 6 caughtact: 7 8 caughtact: 9\n",
            "itr_end:\n",
            "<frame_end>\n",
        ].join(""), "ascii");
        const itr = LosslessDatDocument.fromPlaintext(source).projection.getFrame(1)?.itrs[0];

        assert.deepEqual(
            [itr?.catchingact, itr?.catchingact2, itr?.caughtact, itr?.caughtact2],
            [6, 0, 9, 0],
        );
    });
});
