// dat-skill-flow-build:20260807103603364-e8993c595deb464b9dca008b5de3adcf
import { OVERLAY_COLORS,                  } from "./overlay-geometry.js";
                                                                     
import {
    SKILL_ENTRY_HIT_KEYS,
                              
                    
} from "./skill-entries.js";

                                           
                                              
                                      
                    
                
                                               
                                                
                       
                   
                     
                             
                            
                        
  
                                      
                                           
                         
                           
                       
                     
                       
 
                                           
                         
                    
                       
                     
                       
                                       
 
                             
                     
                 
                                                              
                                     
                           
 
                             
                                
                                        
                                                          
                   
 
                                                                                

export const allOverlayTypes = Object.freeze(Object.keys(OVERLAY_COLORS)                 );
export const frameFieldLabels                                   = Object.freeze({
    occurrence: "同编号帧序号", pic: "图片编号", state: "状态编号", wait: "持续时间", next: "下一帧",
    dvx: "水平速度", dvy: "垂直速度", dvz: "纵深速度", centerx: "中心点 X", centery: "中心点 Y",
    hit_a: "按攻击跳转", hit_d: "按防御跳转", hit_j: "按跳跃跳转", hit_Fj: "防御+跳跃跳转",
    hit_Fa: "防御+攻击跳转", hit_Da: "下+攻击跳转", hit_Ua: "上+攻击跳转", hit_ja: "跳跃+攻击跳转",
    hit_Dj: "下+跳跃跳转", hit_Uj: "上+跳跃跳转", mp: "消耗量", vaction: "武器动作帧",
    kind: "类型", x: "X 坐标", y: "Y 坐标", w: "宽度", h: "高度", zwidth: "Z 宽度",
    injury: "伤害", fall: "击倒值", bdefend: "防御破坏", arest: "攻击休止", vrest: "受击休止",
    effect: "效果", attacking: "攻击动作", catchingact: "抓取动作", caughtact: "被抓动作",
    action: "生成动作", oid: "对象 OID", facing: "朝向", cover: "覆盖", weaponact: "武器动作",
    aaction: "攻击动作", jaction: "跳跃动作", daction: "防御动作", taction: "投掷动作",
});
export const frameGroups = Object.freeze([
    { title: "帧基础属性", keys: ["pic", "state", "wait", "next", "sound"] },
    { title: "移动参数", keys: ["dvx", "dvy", "dvz", "centerx", "centery"] },
    { title: "跳转字段", keys: [...SKILL_ENTRY_HIT_KEYS] },
    { title: "其他", keys: ["mp", "vaction"] },
]);
export const blockCollections                                                          = Object.freeze({
    itr: "itrs", bdy: "bdys", opoint: "opoints", wpoint: "wpoints", bpoint: "bpoints", cpoint: "cpoints",
});

export const record = (value         )       => value !== null && typeof value === "object" ? value         : {};
export const list = (value         )            => Array.isArray(value) ? value : [];
export const text = (value         )         => typeof value === "string" ? value : "";
export const number = (value         , fallback = 0)         => typeof value === "number" && Number.isFinite(value) ? value : fallback;

export function localizedRequestError(statusCode        , path        )         {
    if (statusCode === 403) return "页面会话已经失效，请刷新页面后重试。";
    if (statusCode === 404 && path === "/api/project/open") return "当前对象尚未接入原生预览，请选择 OID 2 Naruto。";
    if (statusCode === 404 && path.startsWith("/api/assets/")) return "图片资源已经失效，请重新打开项目。";
    if (statusCode === 404) return "项目会话已经失效，请重新打开项目。";
    if (statusCode === 409) return "数据版本已经变化，请重新载入后再修改。";
    if (statusCode === 413) return "请求数据过大，服务器已拒绝处理。";
    if (statusCode === 422 && path === "/api/project/preview") return "原生预览输出无效，无法继续播放。";
    if (statusCode === 422) return "图片资源格式无效，无法预览。";
    if (statusCode === 503) return "项目服务尚未就绪，请稍后重试。";
    return `请求失败（HTTP ${statusCode}）。`;
}

export function errorText(error         , fallback        )         {
    return error instanceof Error ? error.message : fallback;
}

export function blockLabel(type             )         {
    return type === "itr" || type === "bdy" ? type.toUpperCase() : type;
}

export function parseBlockSelection(value        )                 {
    if (value === "frame") return { type: "frame" };
    const [type, rawIndex] = value.split(":");
    return allOverlayTypes.includes(type               )
        ? { type: type               , index: Number(rawIndex) }
        : { type: "frame" };
}
