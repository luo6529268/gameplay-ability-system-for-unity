// dat-skill-flow-build:20260810141836294-903df43a38704e7b961c919391961105
                                 
                                                         
                                        

                                                       
                           
                                                                
                       
 

                                          
                        
                                
                           
                                                                  
                                              
 

export function createLatestTaskScheduler                 (
    run                                     ,
    onBusyChange                          = () => undefined,
)                                       {
    let active                                            ;
    let pending                                            ;
    let latestId = 0;
    let generation = 0;
    let busyState = false;
    const setBusy = (busy         )       => {
        if (busyState === busy) return;
        busyState = busy;
        onBusyChange(busy);
    };

    const start = (task                                )       => {
        active = task;
        setBusy(true);
        void run(task.input).then(
            (value) => {
                task.resolve(task.id === latestId && task.generation === generation
                    ? { status: "committed", value }
                    : { status: "superseded" });
            },
            (error) => {
                if (task.id === latestId && task.generation === generation) task.reject(error);
                else task.resolve({ status: "superseded" });
            },
        ).finally(() => {
            active = undefined;
            const next = pending;
            pending = undefined;
            if (next) start(next);
            else setBusy(false);
        });
    };

    return {
        get busy()          { return active !== undefined; },
        schedule(input        )                                     {
            const id = ++latestId;
            return new Promise((resolve, reject) => {
                const task = { id, generation, input, resolve, reject };
                if (!active) {
                    start(task);
                    return;
                }
                pending?.resolve({ status: "superseded" });
                pending = task;
            });
        },
        invalidate()       {
            generation += 1;
            latestId += 1;
            pending?.resolve({ status: "superseded" });
            pending = undefined;
            if (!active) setBusy(false);
        },
    };
}
