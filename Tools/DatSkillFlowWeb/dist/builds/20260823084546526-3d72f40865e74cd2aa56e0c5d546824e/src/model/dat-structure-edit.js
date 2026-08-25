// dat-skill-flow-build:20260823084546526-3d72f40865e74cd2aa56e0c5d546824e
             
                
                 
                   
                
                               

                                        
                           
                                     
 

                                        
                           
                                     
                                     
                                
 

                                                                                

                              
                                                                                                               
                                                                                    
       
                                                                           
                                               
      

function spliceBytes(
    source        ,
    start        ,
    end        ,
    replacement            ,
)         {
    return Buffer.concat([
        source.subarray(0, start),
        Buffer.from(replacement),
        source.subarray(end),
    ]);
}

function hasTrailingLineBreak(source        , end        )          {
    return end > 0 && (source[end - 1] === 0x0a || source[end - 1] === 0x0d);
}

function requireFrame(document                , locator                       )              {
    const frame = document.frames.find((candidate) => candidate.occurrence === locator.frameOccurrence);
    if (frame === undefined) throw new RangeError("The frame structure locator is stale.");
    return frame;
}

function requireBlock(document                , locator                       )   
                       
                       
  {
    const frame = requireFrame(document, {
        kind: "frame",
        frameOccurrence: locator.frameOccurrence,
    });
    const block = frame.blocks.find((candidate) => (
        candidate.type === locator.blockType && candidate.index === locator.blockIndex
    ));
    if (block === undefined) throw new RangeError("The block structure locator is stale.");
    return { frame, block };
}

function requireEditableEncoding(document                )       {
    if (document.encoding === "utf16le"
        || document.encoding === "utf16be"
        || document.encoding === "utf32le"
        || document.encoding === "utf32be") {
        throw new TypeError(`Cannot structurally edit ${document.encoding} DAT bytes.`);
    }
}

export function canDeleteFrame(frame             )          {
    return frame.closed && frame.blocks.every((block) => block.closed);
}

export function canCopyFrame(document                , frame             )          {
    return canDeleteFrame(frame) && hasTrailingLineBreak(document.source, frame.span.end);
}

export function canDeleteBlock(block             )          {
    return block.closed;
}

export function canCopyBlock(document                , block             )          {
    return canDeleteBlock(block) && hasTrailingLineBreak(document.source, block.span.end);
}

export function applyDatStructureEdit(
    document                ,
    edit                  ,
)         {
    requireEditableEncoding(document);
    const source = document.source;

    if (edit.operation === "copy-frame") {
        const frame = requireFrame(document, edit.target);
        if (!Number.isSafeInteger(edit.newFrameId) || edit.newFrameId < 0 || edit.newFrameId >= 600) {
            throw new RangeError("The new frame ID must be an integer from 0 through 599.");
        }
        if (!canCopyFrame(document, frame)) {
            throw new TypeError("The frame does not have a complete, safely insertable byte span.");
        }
        const copy = Buffer.from(source.subarray(frame.span.start, frame.span.end));
        const idStart = frame.frameIdSpan.start - frame.span.start;
        const idEnd = frame.frameIdSpan.end - frame.span.start;
        const rewritten = spliceBytes(copy, idStart, idEnd, Buffer.from(String(edit.newFrameId), "ascii"));
        return spliceBytes(source, frame.span.end, frame.span.end, rewritten);
    }

    if (edit.operation === "delete-frame") {
        const frame = requireFrame(document, edit.target);
        if (!canDeleteFrame(frame)) {
            throw new TypeError("The frame does not have a complete byte span.");
        }
        return spliceBytes(source, frame.span.start, frame.span.end, Buffer.alloc(0));
    }

    const { frame, block } = requireBlock(document, edit.target);
    if (edit.operation === "delete-block") {
        if (!canDeleteBlock(block)) {
            throw new TypeError("The block does not have a complete byte span.");
        }
        return spliceBytes(source, block.span.start, block.span.end, Buffer.alloc(0));
    }
    if (!canCopyBlock(document, block)) {
        throw new TypeError("The block does not have a complete, safely insertable byte span.");
    }
    const template = source.subarray(block.span.start, block.span.end);
    const insertionOffset = edit.operation === "copy-block"
        ? block.span.end
        : frame.blocks.filter((candidate) => candidate.type === block.type).at(-1) .span.end;
    return spliceBytes(source, insertionOffset, insertionOffset, template);
}
