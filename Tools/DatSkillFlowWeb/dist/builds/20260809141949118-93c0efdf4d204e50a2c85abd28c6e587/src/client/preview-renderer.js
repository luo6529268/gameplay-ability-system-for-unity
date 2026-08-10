// dat-skill-flow-build:20260809141949118-93c0efdf4d204e50a2c85abd28c6e587
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

function drawGround(context                          , tick             )       {
    const ground = number(recordValue(tick.background)?.zMin, 0);
    if (ground <= 0) return;
    context.save();
    context.fillStyle = "rgba(80, 60, 30, .45)";
    context.fillRect(0, ground, context.canvas.width, 1);
    context.restore();
}

function recordValue(value         )                                      {
    return typeof value === "object" && value !== null && !Array.isArray(value)
        ? value                           
        : undefined;
}

function ensureImage(
    images                               ,
    assetId        ,
    requestRender            ,
    createImage               = () => new Image(),
)                   {
    let image = images.get(assetId);
    if (image === undefined) {
        image = createImage();
        images.set(assetId, image);
        image.addEventListener("load", requestRender);
        image.addEventListener("error", requestRender);
        image.src = `/api/assets/${encodeURIComponent(assetId)}`;
    }
    return image;
}

function loadImage(input                    , assetId        )                   {
    return ensureImage(input.images, assetId, input.requestRender);
}

export function previewObjectAssetIds(project                )                    {
    const result = new Set        ();
    for (const resource of project.previewObjects ?? []) {
        for (const range of resource.ranges) {
            const assetId = text(range.assetId)
                || project.assets.get(text(range.file))
                || project.assets.get("")
                || "";
            if (assetId) result.add(assetId);
        }
    }
    return Object.freeze([...result]);
}

function waitForImage(image                  )                {
    if (image.complete) return Promise.resolve();
    return new Promise((resolve) => {
        const settle = ()       => {
            image.removeEventListener("load", settle);
            image.removeEventListener("error", settle);
            resolve();
        };
        image.addEventListener("load", settle);
        image.addEventListener("error", settle);
    });
}

