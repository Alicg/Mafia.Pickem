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
            --panel-width: 200px;
            --card-bg: rgba(244, 239, 235, 0.94);
            --card-bg-soft: rgba(224, 216, 210, 0.9);
            --text-main: #1f1b18;
            --text-soft: rgba(31, 27, 24, 0.76);
            --text-faint: rgba(31, 27, 24, 0.48);
            --accent: #ff5454;
            --accent-strong: #ff2f2f;
            --accent-warm: #2a110f;
            --track: rgba(37, 24, 20, 0.12);
            --track-soft: rgba(37, 24, 20, 0.05);
            --shadow: 0 20px 42px rgba(44, 26, 20, 0.22);
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
            align-items: flex-start;
            justify-content: flex-start;
            padding: 22px 0 0 22px;
        }

        .overlay-panel {
            width: var(--panel-width);
            max-width: calc(100vw - 22px);
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
                radial-gradient(circle at top right, rgba(255, 47, 47, 0.14), transparent 34%),
                linear-gradient(180deg, var(--card-bg), var(--card-bg-soft));
            box-shadow: var(--shadow);
            border: 1px solid rgba(109, 86, 76, 0.14);
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
            background: rgba(255, 47, 47, 0.12);
            color: var(--accent);
            font-size: 10px;
            font-weight: 800;
            letter-spacing: 0.14em;
            text-transform: uppercase;
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
            font-size: 24px;
            line-height: 0.92;
            font-weight: 900;
            letter-spacing: -0.06em;
            color: var(--accent-warm);
        }

        .summary-side__label {
            font-size: 10px;
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
        }

        .summary-bar__segment {
            width: var(--segment-width, 0%);
            height: 100%;
        }

        .summary-bar__segment.is-left {
            background: linear-gradient(270deg, rgba(255, 84, 84, 0.98), rgba(255, 47, 47, 0.72));
            box-shadow: 0 0 16px rgba(255, 47, 47, 0.25);
        }

        .summary-bar__segment.is-right {
            background: linear-gradient(90deg, rgba(30, 30, 30, 0.98), rgba(8, 8, 8, 0.98));
            box-shadow: 0 0 16px rgba(0, 0, 0, 0.2);
        }

        .summary-side__count {
            font-size: 13px;
            font-weight: 800;
            line-height: 1;
            color: var(--text-soft);
        }

        .chart-card {
            padding: 10px 12px 11px;
        }

        .chart-title {
            margin-bottom: 7px;
            font-size: 10px;
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
            grid-template-columns: 14px minmax(0, 1fr);
            gap: 6px;
            align-items: center;
        }

        .vote-bar.is-last-round {
            grid-template-columns: 52px minmax(0, 1fr);
        }

        .vote-bar__slot {
            font-size: 11px;
            font-weight: 800;
            line-height: 1;
            text-align: center;
            color: var(--text-main);
        }

        .vote-bar__slot.is-last-round {
            display: inline-flex;
            align-items: center;
            min-height: 18px;
            padding: 0 6px;
            border-radius: 999px;
            font-size: 10px;
            font-weight: 900;
            letter-spacing: 0.02em;
            text-align: left;
        }

        .vote-bar__track {
            position: relative;
            width: 100%;
            min-width: 0;
            height: 12px;
            border-radius: 999px;
            background: linear-gradient(90deg, var(--track-soft), var(--track));
            overflow: hidden;
        }

        .vote-bar__fill {
            width: var(--bar-width, 0%);
            height: 100%;
            border-radius: inherit;
            background: var(--accent-strong);
            box-shadow: 0 0 14px rgba(255, 47, 47, 0.22);
        }

        .vote-bar.is-first-vote .vote-bar__track {
            background: linear-gradient(90deg, rgba(31, 78, 121, 0.1), rgba(31, 78, 121, 0.18));
            box-shadow: inset 0 0 0 1px rgba(31, 78, 121, 0.12);
        }

        .vote-bar.is-first-vote .vote-bar__fill {
            background: linear-gradient(90deg, rgba(57, 140, 219, 0.98), rgba(31, 78, 121, 0.94));
            box-shadow: 0 0 14px rgba(57, 140, 219, 0.2);
        }

        .vote-bar.is-first-vote .vote-bar__slot {
            color: rgba(25, 62, 94, 0.96);
        }

        .vote-bar.is-last-round.is-black-style .vote-bar__slot {
            color: rgba(255, 255, 255, 0.96);
            background: linear-gradient(180deg, rgba(22, 22, 22, 0.96), rgba(0, 0, 0, 0.98));
            box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.04), 0 0 10px rgba(0, 0, 0, 0.18);
        }

        .vote-bar.is-last-round.is-black-style .vote-bar__track {
            background: linear-gradient(90deg, rgba(0, 0, 0, 0.12), rgba(0, 0, 0, 0.18));
            box-shadow: inset 0 0 0 1px rgba(0, 0, 0, 0.12);
        }

        .vote-bar.is-last-round.is-black-style .vote-bar__fill {
            background: linear-gradient(90deg, rgba(24, 24, 24, 0.98), rgba(0, 0, 0, 0.98));
            box-shadow: 0 0 14px rgba(0, 0, 0, 0.18);
        }

        .vote-bar.is-last-round.is-red-style .vote-bar__slot {
            color: rgba(255, 248, 248, 0.98);
            background: linear-gradient(180deg, rgba(255, 84, 84, 0.96), rgba(166, 28, 28, 0.96));
            box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.14), 0 0 10px rgba(255, 47, 47, 0.18);
        }

        .vote-bar.is-last-round.is-red-style .vote-bar__track {
            box-shadow: inset 0 0 0 1px rgba(255, 84, 84, 0.14);
        }

        .vote-bar.is-resolved .vote-bar__fill {
            background: linear-gradient(90deg, rgba(74, 222, 128, 0.98), rgba(34, 197, 94, 0.92));
            box-shadow: 0 0 14px rgba(74, 222, 128, 0.28);
        }

        .vote-bar.is-resolved .vote-bar__slot {
            color: #86efac;
            text-shadow: 0 0 10px rgba(74, 222, 128, 0.35);
        }

        .vote-bar.is-resolved .vote-bar__track {
            box-shadow: inset 0 0 0 1px rgba(74, 222, 128, 0.5), 0 0 14px rgba(74, 222, 128, 0.18);
        }

        .vote-bar__count {
            position: absolute;
            right: 6px;
            top: 50%;
            transform: translateY(-50%);
            font-size: 9px;
            font-weight: 800;
            color: rgba(255, 248, 245, 0.94);
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
        <section class="overlay-panel">
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
                        <div class="vote-bar__count">${count}</div>
                    </div>`;
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
                        <div class="vote-bar__count">${count}</div>
                    </div>`;
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
