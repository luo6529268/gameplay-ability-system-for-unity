// dat-skill-flow-build:20260801070903344-c9b04518e2ff41c996f0c4246acfc769
import {
                     
                        
                     
                           
} from "../syntax/byte-cst.js";
import { dataDiagnostic,                     } from "../syntax/data-diagnostic.js";

                                
                 
              
              
              
              
                
                
                 
                    
                   
                  
                  
                   
                      
                        
                         
                      
                       
                    
                       
                      
                    
                    
                   
                    
                        
                          
 

                                
              
              
              
              
                          
 

                                   
                 
              
              
                   
                
                
                
                   
                          
 

                                   
                 
              
              
                      
                  
                      
                
                
                
                          
 

                                   
              
              
                          
 

                                   
                 
              
              
                   
                  
                    
                    
                    
                    
                    
                    
                    
                    
                        
                     
                     
                       
                         
                        
                          
 

                                     
                    
                       
                
                  
                 
                 
                
                
                
                    
                    
                   
                   
                   
                   
                   
                   
                   
                  
                  
                  
               
                    
                  
                          
                          
                                
                                
                                
                                
                                                                                                                                                           
 

                                        
                    
                    
                 
              
              
                
                
 

                                   
                 
                 
                  
                             
                              
                                
                      
                             
                               
                          
                           
                               
                          
                           
                                
                                 
                                
                                 
                        
                          
                           
                        
                          
                           
                          
                            
                                   
 

const topDefaults                   = {
    name: "",
    head: "",
    small: "",
    weapon_hit_sound: "",
    weapon_drop_sound: "",
    weapon_broken_sound: "",
    weapon_hp: 0,
    weapon_drop_hurt: 0,
    walking_frame_rate: 3.0,
    walking_speed: 4.0,
    walking_speedz: 2.0,
    running_frame_rate: 3.0,
    running_speed: 8.0,
    running_speedz: 3.3,
    heavy_walking_speed: 3.0,
    heavy_walking_speedz: 1.5,
    heavy_running_speed: 5.0,
    heavy_running_speedz: 0.8,
    jump_height: -16.3,
    jump_distance: 8.0,
    jump_distancez: 3.0,
    dash_height: -13.0,
    dash_distance: 15.0,
    dash_distancez: 3.75,
    rowing_height: -2.0,
    rowing_distance: 20.0,
};

const frameDefaults = Object.freeze({
    pic: 0,
    state: 0,
    wait: 1,
    next: 0,
    dvx: 0,
    dvy: 0,
    dvz: 0,
    centerx: 0,
    centery: 0,
    hit_Fa: 0,
    hit_Fj: 0,
    hit_Ua: 0,
    hit_Uj: 0,
    hit_Da: 0,
    hit_Dj: 0,
    hit_ja: 0,
    hit_a: 0,
    hit_d: 0,
    hit_j: 0,
    mp: 0,
    vaction: 0,
});

const itrDefaults                = {
    kind: 0, x: 0, y: 0, w: 0, h: 0,
    dvx: 0, dvy: 0, fall: 0, bdefend: 0, injury: 0,
    arest: 0, vrest: 0, effect: 0, attacking: 0,
    catchingact: 0, catchingact2: 0, caughtact: 0, caughtact2: 0,
    respond: 0, pickingact: 0, pickedact: 0,
    throwvx: 0, throwvy: 0, zwidth: 15, throwvz: 0, throwinjury: 0,
};

const bdyDefaults                = { x: 0, y: 0, w: 0, h: 0 };
const opointDefaults                   = { kind: 0, x: 0, y: 0, action: 0, dvx: 0, dvy: 0, oid: 0, facing: 0 };
const wpointDefaults                   = { kind: 0, x: 0, y: 0, attacking: 0, cover: 0, weaponact: 0, dvx: 0, dvy: 0, dvz: 0 };
const bpointDefaults                   = { x: 0, y: 0 };
const cpointDefaults                   = {
    kind: 0, x: 0, y: 0, injury: 0, cover: 0,
    vaction: 0, aaction: 0, jaction: 0, daction: 0, taction: 0,
    throwvx: 0, throwvy: 0, throwvz: 0, throwinjury: 0,
    hurtable: 0, decrease: 0, dircontrol: 0,
    fronthurtact: 0, backhurtact: 0,
};

function numericValues(rawValue            )           {
    const matches = Buffer.from(rawValue).toString("latin1").match(/[+-]?\d+/g) ?? [];
    return matches.map((match) => Number.parseInt(match, 10)).filter(Number.isSafeInteger);
}

