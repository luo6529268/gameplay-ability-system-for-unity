// dat-skill-flow-build:20260810005429999-705d19cb36ba4100a6c7cdb8bd0d45b8
import {
    parseNativeInt32Token,
                     
                        
                     
                           
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
    return matches.flatMap((match) => {
        const value = parseNativeInt32Token(match);
        return value === undefined ? [] : [value];
    });
}

function projectTopField(target                  , field             )       {
    const rawText = field.rawValue.toString("latin1");
    const nativeInteger = parseNativeInt32Token(field.rawValue);
    switch (field.key) {
        case "name": target.name = rawText; break;
        case "head": target.head = rawText; break;
        case "small": target.small = rawText; break;
        case "weapon_hit_sound": target.weapon_hit_sound = rawText; break;
        case "weapon_drop_sound": target.weapon_drop_sound = rawText; break;
        case "weapon_broken_sound": target.weapon_broken_sound = rawText; break;
        case "weapon_hp": if (nativeInteger !== undefined) target.weapon_hp = nativeInteger; break;
        case "weapon_drop_hurt": if (nativeInteger !== undefined) target.weapon_drop_hurt = nativeInteger; break;
        case "walking_frame_rate": if (nativeInteger !== undefined) target.walking_frame_rate = nativeInteger; break;
        case "walking_speed": if (field.numericValue !== undefined) target.walking_speed = field.numericValue; break;
        case "walking_speedz": if (field.numericValue !== undefined) target.walking_speedz = field.numericValue; break;
        case "running_frame_rate": if (nativeInteger !== undefined) target.running_frame_rate = nativeInteger; break;
        case "running_speed": if (field.numericValue !== undefined) target.running_speed = field.numericValue; break;
        case "running_speedz": if (field.numericValue !== undefined) target.running_speedz = field.numericValue; break;
        case "heavy_walking_speed": if (field.numericValue !== undefined) target.heavy_walking_speed = field.numericValue; break;
        case "heavy_walking_speedz": if (field.numericValue !== undefined) target.heavy_walking_speedz = field.numericValue; break;
        case "heavy_running_speed": if (field.numericValue !== undefined) target.heavy_running_speed = field.numericValue; break;
        case "heavy_running_speedz": if (field.numericValue !== undefined) target.heavy_running_speedz = field.numericValue; break;
        case "jump_height": if (field.numericValue !== undefined) target.jump_height = field.numericValue; break;
        case "jump_distance": if (field.numericValue !== undefined) target.jump_distance = field.numericValue; break;
        case "jump_distancez": if (field.numericValue !== undefined) target.jump_distancez = field.numericValue; break;
        case "dash_height": if (field.numericValue !== undefined) target.dash_height = field.numericValue; break;
        case "dash_distance": if (field.numericValue !== undefined) target.dash_distance = field.numericValue; break;
        case "dash_distancez": if (field.numericValue !== undefined) target.dash_distancez = field.numericValue; break;
        case "rowing_height": if (field.numericValue !== undefined) target.rowing_height = field.numericValue; break;
        case "rowing_distance": if (field.numericValue !== undefined) target.rowing_distance = field.numericValue; break;
    }
}

function projectItr(block             )                {
    const result = { ...itrDefaults };
    for (const field of block.fields) {
        const values = numericValues(field.rawValue);
        if (values.length === 0) continue;
        const value = values[0] ;
        switch (field.key) {
            case "kind": result.kind = value; break;
            case "x": result.x = value; break;
            case "y": result.y = value; break;
            case "w": result.w = value; break;
            case "h": result.h = value; break;
            case "dvx": result.dvx = value; break;
            case "dvy": result.dvy = value; break;
            case "fall": result.fall = value; break;
            case "bdefend": result.bdefend = value; break;
            case "injury": result.injury = value; break;
            case "arest": result.arest = value; break;
            case "vrest": result.vrest = value; break;
            case "effect": result.effect = value; break;
            case "attacking": result.attacking = value; break;
            case "catchingact":
                result.catchingact = value;
                result.catchingact2 = values[1] ?? 0;
                break;
            case "caughtact":
                result.caughtact = value;
                result.caughtact2 = values[1] ?? 0;
                break;
            case "respond": result.respond = value; break;
            case "pickingact": result.pickingact = value; break;
            case "pickedact": result.pickedact = value; break;
            case "throwvx": result.throwvx = value; break;
            case "throwvy": result.throwvy = value; break;
            case "zwidth": result.zwidth = value; break;
            case "throwvz": result.throwvz = value; break;
            case "throwinjury": result.throwinjury = value; break;
        }
    }
    return result;
}

function projectBdy(block             )                {
    const result = { ...bdyDefaults };
    for (const field of block.fields) {
        const value = numericValues(field.rawValue)[0];
        if (value === undefined) continue;
        switch (field.key) {
            case "x": result.x = value; break;
            case "y": result.y = value; break;
            case "w": result.w = value; break;
            case "h": result.h = value; break;
        }
    }
    return result;
}

