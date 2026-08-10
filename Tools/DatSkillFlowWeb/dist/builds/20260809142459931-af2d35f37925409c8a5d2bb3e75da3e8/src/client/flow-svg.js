// dat-skill-flow-build:20260809142459931-af2d35f37925409c8a5d2bb3e75da3e8
             
                  
                       
                       
                   
                  
                         
import { layoutSkillFlow,                       } from "./flow-layout.js";

const SVG_NS = "http://www.w3.org/2000/svg";

                                 
                                                           
                                     
                                                         
                                                              
                                                               
 

function element                                      (name   )                          {
    return document.createElementNS(SVG_NS, name);
}

function center(position                  )                           {
    return {
        x: position.x + position.width / 2,
        y: position.y + position.height / 2,
    };
}

function positionMap(layout                                    )                                        {
    return new Map(layout.nodes.map((position) => [position.node.id, position]));
}

function edgePath(from                  , to                  )         {
    const start = center(from);
    const end = center(to);
    const bend = Math.max(24, Math.abs(end.x - start.x) * 0.35);
    return `M ${start.x} ${start.y} C ${start.x + bend} ${start.y}, ${end.x - bend} ${end.y}, ${end.x} ${end.y}`;
}

function appendTitle(parent            , value        )       {
    const title = element("title");
    title.textContent = value;
    parent.append(title);
}

function renderNode(
    svg               ,
    position                  ,
    options                ,
)       {
    const group = element("g");
    group.classList.add("flow-node", `flow-node-${position.node.kind}`);
    group.dataset.nodeId = position.node.id;
    group.setAttribute("transform", `translate(${position.x} ${position.y})`);
    const rect = element("rect");
    rect.setAttribute("width", String(position.width));
    rect.setAttribute("height", String(position.height));
    group.append(rect);
    const label = element("text");
    label.setAttribute("x", String(position.width / 2));
    label.setAttribute("y", "19");
    label.setAttribute("text-anchor", "middle");
    label.textContent = position.node.kind === "frame"
        ? `帧 ${position.node.frameId}`
        : position.node.kind === "entry"
            ? position.node.label
            : `目标 ${position.node.target}`;
    group.append(label);
    if (position.node.kind === "frame") {
        const occurrence = element("text");
        occurrence.setAttribute("x", String(position.width / 2));
        occurrence.setAttribute("y", "34");
        occurrence.setAttribute("text-anchor", "middle");
        occurrence.classList.add("flow-node-meta");
        occurrence.textContent = `#${position.node.occurrence}`;
        group.append(occurrence);
        group.addEventListener("click", () => options.onSelectNode(position.node));
    } else if (position.node.kind === "entry") {
        const target = element("text");
        target.setAttribute("x", String(position.width / 2));
        target.setAttribute("y", "34");
        target.setAttribute("text-anchor", "middle");
        target.classList.add("flow-node-meta");
        target.textContent = `入口帧 ${position.node.frameId}`;
        group.append(target);
        group.addEventListener("click", () => options.onSelectEntry(position.node));
    } else {
        appendTitle(group, `未解析目标 ${position.node.target}：${position.node.reason}`);
    }
    svg.append(group);
}

export function renderFlowSvg(
    svg               ,
    graph                ,
    options                ,
)             {
    const controller = new AbortController();
    const layout = layoutSkillFlow(graph);
    const positions = positionMap(layout);
    svg.replaceChildren();
    svg.setAttribute("viewBox", `0 0 ${layout.width} ${layout.height}`);
    svg.setAttribute("preserveAspectRatio", "xMinYMin meet");

    const defs = element("defs");
    const marker = element("marker");
    marker.id = "flow-arrow";
    marker.setAttribute("viewBox", "0 0 10 10");
    marker.setAttribute("refX", "9");
    marker.setAttribute("refY", "5");
    marker.setAttribute("markerWidth", "5");
    marker.setAttribute("markerHeight", "5");
    marker.setAttribute("orient", "auto-start-reverse");
    const arrow = element("path");
    arrow.setAttribute("d", "M 0 0 L 10 5 L 0 10 z");
    arrow.setAttribute("fill", "currentColor");
    marker.append(arrow);
    defs.append(marker);
    svg.append(defs);

    for (const edge of graph.edges) {
        const from = positions.get(edge.from);
        const to = positions.get(edge.to);
        if (from === undefined || to === undefined) continue;
        const path = element("path");
        path.classList.add("flow-edge", `flow-edge-${edge.key}`);
        if (options.editableFieldIds.has(edge.id)) path.classList.add("is-editable");
        if (options.selectedEdgeId === edge.id) path.classList.add("is-selected");
        path.dataset.edgeId = edge.id;
        path.setAttribute("d", edgePath(from, to));
        path.setAttribute("marker-end", "url(#flow-arrow)");
        path.setAttribute("tabindex", options.editableFieldIds.has(edge.id) ? "0" : "-1");
        path.setAttribute("role", "button");
        appendTitle(path, `${edge.key}: ${edge.rawTarget}${edge.resolution === "frame" ? "" : `（${edge.resolution}）`}`);
        if (options.editableFieldIds.has(edge.id)) {
            path.addEventListener("click", () => options.onSelectEdge(edge), { signal: controller.signal });
            path.addEventListener("keydown", (event) => {
                if (event.key === "Enter" || event.key === " ") {
                    event.preventDefault();
                    options.onSelectEdge(edge);
                }
            }, { signal: controller.signal });
        }
        svg.append(path);
    }
    for (const position of layout.nodes) renderNode(svg, position, options);
    return () => controller.abort();
}
