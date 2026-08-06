export type LatestTaskResult<T> =
    | { readonly status: "committed"; readonly value: T }
    | { readonly status: "superseded" };

export interface LatestTaskScheduler<TInput, TOutput> {
    readonly busy: boolean;
    schedule(input: TInput): Promise<LatestTaskResult<TOutput>>;
    invalidate(): void;
}

interface ScheduledTask<TInput, TOutput> {
    readonly id: number;
    readonly generation: number;
    readonly input: TInput;
    readonly resolve: (result: LatestTaskResult<TOutput>) => void;
    readonly reject: (error: unknown) => void;
}

export function createLatestTaskScheduler<TInput, TOutput>(
    run: (input: TInput) => Promise<TOutput>,
    onBusyChange: (busy: boolean) => void = () => undefined,
): LatestTaskScheduler<TInput, TOutput> {
    let active: ScheduledTask<TInput, TOutput> | undefined;
    let pending: ScheduledTask<TInput, TOutput> | undefined;
    let latestId = 0;
    let generation = 0;
    let busyState = false;
    const setBusy = (busy: boolean): void => {
        if (busyState === busy) return;
        busyState = busy;
        onBusyChange(busy);
    };

    const start = (task: ScheduledTask<TInput, TOutput>): void => {
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
        get busy(): boolean { return active !== undefined; },
        schedule(input: TInput): Promise<LatestTaskResult<TOutput>> {
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
        invalidate(): void {
            generation += 1;
            latestId += 1;
            pending?.resolve({ status: "superseded" });
            pending = undefined;
            if (!active) setBusy(false);
        },
    };
}
