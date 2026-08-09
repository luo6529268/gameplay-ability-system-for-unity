// dat-skill-flow-build:20260807074945975-db32b3cebbf9429a8a7a5699a1b32f18
import { lastFrameForId, primaryPreviewEntity, spritePlacement } from "./project-client.js";
import { buildOverlayGeometry, OVERLAY_COLORS,                                        } from "./overlay-geometry.js";
import { number, text,                       } from "./editor-support.js";

                                                                                                                 
                                                                                
                                 
                                      
                                     
                                                 
 
                                     
                                       
                                     
                                           
                                             
                                                   
                                                       
                                             
                                       
 

function drawAxes(context                          , canvas                   , tick             , entity               )       {
    const x = number(entity.xInt ?? entity.x) + number(entity.renderOffsetX) - tick.cameraX;
    const y = number(entity.zInt ?? entity.z) + number(entity.yInt ?? entity.y);
    context.save();
    context.strokeStyle = "rgba(45, 199, 201, .45)";
    context.setLineDash([3, 3]);
    context.beginPath();
    context.moveTo(0, y); context.lineTo(canvas.width, y);
    context.moveTo(x, 0); context.lineTo(x, canvas.height);
    context.stroke();
    context.restore();
}

function drawGeometry(context                          , geometry                            )       {
    context.save();
    context.lineWidth = 2;
    for (const item of geometry) {
        context.strokeStyle = OVERLAY_COLORS[item.type];
        context.fillStyle = `${OVERLAY_COLORS[item.type]}30`;
        if (item.kind === "rect") {
            context.fillRect(item.x1, item.y1, item.width, item.height);
            context.strokeRect(item.x1, item.y1, item.width, item.height);
            continue;
        }
        context.beginPath();
        context.moveTo(item.x - 7, item.y); context.lineTo(item.x + 7, item.y);
        context.moveTo(item.x, item.y - 7); context.lineTo(item.x, item.y + 7);
        context.stroke();
    }
    context.restore();
}

function drawDraftGeometry(context                          , geometry                 )       {
    context.save();
    context.lineWidth = 2;
    context.strokeStyle = "#fff4a3";
    context.fillStyle = "rgba(255, 244, 163, .12)";
    context.setLineDash([5, 3]);
    if (geometry.kind === "rect") {
        context.fillRect(geometry.x1, geometry.y1, geometry.width, geometry.height);
        context.strokeRect(geometry.x1, geometry.y1, geometry.width, geometry.height);
    } else {
        context.beginPath();
        context.arc(geometry.x, geometry.y, 8, 0, Math.PI * 2);
        context.stroke();
    }
    context.restore();
}

function loadImage(input                    , assetId        )                   {
    let image = input.images.get(assetId);
    if (image === undefined) {
        image = new Image();
        image.src = `/api/assets/${encodeURIComponent(assetId)}`;
        image.onload = input.requestRender;
        input.images.set(assetId, image);
    }
    return image;
}

function drawEntity(
    context                          ,
    input                    ,
    entity               ,
    primary               ,
)                             {
    const loaded = entity.oid === 2;
    const frame = loaded && entity === primary ? input.runtimeFrame : loaded ? lastFrameForId(input.project.frames, entity.frame) : undefined;
    const pic = number(frame?.pic ?? entity.pic, 999);
    const range = loaded ? input.project.ranges.find((candidate) => pic >= number(candidate.frameLo ?? candidate.frame_lo) && pic <= number(candidate.frameHi ?? candidate.frame_hi, -1)) : undefined;
    const width = number(range?.w, 24), height = number(range?.h, 24), columns = number(range?.row);
    const placement = spritePlacement({ xInt: number(entity.xInt ?? entity.x), yInt: number(entity.yInt ?? entity.y), zInt: number(entity.zInt ?? entity.z), renderOffsetX: number(entity.renderOffsetX), cameraX: input.tick .cameraX, centerX: number(frame?.centerx), centerY: number(frame?.centery), width, facing: number(entity.facing) });
    const assetId = range === undefined ? undefined : (text(range.assetId) || input.project.assets.get(text(range.file)) || input.project.assets.get(""));
    if (!range || !assetId || pic === 999 || columns <= 0) {
        context.strokeStyle = "#e8b828";
        context.strokeRect(placement.x, placement.y, 24, 24);
        return [];
    }

    const local = pic - number(range.frameLo ?? range.frame_lo), image = loadImage(input, assetId);
    context.save();
    if (placement.mirror) {
        context.translate(placement.x + width, placement.y); context.scale(-1, 1);
        context.drawImage(image, (local % columns) * (width + 1), Math.floor(local / columns) * (height + 1), width, height, 0, 0, width, height);
    } else {
        context.drawImage(image, (local % columns) * (width + 1), Math.floor(local / columns) * (height + 1), width, height, placement.x, placement.y, width, height);
    }
    context.restore();
    if (entity !== primary || !frame) return [];
    return buildOverlayGeometry(frame, { left: placement.x, top: placement.y, width, height, mirror: placement.mirror })
        .filter((item) => input.visibleOverlays.has(item.type));
}

export function drawPreviewCanvas(input                    )                             {
    const context = input.canvas.getContext("2d");
    if (context === null) return [];
    context.clearRect(0, 0, input.canvas.width, input.canvas.height);
    if (input.tick === undefined) {
        context.fillStyle = "#8d98a6";
        context.fillText("尚未收到原生预览数据。", 20, 30);
        return [];
    }
    const primary = primaryPreviewEntity(input.tick.entities);
    if (primary === undefined) return [];
    drawAxes(context, input.canvas, input.tick, primary);
    let geometry                             = [];
    for (const entity of input.tick.entities) {
        const candidate = drawEntity(context, input, entity, primary);
        if (candidate.length > 0) geometry = candidate;
    }
    drawGeometry(context, geometry);
    if (input.draftGeometry !== undefined) drawDraftGeometry(context, input.draftGeometry);
    return geometry;
}
