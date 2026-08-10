// dat-skill-flow-build:20260810150907087-4c5138c19d264800acfbc2f7deac1381
import { currentFrame } from "./catalog.js";
import { GATE4_MOTION_RULE } from "./rules.js";
                                            

                                                                                                  

function replace(entity           , changes                    )            {
    return Object.freeze({ ...entity, ...changes });
}

function snapshot(value        , label        )         {
    const result = Math.trunc(value);
    if (!Number.isSafeInteger(result)) throw new TypeError(`${label} must be a safe integer`);
    return result;
}

function applyFrameVelocity(value        , velocity        , direction        )         {
    if (value > 500) return value - 550;
    if (value === 550) return 0;
    if (value === 0) return velocity;
    const candidate = value * direction;
    if (value > 0) return direction >= 0 ? Math.max(velocity, candidate) : Math.min(velocity, candidate);
    return direction >= 0 ? Math.min(velocity, candidate) : Math.max(velocity, candidate);
}

function applyGroundFriction(velocity        )         {
    if (velocity > 0.0001) {
        const reduced = velocity - 1;
        return reduced < 0.0001 ? 0 : reduced;
    }
    if (velocity < -0.0001) {
        const reduced = velocity + 1;
        return reduced > 0.0001 ? 0 : reduced;
    }
    return velocity;
}

export function runMotion(entity           )               {
    const rules           = [GATE4_MOTION_RULE.frameAdvanceGates];
    if (entity.frameDelay !== 0) {
        return { entity: replace(entity, { frameDelay: entity.frameDelay > 0 ? entity.frameDelay - 1 : entity.frameDelay + 1 }), ruleIds: rules };
    }
    if (entity.linkState < 0) return { entity, ruleIds: rules };
    const definition = currentFrame(entity.frames, entity.frame);
    if (definition === undefined || definition.cpoints?.[0]?.kind === 2) return { entity, ruleIds: rules };

    let vx = entity.vx;
    let vy = entity.vy;
    let vz = entity.vz;
    if (entity.rawObjectType !== 0) {
        vx = applyFrameVelocity(definition.dvx ?? 0, vx, entity.facing === 0 ? 1 : -1);
        const dvy = definition.dvy ?? 0;
        if (dvy > 500) vy = dvy - 550;
        else if (dvy !== 0) vy += dvy;
        const dvz = definition.dvz ?? 0;
        if (dvz > 500) vz = dvz - 550;
        else if (dvz !== 0) {
            if (entity.keyUp && entity.cooldowns.up >= entity.cooldowns.down) vz = -dvz;
            if (entity.keyDown && entity.cooldowns.down >= entity.cooldowns.up) vz = dvz;
        }
        rules.push(GATE4_MOTION_RULE.nonCharacterFrameDv);
    }

    let x = entity.x;
    let y = entity.y;
    let z = entity.z;
    if (!((vx > 0 && entity.blockRight) || (vx < 0 && entity.blockLeft))) x += vx;
    if (entity.rawObjectType === 4 || entity.oid === 120) x += vx * 0.2;
    if (entity.oid === 101) x -= vx * 0.2;
    if (!((vz > 0 && entity.blockForwardZ) || (vz < 0 && entity.blockBackZ))) z += vz;
    rules.push(GATE4_MOTION_RULE.integrateXz, GATE4_MOTION_RULE.specialXCorrection);

    if (entity.yInt >= 0) {
        vx = applyGroundFriction(vx);
        vz = applyGroundFriction(vz);
        rules.push(GATE4_MOTION_RULE.groundFriction);
    }

    let frame = entity.frame;
    if ((entity.rawObjectType === 4 || entity.rawObjectType === 6)
        && definition.state === 1000 && Math.abs(vx) > 9) {
        frame = 40;
        rules.push(GATE4_MOTION_RULE.fastProjectileFrame);
    }

    y += vy;
    let attacking = entity.attacking;
    const physicsDefinition = currentFrame(entity.frames, frame) ?? definition;
    if (y < -0.0001) {
        if (entity.rawObjectType !== 3) {
            let gravity = 1.7;
            if (entity.rawObjectType === 6) gravity = 1.1333333333333333;
            else if (entity.rawObjectType === 4) gravity = 0.85;
            else if (physicsDefinition.state === 1002) {
                gravity = entity.oid === 124 ? 0.17
                    : entity.oid === 120 ? 0.425
                        : entity.oid === 101 ? 1.1333333333333333 : 0.5666666666666667;
            }
            vy += gravity;
        }
        rules.push(GATE4_MOTION_RULE.verticalGravity);
        if (entity.rawObjectType === 0 && physicsDefinition.state === 12) {
            if (frame < 185) frame = vy < -8 ? 180 : vy < 1 ? 181 : vy < 8 ? 182 : 183;
            else if (frame > 185 && frame < 191) frame = vy < -8 ? 186 : vy < 1 ? 187 : vy < 8 ? 188 : 189;
            rules.push(GATE4_MOTION_RULE.airborneFrameSelect);
        }
        const selected = currentFrame(entity.frames, frame);
        if (entity.rawObjectType === 0 && selected?.state === 18 && frame < 205 && vy > 1) {
            frame = 205;
            rules.push(GATE4_MOTION_RULE.airborneFrameSelect);
        }
    } else if (entity.rawObjectType === 0 && y > 0.0001 && vy > 0.0001
        && physicsDefinition.state !== 12 && physicsDefinition.state !== 13 && physicsDefinition.state !== 18) {
        vx *= 1 / 3;
        y = 0;
        vy = 0;
        frame = physicsDefinition.state === 100 ? 94 : entity.frame === 212 || physicsDefinition.state === 6 ? 215 : 219;
        attacking = 0;
        rules.push(GATE4_MOTION_RULE.genericCharacterLanding);
    }

    const result = replace(entity, {
        x, y, z, vx, vy, vz, frame, attacking,
        xInt: snapshot(x, "entity.xInt"), yInt: snapshot(y, "entity.yInt"), zInt: snapshot(z, "entity.zInt"),
        blockBackZ: false, blockForwardZ: false, blockLeft: false, blockRight: false,
    });
    rules.push(GATE4_MOTION_RULE.integerSnapshots);
    return { entity: result, ruleIds: rules };
}
