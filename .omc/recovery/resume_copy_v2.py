from __future__ import annotations

import argparse
import ctypes
import csv
import json
import os
import shutil
import sys
import threading
import time
from concurrent.futures import FIRST_COMPLETED, Future, ThreadPoolExecutor, wait
from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path


def long_path(path: str) -> str:
    path = os.fspath(path)
    if os.name != "nt":
        return os.path.abspath(path)
    if path.startswith("\\\\?\\"):
        return path
    if not os.path.isabs(path):
        path = os.path.normpath(os.path.join(os.getcwd(), path))
    if path.startswith("\\\\"):
        return "\\\\?\\UNC\\" + path[2:]
    return "\\\\?\\" + path


def copy_file_data(source: str, target: str) -> bool:
    try:
        shutil.copy2(source, target, follow_symlinks=False)
        return False
    except FileNotFoundError:
        if os.name != "nt" or not os.path.isfile(source):
            raise

    kernel32 = ctypes.WinDLL("kernel32", use_last_error=True)
    copy_file_w = kernel32.CopyFileW
    copy_file_w.argtypes = [
        ctypes.c_wchar_p,
        ctypes.c_wchar_p,
        ctypes.c_int,
    ]
    copy_file_w.restype = ctypes.c_int
    if not copy_file_w(source, target, 0):
        error = ctypes.get_last_error()
        raise OSError(error, ctypes.FormatError(error), source)
    return True


@dataclass
class Totals:
    scanned: int = 0
    copied_missing: int = 0
    overwritten_time: int = 0
    overwritten_size: int = 0
    skipped_same: int = 0
    skipped_links: int = 0
    conflicts: int = 0
    failed: int = 0
    bytes_copied: int = 0
    lock: threading.Lock = field(default_factory=threading.Lock)

    def update(self, **changes: int) -> None:
        with self.lock:
            for name, amount in changes.items():
                setattr(self, name, getattr(self, name) + amount)

    def snapshot(self) -> dict[str, int]:
        with self.lock:
            return {
                key: value
                for key, value in vars(self).items()
                if key != "lock"
            }


class Ledger:
    def __init__(self, log_dir: str) -> None:
        os.makedirs(log_dir, exist_ok=True)
        stamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        self.csv_path = os.path.join(log_dir, f"resume_copy_{stamp}.csv")
        self.progress_path = os.path.join(log_dir, "resume_copy_progress.json")
        self.lock = threading.Lock()
        self.handle = open(
            self.csv_path, "w", newline="", encoding="utf-8-sig", buffering=1
        )
        self.writer = csv.writer(self.handle)
        self.writer.writerow(
            ["time", "action", "source", "target", "bytes", "detail"]
        )

    def record(
        self,
        action: str,
        source: str,
        target: str,
        size: int = 0,
        detail: str = "",
    ) -> None:
        with self.lock:
            self.writer.writerow(
                [
                    datetime.now().isoformat(timespec="seconds"),
                    action,
                    source,
                    target,
                    size,
                    detail,
                ]
            )

    def progress(
        self,
        status: str,
        source: str,
        target: str,
        totals: Totals,
        started: float,
        error: str = "",
    ) -> None:
        payload = {
            "status": status,
            "source": source,
            "target": target,
            "pid": os.getpid(),
            "started": datetime.fromtimestamp(started).isoformat(timespec="seconds"),
            "updated": datetime.now().isoformat(timespec="seconds"),
            "elapsed_seconds": round(time.time() - started, 1),
            **totals.snapshot(),
        }
        if error:
            payload["error"] = error
        temporary = self.progress_path + ".tmp"
        with self.lock:
            with open(temporary, "w", encoding="utf-8") as handle:
                json.dump(payload, handle, ensure_ascii=False, indent=2)
            os.replace(temporary, self.progress_path)

    def close(self) -> None:
        with self.lock:
            self.handle.close()