export async function preloadPreviewObjectAssets(
    project                ,
    images                               ,
    requestRender            ,
    createImage               = () => new Image(),
)                {
    await Promise.all(previewObjectAssetIds(project).map(async (assetId) => {
        const image = ensureImage(images, assetId, requestRender, createImage);
        await waitForImage(image);
    }));
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

function imageReady(image                  )          {
    return image.complete && (image.naturalWidth || image.width) > 0 && (image.naturalHeight || image.height) > 0;
}

function drawBackgroundLayer(
    context                          ,
    input                    ,
    layer                                                           ,
    tick             ,
)       {
    const assetId = text(layer.assetId);
    if (!assetId || (layer.cc > 0 && (layer.animCounter < layer.c1 || layer.animCounter > layer.c2))) return;
    const image = loadImage(input, assetId);
    if (!imageReady(image)) return;
    const source = layer.transparency === 0 ? image : loadColorKeyImage(input, assetId, image);
    if (!source) return;
    const stageWidth = number(input.project.stage?.width, number(tick.background?.width, context.canvas.width));
    const parallaxWidth = number(layer.parallaxWidth, stageWidth);
    const parallax = stageParallaxOffset(stageWidth, context.canvas.width, parallaxWidth, tick.cameraX);
    if (layer.x >= parallaxWidth) return;
    const x = layer.x + parallax;
    const y = layer.y;
    const width = image.naturalWidth || image.width;
    const height = image.naturalHeight || image.height;
    context.drawImage(source, x, y, width, height);
    if (layer.loop <= 0) return;
    for (let tilePosition = layer.x + layer.loop, tileX = x + layer.loop, tileCount = 0;
        tilePosition < parallaxWidth && tileCount < 4096;
        tilePosition += layer.loop, tileX += layer.loop, tileCount += 1) {
        context.drawImage(source, tileX, y, width, height);
    }
}

function drawSceneBackground(context                          , input                    , tick             )       {
    context.fillStyle = "#151b25";
    context.fillRect(0, 0, context.canvas.width, context.canvas.height);
    for (const layer of input.project.stage?.background?.layers ?? []) {
        drawBackgroundLayer(context, input, layer, tick);
    }
    drawGround(context, tick);
}

function drawShadow(
    context                          ,
    input                    ,
    entity               ,
    primary                           ,
    resource                           ,
)       {
    const shadow = input.project.stage?.background?.shadow;
    const assetId = text(shadow?.assetId);
    const hitStop = number(entity.hitStop);
    if (!shadow || !assetId || entity.link < 0 || hitStop <= -70 || Math.abs(hitStop) % 4 >= 2) return;
    const isPrimary = entity === primary;
    const frames = resource?.frames ?? (isPrimary ? input.project.frames : []);
    const frame = isPrimary ? input.runtimeFrame ?? lastFrameForId(frames, entity.frame) : lastFrameForId(frames, entity.frame);
    if (frame?.state === 3005 || frame?.state === 9997) return;
    const image = loadImage(input, assetId);
    if (!imageReady(image)) return;
    const source = loadColorKeyImage(input, assetId, image);
    if (!source) return;
    const width = number(shadow.width, image.naturalWidth || image.width);
    const height = number(shadow.height, image.naturalHeight || image.height);
    const x = number(entity.xInt ?? entity.x) + number(entity.renderOffsetX) - input.tick .cameraX - width / 2;
    const y = number(entity.zInt ?? entity.z) - height / 2;
    context.drawImage(source, 0, 0, width, height, x, y, width, height);
}

export function spriteSheetColumnCount(range                  )         {
    // ntsd_cpp passes DAT SpriteRange::row as Renderer::load_sprite(..., cols).
    return number(range?.row);
}

export function effectivePreviewPic(entity               , frame                   )         {
    return number(entity.renderPic ?? frame?.pic ?? entity.pic, 999);
}

export function sortPreviewEntities(entities                          )                  {
    return [...entities].sort((left, right) =>
        number(left.zInt ?? left.z) - number(right.zInt ?? right.z));
}

export function stageParallaxOffset(
    stageWidth        ,
    viewportWidth        ,
    parallaxWidth        ,
    cameraX        ,
)         {
    if (stageWidth <= viewportWidth) return 0;
    const offset = -((parallaxWidth - viewportWidth) * cameraX) / (stageWidth - viewportWidth);
    return offset === 0 ? 0 : offset;
}

function drawEntity(
    context                          ,
    input                    ,
    entity               ,
    primary                           ,
    resource                           ,
)                             {
    const hitStop = number(entity.hitStop);
    if (hitStop <= -25 || Math.abs(hitStop) % 4 >= 2) return [];
    const isPrimary = entity === primary;
    const loaded = resource !== undefined || isPrimary;
    const frames = resource?.frames ?? (isPrimary ? input.project.frames : []);
    const ranges = resource?.ranges ?? (isPrimary ? input.project.ranges : []);
    const frame = isPrimary ? input.runtimeFrame ?? lastFrameForId(frames, entity.frame) : lastFrameForId(frames, entity.frame);
    const pic = effectivePreviewPic(entity, frame);
    const range = loaded ? ranges.find((candidate) => pic >= number(candidate.frameLo ?? candidate.frame_lo) && pic <= number(candidate.frameHi ?? candidate.frame_hi, -1)) : undefined;
    const width = number(range?.w), height = number(range?.h), columns = spriteSheetColumnCount(range);
    const rows = number(range?.col);
    const assetId = range === undefined ? undefined : (text(range.assetId) || input.project.assets.get(text(range.file)) || input.project.assets.get(""));
    if (!range || !assetId || pic === 999 || columns <= 0 || rows <= 0 || width <= 0 || height <= 0) {
        return [];
    }

    const renderPhase = Math.trunc(number(input.tick .tick)) & 1;
    const extraX = number(entity.frameDelay) < 0 ? 6 * renderPhase - 3 : 0;
    const placement = spritePlacement({
        xInt: number(entity.xInt ?? entity.x) + extraX,
        yInt: number(entity.yInt ?? entity.y),
        zInt: number(entity.displayZ ?? entity.zInt ?? entity.z),
        renderOffsetX: number(entity.renderOffsetX),
        cameraX: input.tick .cameraX,
        centerX: number(frame?.centerx),
        centerY: number(frame?.centery),
        width,
        facing: number(entity.facing),
    });
    const drawX = frame?.state === 9997 ? Math.max(0, Math.min(714, placement.x)) : placement.x;

    const local = pic - number(range.frameLo ?? range.frame_lo), image = loadImage(input, assetId);
    const colorKeyImage = loadColorKeyImage(input, assetId, image);
    if (colorKeyImage === undefined) {
        return [];
    }
    context.save();
    if (placement.mirror) {
        context.translate(drawX + width, placement.y); context.scale(-1, 1);
        context.drawImage(colorKeyImage, (local % columns) * (width + 1), Math.floor(local / columns) * (height + 1), width, height, 0, 0, width, height);
    } else {
        context.drawImage(colorKeyImage, (local % columns) * (width + 1), Math.floor(local / columns) * (height + 1), width, height, drawX, placement.y, width, height);
    }
    context.restore();
    if (entity !== primary || !frame) return [];
    return buildOverlayGeometry(frame, { left: drawX, top: placement.y, width, height, mirror: placement.mirror })
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
    drawSceneBackground(context, input, input.tick);
    const primary = primaryPreviewEntity(input.tick.entities);
    const axisEntity = primary ?? input.tick.entities[0];
    if (axisEntity !== undefined) drawAxes(context, input.canvas, input.tick, axisEntity);
    let geometry                             = [];
    const entities = sortPreviewEntities(input.tick.entities);
    const resourcesByOid = new Map((input.project.previewObjects ?? []).map((resource) => [resource.oid, resource]));
    for (const entity of entities) {
        const resource = resourcesByOid.get(entity.oid);
        drawShadow(context, input, entity, primary, resource);
        const candidate = drawEntity(context, input, entity, primary, resource);
        if (candidate.length > 0) geometry = candidate;
    }
    drawGeometry(context, geometry);
    if (input.draftGeometry !== undefined) drawDraftGeometry(context, input.draftGeometry);
    return geometry;
}