function assignNumericFields(target                        , fields                        )       {
    for (const field of fields) {
        const values = numericValues(field.rawValue);
        if (values.length === 0) continue;
        target[field.key] = values[0] ;
    }
}

function projectItr(block             )                {
    const result = { ...itrDefaults };
    for (const field of block.fields) {
        const values = numericValues(field.rawValue);
        if (values.length === 0) continue;
        result[field.key] = values[0] ;
        if (field.key === "catchingact" && values.length > 1) result.catchingact2 = values[1] ;
        if (field.key === "caughtact" && values.length > 1) result.caughtact2 = values[1] ;
    }
    return result;
}

function projectBdy(block             )                {
    const result = { ...bdyDefaults };
    for (const field of block.fields) {
        const value = numericValues(field.rawValue)[0];
        if (value === undefined) continue;
        if (field.key === "x" || field.key === "y" || field.key === "w" || field.key === "h") {
            result[field.key] = value;
        }
    }
    return result;
}

function projectSimple                                  (defaults   , block             )    {
    const result = { ...defaults };
    assignNumericFields(result, block.fields);
    return result;
}

function projectCpoint(block             )                   {
    const result = { ...cpointDefaults };
    for (const field of block.fields) {
        const value = numericValues(field.rawValue)[0];
        if (value === undefined) continue;
        result[field.key] = value;
        if (field.key === "fronthurtact") result.injury = value;
        if (field.key === "backhurtact") result.cover = value;
    }
    return result;
}

function projectFrame(frame                                  )                     {
    const result                     = {
        frameId: frame.frameId,
        occurrence: frame.occurrence,
        ...frameDefaults,
        sound: "",
        itrs: [],
        bdys: [],
        opoints: [],
        wpoints: [],
        bpoints: [],
        cpoints: [],
    };
    for (const field of frame.fields) {
        if (field.key === "sound") {
            result.sound = field.rawValue.toString("latin1");
            continue;
        }
        const value = numericValues(field.rawValue)[0];
        if (value !== undefined && field.key in frameDefaults) result[field.key] = value;
    }
    for (const block of frame.blocks) {
        switch (block.type) {
            case "itr": result.itrs.push(projectItr(block)); break;
            case "bdy": result.bdys.push(projectBdy(block)); break;
            case "opoint": result.opoints.push(projectSimple(opointDefaults, block)); break;
            case "wpoint": result.wpoints.push(projectSimple(wpointDefaults, block)); break;
            case "bpoint": result.bpoints.push(projectSimple(bpointDefaults, block)); break;
            case "cpoint": result.cpoints.push(projectCpoint(block)); break;
        }
    }
    return result;
}

function spriteRangeValue(range                   , key        )         {
    let result = 0;
    for (const field of range.fields) {
        if (field.key !== key) continue;
        const value = numericValues(field.rawValue)[0];
        if (value !== undefined) result = value;
    }
    return result;
}

export class DatProjection {
                    top                            ;
                    spriteRanges                                  ;
                    frames                               ;
                    diagnostics                           ;
                     frameIndex = new Map                            ();

           constructor(cst                ) {
        const top                   = { ...topDefaults };
        for (const field of cst.topFields) {
            top[field.key] = field.numericValue ?? field.rawValue.toString("latin1");
        }
        this.top = Object.freeze(top);
        this.spriteRanges = cst.spriteRanges.map((range) => ({
            frameLo: range.frameLo,
            frameHi: range.frameHi,
            file: range.file,
            w: spriteRangeValue(range, "w"),
            h: spriteRangeValue(range, "h"),
            row: spriteRangeValue(range, "row"),
            col: spriteRangeValue(range, "col"),
        }));
        const authorityFrames = cst.frames.filter((frame) => frame.frameId >= 0 && frame.frameId < 600);
        this.frames = authorityFrames.map(projectFrame);
        this.diagnostics = cst.frames
            .filter((frame) => frame.frameId < 0 || frame.frameId >= 600)
            .map((frame) => dataDiagnostic(
                "malformed-frame",
                `Frame id ${frame.frameId} is outside the C++ CharData authority range [0, 600).`,
                { span: { ...frame.span } },
            ));
        for (const frame of this.frames) this.frameIndex.set(frame.frameId, frame);
    }

           getFrame(frameId        )                                 {
        return this.frameIndex.get(frameId);
    }
}

export function projectDatCst(cst                )                {
    return new DatProjection(cst);
}