def safe_copy(
    source: str,
    target: str,
    reason: str,
    totals: Totals,
    ledger: Ledger,
    retries: int,
) -> None:
    source_long = long_path(source)
    target_long = long_path(target)
    temporary = (
        target_long
        + f".codex-copying-{os.getpid()}-{threading.get_ident()}"
    )
    size = 0
    try:
        size = os.stat(source_long, follow_symlinks=False).st_size
    except OSError:
        pass

    for attempt in range(1, retries + 1):
        try:
            os.makedirs(os.path.dirname(target_long), exist_ok=True)
            if os.path.lexists(temporary):
                os.remove(temporary)
            used_win32_fallback = copy_file_data(source_long, temporary)
            os.replace(temporary, target_long)

            update = {"bytes_copied": size}
            if reason == "missing":
                update["copied_missing"] = 1
            elif reason == "time":
                update["overwritten_time"] = 1
            else:
                update["overwritten_size"] = 1
            totals.update(**update)
            ledger.record(
                "copied_" + reason,
                source,
                target,
                size,
                "CopyFileW fallback" if used_win32_fallback else "",
            )
            return
        except Exception as exc:
            try:
                if os.path.lexists(temporary):
                    os.remove(temporary)
            except OSError:
                pass
            if attempt < retries:
                time.sleep(float(attempt))
                continue
            totals.update(failed=1)
            ledger.record(
                "failed",
                source,
                target,
                size,
                f"{type(exc).__name__}: {exc}",
            )


def drain(
    pending: set[Future[None]],
    block: bool,
) -> set[Future[None]]:
    if not pending:
        return pending
    done, remaining = wait(
        pending,
        timeout=None if block else 0,
        return_when=FIRST_COMPLETED,
    )
    for future in done:
        future.result()
    return set(remaining)


