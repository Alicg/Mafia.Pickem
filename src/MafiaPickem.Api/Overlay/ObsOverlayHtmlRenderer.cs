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
            --card-bg: rgba(10, 10, 10, 0.94);
            --card-bg-soft: rgba(26, 26, 26, 0.9);
            --text-main: #f5f5f5;
            --text-soft: rgba(245, 245, 245, 0.76);
            --text-faint: rgba(245, 245, 245, 0.48);
            --accent: #ff5454;
            --accent-strong: #ff2f2f;
            --accent-warm: #ffffff;
            --track: rgba(255, 255, 255, 0.08);
            --track-soft: rgba(255, 255, 255, 0.03);
            --shadow: 0 20px 42px rgba(0, 0, 0, 0.34);
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
                radial-gradient(circle at top right, rgba(255, 47, 47, 0.16), transparent 34%),
                linear-gradient(180deg, var(--card-bg), var(--card-bg-soft));
            box-shadow: var(--shadow);
        }

        .summary-card {
            padding: 10px 12px 11px;
        }

        .summary-top {
            display: flex;
            justify-content: space-between;
            align-items: center;
            gap: 8px;
            margin-bottom: 6px;
        }

        .state-pill {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-height: 20px;
            padding: 0 8px;
            border-radius: 999px;
            background: rgba(255, 47, 47, 0.14);
            color: var(--accent);
            font-size: 10px;
            font-weight: 800;
            letter-spacing: 0.14em;
            text-transform: uppercase;
        }

        .status-text {
            min-width: 0;
            font-size: 10px;
            color: var(--text-faint);
            text-align: right;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .summary-main {
            display: flex;
            flex-direction: column;
            align-items: flex-start;
            gap: 3px;
        }

        .summary-value {
            display: flex;
            align-items: baseline;
            gap: 7px;
            min-width: 0;
        }

        .summary-value__number {
            font-size: 38px;
            line-height: 0.88;
            font-weight: 900;
            letter-spacing: -0.06em;
            color: var(--accent-warm);
        }

        .summary-value__label {
            font-size: 11px;
            color: var(--text-soft);
            padding-bottom: 5px;
            white-space: nowrap;
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
            grid-template-columns: 14px 160px;
            gap: 6px;
            align-items: center;
        }

        .vote-bar__slot {
            font-size: 11px;
            font-weight: 800;
            line-height: 1;
            text-align: center;
            color: var(--text-main);
        }

        .vote-bar__track {
            position: relative;
            width: 160px;
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
            color: rgba(255, 255, 255, 0.9);
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
                        <div class="status-text" id="statusText">Ожидание публикации</div>
                    </div>

                    <div class="summary-main">
                        <div class="summary-value">
                            <div class="summary-value__number" id="redPercent">-</div>
                            <div class="summary-value__label">за красных</div>
                        </div>
                        <div class="summary-side__count" id="totalPredictionsCount">0 прогнозов</div>
                    </div>
                </div>

                <div class="chart-card">
                    <div class="chart-title">Заголосуют первым</div>
                    <div class="vote-chart" id="voteChart"></div>
                </div>

                <div class="chart-card">
                    <div class="chart-title">Последний круг</div>
                    <div class="vote-chart" id="lastRoundChart"></div>
                </div>
            </div>
        </section>
    </div>

    <script>
        const tournamentId = {{tournamentId}};
        const redPercent = document.getElementById('redPercent');
        const totalPredictionsCount = document.getElementById('totalPredictionsCount');
        const matchState = document.getElementById('matchState');
        const statusText = document.getElementById('statusText');
        const voteChart = document.getElementById('voteChart');
        const lastRoundChart = document.getElementById('lastRoundChart');

        const formatPercent = (value) => `${Number(value || 0).toFixed(1)}%`;
        const formatUpdatedAt = (value) => {
            if (!value) {
                return 'Ожидание публикации';
            }

            const date = new Date(value);
            if (Number.isNaN(date.getTime())) {
                return 'Ожидание публикации';
            }

            return date.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
        };

        const getDataUrl = () => {
            const url = new URL(window.location.href);
            url.pathname = `${window.location.pathname.replace(/\/$/, '')}/data`;
            url.searchParams.set('_', Date.now().toString());
            return url.toString();
        };

        const setSummary = (redSide, totalPredictions) => {
            redPercent.textContent = formatPercent(redSide?.percent || 0);
            totalPredictionsCount.textContent = `${Number(totalPredictions || 0)} прогнозов`;
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
                item.className = `vote-bar${seat.isResolved ? ' is-resolved' : ''}${count === 0 ? ' is-empty' : ''}`;
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
                const item = document.createElement('div');
                item.className = `vote-bar${isResolved ? ' is-resolved' : ''}${count === 0 ? ' is-empty' : ''}`;
                item.innerHTML = `
                    <div class="vote-bar__slot" style="width:52px;text-align:left;font-size:10px">${lr.label || lr.lastRound}</div>
                    <div class="vote-bar__track">
                        <div class="vote-bar__fill" ${fillStyle}></div>
                        <div class="vote-bar__count">${count}</div>
                    </div>`;
                lastRoundChart.appendChild(item);
            });
        };

        const applyDisconnectedState = (message) => {
            matchState.textContent = 'Нет связи';
            statusText.textContent = 'Ошибка обновления';
            redPercent.textContent = '-';
            totalPredictionsCount.textContent = '0 прогнозов';
            renderSeats({ seatVotes: [] });
            renderLastRound({ lastRoundVotes: [] });
        };

        const renderPayload = (payload) => {
            if (payload.status !== 'ready') {
                matchState.textContent = 'Ожидание';
                statusText.textContent = 'Ожидание публикации';
                redPercent.textContent = '-';
                totalPredictionsCount.textContent = '0 прогнозов';
                renderSeats(payload);
                renderLastRound(payload);
                return;
            }

            const totalPredictions = Number(payload.totalPredictions || 0);

            matchState.textContent = payload.matchState === 'Resolved' ? 'Закрыто' : payload.matchState === 'FirstVoted' ? '9-ка' : 'Лайв';
            statusText.textContent = formatUpdatedAt(payload.updatedAt);
            setSummary(
                payload.redSide,
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
