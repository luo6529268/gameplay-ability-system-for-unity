#!/usr/bin/env python3
"""Resume a large recovered-data copy without deleting destination data.

Policy:
  * Copy files missing from the destination.
  * Replace an existing file when its size differs, or when the absolute
    modification-time difference is greater than the configured threshold.
  * Skip otherwise.

Replacement is atomic: data is copied to a temporary sibling first and only
then swapped into place. Directory/file type conflicts are logged and skipped.
The script never purges, mirrors, moves, or deletes source/destination content.
"""

from __future__ import annotations

import argparse
import csv
import json
import os
import shutil
import sys
import threading
import time
from concurrent.futures import FIRST_COMPLETED, ThreadPoolExecutor, wait
from dataclasses import asdict, dataclass
from datetime import datetime
from pathlib import Path


@dataclass
class Counters:
    scanned_files: int = 0
    scanned_dirs: int = 0
    copied_missing: int = 0
    overwritten_size: int = 0
    overwritten_time: int = 0
    skipped_same: int = 0
    skipped_conflict: int = 0
    skipped_link: int = 0
    failed: int = 0
    bytes_copied: int = 0


def long_path(path: str) -> str:
    """Return a Windows long-path form while remaining harmless elsewhere."""
    absolute = os.path.abspath(path)
    if os.name != "nt" or absolute.startswith("\\\\?\\"):
        return absolute
    if absolute.startswith("\\\\"):
        return "\\\\?\\UNC\\" + absolute[2:]
    return "\\\\?\\" + absolute


def display_path(path: str) -> str:
    if path.startswith("\\\\?\\UNC\\"):
        return "\\\\" + path[8:]
    if path.startswith("\\\\?\\"):
        return path[4:]
    return path


