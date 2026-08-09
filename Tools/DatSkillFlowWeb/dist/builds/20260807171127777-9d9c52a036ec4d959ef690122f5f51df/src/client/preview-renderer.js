// dat-skill-flow-build:20260807171127777-9d9c52a036ec4d959ef690122f5f51df
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

function loadColorKeyImage(
    input                    ,
    assetId        ,
    image                  ,
)                                {
    const existing = input.colorKeyImages.get(assetId);
    if (existing !== undefined) return existing;
    const width = image.naturalWidth || image.width;
    const height = image.naturalHeight || image.height;
    if ((!image.complete && width <= 0) || width <= 0 || height <= 0) return undefined;
    const canvas = document.createElement("canvas");
    canvas.width = width;
    canvas.height = height;
    const canvasContext = canvas.getContext("2d");
    if (canvasContext === null) return undefined;
    try {
        canvasContext.drawImage(image, 0, 0);
        const pixels = canvasContext.getImageData(0, 0, width, height);
        for (let index = 0; index < pixels.data.length; index += 4) {
            if (pixels.data[index] === 0 && pixels.data[index + 1] === 0 && pixels.data[index + 2] === 0) {
                pixels.data[index + 3] = 0;
            }
        }
        canvasContext.putImageData(pixels, 0, 0);
    } catch {
        return undefined;
    }
    input.colorKeyImages.set(assetId, canvas);
    return canvas;
}

export function spriteSheetColumnCount(range                  )         {
    return number(range?.col);
}

function drawEntity(
    context                          ,
    input                    ,
    entity               ,
    primary                           ,
)                             {
    const resource = input.project.previewObjects?.find((candidate) => candidate.oid === entity.oid);
    const isPrimary = entity === primary;
    const loaded = resource !== undefined || isPrimary;
    const frames = resource?.frames ?? (isPrimary ? input.project.frames : []);
    const ranges = resource?.ranges ?? (isPrimary ? input.project.ranges : []);
    const frame = isPrimary ? input.runtimeFrame ?? lastFrameForId(frames, entity.frame) : lastFrameForId(frames, entity.frame);
    const pic = number(frame?.pic ?? entity.pic, 999);
    const range = loaded ? ranges.find((candidate) => pic >= number(candidate.frameLo ?? candidate.frame_lo) && pic <= number(candidate.frameHi ?? candidate.frame_hi, -1)) : undefined;
    const width = number(range?.w, 24), height = number(range?.h, 24), columns = spriteSheetColumnCount(range);
    const placement = spritePlacement({ xInt: number(entity.xInt ?? entity.x), yInt: number(entity.yInt ?? entity.y), zInt: number(entity.zInt ?? entity.z), renderOffsetX: number(entity.renderOffsetX), cameraX: input.tick .cameraX, centerX: number(frame?.centerx), centerY: number(frame?.centery), width, facing: number(entity.facing) });
    const assetId = range === undefined ? undefined : (text(range.assetId) || input.project.assets.get(text(range.file)) || input.project.assets.get(""));
    if (!range || !assetId || pic === 999 || columns <= 0) {
        context.strokeStyle = "#e8b828";
        context.strokeRect(placement.x, placement.y, 24, 24);
        return [];
    }

    const local = pic - number(range.frameLo ?? range.frame_lo), image = loadImage(input, assetId);
    const colorKeyImage = loadColorKeyImage(input, assetId, image);
    if (colorKeyImage === undefined) {
        context.strokeStyle = "#e8b828";
        context.strokeRect(placement.x, placement.y, 24, 24);
        return [];
    }
    context.save();
    if (placement.mirror) {
        context.translate(placement.x + width, placement.y); context.scale(-1, 1);
        context.drawImage(colorKeyImage, (local % columns) * (width + 1), Math.floor(local / columns) * (height + 1), width, height, 0, 0, width, height);
    } else {
        context.drawImage(colorKeyImage, (local % columns) * (width + 1), Math.floor(local / columns) * (height + 1), width, height, placement.x, placement.y, width, height);
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
    const axisEntity = primary ?? input.tick.entities[0];
    if (axisEntity === undefined) return [];
    drawAxes(context, input.canvas, input.tick, axisEntity);
    let geometry                             = [];
    for (const entity of input.tick.entities) {
        const candidate = drawEntity(context, input, entity, primary);
        if (candidate.length > 0) geometry = candidate;
    }
    drawGeometry(context, geometry);
    if (input.draftGeometry !== undefined) drawDraftGeometry(context, input.draftGeometry);
    return geometry;
}
