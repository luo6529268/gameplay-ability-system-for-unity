// dat-skill-flow-build:20260807105943896-4ff3ebaa634341a5826098342c043371
import { dataDiagnostic,                                    } from "./data-diagnostic.js";

                           
           
                  
                  
               
           
                  
             
              
            
               

                            
                        
                   
 

                                                                                     
                                                           

                              
                
                      
                        
                     
                              
                          
                                                 
                             
                             
                        
 

                              
                       
                  
                   
                    
                          
 

                              
                    
                       
                  
                   
                          
                    
                          
                          
 

                                    
                    
                    
                 
                           
                          
                   
 

                                                                                                        

                                 
                   
                                   
                        
                             
                                      
                          
                                  
                   
 

                    
                      
                   
 

const blockTypes = new Set              (["itr", "bdy", "opoint", "wpoint", "bpoint", "cpoint"]);

function isAsciiIdentifierByte(value        )          {
    return (value >= 0x41 && value <= 0x5a)
        || (value >= 0x61 && value <= 0x7a)
        || (value >= 0x30 && value <= 0x39)
        || value === 0x5f;
}

function isAsciiLetterOrUnderscore(value        )          {
    return (value >= 0x41 && value <= 0x5a)
        || (value >= 0x61 && value <= 0x7a)
        || value === 0x5f;
}

function isHorizontalWhitespace(value        )          {
    return value === 0x20 || value === 0x09;
}

function isOpaqueByte(value        )          {
    return value === 0;
}

function ascii(source            , start        , end        )         {
    let result = "";
    for (let index = start; index < end; index += 1) {
        result += String.fromCharCode(source[index] ?? 0);
    }
    return result;
}

function detectEncoding(source            )                       {
    if (source.length >= 4) {
        if (source[0] === 0xff && source[1] === 0xfe && source[2] === 0 && source[3] === 0) return "utf32le";
        if (source[0] === 0 && source[1] === 0 && source[2] === 0xfe && source[3] === 0xff) return "utf32be";
    }
    if (source[0] === 0xff && source[1] === 0xfe) return "utf16le";
    if (source[0] === 0xfe && source[1] === 0xff) return "utf16be";
    if (source[0] === 0xef && source[1] === 0xbb && source[2] === 0xbf) return "utf8-bom";
    return "bytes";
}

export function lexBytes(input            )              {
    const source = Buffer.from(input);
    const tokens              = [];
    let offset = 0;
    if (source[0] === 0xef && source[1] === 0xbb && source[2] === 0xbf) {
        tokens.push({ kind: "bom", span: { start: 0, end: 3 } });
        offset = 3;
    } else if ((source[0] === 0xff && source[1] === 0xfe) || (source[0] === 0xfe && source[1] === 0xff)) {
        const end = source.length >= 4 && source[2] === 0 && source[3] === 0 ? 4 : 2;
        tokens.push({ kind: "bom", span: { start: 0, end } });
        offset = end;
    }

    while (offset < source.length) {
        const start = offset;
        const value = source[offset] ?? 0;
        if (value === 0x0d || value === 0x0a) {
            offset += value === 0x0d && source[offset + 1] === 0x0a ? 2 : 1;
            tokens.push({ kind: "line-break", span: { start, end: offset } });
        } else if (isHorizontalWhitespace(value)) {
            while (offset < source.length && isHorizontalWhitespace(source[offset] ?? 0)) offset += 1;
            tokens.push({ kind: "whitespace", span: { start, end: offset } });
        } else if (value === 0x23) {
            while (offset < source.length && source[offset] !== 0x0d && source[offset] !== 0x0a) offset += 1;
            tokens.push({ kind: "comment", span: { start, end: offset } });
        } else if (value === 0x3c) {
            offset += 1;
            while (offset < source.length && source[offset] !== 0x3e && source[offset] !== 0x0d && source[offset] !== 0x0a) offset += 1;
            if (source[offset] === 0x3e) offset += 1;
            tokens.push({ kind: "tag", span: { start, end: offset } });
        } else if (isAsciiLetterOrUnderscore(value)) {
            offset += 1;
            while (offset < source.length && isAsciiIdentifierByte(source[offset] ?? 0)) offset += 1;
            tokens.push({ kind: "identifier", span: { start, end: offset } });
        } else if (value === 0x3a) {
            offset += 1;
            tokens.push({ kind: "colon", span: { start, end: offset } });
        } else if ((value >= 0x30 && value <= 0x39)
            || ((value === 0x2b || value === 0x2d) && (source[offset + 1] ?? 0) >= 0x30 && (source[offset + 1] ?? 0) <= 0x39)) {
            offset += 1;
            while (offset < source.length && (source[offset] ?? 0) >= 0x30 && (source[offset] ?? 0) <= 0x39) offset += 1;
            tokens.push({ kind: "number", span: { start, end: offset } });
        } else if (isOpaqueByte(value)) {
            offset += 1;
            while (offset < source.length && isOpaqueByte(source[offset] ?? 0)) offset += 1;
            tokens.push({ kind: "opaque", span: { start, end: offset } });
        } else {
            offset += 1;
            while (offset < source.length) {
                const next = source[offset] ?? 0;
                if (next === 0x0d || next === 0x0a || next === 0x23 || next === 0x3c || next === 0x3a
                    || isHorizontalWhitespace(next) || isAsciiLetterOrUnderscore(next) || isOpaqueByte(next)) break;
                offset += 1;
            }
            tokens.push({ kind: "text", span: { start, end: offset } });
        }
    }
    return tokens;
}

