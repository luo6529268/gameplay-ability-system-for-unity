import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { createLatestTaskScheduler } from "../../src/client/latest-task-scheduler.js";

function deferred<T>(): {
    readonly promise: Promise<T>;
    readonly resolve: (value: T) => void;
    readonly reject: (error: unknown) => void;
} {
    let resolve!: (value: T) => void;
    let reject!: (error: unknown) => void;
    const promise = new Promise<T>((resolvePromise, rejectPromise) => {
        resolve = resolvePromise;
        reject = rejectPromise;
    });
    return { promise, resolve, reject };
}

describe("latest task scheduler", () => {
    it("runs one task at a time and retains only the latest pending task", async () => {
        const first = deferred<number>(), third = deferred<number>();
        const started: number[] = [], busy: boolean[] = [];
        const scheduler = createLatestTaskScheduler<number, number>((value) => {
            started.push(value);
            return value === 1 ? first.promise : third.promise;
        }, (value) => busy.push(value));

        const firstResult = scheduler.schedule(1);
        const secondResult = scheduler.schedule(2);
        const thirdResult = scheduler.schedule(3);
        assert.deepEqual(started, [1]);
        assert.deepEqual(await secondResult, { status: "superseded" });

        first.resolve(10);
        assert.deepEqual(await firstResult, { status: "superseded" });
        await new Promise((resolve) => setImmediate(resolve));
        assert.deepEqual(started, [1, 3]);

        third.resolve(30);
        assert.deepEqual(await thirdResult, { status: "committed", value: 30 });
        await new Promise((resolve) => setImmediate(resolve));
        assert.deepEqual(busy, [true, false]);
        assert.equal(scheduler.busy, false);
    });

    it("invalidates active and pending results without leaking rejections", async () => {
        const active = deferred<number>();
        const scheduler = createLatestTaskScheduler<number, number>(() => active.promise);
        const activeResult = scheduler.schedule(1);
        const pendingResult = scheduler.schedule(2);
        scheduler.invalidate();
        assert.deepEqual(await pendingResult, { status: "superseded" });
        active.reject(new Error("stale"));
        assert.deepEqual(await activeResult, { status: "superseded" });
    });
});
