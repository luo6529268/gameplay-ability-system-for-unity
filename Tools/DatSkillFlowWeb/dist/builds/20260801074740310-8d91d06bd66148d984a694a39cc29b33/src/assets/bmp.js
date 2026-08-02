// dat-skill-flow-build:20260801074740310-8d91d06bd66148d984a694a39cc29b33
import { dataDiagnostic,                     } from "../syntax/data-diagnostic.js";

                                        
                  
           
             
            
 

                                    
                
                  
                   
                         
                     
                     
                        
                          
                        
                      
                           
                                    
                                  
 

                                      
                    
                    
                 
              
              
                
                
 

                                   
              
              
                  
                   
 

                                   
                                                                               
       
                     
                     
                           
                             
                       
                    
                                 
                                        
      

export const BLACK_COLORKEY                        = Object.freeze({
    enabled: true,
    red: 0,
    green: 0,
    blue: 0,
});

function invalidBmp(message        )                    {
    return {
        ok: false,
        width: 0,
        height: 0,
        storedHeight: 0,
        topDown: false,
        bitDepth: 0,
        pixelOffset: 0,
        dibHeaderSize: 0,
        compression: 0,
        rowStride: 0,
        paletteEntries: 0,
        colorKey: BLACK_COLORKEY,
        diagnostics: [dataDiagnostic("invalid-bmp", message)],
    };
}

export function parseBmpMetadata(input            )                    {
    const bytes = Buffer.from(input);
    if (bytes.length < 54 || bytes[0] !== 0x42 || bytes[1] !== 0x4d) {
        return invalidBmp("BMP must contain a BITMAPFILEHEADER and BITMAPINFOHEADER with BM signature.");
    }
    const pixelOffset = bytes.readUInt32LE(10);
    const dibHeaderSize = bytes.readUInt32LE(14);
    if (dibHeaderSize < 40 || 14 + dibHeaderSize > bytes.length) {
        return invalidBmp(`Unsupported or truncated DIB header size: ${dibHeaderSize}.`);
    }
    const width = bytes.readInt32LE(18);
    const storedHeight = bytes.readInt32LE(22);
    const planes = bytes.readUInt16LE(26);
    const bitDepth = bytes.readUInt16LE(28);
    const compression = bytes.readUInt32LE(30);
    if (width <= 0 || storedHeight === 0 || planes !== 1) {
        return invalidBmp("BMP dimensions and plane count are invalid.");
    }
    if (bitDepth !== 8 && bitDepth !== 24 && bitDepth !== 32) {
        return invalidBmp(`Unsupported BMP bit depth ${bitDepth}; Gate1A supports 8, 24, and 32.`);
    }
    if (compression !== 0 && !(bitDepth === 32 && compression === 3)) {
        return invalidBmp(`Unsupported BMP compression ${compression} for ${bitDepth}-bit data.`);
    }
    if (pixelOffset < 14 + dibHeaderSize || pixelOffset > bytes.length) {
        return invalidBmp(`BMP pixel offset ${pixelOffset} is outside the file.`);
    }
    const height = Math.abs(storedHeight);
    const rowStride = Math.floor((bitDepth * width + 31) / 32) * 4;
    if (pixelOffset + rowStride * height > bytes.length) {
        return invalidBmp("BMP pixel array is truncated.");
    }
    const declaredPaletteEntries = dibHeaderSize >= 40 ? bytes.readUInt32LE(46) : 0;
    const paletteEntries = bitDepth === 8 ? (declaredPaletteEntries || 256) : 0;
    return {
        ok: true,
        width,
        height,
        storedHeight,
        topDown: storedHeight < 0,
        bitDepth,
        pixelOffset,
        dibHeaderSize,
        compression,
        rowStride,
        paletteEntries,
        colorKey: BLACK_COLORKEY,
        diagnostics: [],
    };
}

export function findSpriteRange(
    ranges                                ,
    picture        ,
)                                  {
    return ranges.find((range) => picture >= range.frameLo && picture <= range.frameHi);
}

export function resolveSpriteFrame(
    picture        ,
    ranges                                ,
)                        {
    if (picture === 999) return { render: false, reason: "pic-999" };
    const rangeIndex = ranges.findIndex((range) => picture >= range.frameLo && picture <= range.frameHi);
    if (rangeIndex < 0) return { render: false, reason: "range-not-found" };
    const range = ranges[rangeIndex] ;
    if (range.row <= 0 || range.col <= 0 || range.w <= 0 || range.h <= 0) {
        return { render: false, reason: "invalid-grid" };
    }
    const localPicture = picture - range.frameLo;
    const columns = range.row;
    const column = localPicture % columns;
    const row = Math.floor(localPicture / columns);
    return {
        render: true,
        file: range.file,
        rangeIndex,
        localPicture,
        column,
        row,
        source: {
            x: column * (range.w + 1),
            y: row * (range.h + 1),
            width: range.w,
            height: range.h,
        },
        colorKey: BLACK_COLORKEY,
    };
}
