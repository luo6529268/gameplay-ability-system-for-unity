// dat-skill-flow-build:20260811031827464-4f29b050d0ca427fa8cfccf4def5e41d
                                              

                              
                          
                           
 

                                                  
                            
                                   
                                 
                                 
                                  
                                  
 

export const PANEL_SEPARATOR_WIDTH = 6;
export const LEFT_PANEL_MINIMUM = 200;
export const LEFT_PANEL_MAXIMUM = 420;
export const RIGHT_PANEL_MINIMUM = 240;
export const RIGHT_PANEL_MAXIMUM = 460;
export const MOBILE_PANEL_MAXIMUM = 850;
export const COMPACT_PANEL_MAXIMUM = 1120;
export const COMPACT_DEFAULT_PANEL_WIDTHS = { left: 230, right: 286 }         ;
export const WIDE_DEFAULT_PANEL_WIDTHS = { left: 286, right: 330 }         ;

const COMPACT_MIDDLE_MINIMUM = 360;
const WIDE_MIDDLE_MINIMUM = 420;

function clamp(value        , minimum        , maximum        )         {
    return Math.max(minimum, Math.min(maximum, value));
}

function integer(value        , fallback        )         {
    return Number.isFinite(value) ? Math.round(value) : fallback;
}

export function defaultPanelWidths(containerWidth        )              {
    return containerWidth <= COMPACT_PANEL_MAXIMUM
        ? COMPACT_DEFAULT_PANEL_WIDTHS
        : WIDE_DEFAULT_PANEL_WIDTHS;
}

export function middleMinimumForWidth(containerWidth        )         {
    return containerWidth <= COMPACT_PANEL_MAXIMUM
        ? COMPACT_MIDDLE_MINIMUM
        : WIDE_MIDDLE_MINIMUM;
}

export function clampPanelWidths(
    containerWidth        ,
    requested             ,
    resizedPanel                 ,
)              {
    const width = Math.max(0, Math.floor(containerWidth));
    const separatorTotal = PANEL_SEPARATOR_WIDTH * 2;
    const middleMinimum = middleMinimumForWidth(width);
    const sideBudget = Math.max(
        LEFT_PANEL_MINIMUM + RIGHT_PANEL_MINIMUM,
        width - separatorTotal - middleMinimum,
    );
    const defaults = defaultPanelWidths(width);
    let left = clamp(integer(requested.left, defaults.left), LEFT_PANEL_MINIMUM, LEFT_PANEL_MAXIMUM);
    let right = clamp(integer(requested.right, defaults.right), RIGHT_PANEL_MINIMUM, RIGHT_PANEL_MAXIMUM);

    if (left + right > sideBudget) {
        if (resizedPanel === "left") {
            right = Math.min(right, sideBudget - LEFT_PANEL_MINIMUM);
            left = Math.min(left, sideBudget - right);
        } else if (resizedPanel === "right") {
            left = Math.min(left, sideBudget - RIGHT_PANEL_MINIMUM);
            right = Math.min(right, sideBudget - left);
        } else {
            const minimumTotal = LEFT_PANEL_MINIMUM + RIGHT_PANEL_MINIMUM;
            const flexibleTotal = left + right - minimumTotal;
            const flexibleBudget = Math.max(0, sideBudget - minimumTotal);
            const ratio = flexibleTotal === 0 ? 0 : Math.min(1, flexibleBudget / flexibleTotal);
            left = LEFT_PANEL_MINIMUM + Math.floor((left - LEFT_PANEL_MINIMUM) * ratio);
            right = RIGHT_PANEL_MINIMUM + Math.floor((right - RIGHT_PANEL_MINIMUM) * ratio);
        }
    }

    const middle = Math.max(0, width - separatorTotal - left - right);
    return {
        left,
        right,
        middle,
        middleMinimum,
        leftMinimum: LEFT_PANEL_MINIMUM,
        leftMaximum: Math.max(
            LEFT_PANEL_MINIMUM,
            Math.min(LEFT_PANEL_MAXIMUM, sideBudget - right),
        ),
        rightMinimum: RIGHT_PANEL_MINIMUM,
        rightMaximum: Math.max(
            RIGHT_PANEL_MINIMUM,
            Math.min(RIGHT_PANEL_MAXIMUM, sideBudget - left),
        ),
    };
}

export function resizePanelWidths(
    containerWidth        ,
    start             ,
    panel                ,
    horizontalDelta        ,
)              {
    const delta = integer(horizontalDelta, 0);
    return clampPanelWidths(
        containerWidth,
        {
            left: panel === "left" ? start.left + delta : start.left,
            right: panel === "right" ? start.right - delta : start.right,
        },
        panel,
    );
}