function splitLines(source            )             {
    const lines             = [];
    let start = 0;
    let offset = 0;
    while (offset < source.length) {
        if (source[offset] === 0x0d || source[offset] === 0x0a) {
            const contentEnd = offset;
            offset += source[offset] === 0x0d && source[offset + 1] === 0x0a ? 2 : 1;
            lines.push({ content: { start, end: contentEnd }, full: { start, end: offset } });
            start = offset;
        } else {
            offset += 1;
        }
    }
    if (start < source.length || source.length === 0) {
        lines.push({ content: { start, end: source.length }, full: { start, end: source.length } });
    }
    return lines;
}

function trimHorizontal(source            , span          )           {
    let start = span.start;
    let end = span.end;
    while (start < end && isHorizontalWhitespace(source[start] ?? 0)) start += 1;
    while (end > start && isHorizontalWhitespace(source[end - 1] ?? 0)) end -= 1;
    return { start, end };
}

function lineWithoutComment(source            , line          )           {
    for (let offset = line.start; offset < line.end; offset += 1) {
        if (source[offset] === 0x23) return { start: line.start, end: offset };
    }
    return { ...line };
}

                      
                
                     
                   
                     
 

function findFieldMatches(source            , span          )               {
    const matches               = [];
    let offset = span.start;
    while (offset < span.end) {
        const value = source[offset] ?? 0;
        if (!isAsciiLetterOrUnderscore(value)) {
            offset += 1;
            continue;
        }
        const keyStart = offset;
        offset += 1;
        while (offset < span.end && isAsciiIdentifierByte(source[offset] ?? 0)) offset += 1;
        const keyEnd = offset;
        while (offset < span.end && isHorizontalWhitespace(source[offset] ?? 0)) offset += 1;
        if (source[offset] === 0x3a) {
            matches.push({ key: ascii(source, keyStart, keyEnd), keyStart, keyEnd, colonEnd: offset + 1 });
            offset += 1;
        }
    }
    return matches;
}

