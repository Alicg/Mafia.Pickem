namespace MafiaPickem.Api.Overlay;

public static class ObsOverlayHtmlRenderer
{
    public static string Render(int tournamentId)
    {
        return $$"""
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Mafia Pickem Overlay</title>
    <style>
        :root {
            --panel-width: 232px;
            --overlay-panel-fill: linear-gradient(180deg, rgba(22, 58, 97, 0.92), rgba(11, 31, 58, 0.86));
            --card-highlight: rgba(255, 243, 224, 0.09);
            --card-edge: rgba(247, 224, 193, 0.18);
            --red-bar: linear-gradient(180deg, #d9342b 0%, #a91515 100%);
            --black-bar: linear-gradient(180deg, #5a6370 0%, #232931 100%);
            --text-main: rgba(251, 252, 255, 0.98);
            --text-soft: rgba(236, 236, 230, 0.92);
            --text-faint: rgba(244, 248, 252, 0.92);
            --accent: #f1d8bf;
            --accent-strong: rgba(246, 223, 188, 0.92);
            --accent-warm: #fff1dd;
            --track: rgba(209, 181, 153, 0.34);
            --track-soft: rgba(14, 25, 41, 0.44);
            --shadow: 0 18px 42px rgba(4, 11, 20, 0.34);
            --badge-bg: rgba(245, 222, 192, 0.16);
            --badge-edge: rgba(255, 242, 224, 0.24);
            --badge-text: rgba(255, 246, 238, 0.98);
            --footer-edge: rgba(255, 245, 230, 0.16);
            --footer-icon: #38bdf8;
            --first-track: linear-gradient(90deg, rgba(44, 97, 155, 0.32), rgba(28, 70, 121, 0.42));
            --first-track-edge: rgba(194, 223, 255, 0.18);
            --first-fill: linear-gradient(90deg, rgba(96, 177, 232, 0.96), rgba(40, 89, 157, 0.9));
            --first-slot: rgba(244, 248, 255, 0.98);
            --black-track: linear-gradient(90deg, rgba(124, 136, 152, 0.42), rgba(84, 93, 106, 0.5));
            --black-track-edge: rgba(255, 255, 255, 0.28);
            --red-track: rgba(160, 24, 24, 0.28);
            --red-track-edge: rgba(244, 206, 206, 0.18);
            --resolved-fill: linear-gradient(90deg, rgba(102, 199, 112, 0.94), rgba(53, 153, 72, 0.9));
            --resolved-edge: rgba(165, 230, 173, 0.52);
            --resolved-text: rgba(227, 250, 230, 0.98);
            --count-text: rgba(255, 251, 245, 0.96);
            --white-frame: rgba(255, 255, 255, 0.32);
            font-family: "Bahnschrift", "Segoe UI Variable Display", "Trebuchet MS", sans-serif;
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
            color: var(--text-main);
        }

        body {
            position: relative;
        }

        .overlay-root {
            position: relative;
            width: 100vw;
            height: 100vh;
            pointer-events: none;
        }

        .overlay-panel {
            position: relative;
            width: 100%;
            height: 100%;
        }

        .overlay-stack-panel {
            position: absolute;
            display: flex;
            flex-direction: column;
            gap: 6px;
            width: var(--panel-width);
            max-width: 100%;
        }

        .overlay-stack-panel.is-left {
            left: 15px;
        }

        .overlay-stack-panel.is-right {
            right: 15px;
        }

        .overlay-block {
            position: relative;
            width: 100%;
        }

        .overlay-block.is-dynamic-managed {
            transition: transform 420ms cubic-bezier(0.22, 1, 0.36, 1), opacity 320ms ease;
            will-change: transform, opacity;
        }

        .overlay-block.is-dynamic-hidden-left {
            transform: translateX(calc(-100% - 24px));
            opacity: 0;
        }

        .overlay-block.is-dynamic-hidden-right {
            transform: translateX(calc(100% + 24px));
            opacity: 0;
        }

        #summaryBlock,
        #lastRoundBlock,
        #footerBlock {
            width: min(182px, 100%);
        }

        .overlay-footer__link {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 7px;
            width: 100%;
            min-height: 24px;
            padding: 0 10px;
            border-radius: 999px;
            background: var(--overlay-panel-fill);
            border: 1px solid var(--footer-edge);
            color: rgba(255, 255, 255, 0.96);
            font-size: 17px;
            font-weight: 800;
            letter-spacing: 0.04em;
            text-decoration: none;
            box-shadow: 0 10px 24px rgba(2, 6, 23, 0.24);
            text-shadow: 0 1px 10px rgba(2, 6, 23, 0.4);
            pointer-events: auto;
            max-width: 100%;
        }

        .overlay-footer__icon {
            display: inline-flex;
            width: 20px;
            height: 20px;
            flex: 0 0 20px;
            color: var(--footer-icon);
            filter: drop-shadow(0 0 8px rgba(255, 243, 224, 0.22));
        }

        .overlay-footer__icon svg {
            width: 100%;
            height: 100%;
            display: block;
        }

        .summary-card,
        .chart-card {
            position: relative;
            overflow: hidden;
            border-radius: 16px;
            background:
                radial-gradient(circle at top right, rgba(255, 255, 255, 0.16), transparent 34%),
                linear-gradient(135deg, var(--card-highlight), transparent 45%),
                var(--overlay-panel-fill);
            box-shadow: var(--shadow);
            border: 1px solid var(--card-edge);
        }

        .summary-card {
            padding: 10px 12px 11px;
        }

        .summary-top {
            display: flex;
            align-items: center;
            margin-bottom: 8px;
        }

        .state-pill {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-height: 20px;
            padding: 0 8px;
            border-radius: 999px;
            background: var(--badge-bg);
            color: var(--badge-text);
            font-size: 16px;
            font-weight: 800;
            letter-spacing: 0.14em;
            text-transform: uppercase;
            box-shadow: inset 0 0 0 1px var(--badge-edge);
        }

        .summary-main {
            display: flex;
            flex-direction: column;
            gap: 8px;
        }

        .summary-flow {
            display: flex;
            flex-direction: column;
            gap: 6px;
        }

        .summary-side {
            display: flex;
            align-items: baseline;
            justify-content: space-between;
            gap: 6px;
        }

        .summary-side__percent {
            font-size: 21px;
            line-height: 1;
            font-weight: 900;
            letter-spacing: -0.06em;
            color: var(--accent-warm);
            text-shadow: 0 2px 14px rgba(15, 23, 42, 0.32);
            white-space: nowrap;
        }

        .summary-side__label {
            font-size: 12px;
            font-weight: 700;
            letter-spacing: 0.08em;
            text-transform: uppercase;
            color: var(--text-soft);
            white-space: nowrap;
        }

        .summary-bar {
            display: flex;
            width: 100%;
            height: 12px;
            border-radius: 999px;
            background: linear-gradient(90deg, var(--track-soft), var(--track));
            overflow: hidden;
            align-items: stretch;
            box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.48), 0 0 0 1px rgba(255, 255, 255, 0.1);
        }

        .summary-bar__segment {
            width: var(--segment-width, 0%);
            height: 100%;
            border-radius: inherit;
        }

        .summary-bar__segment.is-left {
            background: var(--red-bar);
            box-shadow: inset 0 0 0 1px var(--white-frame), 0 0 16px rgba(125, 20, 20, 0.28);
        }

        .summary-bar__segment.is-right {
            background: var(--black-bar);
            box-shadow: inset 0 0 0 1px var(--white-frame), 0 0 16px rgba(38, 42, 49, 0.24);
        }

        .summary-side__count {
            font-size: 12px;
            font-weight: 800;
            line-height: 1;
            color: var(--text-soft);
        }

        .chart-card {
            padding: 10px 12px 11px;
        }

        .chart-title {
            margin-bottom: 7px;
            font-size: 12px;
            font-weight: 800;
            letter-spacing: 0.16em;
            text-transform: uppercase;
            color: var(--text-faint);  
            text-align: center;
        }

        .vote-chart {
            display: flex;
            flex-direction: column;
            gap: 4px;
        }

        .vote-bar {
            display: grid;
            grid-template-columns: 14px minmax(0, 1fr) auto;
            gap: 6px;
            align-items: center;
        }

        .vote-bar.is-last-round {
            grid-template-columns: 64px minmax(0, 1fr) auto;
            gap: 6px;
        }

        .vote-bar__slot {
            font-size: 17px;
            font-weight: 800;
            line-height: 1;
            text-align: center;
            color: var(--text-main);
            text-shadow: 0 1px 10px rgba(2, 6, 23, 0.3);
        }

        .vote-bar__slot.is-last-round {
            display: inline-flex;
            align-items: center;
            min-height: 22px;
            padding: 0 8px;
            border-radius: 999px;
            font-size: 16px;
            font-weight: 900;
            letter-spacing: 0.02em;
            text-align: left;
            box-shadow: inset 0 0 0 1px var(--white-frame);
        }

        .vote-bar__track {
            position: relative;
            width: 100%;
            min-width: 0;
            height: 12px;
            border-radius: 999px;
            background: linear-gradient(90deg, var(--track-soft), var(--track));
            overflow: hidden;
            box-shadow: none;
        }

        .vote-bar__fill {
            width: var(--bar-width, 0%);
            height: 100%;
            border-radius: inherit;
            background: var(--red-bar);
            box-shadow: inset 0 0 0 1px var(--white-frame), 0 0 14px rgba(125, 20, 20, 0.24);
        }

        .vote-bar.is-first-vote .vote-bar__track {
            background: var(--first-track);
            box-shadow: none;
        }

        .vote-bar.is-first-vote .vote-bar__fill {
            background: var(--first-fill);
            box-shadow: inset 0 0 0 1px var(--white-frame), 0 0 14px rgba(46, 101, 168, 0.2);
        }

        .vote-bar.is-first-vote .vote-bar__slot {
            color: var(--first-slot);
        }

        .vote-bar.is-last-round.is-black-style .vote-bar__slot {
            color: rgba(255, 255, 255, 0.96);
            background: var(--black-bar);
            box-shadow: inset 0 0 0 1px var(--white-frame), 0 0 10px rgba(0, 0, 0, 0.18);
        }

        .vote-bar.is-last-round.is-black-style .vote-bar__track {
            background: var(--black-track);
            box-shadow: none;
        }

        .vote-bar.is-last-round.is-black-style .vote-bar__fill {
            background: var(--black-bar);
            box-shadow: inset 0 0 0 1px var(--white-frame), 0 0 14px rgba(38, 42, 49, 0.22);
        }

        .vote-bar.is-last-round.is-red-style .vote-bar__slot {
            color: rgba(255, 248, 248, 0.98);
            background: var(--red-bar);
            box-shadow: inset 0 0 0 1px var(--white-frame), 0 0 10px rgba(160, 24, 24, 0.2);
        }

        .vote-bar.is-last-round.is-red-style .vote-bar__track {
            background: var(--red-track);
            box-shadow: none;
        }

        .vote-bar.is-last-round.is-red-style .vote-bar__fill {
            background: var(--red-bar);
            box-shadow: inset 0 0 0 1px var(--white-frame), 0 0 14px rgba(125, 20, 20, 0.24);
        }

        .vote-bar.is-resolved .vote-bar__fill {
            background: var(--resolved-fill);
            box-shadow: inset 0 0 0 1px var(--white-frame), 0 0 14px rgba(69, 162, 89, 0.28);
        }

        .vote-bar.is-resolved .vote-bar__slot {
            color: var(--resolved-text);
            text-shadow: 0 0 10px rgba(74, 222, 128, 0.35);
        }

        .vote-bar.is-resolved .vote-bar__track {
            box-shadow: 0 0 14px rgba(74, 222, 128, 0.14);
        }

        .vote-bar__count {
            font-size: 15px;
            font-weight: 800;
            line-height: 1;
            min-width: 2ch;
            text-align: right;
            color: var(--count-text);
            text-shadow: 0 1px 10px rgba(2, 6, 23, 0.42);
        }

        .vote-bar.is-empty .vote-bar__count {
            color: var(--text-faint);
        }

        @media (max-width: 900px) {
            .overlay-stack-panel {
                width: min(var(--panel-width), calc(100vw - 24px));
            }
        }
    </style>
</head>
<body>
    <div class="overlay-root">
        <section class="overlay-panel" id="overlayPanel">
            <div class="overlay-stack-panel is-left" id="leftStackPanel">
                <div class="overlay-block" id="summaryBlock">
                    <div class="summary-card" id="summaryCard">
                        <div class="summary-top">
                            <div class="state-pill" id="matchState">Ожидание</div>
                        </div>

                        <div class="summary-main">
                            <div class="summary-flow">
                                <div class="summary-side">
                                    <div class="summary-side__label">за красных</div>
                                    <div class="summary-side__percent" id="redPercent">-</div>
                                </div>

                                <div class="summary-bar" aria-hidden="true">
                                    <div class="summary-bar__segment is-left" id="redBarFill"></div>
                                </div>

                                <div class="summary-bar" aria-hidden="true">
                                    <div class="summary-bar__segment is-right" id="blackBarFill"></div>
                                </div>

                                <div class="summary-side">
                                    <div class="summary-side__label">за черных</div>
                                    <div class="summary-side__percent" id="blackPercent">-</div>
                                </div>
                            </div>

                            <div class="summary-side__count" id="totalPredictionsCount">0 прогнозов</div>
                        </div>
                    </div>
                </div>

                <div class="overlay-block" id="lastRoundBlock">
                    <div class="chart-card" id="lastRoundCard">
                        <div class="chart-title">Последний круг</div>
                        <div class="vote-chart" id="lastRoundChart"></div>
                    </div>
                </div>

                <div class="overlay-block" id="footerBlock">
                    <a class="overlay-footer__link" href="https://t.me/MafiaPickemBot" target="_blank" rel="noreferrer noopener">
                        <span class="overlay-footer__icon" aria-hidden="true">
                            <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                <circle cx="12" cy="12" r="12" fill="currentColor" fill-opacity="0.18" />
                                <path d="M17.94 6.62L5.98 11.23C5.17 11.56 5.18 12.01 5.83 12.21L8.9 13.17L15.99 8.69C16.33 8.48 16.64 8.59 16.39 8.81L10.65 13.99L10.43 17.12C10.75 17.12 10.89 16.97 11.07 16.79L12.56 15.34L15.66 17.63C16.23 17.94 16.64 17.78 16.78 17.12L18.81 7.54C19.02 6.73 18.49 6.36 17.94 6.62Z" fill="currentColor" />
                            </svg>
                        </span>
                        <span>@MafiaPickemBot</span>
                    </a>
                </div>
            </div>

            <div class="overlay-stack-panel is-right" id="rightStackPanel">
                <div class="overlay-block" id="voteChartBlock">
                    <div class="chart-card" id="voteChartCard">
                        <div class="chart-title">Заголосуют первым</div>
                        <div class="vote-chart" id="voteChart"></div>
                    </div>
                </div>
            </div>
        </section>
    </div>

    <script>
        const tournamentId = {{tournamentId}};
        const redPercent = document.getElementById('redPercent');
        const blackPercent = document.getElementById('blackPercent');
        const redBarFill = document.getElementById('redBarFill');
        const blackBarFill = document.getElementById('blackBarFill');
        const totalPredictionsCount = document.getElementById('totalPredictionsCount');
        const matchState = document.getElementById('matchState');
        const voteChart = document.getElementById('voteChart');
        const lastRoundChart = document.getElementById('lastRoundChart');
        const voteChartCard = document.getElementById('voteChartCard');
        const lastRoundCard = document.getElementById('lastRoundCard');
        const leftStackPanel = document.getElementById('leftStackPanel');
        const rightStackPanel = document.getElementById('rightStackPanel');
        const summaryBlock = document.getElementById('summaryBlock');
        const voteChartBlock = document.getElementById('voteChartBlock');
        const lastRoundBlock = document.getElementById('lastRoundBlock');
        const footerBlock = document.getElementById('footerBlock');
        const overlayPanel = document.getElementById('overlayPanel');
        const rootStyles = document.documentElement;
        const dynamicDisplayControllers = new Map();
        const dynamicDisplayAnimationMs = 420;

        const defaultOverlaySettings = {
            hideBlocksByPhase: true,
            theme: {
                fillColorStart: '#163A61',
                fillColorEnd: '#0B1F3A',
                fillOpacity: 92,
                useGradient: true,
            },
            leftPanel: { edgeOffset: 15, topOffset: 138 },
            rightPanel: { edgeOffset: 15, topOffset: 394 },
            summaryBlock: { panel: 'left', isVisible: true, dynamicDisplay: { enabled: false, intervalSeconds: 30, visibleDurationSeconds: 8 } },
            firstVoteBlock: { panel: 'right', isVisible: true, dynamicDisplay: { enabled: false, intervalSeconds: 30, visibleDurationSeconds: 8 } },
            lastRoundBlock: { panel: 'left', isVisible: true, dynamicDisplay: { enabled: false, intervalSeconds: 30, visibleDurationSeconds: 8 } },
            footerBlock: { panel: 'left', isVisible: true, dynamicDisplay: { enabled: false, intervalSeconds: 30, visibleDurationSeconds: 8 } },
        };

        const formatPercent = (value) => `${Math.round(Number(value || 0))}%`;
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

        const getDataUrl = () => {
            const url = new URL(window.location.href);
            url.pathname = `${window.location.pathname.replace(/\/$/, '')}/data`;
            url.searchParams.set('_', Date.now().toString());
            return url.toString();
        };

        const clamp = (value, min, max) => Math.min(Math.max(value, min), max);

        const normalizeColor = (value, fallback) => {
            if (typeof value !== 'string') {
                return fallback;
            }

            const trimmed = value.trim();
            return /^#[0-9a-fA-F]{6}$/.test(trimmed) ? trimmed.toUpperCase() : fallback;
        };

        const normalizePanelLayout = (layout, fallback) => {
            const edgeOffset = clamp(Number(layout?.edgeOffset ?? fallback.edgeOffset) || 0, 0, 4000);
            const topOffset = clamp(Number(layout?.topOffset ?? fallback.topOffset) || 0, 0, 4000);

            return {
                edgeOffset,
                topOffset,
            };
        };

        const normalizeDynamicDisplay = (dynamicDisplay, fallback) => {
            const intervalSeconds = clamp(Number(dynamicDisplay?.intervalSeconds ?? fallback.intervalSeconds) || 0, 1, 3600);
            const visibleDurationSeconds = clamp(Number(dynamicDisplay?.visibleDurationSeconds ?? fallback.visibleDurationSeconds) || 0, 1, 3600);

            return {
                enabled: typeof dynamicDisplay?.enabled === 'boolean'
                    ? dynamicDisplay.enabled
                    : fallback.enabled,
                intervalSeconds,
                visibleDurationSeconds: Math.min(visibleDurationSeconds, intervalSeconds),
            };
        };

        const normalizeBlockPlacement = (placement, fallback) => ({
            panel: placement?.panel === 'right'
                ? 'right'
                : placement?.panel === 'left'
                    ? 'left'
                    : fallback.panel,
            isVisible: typeof placement?.isVisible === 'boolean'
                ? placement.isVisible
                : fallback.isVisible,
            dynamicDisplay: normalizeDynamicDisplay(placement?.dynamicDisplay, fallback.dynamicDisplay),
        });

        const normalizeOverlaySettings = (settings) => ({
            hideBlocksByPhase: typeof settings?.hideBlocksByPhase === 'boolean'
                ? settings.hideBlocksByPhase
                : defaultOverlaySettings.hideBlocksByPhase,
            theme: {
                fillColorStart: normalizeColor(settings?.theme?.fillColorStart, defaultOverlaySettings.theme.fillColorStart),
                fillColorEnd: normalizeColor(settings?.theme?.fillColorEnd, defaultOverlaySettings.theme.fillColorEnd),
                fillOpacity: clamp(Number(settings?.theme?.fillOpacity ?? defaultOverlaySettings.theme.fillOpacity) || 0, 0, 100),
                useGradient: typeof settings?.theme?.useGradient === 'boolean'
                    ? settings.theme.useGradient
                    : defaultOverlaySettings.theme.useGradient,
            },
            leftPanel: normalizePanelLayout(settings?.leftPanel, defaultOverlaySettings.leftPanel),
            rightPanel: normalizePanelLayout(settings?.rightPanel, defaultOverlaySettings.rightPanel),
            summaryBlock: normalizeBlockPlacement(settings?.summaryBlock, defaultOverlaySettings.summaryBlock),
            firstVoteBlock: normalizeBlockPlacement(settings?.firstVoteBlock, defaultOverlaySettings.firstVoteBlock),
            lastRoundBlock: normalizeBlockPlacement(settings?.lastRoundBlock, defaultOverlaySettings.lastRoundBlock),
            footerBlock: normalizeBlockPlacement(settings?.footerBlock, defaultOverlaySettings.footerBlock),
        });

        const hexToRgb = (value) => {
            const normalized = normalizeColor(value, '#000000');
            return [1, 3, 5]
                .map((start) => Number.parseInt(normalized.slice(start, start + 2), 16))
                .join(', ');
        };

        const buildPanelFill = (theme) => {
            const opacity = clamp(theme.fillOpacity, 0, 100) / 100;
            const start = `rgba(${hexToRgb(theme.fillColorStart)}, ${opacity})`;
            const end = `rgba(${hexToRgb(theme.fillColorEnd)}, ${opacity})`;
            return theme.useGradient ? `linear-gradient(180deg, ${start}, ${end})` : start;
        };

        const applyPanelLayout = (element, side, layout) => {
            if (!element) {
                return;
            }

            element.classList.toggle('is-left', side === 'left');
            element.classList.toggle('is-right', side === 'right');
            element.style.top = `${layout.topOffset}px`;
            element.style.left = side === 'left' ? `${layout.edgeOffset}px` : 'auto';
            element.style.right = side === 'right' ? `${layout.edgeOffset}px` : 'auto';
        };

        const arrangeBlocksIntoPanels = (settings) => {
            const blocks = [
                { element: summaryBlock, placement: settings.summaryBlock },
                { element: voteChartBlock, placement: settings.firstVoteBlock },
                { element: lastRoundBlock, placement: settings.lastRoundBlock },
                { element: footerBlock, placement: settings.footerBlock },
            ];

            blocks.forEach(({ element, placement }) => {
                if (!element) {
                    return;
                }

                const targetPanel = placement.panel === 'right' ? rightStackPanel : leftStackPanel;
                if (targetPanel && element.parentElement !== targetPanel) {
                    targetPanel.appendChild(element);
                }
            });
        };

        const getDynamicDisplayController = (blockKey) => {
            let controller = dynamicDisplayControllers.get(blockKey);
            if (!controller) {
                controller = {
                    signature: '',
                    cycleTimerId: 0,
                    hideTimerId: 0,
                    finalizeHideTimerId: 0,
                    animationFrameId: 0,
                    token: 0,
                    active: false,
                };
                dynamicDisplayControllers.set(blockKey, controller);
            }

            return controller;
        };

        const clearDynamicDisplayController = (controller) => {
            if (controller.cycleTimerId) {
                window.clearTimeout(controller.cycleTimerId);
                controller.cycleTimerId = 0;
            }

            if (controller.hideTimerId) {
                window.clearTimeout(controller.hideTimerId);
                controller.hideTimerId = 0;
            }

            if (controller.finalizeHideTimerId) {
                window.clearTimeout(controller.finalizeHideTimerId);
                controller.finalizeHideTimerId = 0;
            }

            if (controller.animationFrameId) {
                window.cancelAnimationFrame(controller.animationFrameId);
                controller.animationFrameId = 0;
            }

            controller.token += 1;
            controller.active = false;
        };

        const resetDynamicDisplayClasses = (element) => {
            if (!element) {
                return;
            }

            element.classList.remove('is-dynamic-managed', 'is-dynamic-hidden-left', 'is-dynamic-hidden-right');
        };

        const getHiddenDirectionClass = (panel) => panel === 'right' ? 'is-dynamic-hidden-right' : 'is-dynamic-hidden-left';

        const runDynamicDisplayCycle = (controller, element, panel, dynamicDisplay, token) => {
            if (!controller.active || controller.token !== token || !element) {
                return;
            }

            const hiddenClass = getHiddenDirectionClass(panel);
            const oppositeHiddenClass = hiddenClass === 'is-dynamic-hidden-right' ? 'is-dynamic-hidden-left' : 'is-dynamic-hidden-right';

            element.style.display = '';
            element.classList.add('is-dynamic-managed');
            element.classList.remove(oppositeHiddenClass);
            element.classList.add(hiddenClass);
            void element.offsetWidth;

            controller.animationFrameId = window.requestAnimationFrame(() => {
                if (!controller.active || controller.token !== token) {
                    return;
                }

                element.classList.remove(hiddenClass);
            });

            controller.hideTimerId = window.setTimeout(() => {
                if (!controller.active || controller.token !== token) {
                    return;
                }

                element.classList.add(hiddenClass);
                controller.finalizeHideTimerId = window.setTimeout(() => {
                    if (!controller.active || controller.token !== token) {
                        return;
                    }

                    element.style.display = 'none';
                    controller.cycleTimerId = window.setTimeout(
                        () => runDynamicDisplayCycle(controller, element, panel, dynamicDisplay, token),
                        dynamicDisplay.intervalSeconds * 1000
                    );
                }, dynamicDisplayAnimationMs);
            }, dynamicDisplay.visibleDurationSeconds * 1000);
        };

        const syncBlockDisplay = (blockKey, element, placement, shouldBeVisible) => {
            if (!element) {
                return;
            }

            const controller = getDynamicDisplayController(blockKey);
            const signature = JSON.stringify({
                panel: placement.panel,
                shouldBeVisible,
                dynamicDisplay: placement.dynamicDisplay,
            });

            if (controller.signature === signature) {
                return;
            }

            clearDynamicDisplayController(controller);
            controller.signature = signature;
            resetDynamicDisplayClasses(element);

            if (!shouldBeVisible) {
                element.style.display = 'none';
                return;
            }

            if (!placement.dynamicDisplay.enabled) {
                element.style.display = '';
                return;
            }

            controller.active = true;
            const token = controller.token;
            runDynamicDisplayCycle(controller, element, placement.panel, placement.dynamicDisplay, token);
        };

        const getBlockVisibilityState = (payload, settings) => {
            const currentMatchState = typeof payload?.matchState === 'string' ? payload.matchState : '';
            const hasFirstVotedResult = currentMatchState === 'FirstVoted' || currentMatchState === 'Resolved';

            return {
                summaryBlock: settings.summaryBlock.isVisible,
                firstVoteBlock: settings.firstVoteBlock.isVisible && (!settings.hideBlocksByPhase || !hasFirstVotedResult),
                lastRoundBlock: settings.lastRoundBlock.isVisible && (!settings.hideBlocksByPhase || hasFirstVotedResult),
                footerBlock: settings.footerBlock.isVisible,
            };
        };

        const syncOverlayBlocks = (payload, settings) => {
            const visibilityState = getBlockVisibilityState(payload, settings);
            syncBlockDisplay('summaryBlock', summaryBlock, settings.summaryBlock, visibilityState.summaryBlock);
            syncBlockDisplay('firstVoteBlock', voteChartBlock, settings.firstVoteBlock, visibilityState.firstVoteBlock);
            syncBlockDisplay('lastRoundBlock', lastRoundBlock, settings.lastRoundBlock, visibilityState.lastRoundBlock);
            syncBlockDisplay('footerBlock', footerBlock, settings.footerBlock, visibilityState.footerBlock);
        };

        const applyOverlaySettings = (payload) => {
            const settings = normalizeOverlaySettings(payload?.overlaySettings);
            rootStyles.style.setProperty('--overlay-panel-fill', buildPanelFill(settings.theme));
            applyPanelLayout(leftStackPanel, 'left', settings.leftPanel);
            applyPanelLayout(rightStackPanel, 'right', settings.rightPanel);
            arrangeBlocksIntoPanels(settings);
            return settings;
        };

        const setSummary = (redSide, blackSide, totalPredictions) => {
            const redValue = Number(redSide?.percent || 0);
            const blackValue = Number(blackSide?.percent || 0);

            redPercent.textContent = formatPercent(redValue);
            blackPercent.textContent = formatPercent(blackValue);
            redBarFill.style.setProperty('--segment-width', `${Math.max(0, Math.min(redValue, 100))}%`);
            blackBarFill.style.setProperty('--segment-width', `${Math.max(0, Math.min(blackValue, 100))}%`);
            totalPredictionsCount.textContent = formatPredictionsCount(totalPredictions);
        };

        const setPanelVisible = (isVisible) => {
            overlayPanel.style.display = isVisible ? '' : 'none';
        };

        const renderSeats = (payload) => {
            const seatVotes = Array.isArray(payload.seatVotes) ? payload.seatVotes : [];
            const maxCount = Math.max(...seatVotes.map((seat) => Number(seat.count || 0)), 1);

            voteChart.innerHTML = '';

            seatVotes.forEach((seat) => {
                const count = Number(seat.count || 0);
                const barWidth = count > 0 ? Math.max((count / maxCount) * 100, 10) : 0;
                const fillStyle = count > 0
                    ? `style="--bar-width:${barWidth}%"`
                    : 'style="--bar-width:0%"';
                const item = document.createElement('div');
                item.className = `vote-bar is-first-vote${seat.isResolved ? ' is-resolved' : ''}${count === 0 ? ' is-empty' : ''}`;
                item.innerHTML = `
                    <div class="vote-bar__slot">${seat.slot}</div>
                    <div class="vote-bar__track">
                        <div class="vote-bar__fill" ${fillStyle}></div>
                    </div>
                    <div class="vote-bar__count">${count}</div>`;
                voteChart.appendChild(item);
            });
        };

        const renderLastRound = (payload) => {
            const lastRoundVotes = Array.isArray(payload.lastRoundVotes) ? payload.lastRoundVotes : [];
            const resolvedLR = payload.resolvedLastRound || 0;
            const maxCount = Math.max(...lastRoundVotes.map((lr) => Number(lr.count || 0)), 1);

            lastRoundChart.innerHTML = '';

            lastRoundVotes.forEach((lr) => {
                const count = Number(lr.count || 0);
                const barWidth = count > 0 ? Math.max((count / maxCount) * 100, 10) : 0;
                const fillStyle = count > 0
                    ? `style="--bar-width:${barWidth}%"`
                    : 'style="--bar-width:0%"';
                const isResolved = resolvedLR > 0 && lr.lastRound === resolvedLR;
                const styleClass = lr.lastRound >= 3 ? ' is-black-style' : ' is-red-style';
                const item = document.createElement('div');
                item.className = `vote-bar is-last-round${styleClass}${isResolved ? ' is-resolved' : ''}${count === 0 ? ' is-empty' : ''}`;
                item.innerHTML = `
                    <div class="vote-bar__slot is-last-round">${lr.label || lr.lastRound}</div>
                    <div class="vote-bar__track">
                        <div class="vote-bar__fill" ${fillStyle}></div>
                    </div>
                    <div class="vote-bar__count">${count}</div>`;
                lastRoundChart.appendChild(item);
            });
        };

        const applyDisconnectedState = (message) => {
            matchState.textContent = 'Нет связи';
            redPercent.textContent = '-';
            blackPercent.textContent = '-';
            redBarFill.style.setProperty('--segment-width', '0%');
            blackBarFill.style.setProperty('--segment-width', '0%');
            totalPredictionsCount.textContent = formatPredictionsCount(0);
            const settings = applyOverlaySettings(null);
            syncOverlayBlocks({ matchState: '' }, settings);
            renderSeats({ seatVotes: [] });
            renderLastRound({ lastRoundVotes: [] });
        };

        const renderPayload = (payload) => {
            const settings = applyOverlaySettings(payload);

            if (payload.status === 'no-match') {
                setPanelVisible(false);
                return;
            }

            setPanelVisible(true);
            syncOverlayBlocks(payload, settings);

            if (payload.status !== 'ready') {
                matchState.textContent = 'Ожидание';
                redPercent.textContent = '-';
                blackPercent.textContent = '-';
                redBarFill.style.setProperty('--segment-width', '0%');
                blackBarFill.style.setProperty('--segment-width', '0%');
                totalPredictionsCount.textContent = formatPredictionsCount(0);
                renderSeats(payload);
                renderLastRound(payload);
                return;
            }

            const totalPredictions = Number(payload.totalPredictions || 0);

            matchState.textContent = payload.matchState === 'Resolved' ? 'Закрыто' : payload.matchState === 'FirstVoted' ? 'Лайв' : 'Лайв';
            setSummary(
                payload.redSide,
                payload.blackSide,
                totalPredictions
            );
            renderSeats(payload);
            renderLastRound(payload);
        };

        const refreshOverlay = async () => {
            try {
                const response = await fetch(getDataUrl(), { cache: 'no-store' });
                if (!response.ok) {
                    applyDisconnectedState(`Эндпоинт вернул HTTP ${response.status}.`);
                    return;
                }

                const payload = await response.json();
                renderPayload(payload);
            } catch (error) {
                applyDisconnectedState('Не удалось получить данные оверлея.');
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