function projectOpoint(block             )                   {
    const result = { ...opointDefaults };
    for (const field of block.fields) {
        const value = numericValues(field.rawValue)[0];
        if (value === undefined) continue;
        switch (field.key) {
            case "kind": result.kind = value; break;
            case "x": result.x = value; break;
            case "y": result.y = value; break;
            case "action": result.action = value; break;
            case "dvx": result.dvx = value; break;
            case "dvy": result.dvy = value; break;
            case "oid": result.oid = value; break;
            case "facing": result.facing = value; break;
        }
    }
    return result;
}

function projectWpoint(block             )                   {
    const result = { ...wpointDefaults };
    for (const field of block.fields) {
        const value = numericValues(field.rawValue)[0];
        if (value === undefined) continue;
        switch (field.key) {
            case "kind": result.kind = value; break;
            case "x": result.x = value; break;
            case "y": result.y = value; break;
            case "attacking": result.attacking = value; break;
            case "cover": result.cover = value; break;
            case "weaponact": result.weaponact = value; break;
            case "dvx": result.dvx = value; break;
            case "dvy": result.dvy = value; break;
            case "dvz": result.dvz = value; break;
        }
    }
    return result;
}

function projectBpoint(block             )                   {
    const result = { ...bpointDefaults };
    for (const field of block.fields) {
        const value = numericValues(field.rawValue)[0];
        if (value === undefined) continue;
        switch (field.key) {
            case "x": result.x = value; break;
            case "y": result.y = value; break;
        }
    }
    return result;
}

function projectCpoint(block             )                   {
    const result = { ...cpointDefaults };
    for (const field of block.fields) {
        const value = numericValues(field.rawValue)[0];
        if (value === undefined) continue;
        switch (field.key) {
            case "kind": result.kind = value; break;
            case "x": result.x = value; break;
            case "y": result.y = value; break;
            case "injury": result.injury = value; break;
            case "cover": result.cover = value; break;
            case "vaction": result.vaction = value; break;
            case "aaction": result.aaction = value; break;
            case "jaction": result.jaction = value; break;
            case "daction": result.daction = value; break;
            case "taction": result.taction = value; break;
            case "throwvx": result.throwvx = value; break;
            case "throwvy": result.throwvy = value; break;
            case "throwvz": result.throwvz = value; break;
            case "throwinjury": result.throwinjury = value; break;
            case "hurtable": result.hurtable = value; break;
            case "decrease": result.decrease = value; break;
            case "dircontrol": result.dircontrol = value; break;
            case "fronthurtact": result.fronthurtact = value; result.injury = value; break;
            case "backhurtact": result.backhurtact = value; result.cover = value; break;
        }
    }
    return result;
}

function projectFrameField(result                    , field             )       {
    if (field.key === "sound") {
        result.sound = field.rawValue.toString("latin1");
        return;
    }
    const value = numericValues(field.rawValue)[0];
    if (value === undefined) return;
    switch (field.key) {
        case "pic": result.pic = value; break;
        case "state": result.state = value; break;
        case "wait": result.wait = value; break;
        case "next": result.next = value; break;
        case "dvx": result.dvx = value; break;
        case "dvy": result.dvy = value; break;
        case "dvz": result.dvz = value; break;
        case "centerx": result.centerx = value; break;
        case "centery": result.centery = value; break;
        case "hit_Fa": result.hit_Fa = value; break;
        case "hit_Fj": result.hit_Fj = value; break;
        case "hit_Ua": result.hit_Ua = value; break;
        case "hit_Uj": result.hit_Uj = value; break;
        case "hit_Da": result.hit_Da = value; break;
        case "hit_Dj": result.hit_Dj = value; break;
        case "hit_ja": result.hit_ja = value; break;
        case "hit_a": result.hit_a = value; break;
        case "hit_d": result.hit_d = value; break;
        case "hit_j": result.hit_j = value; break;
        case "mp": result.mp = value; break;
        case "vaction": result.vaction = value; break;
    }
}

function projectFrame(frame                                  )                     {
    const result                     = {
        frameId: frame.frameId,
        occurrence: frame.occurrence,
        label: frame.label,
        ...frameDefaults,
        sound: "",
        itrs: [],
        bdys: [],
        opoints: [],
        wpoints: [],
        bpoints: [],
        cpoints: [],
    };
    for (const field of frame.fields) projectFrameField(result, field);
    for (const block of frame.blocks) {
        switch (block.type) {
            case "itr": result.itrs.push(projectItr(block)); break;
            case "bdy": result.bdys.push(projectBdy(block)); break;
            case "opoint": result.opoints.push(projectOpoint(block)); break;
            case "wpoint": result.wpoints.push(projectWpoint(block)); break;
            case "bpoint": result.bpoints.push(projectBpoint(block)); break;
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
        for (const field of cst.topFields) projectTopField(top, field);
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