class RecoveryCopy:
    def __init__(
        self,
        source: str,
        target: str,
        log_dir: str,
        threshold_seconds: int,
        workers: int,
        retries: int,
    ) -> None:
        self.source = long_path(source)
        self.target = long_path(target)
        self.source_display = display_path(self.source)
        self.target_display = display_path(self.target)
        self.log_dir = Path(log_dir)
        self.log_dir.mkdir(parents=True, exist_ok=True)
        stamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        self.run_id = f"resume_copy_{stamp}_{os.getpid()}"
        self.ledger_path = self.log_dir / f"{self.run_id}.csv"
        self.progress_path = self.log_dir / "resume_copy_progress.json"
        self.summary_path = self.log_dir / f"{self.run_id}_summary.json"
        self.stop_path = self.log_dir / f"{self.run_id}.stop"
        self.threshold = threshold_seconds
        self.workers = workers
        self.retries = retries
        self.counters = Counters()
        self.counter_lock = threading.Lock()
        self.log_lock = threading.Lock()
        self.started = time.time()
        self.last_progress = 0.0
        self.fatal_error: str | None = None
        self.ledger = self.ledger_path.open(
            "w", encoding="utf-8-sig", newline="", buffering=1
        )
        self.csv = csv.writer(self.ledger)
        self.csv.writerow(
            [
                "time",
                "action",
                "relative_path",
                "source_size",
                "destination_size_before",
                "source_mtime",
                "destination_mtime_before",
                "detail",
            ]
        )

    def update(self, **increments: int) -> None:
        with self.counter_lock:
            for key, value in increments.items():
                setattr(self.counters, key, getattr(self.counters, key) + value)

    def log(
        self,
        action: str,
        relative: str,
        source_size: int | str = "",
        destination_size: int | str = "",
        source_mtime: float | str = "",
        destination_mtime: float | str = "",
        detail: str = "",
    ) -> None:
        with self.log_lock:
            self.csv.writerow(
                [
                    datetime.now().isoformat(timespec="seconds"),
                    action,
                    relative,
                    source_size,
                    destination_size,
                    source_mtime,
                    destination_mtime,
                    detail,
                ]
            )

    def write_progress(self, status: str, force: bool = False) -> None:
        now = time.time()
        if not force and now - self.last_progress < 10:
            return
        self.last_progress = now
        with self.counter_lock:
            counters = asdict(self.counters)
        payload = {
            "run_id": self.run_id,
            "status": status,
            "source": self.source_display,
            "target": self.target_display,
            "policy": {
                "mtime_difference_seconds": self.threshold,
                "overwrite_when_size_differs": True,
                "delete_destination_extras": False,
            },
            "started": datetime.fromtimestamp(self.started).isoformat(
                timespec="seconds"
            ),
            "updated": datetime.now().isoformat(timespec="seconds"),
            "elapsed_seconds": round(now - self.started, 1),
            "ledger": str(self.ledger_path),
            "stop_file": str(self.stop_path),
            "fatal_error": self.fatal_error,
            "counters": counters,
        }
        temporary = self.progress_path.with_suffix(".json.tmp")
        temporary.write_text(
            json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8"
        )
        os.replace(temporary, self.progress_path)
        if force:
            self.summary_path.write_text(
                json.dumps(payload, ensure_ascii=False, indent=2),
                encoding="utf-8",
            )

    def should_stop(self) -> bool:
        return self.stop_path.exists()

    def copy_one(
        self,
        relative: str,
        source_stat: os.stat_result,
        destination_stat: os.stat_result | None,
        reason: str,
    ) -> None:
        source = long_path(os.path.join(self.source, relative))
        destination = long_path(os.path.join(self.target, relative))
        destination_dir = os.path.dirname(destination)
        destination_size = destination_stat.st_size if destination_stat else ""
        destination_mtime = destination_stat.st_mtime if destination_stat else ""
        os.makedirs(destination_dir, exist_ok=True)
        temp = (
            destination
            + f".codex-copying-{os.getpid()}-{threading.get_ident()}"
        )
        last_error = ""
        for attempt in range(1, self.retries + 1):
            try:
                if os.path.exists(temp):
                    os.unlink(temp)
                shutil.copy2(source, temp)
                copied_stat = os.stat(temp)
                if copied_stat.st_size != source_stat.st_size:
                    raise OSError(
                        f"temporary size mismatch: {copied_stat.st_size} "
                        f"!= {source_stat.st_size}"
                    )
                os.replace(temp, destination)
                if reason == "missing":
                    self.update(
                        copied_missing=1, bytes_copied=source_stat.st_size
                    )
                elif reason == "size":
                    self.update(
                        overwritten_size=1, bytes_copied=source_stat.st_size
                    )
                else:
                    self.update(
                        overwritten_time=1, bytes_copied=source_stat.st_size
                    )
                self.log(
                    f"copied_{reason}",
                    relative,
                    source_stat.st_size,
                    destination_size,
                    source_stat.st_mtime,
                    destination_mtime,
                    f"attempt={attempt}",
                )
                return
            except Exception as exc:  # noqa: BLE001 - recovery must keep going
                last_error = f"{type(exc).__name__}: {exc}"
                try:
                    if os.path.exists(temp):
                        os.unlink(temp)
                except OSError:
                    pass
                if attempt < self.retries:
                    time.sleep(min(4, attempt))
        self.update(failed=1)
        self.log(
            "failed",
            relative,
            source_stat.st_size,
            destination_size,
            source_stat.st_mtime,
            destination_mtime,
            last_error,
        )

    def run(self) -> int:
        if not os.path.isdir(self.source):
            raise RuntimeError(f"source directory is missing: {self.source_display}")
        if not os.path.isdir(self.target):
            raise RuntimeError(f"target directory is missing: {self.target_display}")
        if os.path.normcase(self.source) == os.path.normcase(self.target):
            raise RuntimeError("source and target resolve to the same directory")

        pending = set()
        max_pending = max(32, self.workers * 32)
        stack = [""]
        status = "running"
        try:
            with ThreadPoolExecutor(max_workers=self.workers) as executor:
                while stack or pending:
                    if self.should_stop():
                        status = "stopped"
                        break

                    while stack and len(pending) < max_pending:
                        relative_dir = stack.pop()
                        source_dir = long_path(
                            os.path.join(self.source, relative_dir)
                        )
                        target_dir = long_path(
                            os.path.join(self.target, relative_dir)
                        )
                        try:
                            os.makedirs(target_dir, exist_ok=True)
                            with os.scandir(source_dir) as iterator:
                                entries = list(iterator)
                        except Exception as exc:  # noqa: BLE001
                            self.update(failed=1)
                            self.log(
                                "failed_directory",
                                relative_dir,
                                detail=f"{type(exc).__name__}: {exc}",
                            )
                            continue

                        self.update(scanned_dirs=1)
                        for entry in entries:
                            relative = (
                                os.path.join(relative_dir, entry.name)
                                if relative_dir
                                else entry.name
                            )
                            try:
                                if entry.is_symlink():
                                    self.update(skipped_link=1)
                                    self.log(
                                        "skipped_link",
                                        relative,
                                        detail="symbolic link or reparse point",
                                    )
                                    continue
                                if entry.is_dir(follow_symlinks=False):
                                    destination = long_path(
                                        os.path.join(self.target, relative)
                                    )
                                    if os.path.exists(destination) and not os.path.isdir(
                                        destination
                                    ):
                                        self.update(skipped_conflict=1)
                                        self.log(
                                            "skipped_type_conflict",
                                            relative,
                                            detail="source is directory; target is file",
                                        )
                                    else:
                                        stack.append(relative)
                                    continue
                                if not entry.is_file(follow_symlinks=False):
                                    self.update(skipped_link=1)
                                    self.log(
                                        "skipped_special",
                                        relative,
                                        detail="not a regular file",
                                    )
                                    continue

                                source_stat = entry.stat(follow_symlinks=False)
                                self.update(scanned_files=1)
                                destination = long_path(
                                    os.path.join(self.target, relative)
                                )
                                destination_stat = None
                                reason = "missing"
                                try:
                                    destination_stat = os.stat(destination)
                                    if not os.path.isfile(destination):
                                        self.update(skipped_conflict=1)
                                        self.log(
                                            "skipped_type_conflict",
                                            relative,
                                            source_stat.st_size,
                                            detail=(
                                                "source is file; target is directory "
                                                "or special object"
                                            ),
                                        )
                                        continue
                                    if destination_stat.st_size != source_stat.st_size:
                                        reason = "size"
                                    elif (
                                        abs(
                                            destination_stat.st_mtime
                                            - source_stat.st_mtime
                                        )
                                        > self.threshold
                                    ):
                                        reason = "time"
                                    else:
                                        self.update(skipped_same=1)
                                        continue
                                except FileNotFoundError:
                                    destination_stat = None

                                pending.add(
                                    executor.submit(
                                        self.copy_one,
                                        relative,
                                        source_stat,
                                        destination_stat,
                                        reason,
                                    )
                                )
                                if len(pending) >= max_pending:
                                    break
                            except Exception as exc:  # noqa: BLE001
                                self.update(failed=1)
                                self.log(
                                    "failed_inspect",
                                    relative,
                                    detail=f"{type(exc).__name__}: {exc}",
                                )
                        self.write_progress(status)

                    if pending:
                        done, pending = wait(
                            pending, timeout=2, return_when=FIRST_COMPLETED
                        )
                        for future in done:
                            try:
                                future.result()
                            except Exception as exc:  # defensive guard
                                self.update(failed=1)
                                self.log(
                                    "failed_worker",
                                    "",
                                    detail=f"{type(exc).__name__}: {exc}",
                                )
                        self.write_progress(status)
                if status == "running":
                    status = "completed"
        except KeyboardInterrupt:
            status = "stopped"
        finally:
            self.write_progress(status, force=True)
            self.ledger.close()
        return 2 if status == "stopped" else (1 if self.counters.failed else 0)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--source", default=r"E:\I_Recovery_20260727\00_Sample"
    )
    parser.add_argument("--target", default="I:\\")
    parser.add_argument("--log-dir", default=r"E:\RecoveryLogs")
    parser.add_argument("--hours", type=float, default=24.0)
    parser.add_argument("--workers", type=int, default=4)
    parser.add_argument("--retries", type=int, default=3)
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    recovery = RecoveryCopy(
        source=args.source,
        target=args.target,
        log_dir=args.log_dir,
        threshold_seconds=round(args.hours * 3600),
        workers=max(1, min(args.workers, 8)),
        retries=max(1, args.retries),
    )
    try:
        return recovery.run()
    except Exception as exc:  # noqa: BLE001
        recovery.fatal_error = f"{type(exc).__name__}: {exc}"
        recovery.log("fatal", "", detail=recovery.fatal_error)
        recovery.write_progress("failed", force=True)
        recovery.ledger.close()
        return 1


if __name__ == "__main__":
    sys.exit(main())
