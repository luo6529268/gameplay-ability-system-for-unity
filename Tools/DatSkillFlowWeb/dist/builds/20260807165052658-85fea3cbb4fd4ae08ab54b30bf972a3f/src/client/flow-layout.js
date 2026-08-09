// dat-skill-flow-build:20260807165052658-85fea3cbb4fd4ae08ab54b30bf972a3f
                                                                     

                                   
                                 
                       
                       
                           
                            
 

                             
                           
                            
                                                
 

export function layoutSkillFlow(graph                )             {
    const columns = 3;
    const horizontalGap = 28;
    const verticalGap = 22;
    const nodeWidth = 112;
    const nodeHeight = 42;
    const width = 18 + columns * nodeWidth + (columns - 1) * horizontalGap + 18;
    const height = 76;
    const nodes = graph.nodes.map((node, index) => ({
        node,
        x: 18 + (index % columns) * (nodeWidth + horizontalGap),
        y: 16 + Math.floor(index / columns) * (nodeHeight + verticalGap),
        width: nodeWidth,
        height: nodeHeight,
    }));
    return Object.freeze({
        width,
        height: Math.max(height, 16 + Math.ceil(nodes.length / columns) * (nodeHeight + verticalGap)),
        nodes: Object.freeze(nodes),
    });
}