def run(args: argparse.Namespace) -> int:
    source = os.path.abspath(args.source)
    target = os.path.abspath(args.target)
    log_dir = os.path.abspath(args.log_dir)
    threshold = args.hours * 3600.0

    if not os.path.isdir(source):
        raise RuntimeError(f"源目录不存在：{source}")
    if not os.path.isdir(target):
        raise RuntimeError(f"目标目录不存在：{target}")
    source_drive = os.path.splitdrive(source)[0].casefold()
    target_drive = os.path.splitdrive(target)[0].casefold()
    if source_drive == target_drive:
        common = os.path.commonpath([source, target]).casefold()
        if common in {source.casefold(), target.casefold()}:
            raise RuntimeError("源目录和目标目录不能互相包含。")

    started = time.time()
    totals = Totals()
    ledger = Ledger(log_dir)
    ledger.progress("starting", source, target, totals, started)
    ledger.record(
        "policy",
        source,
        target,
        detail=(
            f"missing=copy; size-different=overwrite; "
            f"absolute-time-difference>{args.hours}h=overwrite; "
            f"otherwise=skip; workers={args.workers}"
        ),
    )

    source_long = long_path(source)
    stack: list[tuple[str, str]] = [(source_long, target)]
    pending: set[Future[None]] = set()
    max_pending = max(args.workers * 16, 32)
    last_progress = 0.0

    try:
        with ThreadPoolExecutor(
            max_workers=args.workers,
            thread_name_prefix="recovery-copy",
        ) as pool:
            while stack:
                current_source, current_target = stack.pop()
                try:
                    entries = list(os.scandir(current_source))
                except Exception as exc:
                    totals.update(failed=1)
                    ledger.record(
                        "scan_failed",
                        current_source,
                        current_target,
                        detail=f"{type(exc).__name__}: {exc}",
                    )
                    continue

                try:
                    os.makedirs(long_path(current_target), exist_ok=True)
                except Exception as exc:
                    totals.update(failed=1)
                    ledger.record(
                        "mkdir_failed",
                        current_source,
                        current_target,
                        detail=f"{type(exc).__name__}: {exc}",
                    )
                    continue

                for entry in entries:
                    source_item = entry.path
                    target_item = os.path.join(current_target, entry.name)
                    totals.update(scanned=1)

                    try:
                        if entry.is_symlink():
                            totals.update(skipped_links=1)
                            ledger.record(
                                "skipped_link",
                                source_item,
                                target_item,
                            )
                            continue
                        if entry.is_dir(follow_symlinks=False):
                            if os.path.lexists(long_path(target_item)) and not os.path.isdir(
                                long_path(target_item)
                            ):
                                totals.update(conflicts=1)
                                ledger.record(
                                    "type_conflict",
                                    source_item,
                                    target_item,
                                    detail="source=directory,target=file",
                                )
                            else:
                                stack.append((source_item, target_item))
                            continue
                        if not entry.is_file(follow_symlinks=False):
                            totals.update(skipped_links=1)
                            ledger.record(
                                "skipped_special",
                                source_item,
                                target_item,
                            )
                            continue

                        source_stat = entry.stat(follow_symlinks=False)
                        target_long = long_path(target_item)
                        if not os.path.lexists(target_long):
                            reason = "missing"
                        elif not os.path.isfile(target_long):
                            totals.update(conflicts=1)
                            ledger.record(
                                "type_conflict",
                                source_item,
                                target_item,
                                source_stat.st_size,
                                "source=file,target=directory",
                            )
                            continue
                        else:
                            target_stat = os.stat(target_long, follow_symlinks=False)
                            if source_stat.st_size != target_stat.st_size:
                                reason = "size"
                            elif abs(source_stat.st_mtime - target_stat.st_mtime) > threshold:
                                reason = "time"
                            else:
                                totals.update(skipped_same=1)
                                continue

                        pending.add(
                            pool.submit(
                                safe_copy,
                                source_item,
                                target_item,
                                reason,
                                totals,
                                ledger,
                                args.retries,
                            )
                        )
                        if len(pending) >= max_pending:
                            pending = drain(pending, block=True)
                    except Exception as exc:
                        totals.update(failed=1)
                        ledger.record(
                            "inspect_failed",
                            source_item,
                            target_item,
                            detail=f"{type(exc).__name__}: {exc}",
                        )

                    now = time.time()
                    if now - last_progress >= args.progress_seconds:
                        ledger.progress(
                            "running", source, target, totals, started
                        )
                        last_progress = now

                pending = drain(pending, block=False)

            while pending:
                pending = drain(pending, block=True)
                ledger.progress("finishing", source, target, totals, started)

        status = "completed_with_failures" if totals.snapshot()["failed"] else "completed"
        ledger.progress(status, source, target, totals, started)
        ledger.record(status, source, target, detail=json.dumps(totals.snapshot()))
        return 2 if totals.snapshot()["failed"] else 0
    except Exception as exc:
        ledger.progress(
            "fatal_error",
            source,
            target,
            totals,
            started,
            f"{type(exc).__name__}: {exc}",
        )
        ledger.record(
            "fatal_error",
            source,
            target,
            detail=f"{type(exc).__name__}: {exc}",
        )
        raise
    finally:
        ledger.close()


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="恢复数据的可续传复制：仅补缺失项，并按大小/时间差覆盖。"
    )
    parser.add_argument(
        "--source",
        default=r"E:\I_Recovery_20260727\00_Sample",
    )
    parser.add_argument("--target", default=r"I:\.")
    parser.add_argument("--log-dir", default=r"E:\RecoveryLogs")
    parser.add_argument("--hours", type=float, default=24.0)
    parser.add_argument("--workers", type=int, default=4)
    parser.add_argument("--retries", type=int, default=3)
    parser.add_argument("--progress-seconds", type=float, default=10.0)
    return parser.parse_args()


if __name__ == "__main__":
    try:
        sys.exit(run(parse_args()))
    except Exception as error:
        print(f"FATAL: {type(error).__name__}: {error}", file=sys.stderr)
        sys.exit(1)
