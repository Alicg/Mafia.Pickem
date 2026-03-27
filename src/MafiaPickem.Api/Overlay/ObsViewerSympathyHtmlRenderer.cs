namespace MafiaPickem.Api.Overlay;

public static class ObsViewerSympathyHtmlRenderer
{
    private static readonly Lazy<string> FontBase64 = new(() =>
    {
        using var stream = typeof(ObsViewerSympathyHtmlRenderer).Assembly
            .GetManifestResourceStream("Actay-Regular.otf")!;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return Convert.ToBase64String(ms.ToArray());
    });

    public static string Render(int tournamentId)
    {
        var fontBase64 = FontBase64.Value;
        return $$"""
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Mafia Pickem Overlay</title>
    <style>
        @font-face {
            font-family: 'Actay';
            src: url('data:font/otf;base64,{{fontBase64}}') format('opentype');
            font-weight: normal;
            font-style: normal;
            font-display: block;
        }

        :root {
            --offset-x: 0px;
            --offset-y: 24px;
            --card-bg: linear-gradient(180deg, rgba(10, 10, 12, 0.96), rgba(24, 24, 28, 0.92));
            --card-edge: rgba(255, 255, 255, 0.08);
            --card-shadow: 0 18px 45px rgba(0, 0, 0, 0.44);
            --text-main: rgba(255, 248, 238, 0.98);
            --text-muted: rgba(223, 221, 217, 0.72);
            --title-color: rgba(243, 228, 207, 0.94);
            --town-label: rgba(219, 200, 177, 0.92);
            --mafia-label: rgba(205, 214, 244, 0.92);
            --track-bg: rgba(255, 255, 255, 0.12);
            --track-edge: rgba(255, 255, 255, 0.08);
            --town-fill: linear-gradient(90deg, #b88763 0%, #d9aa80 100%);
            --mafia-fill: linear-gradient(90deg, #88a0e8 0%, #b2c1ff 100%);
            --badge-live-bg: linear-gradient(180deg, #6e2a2a 0%, #3a1515 100%);
            --badge-finish-bg: linear-gradient(180deg, #2d4d30 0%, #18321c 100%);
            font-family: "Actay", "Bahnschrift", "Segoe UI Variable Display", "Trebuchet MS", sans-serif;
        }

        * {
            box-sizing: border-box;
        }

        html,
        body {
            width: 100%;
            height: 100%;
            margin: 0;
            overflow: hidden;
            background: transparent;
        }

        body {
            color: var(--text-main);
        }

        .overlay-root {
            position: relative;
            width: 100vw;
            height: 100vh;
            pointer-events: none;
        }

        .overlay-root.is-hidden {
            display: none;
        }

        /* ── shared block wrapper ── */
        .block-wrap {
            position: absolute;
            top: calc(var(--offset-y));
            left: calc(var(--offset-x));
            transform: scale(var(--overlay-scale, 1));
            transform-origin: top left;
            width: min(640px, calc(100vw - 32px));
            display: flex;
            flex-direction: column;
            align-items: center;
        }

        .block-wrap.is-dynamic-managed {
            transition: transform var(--animation-duration, 420ms) cubic-bezier(0.22, 1, 0.36, 1), opacity var(--animation-duration, 420ms) ease;
            will-change: transform, opacity;
        }

        .block-wrap.is-dynamic-hidden-top {
            transform: scale(var(--overlay-scale, 1)) translateY(calc(-100% - 24px));
            opacity: 0;
        }

        /* ── header (shared) ── */
        .block-header {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
            margin-bottom: 8px;
        }

        .block-badge {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-width: 58px;
            min-height: 28px;
            padding: 0 12px;
            border-radius: 999px;
            background: var(--badge-live-bg);
            border: 1px solid rgba(255, 255, 255, 0.1);
            color: rgba(255, 244, 238, 0.96);
            font-size: 15px;
            font-weight: 800;
            letter-spacing: 0.08em;
            text-transform: uppercase;
            box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.04);
        }

        .block-wrap.is-finished .block-badge {
            background: var(--badge-finish-bg);
        }

        .block-title {
            font-size: 24px;
            font-weight: 700;
            letter-spacing: 0.01em;
            color: var(--title-color);
            white-space: nowrap;
        }

        .block-footer {
            margin-top: 8px;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 16px;
            font-size: 23px;
            font-weight: 700;
            color: var(--text-muted);
        }

        .block-handle {
            display: inline-flex;
            align-items: center;
            gap: 4px;
            color: rgba(218, 218, 222, 0.7);
        }

        .block-handle__icon {
            display: inline-flex;
            width: 1.3em;
            height: 1.3em;
            flex: 0 0 1.3em;
            color: #38bdf8;
            filter: drop-shadow(0 0 8px rgba(255, 243, 224, 0.22));
            margin-bottom: 2px;
        }

        .block-handle__icon svg {
            width: 100%;
            height: 100%;
            display: block;
        }

        /* ── shared card ── */
        .block-card {
            width: 100%;
            padding: 10px 16px;
            border-radius: 999px;
            border: 1px solid var(--card-edge);
            background:
                radial-gradient(circle at top left, rgba(255, 255, 255, 0.08), transparent 34%),
                radial-gradient(circle at bottom right, rgba(255, 255, 255, 0.04), transparent 30%),
                var(--card-bg);
            box-shadow: var(--card-shadow);
            backdrop-filter: blur(12px);
        }

        /* ── sympathy block ── */
        .sympathy-row {
            display: flex;
            align-items: center;
            gap: 0;
        }

        .sympathy-track {
            position: relative;
            flex: 1;
            height: 16px;
            border-radius: 999px;
            overflow: hidden;
            background: var(--track-bg);
            box-shadow: inset 0 0 0 1px var(--track-edge);
        }

        .sympathy-track__fill {
            position: absolute;
            top: 0;
            height: 100%;
            border-radius: inherit;
            transition: width 0.5s ease;
        }

        .sympathy-track.is-town .sympathy-track__fill {
            right: 0;
            background: var(--town-fill);
            box-shadow: inset 0 0 0 1px rgba(255, 242, 229, 0.28), 0 0 20px rgba(202, 147, 106, 0.24);
        }

        .sympathy-track.is-mafia .sympathy-track__fill {
            left: 0;
            background: var(--mafia-fill);
            box-shadow: inset 0 0 0 1px rgba(237, 242, 255, 0.28), 0 0 20px rgba(124, 148, 230, 0.24);
        }

        .sympathy-circle {
            flex: 0 0 auto;
            display: flex;
            align-items: center;
            justify-content: center;
            width: 52px;
            height: 52px;
            border-radius: 50%;
            font-size: 26px;
            font-weight: 700;
            line-height: 1;
            letter-spacing: 0;
            text-align: center;
            color: var(--text-main);
            margin: 0 6px;
            padding-top: 2px;
        }

        .sympathy-circle.is-town {
            background: linear-gradient(180deg, rgba(184, 135, 99, 0.5) 0%, rgba(184, 135, 99, 0.3) 100%);
            border: 1.5px solid rgba(219, 170, 128, 0.4);
        }

        .sympathy-circle.is-mafia {
            background: linear-gradient(180deg, rgba(136, 160, 232, 0.5) 0%, rgba(136, 160, 232, 0.3) 100%);
            border: 1.5px solid rgba(178, 193, 255, 0.4);
        }

        .sympathy-percent-sign {
            flex: 0 0 auto;
            font-size: 20px;
            font-weight: 600;
            color: var(--text-muted);
            margin: 0 0px;
        }

        /* ── poll (top-3 vote) block ── */
        .poll-card {
            width: 100%;
            padding: 12px 16px;
                border-radius: 999px;
            border: 1px solid var(--card-edge);
            background:
                radial-gradient(circle at top left, rgba(255, 255, 255, 0.08), transparent 34%),
                radial-gradient(circle at bottom right, rgba(255, 255, 255, 0.04), transparent 30%),
                var(--card-bg);
            box-shadow: var(--card-shadow);
            backdrop-filter: blur(12px);
        }

        .poll-items {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
            flex-wrap: wrap;
        }

        .poll-item {
            display: flex;
            align-items: center;
            gap: 8px;
            padding: 6px 16px 6px 6px;
            border-radius: 999px;
            background: rgba(255, 255, 255, 0.06);
            border: 1px solid rgba(255, 255, 255, 0.08);
        }

        .poll-item__pct {
            display: flex;
            align-items: center;
            justify-content: center;
            min-width: 46px;
            height: 38px;
            padding: 0 8px;
            border-radius: 999px;
            font-size: 20px;
            font-weight: 700;
            line-height: 1;
            color: var(--text-main);
        }

        .poll-item:nth-child(1) .poll-item__pct {
            background: linear-gradient(180deg, rgba(184, 135, 99, 0.55) 0%, rgba(184, 135, 99, 0.30) 100%);
            border: 1.5px solid rgba(219, 170, 128, 0.45);
        }

        .poll-item:nth-child(2) .poll-item__pct {
            background: linear-gradient(180deg, rgba(136, 160, 232, 0.50) 0%, rgba(136, 160, 232, 0.28) 100%);
            border: 1.5px solid rgba(178, 193, 255, 0.40);
        }

        .poll-item:nth-child(3) .poll-item__pct {
            background: linear-gradient(180deg, rgba(136, 160, 232, 0.50) 0%, rgba(136, 160, 232, 0.28) 100%);
            border: 1.5px solid rgba(178, 193, 255, 0.40);
        }

        .poll-item__name {
            font-size: 20px;
            font-weight: 600;
            color: var(--text-main);
            white-space: nowrap;
        }

        @media (max-width: 720px) {
            .block-card, .poll-card {
                padding: 8px 14px;
            }

            .block-title {
                font-size: 20px;
            }

            .sympathy-circle {
                width: 42px;
                height: 42px;
                font-size: 21px;
            }

            .sympathy-percent-sign {
                font-size: 16px;
            }

            .block-footer {
                font-size: 23px;
                font-weight: 700;
                gap: 10px;
            }

            .poll-item__pct {
                font-size: 17px;
                min-width: 40px;
                height: 34px;
            }

            .poll-item__name {
                font-size: 17px;
            }
        }
    </style>
</head>
<body>
    <div class="overlay-root is-hidden" id="overlayRoot">
        <!-- Block 1: Viewer sympathy (town vs mafia) -->
        <div class="block-wrap" id="sympathyWrap">
            <div class="block-header">
                <div class="block-badge" id="sympathyBadge">LIVE</div>
                <div class="block-title">Зрительские симпатии</div>
            </div>

            <section class="block-card">
                <div class="sympathy-row">
                    <div class="sympathy-track is-town">
                        <div class="sympathy-track__fill" id="townFill"></div>
                    </div>
                    <div class="sympathy-circle is-town" id="townPercent">-</div>
                    <div class="sympathy-percent-sign">%</div>
                    <div class="sympathy-circle is-mafia" id="mafiaPercent">-</div>
                    <div class="sympathy-track is-mafia">
                        <div class="sympathy-track__fill" id="mafiaFill"></div>
                    </div>
                </div>
            </section>

            <div class="block-footer">
                <span id="sympathyMeta">0 прогнозов через</span>
                <span class="block-handle"><span class="block-handle__icon" aria-hidden="true"><svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><circle cx="12" cy="12" r="12" fill="currentColor" fill-opacity="0.18"/><path d="M17.94 6.62L5.98 11.23C5.17 11.56 5.18 12.01 5.83 12.21L8.9 13.17L15.99 8.69C16.33 8.48 16.64 8.59 16.39 8.81L10.65 13.99L10.43 17.12C10.75 17.12 10.89 16.97 11.07 16.79L12.56 15.34L15.66 17.63C16.23 17.94 16.64 17.78 16.78 17.12L18.81 7.54C19.02 6.73 18.49 6.36 17.94 6.62Z" fill="currentColor"/></svg></span>@MafiaPickemBot</span>
            </div>
        </div>

        <!-- Block 2: Top-3 first-vote poll -->
        <div class="block-wrap" id="pollWrap" style="display:none">
            <div class="block-header">
                <div class="block-badge" id="pollBadge">LIVE-ОПРОС</div>
                <div class="block-title">Кого заголосуют первым?</div>
            </div>

            <section class="poll-card">
                <div class="poll-items" id="pollItems"></div>
            </section>

            <div class="block-footer">
                <span id="pollMeta">0 голосов через</span>
                <span class="block-handle"><span class="block-handle__icon" aria-hidden="true"><svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><circle cx="12" cy="12" r="12" fill="currentColor" fill-opacity="0.18"/><path d="M17.94 6.62L5.98 11.23C5.17 11.56 5.18 12.01 5.83 12.21L8.9 13.17L15.99 8.69C16.33 8.48 16.64 8.59 16.39 8.81L10.65 13.99L10.43 17.12C10.75 17.12 10.89 16.97 11.07 16.79L12.56 15.34L15.66 17.63C16.23 17.94 16.64 17.78 16.78 17.12L18.81 7.54C19.02 6.73 18.49 6.36 17.94 6.62Z" fill="currentColor"/></svg></span>@MafiaPickemBot</span>
            </div>
        </div>
    </div>

    <script>
        const tournamentId = {{tournamentId}};
        const overlayRoot = document.getElementById('overlayRoot');
        const sympathyWrap = document.getElementById('sympathyWrap');
        const sympathyBadge = document.getElementById('sympathyBadge');
        const townPercent = document.getElementById('townPercent');
        const mafiaPercent = document.getElementById('mafiaPercent');
        const townFill = document.getElementById('townFill');
        const mafiaFill = document.getElementById('mafiaFill');
        const sympathyMeta = document.getElementById('sympathyMeta');
        const pollWrap = document.getElementById('pollWrap');
        const pollBadge = document.getElementById('pollBadge');
        const pollItems = document.getElementById('pollItems');
        const pollMeta = document.getElementById('pollMeta');
        const rootStyles = document.documentElement;

        const defaultOverlaySettings = {
            viewerSympathyBlock: {
                horizontalOffset: 0,
                verticalOffset: 24,
                scale: 10,
                dynamicDisplay: {
                    enabled: false,
                    intervalSeconds: 30,
                    visibleDurationSeconds: 8,
                    animationDurationMs: 420,
                },
            },
        };

        const formatPercent = (value) => `${Math.round(Number(value || 0))}`;
        const formatPredictionsCount = (value) => {
            const count = Math.max(0, Math.trunc(Number(value || 0)));
            const mod10 = count % 10;
            const mod100 = count % 100;

            if (mod10 === 1 && mod100 !== 11) {
                return `${count} прогноз`;
            }

            if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) {
                return `${count} прогноза`;
            }

            return `${count} прогнозов`;
        };

        const formatVotesCount = (value) => {
            const count = Math.max(0, Math.trunc(Number(value || 0)));
            const mod10 = count % 10;
            const mod100 = count % 100;

            if (mod10 === 1 && mod100 !== 11) {
                return `${count} голос`;
            }

            if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) {
                return `${count} голоса`;
            }

            return `${count} голосов`;
        };

        const clamp = (value, min, max) => Math.min(Math.max(value, min), max);

        const normalizeViewerSympathyBlock = (settings) => {
            const dd = settings?.dynamicDisplay;
            const ddd = defaultOverlaySettings.viewerSympathyBlock.dynamicDisplay;
            const intervalSeconds = clamp(Number(dd?.intervalSeconds ?? ddd.intervalSeconds) || 30, 1, 3600);
            const visibleDurationSeconds = Math.min(
                clamp(Number(dd?.visibleDurationSeconds ?? ddd.visibleDurationSeconds) || 8, 1, 3600),
                intervalSeconds
            );

            return {
                horizontalOffset: clamp(Number(settings?.horizontalOffset ?? defaultOverlaySettings.viewerSympathyBlock.horizontalOffset) || 0, -4000, 4000),
                verticalOffset: clamp(Number(settings?.verticalOffset ?? defaultOverlaySettings.viewerSympathyBlock.verticalOffset) || 0, -4000, 4000),
                scale: clamp(Number(settings?.scale ?? defaultOverlaySettings.viewerSympathyBlock.scale) || 10, 1, 10),
                dynamicDisplay: {
                    enabled: typeof dd?.enabled === 'boolean' ? dd.enabled : ddd.enabled,
                    intervalSeconds,
                    visibleDurationSeconds,
                    animationDurationMs: clamp(Number(dd?.animationDurationMs ?? ddd.animationDurationMs) || 420, 50, 5000),
                },
            };
        };

        const getDataUrl = () => {
            const url = new URL(window.location.href);
            url.pathname = `${window.location.pathname.replace(/\/$/, '')}/data`;
            url.searchParams.set('_', Date.now().toString());
            return url.toString();
        };

        const applyOverlaySettings = (payload) => {
            const block = normalizeViewerSympathyBlock(payload?.overlaySettings?.viewerSympathyBlock);
            rootStyles.style.setProperty('--offset-x', `${block.horizontalOffset}px`);
            rootStyles.style.setProperty('--offset-y', `${block.verticalOffset}px`);
            rootStyles.style.setProperty('--overlay-scale', `${block.scale / 10}`);
            return block;
        };

        const setSympathy = (redSide, blackSide, totalPredictions) => {
            const redValue = clamp(Number(redSide?.percent || 0), 0, 100);
            const blackValue = clamp(Number(blackSide?.percent || 0), 0, 100);

            townPercent.textContent = formatPercent(redValue);
            mafiaPercent.textContent = formatPercent(blackValue);
            townFill.style.width = `${redValue}%`;
            mafiaFill.style.width = `${blackValue}%`;
            sympathyMeta.textContent = formatPredictionsCount(totalPredictions) + ' через';
        };

        const setPollData = (seatVotes, matchState) => {
            const votes = (seatVotes || [])
                .filter(v => !v.isResolved && v.count > 0)
                .sort((a, b) => b.count - a.count)
                .slice(0, 3);

            const totalVotes = votes.reduce((sum, v) => sum + v.count, 0);

            pollItems.innerHTML = '';
            votes.forEach(v => {
                const item = document.createElement('div');
                item.className = 'poll-item';

                const pct = document.createElement('div');
                pct.className = 'poll-item__pct';
                pct.textContent = `${Math.round(v.percent)}%`;

                const name = document.createElement('span');
                name.className = 'poll-item__name';
                name.textContent = `Игрок ${v.slot}`;

                item.appendChild(pct);
                item.appendChild(name);
                pollItems.appendChild(item);
            });

            pollBadge.textContent = matchState === 'Resolved' ? 'ИТОГ' : 'LIVE-ОПРОС';
            pollMeta.textContent = formatVotesCount(totalVotes) + ' через';
        };

        /* ── alternating cycle controller ── */
        const dynamicDisplayAnimationMs = 420;
        const blocks = [sympathyWrap, pollWrap];
        let currentBlockIndex = 0;

        const cycleCtrl = {
            signature: '',
            cycleTimerId: 0,
            hideTimerId: 0,
            finalizeTimerId: 0,
            animFrameId: 0,
            token: 0,
            active: false,
        };

        const clearCycleCtrl = () => {
            if (cycleCtrl.cycleTimerId) { window.clearTimeout(cycleCtrl.cycleTimerId); cycleCtrl.cycleTimerId = 0; }
            if (cycleCtrl.hideTimerId) { window.clearTimeout(cycleCtrl.hideTimerId); cycleCtrl.hideTimerId = 0; }
            if (cycleCtrl.finalizeTimerId) { window.clearTimeout(cycleCtrl.finalizeTimerId); cycleCtrl.finalizeTimerId = 0; }
            if (cycleCtrl.animFrameId) { window.cancelAnimationFrame(cycleCtrl.animFrameId); cycleCtrl.animFrameId = 0; }
            cycleCtrl.token += 1;
            cycleCtrl.active = false;
        };

        const resetAllBlocks = () => {
            blocks.forEach(el => {
                el.classList.remove('is-dynamic-managed', 'is-dynamic-hidden-top');
            });
        };

        const showBlock = (element, animMs, token) => {
            element.style.setProperty('--animation-duration', `${animMs}ms`);
            element.style.display = '';
            element.classList.add('is-dynamic-managed');
            element.classList.add('is-dynamic-hidden-top');
            void element.offsetWidth;

            cycleCtrl.animFrameId = window.requestAnimationFrame(() => {
                if (!cycleCtrl.active || cycleCtrl.token !== token) return;
                element.classList.remove('is-dynamic-hidden-top');
            });
        };

        const hideBlock = (element) => {
            element.classList.add('is-dynamic-hidden-top');
        };

        const runAlternatingCycle = (dynamicDisplay, token) => {
            if (!cycleCtrl.active || cycleCtrl.token !== token) return;

            const animMs = dynamicDisplay.animationDurationMs || dynamicDisplayAnimationMs;
            const currentEl = blocks[currentBlockIndex];

            showBlock(currentEl, animMs, token);

            cycleCtrl.hideTimerId = window.setTimeout(() => {
                if (!cycleCtrl.active || cycleCtrl.token !== token) return;
                hideBlock(currentEl);

                cycleCtrl.finalizeTimerId = window.setTimeout(() => {
                    if (!cycleCtrl.active || cycleCtrl.token !== token) return;
                    currentEl.style.display = 'none';
                    currentBlockIndex = (currentBlockIndex + 1) % blocks.length;

                    cycleCtrl.cycleTimerId = window.setTimeout(
                        () => runAlternatingCycle(dynamicDisplay, token),
                        dynamicDisplay.intervalSeconds * 1000
                    );
                }, animMs);
            }, dynamicDisplay.visibleDurationSeconds * 1000);
        };

        const syncDynamicDisplay = (dynamicDisplay, shouldBeVisible) => {
            const signature = JSON.stringify({ shouldBeVisible, dynamicDisplay });
            if (cycleCtrl.signature === signature) return;

            clearCycleCtrl();
            cycleCtrl.signature = signature;
            resetAllBlocks();

            if (!shouldBeVisible) {
                blocks.forEach(el => { el.style.display = 'none'; });
                return;
            }

            if (!dynamicDisplay.enabled) {
                sympathyWrap.style.display = '';
                pollWrap.style.display = 'none';
                return;
            }

            currentBlockIndex = 0;
            cycleCtrl.active = true;
            runAlternatingCycle(dynamicDisplay, cycleCtrl.token);
        };

        const setPendingState = () => {
            sympathyWrap.classList.remove('is-finished');
            pollWrap.classList.remove('is-finished');
            sympathyBadge.textContent = 'LIVE';
            setSympathy(null, null, 0);
            setPollData(null, null);
        };

        const renderPayload = (payload) => {
            const block = applyOverlaySettings(payload);

            if (payload?.status === 'no-match') {
                overlayRoot.classList.add('is-hidden');
                syncDynamicDisplay(block.dynamicDisplay, false);
                return;
            }

            overlayRoot.classList.remove('is-hidden');

            if (payload?.status !== 'ready') {
                setPendingState();
                syncDynamicDisplay(block.dynamicDisplay, true);
                return;
            }

            const isFinished = payload?.matchState === 'Resolved';
            sympathyWrap.classList.toggle('is-finished', isFinished);
            pollWrap.classList.toggle('is-finished', isFinished);
            sympathyBadge.textContent = isFinished ? 'ИТОГ' : 'LIVE';
            setSympathy(payload?.redSide, payload?.blackSide, payload?.totalPredictions);
            setPollData(payload?.seatVotes, payload?.matchState);
            syncDynamicDisplay(block.dynamicDisplay, true);
        };

        const applyDisconnectedState = () => {
            applyOverlaySettings(null);
            overlayRoot.classList.add('is-hidden');
            syncDynamicDisplay(defaultOverlaySettings.viewerSympathyBlock.dynamicDisplay, false);
        };

        const refreshOverlay = async () => {
            try {
                const response = await fetch(getDataUrl(), { cache: 'no-store' });
                if (!response.ok) {
                    applyDisconnectedState();
                    return;
                }

                const payload = await response.json();
                renderPayload(payload);
            } catch (error) {
                applyDisconnectedState();
            }
        };

        refreshOverlay();
        window.setInterval(refreshOverlay, 3000);
    </script>
</body>
</html>
""";
    }
}