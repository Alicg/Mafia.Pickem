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
            --panel-offset-y: -28px;
            --card-bg: rgba(22, 58, 97, 0.92);
            --card-bg-soft: rgba(11, 31, 58, 0.86);
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
            --footer-bg: linear-gradient(180deg, rgba(33, 57, 84, 0.92), rgba(21, 37, 58, 0.9));
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
            display: flex;
            align-items: center;
            justify-content: flex-start;
            padding: 0 0 0 15px;
        }

        .overlay-panel {
            width: var(--panel-width);
            max-width: calc(100vw - 22px);
            transform: translateY(var(--panel-offset-y));
        }

        .overlay-footer__link {
            display: inline-flex;
            align-items: center;
            gap: 7px;
            min-height: 24px;
            padding: 0 10px;
            border-radius: 999px;
            background: var(--footer-bg);
            border: 1px solid var(--footer-edge);
            color: rgba(255, 255, 255, 0.96);
            font-size: 17px;
            font-weight: 800;
            letter-spacing: 0.04em;
            text-decoration: none;
            box-shadow: 0 10px 24px rgba(2, 6, 23, 0.24);
            text-shadow: 0 1px 10px rgba(2, 6, 23, 0.4);
            pointer-events: auto;
            align-self: flex-start;
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

        .stack {
            display: flex;
            flex-direction: column;
            gap: 6px;
        }

        .summary-card,
        .chart-card {
            position: relative;
            overflow: hidden;
            border-radius: 16px;
            background:
                radial-gradient(circle at top right, rgba(255, 255, 255, 0.16), transparent 34%),
                linear-gradient(135deg, var(--card-highlight), transparent 45%),
                linear-gradient(180deg, var(--card-bg), var(--card-bg-soft));
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

        .summary-sides {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 10px;
            align-items: end;
        }

        .summary-side {
            display: flex;
            flex-direction: column;
            gap: 1px;
        }

        .summary-side.is-right {
            align-items: flex-end;
            text-align: right;
        }

        .summary-side__percent {
            font-size: 29px;
            line-height: 0.92;
            font-weight: 900;
            letter-spacing: -0.06em;
            color: var(--accent-warm);
            text-shadow: 0 2px 14px rgba(15, 23, 42, 0.32);
        }

        .summary-side__label {
            font-size: 14px;
            font-weight: 700;
            letter-spacing: 0.08em;
            text-transform: uppercase;
            color: var(--text-soft);
            white-space: nowrap;
        }

        .summary-bar {
            display: flex;
            width: 100%;
            height: 14px;
            border-radius: 999px;
            background: linear-gradient(90deg, var(--track-soft), var(--track));
            overflow: hidden;
            align-items: stretch;
            box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.48), 0 0 0 1px rgba(255, 255, 255, 0.1);
        }

        .summary-bar__segment {
            width: var(--segment-width, 0%);
            height: 100%;
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
            font-size: 19px;
            font-weight: 800;
            line-height: 1;
            color: var(--text-soft);
        }

        .chart-card {
            padding: 10px 12px 11px;
        }

        .chart-title {
            margin-bottom: 7px;
            font-size: 14px;
            font-weight: 800;
            letter-spacing: 0.16em;
            text-transform: uppercase;
            color: var(--text-faint);
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
            grid-template-columns: 76px minmax(0, 1fr) auto;
            gap: 8px;
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
            .overlay-panel {
                max-width: calc(100vw - 22px);
            }
        }
    </style>
</head>
<body>
    <div class="overlay-root">
        <section class="overlay-panel" id="overlayPanel">
            <div class="stack">
                <div class="summary-card">
                    <div class="summary-top">
                        <div class="state-pill" id="matchState">Ожидание</div>
                    </div>

                    <div class="summary-main">
                        <div class="summary-sides">
                            <div class="summary-side">
                                <div class="summary-side__percent" id="redPercent">-</div>
                                <div class="summary-side__label">за красных</div>
                            </div>
                            <div class="summary-side is-right">
                                <div class="summary-side__percent" id="blackPercent">-</div>
                                <div class="summary-side__label">за черных</div>
                            </div>
                        </div>

                        <div class="summary-bar" aria-hidden="true">
                            <div class="summary-bar__segment is-left" id="redBarFill"></div>
                            <div class="summary-bar__segment is-right" id="blackBarFill"></div>
                        </div>

                        <div class="summary-side__count" id="totalPredictionsCount">0 прогнозов</div>
                    </div>
                </div>

                <div class="chart-card">
                    <div class="chart-title">Последний круг</div>
                    <div class="vote-chart" id="lastRoundChart"></div>
                </div>

                <div class="chart-card">
                    <div class="chart-title">Заголосуют первым</div>
                    <div class="vote-chart" id="voteChart"></div>
                </div>

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
        const overlayPanel = document.getElementById('overlayPanel');

        const formatPercent = (value) => `${Number(value || 0).toFixed(1)}%`;
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

        const setSummary = (redSide, blackSide, totalPredictions) => {
            const redValue = Number(redSide?.percent || 0);
            const blackValue = Number(blackSide?.percent || 0);
            const totalValue = redValue + blackValue;
            const normalizedRed = totalValue > 0 ? (redValue / totalValue) * 100 : 0;
            const normalizedBlack = totalValue > 0 ? 100 - normalizedRed : 0;

            redPercent.textContent = formatPercent(redValue);
            blackPercent.textContent = formatPercent(blackValue);
            redBarFill.style.setProperty('--segment-width', `${Math.max(0, Math.min(normalizedRed, 100))}%`);
            blackBarFill.style.setProperty('--segment-width', `${Math.max(0, Math.min(normalizedBlack, 100))}%`);
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
            renderSeats({ seatVotes: [] });
            renderLastRound({ lastRoundVotes: [] });
        };

        const renderPayload = (payload) => {
            if (payload.status === 'no-match') {
                setPanelVisible(false);
                return;
            }

            setPanelVisible(true);

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

            matchState.textContent = payload.matchState === 'Resolved' ? 'Закрыто' : payload.matchState === 'FirstVoted' ? '9-ка' : 'Лайв';
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
