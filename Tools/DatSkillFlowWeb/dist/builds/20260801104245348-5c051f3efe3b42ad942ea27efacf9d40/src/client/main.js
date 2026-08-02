// dat-skill-flow-build:20260801104245348-5c051f3efe3b42ad942ea27efacf9d40
import { Gate2TimelinePreviewController } from "./timeline-controller.js";

                          
                
            
                       
                     
      
 

const statusElement = document.querySelector             ("#server-status");
const diagnosticsElement = document.querySelector             ("#diagnostics");
const canvas = document.querySelector                   ("#preview-canvas");
const playButton = document.querySelector                   ("#play-toggle");
const stepButton = document.querySelector                   ("#step-once");
const seekInput = document.querySelector                  ("#timeline-seek");
const loopEnabledInput = document.querySelector                  ("#loop-enabled");
const loopStartInput = document.querySelector                  ("#loop-start");
const loopEndInput = document.querySelector                  ("#loop-end");
const tickElement = document.querySelector             ("#tick-readout");
const frameElement = document.querySelector             ("#frame-readout");
const timeElement = document.querySelector             ("#time-readout");
const traceElement = document.querySelector             ("#trace-summary");
const fixtureElement = document.querySelector             ("#fixture-label");

const timeline = new Gate2TimelinePreviewController();
let advanceTimer                    ;

function parseTick(input                         , fallback        )         {
    if (input === null) {
        return fallback;
    }
    const value = Number(input.value);
    return Number.isSafeInteger(value) ? value : fallback;
}

function drawPreview()       {
    if (canvas === null) {
        return;
    }
    const view = timeline.viewModel(0.5);
    const context = canvas.getContext("2d");
    if (context === null) {
        return;
    }
    const { width, height } = canvas;
    context.clearRect(0, 0, width, height);
    const glow = context.createRadialGradient(width * 0.52, height * 0.42, 10, width * 0.52, height * 0.42, width * 0.6);
    glow.addColorStop(0, "rgba(244, 179, 75, 0.2)");
    glow.addColorStop(1, "rgba(11, 18, 22, 0)");
    context.fillStyle = glow;
    context.fillRect(0, 0, width, height);
    context.strokeStyle = "rgba(217, 229, 223, 0.10)";
    context.lineWidth = 1;
    for (let x = 32; x < width; x += 32) {
        context.beginPath();
        context.moveTo(x, 0);
        context.lineTo(x, height);
        context.stroke();
    }
    for (let y = 32; y < height; y += 32) {
        context.beginPath();
        context.moveTo(0, y);
        context.lineTo(width, y);
        context.stroke();
    }
    const centerX = width / 2;
    const centerY = height / 2;
    context.fillStyle = "#f4b34b";
    context.beginPath();
    context.arc(centerX, centerY, 54, 0, Math.PI * 2);
    context.fill();
    context.fillStyle = "#0b1216";
    context.font = "700 22px Georgia, serif";
    context.textAlign = "center";
    context.fillText(`F${view.presentation.entities[0]?.toFrame ?? view.frame}`, centerX, centerY + 8);
    context.textAlign = "left";
    context.font = "600 14px Georgia, serif";
    context.fillStyle = "#e8eee9";
    context.fillText("SYNTHETIC AUTHORITY FIXTURE", 26, 36);
    context.fillStyle = "#aab8b0";
    context.font = "14px Georgia, serif";
    context.fillText("Canvas is a presentation sample; it never advances canonical state.", 26, height - 26);
}

function syncReadOnlyUi()       {
    const view = timeline.viewModel(0.5);
    if (playButton !== null) {
        playButton.textContent = view.playing ? "Pause" : "Play";
        playButton.setAttribute("aria-pressed", String(view.playing));
    }
    if (tickElement !== null) tickElement.textContent = String(view.tick);
    if (frameElement !== null) frameElement.textContent = String(view.frame);
    if (timeElement !== null) timeElement.textContent = `${view.timeMs} ms`;
    if (traceElement !== null) traceElement.textContent = view.traceSummary;
    if (fixtureElement !== null) fixtureElement.textContent = view.fixtureLabel;
    if (diagnosticsElement !== null) diagnosticsElement.textContent = view.diagnostics.join(" ");
    const maximum = String(view.recordedEndTick);
    for (const input of [seekInput, loopStartInput, loopEndInput]) {
        if (input !== null) input.max = maximum;
    }
}

function syncEditableUi()       {
    const view = timeline.viewModel(0.5);
    syncReadOnlyUi();
    if (seekInput !== null) seekInput.value = String(view.tick);
    if (loopStartInput !== null) loopStartInput.value = String(view.loopStartTick);
    if (loopEndInput !== null) loopEndInput.value = String(view.loopEndTick);
    if (loopEnabledInput !== null) loopEnabledInput.checked = view.loopEnabled;
}

function scheduleAdvance()       {
    if (advanceTimer !== undefined || !timeline.playing) {
        return;
    }
    advanceTimer = window.setTimeout(() => {
        advanceTimer = undefined;
        timeline.advance();
        syncReadOnlyUi();
        scheduleAdvance();
    }, 33);
}

playButton?.addEventListener("click", () => {
    timeline.togglePlayback();
    scheduleAdvance();
    syncEditableUi();
});
stepButton?.addEventListener("click", () => {
    timeline.step();
    syncEditableUi();
});
seekInput?.addEventListener("input", () => {
    timeline.seek(parseTick(seekInput, timeline.canonical.tickIndex));
    syncEditableUi();
});
loopEnabledInput?.addEventListener("change", () => {
    timeline.setLoopEnabled(loopEnabledInput.checked);
    syncEditableUi();
});
for (const loopInput of [loopStartInput, loopEndInput]) {
    loopInput?.addEventListener("change", () => {
        timeline.setLoopBounds(
            parseTick(loopStartInput, 0),
            parseTick(loopEndInput, timeline.canonical.tickIndex),
        );
        syncEditableUi();
    });
}

async function connect()                {
    if (statusElement === null || diagnosticsElement === null) {
        return;
    }
    try {
        const response = await fetch("/api/health", { headers: { Accept: "application/json" } });
        const health = await response.json()                  ;
        if (!response.ok || !health.ok || health.data?.host !== "127.0.0.1") {
            throw new Error("Unexpected health response");
        }
        statusElement.textContent = "Connected to local server";
        statusElement.dataset.state = "connected";
    } catch {
        statusElement.textContent = "Local server unavailable";
        statusElement.dataset.state = "error";
        diagnosticsElement.textContent = "The loopback health check failed. No data was changed.";
    }
}

void connect();
syncEditableUi();

function renderFrame()       {
    drawPreview();
    window.requestAnimationFrame(renderFrame);
}

window.requestAnimationFrame(renderFrame);