function scalarKindAndNumber(raw        )                                                       {
    const rawText = raw.toString("latin1");
    if (/^[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?$/.test(rawText)) {
        const number = Number(rawText);
        if (Number.isFinite(number)) return { scalarKind: "number", numericValue: number };
    }
    for (const value of raw) {
        if (isOpaqueByte(value)) return { scalarKind: "opaque" };
    }
    return { scalarKind: "string" };
}

                                                            

const topAuthorityStringKeys = new Set([
    "name", "head", "small", "weapon_hit_sound", "weapon_drop_sound", "weapon_broken_sound",
]);

function isAuthorityStringField(key        , scope                 )          {
    return (scope === "top" && topAuthorityStringKeys.has(key))
        || (scope === "frame" && key === "sound")
        || (scope === "sprite" && key === "file");
}

const INT32_MIN = -2_147_483_648;
const INT32_MAX = 2_147_483_647;

export function isSignedInt32(value         )                  {
    return typeof value === "number"
        && Number.isSafeInteger(value)
        && value >= INT32_MIN
        && value <= INT32_MAX;
}

function parseIntegerPair(raw        )                                        {
    const match = /^[+-]?\d+[ \t]+[+-]?\d+$/.exec(raw.toString("latin1"));
    if (match === null) return undefined;
    const values = raw.toString("latin1").split(/[ \t]+/).map((value) => Number(value));
    if (values.length !== 2 || values.some((value) => !isSignedInt32(value))) return undefined;
    return [values[0] , values[1] ]         ;
}

function scalarKindForField(
    raw        ,
    key        ,
    scope                 ,
    blockType               ,
)                                                                                                     {
    if (isAuthorityStringField(key, scope)) {
        for (const value of raw) {
            if (isOpaqueByte(value)) return { scalarKind: "opaque" };
        }
        return { scalarKind: "string" };
    }
    if (scope === "block" && blockType === "itr" && (key === "catchingact" || key === "caughtact")) {
        const integerPairValue = parseIntegerPair(raw);
        if (integerPairValue !== undefined) return { scalarKind: "number", integerPairValue };
    }
    return scalarKindAndNumber(raw);
}

function parseFieldsOnLine(
    source        ,
    contentInput          ,
    ignoredKeys                     ,
    context                                                                                                     ,
)                {
    const content = lineWithoutComment(source, contentInput);
    const matches = findFieldMatches(source, content);
    const fields                = [];
    for (let index = 0; index < matches.length; index += 1) {
        const match = matches[index] ;
        if (ignoredKeys.has(match.key) || match.key.endsWith("_end")) continue;
        const nextStart = matches[index + 1]?.keyStart ?? content.end;
        const valueSpan = trimHorizontal(source, { start: match.colonEnd, end: nextStart });
        const rawValue = Buffer.from(source.subarray(valueSpan.start, valueSpan.end));
        const parsed = scalarKindForField(rawValue, match.key, context.scope, context.blockType);
        const { scope: _scope, ...fieldContext } = context;
        fields.push({
            key: match.key,
            keySpan: { start: match.keyStart, end: match.keyEnd },
            valueSpan,
            rawValue,
            ...parsed,
            ...fieldContext,
        });
    }
    return fields;
}

function parseFrameHeader(
    source            ,
    span          ,
)                                                                        {
    const text = ascii(source, span.start, span.end);
    const match = /^\s*<frame>\s*([+-]?\d+)/.exec(text);
    if (!match) return undefined;
    const rawId = match[1] ;
    const frameId = Number.parseInt(rawId, 10);
    if (!Number.isSafeInteger(frameId)) return undefined;
    const relativeStart = match[0].lastIndexOf(rawId);
    return {
        frameId,
        label: text.slice(match[0].length).trim(),
        frameIdSpan: {
            start: span.start + relativeStart,
            end: span.start + relativeStart + rawId.length,
        },
    };
}

function parseSpriteRange(source        , line          )                                {
    const content = lineWithoutComment(source, line.content);
    const text = ascii(source, content.start, content.end);
    const match = /^\s*file\(\s*([+-]?\d+)\s*(?:-\s*([+-]?\d+)\s*)?\)\s*:\s*/.exec(text);
    if (!match) return undefined;
    const frameLo = Number.parseInt(match[1] , 10);
    const frameHi = match[2] === undefined ? frameLo : Number.parseInt(match[2], 10);
    const prefixLength = match[0].length;
    let fileStart = content.start + prefixLength;
    while (fileStart < content.end && isHorizontalWhitespace(source[fileStart] ?? 0)) fileStart += 1;
    let fileEnd = fileStart;
    while (fileEnd < content.end && !isHorizontalWhitespace(source[fileEnd] ?? 0)) fileEnd += 1;
    const rawValue = Buffer.from(source.subarray(fileStart, fileEnd));
    const fileField              = {
        key: "file",
        keySpan: { start: content.start + (match[0].indexOf("file")), end: content.start + match[0].indexOf("file") + 4 },
        valueSpan: { start: fileStart, end: fileEnd },
        rawValue,
        ...scalarKindForField(rawValue, "file", "sprite"),
    };
    const fields = parseFieldsOnLine(source, { start: fileEnd, end: content.end }, new Set(), { scope: "sprite" });
    return {
        frameLo,
        frameHi,
        file: ascii(source, fileStart, fileEnd),
        fileField,
        fields,
        span: { ...line.full },
    };
}

export function parseDatCst(input            )                 {
    const source = Buffer.from(input);
    const encoding = detectEncoding(source);
    const diagnostics                   = [];
    if (encoding === "utf16le" || encoding === "utf16be" || encoding === "utf32le" || encoding === "utf32be") {
        diagnostics.push(dataDiagnostic(
            "unsupported-encoding",
            `DAT editing does not support ${encoding}; original bytes remain available for no-op emission.`,
            { span: { start: 0, end: Math.min(source.length, 4) } },
        ));
    }

    const topFields                = [];
    const spriteRanges                      = [];
    const frames                = [];
    let currentFrame                         ;
    let currentBlock                         ;
    let currentBlockCounts = new Map                      ();
    let frameOccurrence = 0;

    for (const line of splitLines(source)) {
        const content = lineWithoutComment(source, line.content);
        const trimmed = trimHorizontal(source, content);
        const lineText = ascii(source, trimmed.start, trimmed.end);
        const frameHeader = parseFrameHeader(source, trimmed);
        if (frameHeader !== undefined) {
            if (currentBlock) {
                diagnostics.push(dataDiagnostic("malformed-block", `Unclosed ${currentBlock.type} block.`, { span: currentBlock.span }));
            }
            if (currentFrame) {
                diagnostics.push(dataDiagnostic("malformed-frame", "Unclosed frame before the next <frame> marker.", { span: currentFrame.span }));
            }
            currentFrame = {
                frameId: frameHeader.frameId,
                occurrence: frameOccurrence,
                label: frameHeader.label,
                span: { start: line.full.start, end: line.full.end },
                frameIdSpan: frameHeader.frameIdSpan,
                closed: false,
                fields: [],
                blocks: [],
            };
            frameOccurrence += 1;
            frames.push(currentFrame);
            currentBlock = undefined;
            currentBlockCounts = new Map();
            continue;
        }

        if (/^<frame_end>/.test(lineText)) {
            if (currentBlock) {
                diagnostics.push(dataDiagnostic("malformed-block", `Unclosed ${currentBlock.type} block at frame end.`, { span: currentBlock.span }));
                currentBlock.span.end = line.full.start;
                currentBlock = undefined;
            }
            if (currentFrame) {
                currentFrame.span.end = line.full.end;
                currentFrame.closed = true;
                currentFrame = undefined;
                currentBlockCounts = new Map();
            }
            continue;
        }

        if (currentFrame) {
            const blockStart = /^\s*(itr|bdy|opoint|wpoint|bpoint|cpoint)\s*:/.exec(lineText);
            if (blockStart && blockTypes.has(blockStart[1]                )) {
                if (currentBlock) {
                    diagnostics.push(dataDiagnostic("malformed-block", `Unclosed ${currentBlock.type} block.`, { span: currentBlock.span }));
                }
                const type = blockStart[1]                ;
                const index = currentBlockCounts.get(type) ?? 0;
                currentBlockCounts.set(type, index + 1);
                currentBlock = {
                    type,
                    index,
                    span: { start: line.full.start, end: line.full.end },
                    closed: false,
                    fields: [],
                };
                currentFrame.blocks.push(currentBlock);
                currentBlock.fields.push(...parseFieldsOnLine(
                    source,
                    content,
                    new Set([type]),
                    { scope: "block", frameOccurrence: currentFrame.occurrence, blockType: type, blockIndex: index },
                ));
                continue;
            }

            const blockEnd = /^\s*(itr|bdy|opoint|wpoint|bpoint|cpoint)_end\s*:/.exec(lineText);
            if (blockEnd) {
                if (currentBlock?.type === blockEnd[1]) {
                    currentBlock.span.end = line.full.end;
                    currentBlock.closed = true;
                    currentBlock = undefined;
                } else {
                    diagnostics.push(dataDiagnostic("malformed-block", `Unexpected ${blockEnd[1]}_end marker.`, { span: { ...line.content } }));
                }
                continue;
            }

            if (currentBlock) {
                currentBlock.span.end = line.full.end;
                currentBlock.fields.push(...parseFieldsOnLine(
                    source,
                    content,
                    new Set(),
                    {
                        scope: "block",
                        frameOccurrence: currentFrame.occurrence,
                        blockType: currentBlock.type,
                        blockIndex: currentBlock.index,
                    },
                ));
            } else {
                currentFrame.span.end = line.full.end;
                currentFrame.fields.push(...parseFieldsOnLine(
                    source,
                    content,
                    new Set(),
                    { scope: "frame", frameOccurrence: currentFrame.occurrence },
                ));
            }
            continue;
        }

        const spriteRange = parseSpriteRange(source, line);
        if (spriteRange) {
            spriteRanges.push(spriteRange);
            continue;
        }
        topFields.push(...parseFieldsOnLine(source, content, new Set(), { scope: "top" }));
    }

    if (currentBlock) {
        diagnostics.push(dataDiagnostic("malformed-block", `Unclosed ${currentBlock.type} block at end of file.`, { span: currentBlock.span }));
    }
    if (currentFrame) {
        diagnostics.push(dataDiagnostic("malformed-frame", "Unclosed frame at end of file.", { span: currentFrame.span }));
    }

    return {
        source,
        encoding,
        tokens: lexBytes(source),
        topFields,
        spriteRanges,
        frames,
        diagnostics,
        emit: () => Buffer.from(source),
    };
}
