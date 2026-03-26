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

        .sympathy-wrap {
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

        .sympathy-header {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
            margin-bottom: 8px;
        }

        .sympathy-card {
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

        .sympathy-badge {
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

        .sympathy-wrap.is-finished .sympathy-badge {
            background: var(--badge-finish-bg);
        }

        .sympathy-title {
            font-size: 24px;
            font-weight: 700;
            letter-spacing: 0.01em;
            color: var(--title-color);
            white-space: nowrap;
        }

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
            left: 0;
            background: var(--town-fill);
            box-shadow: inset 0 0 0 1px rgba(255, 242, 229, 0.28), 0 0 20px rgba(202, 147, 106, 0.24);
        }

        .sympathy-track.is-mafia .sympathy-track__fill {
            right: 0;
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
            margin: 0 2px;
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
            margin: 0 2px;
        }

        .sympathy-footer {
            margin-top: 8px;
            display: flex;
            align-items: baseline;
            justify-content: center;
            gap: 16px;
            font-size: 17px;
            font-weight: 500;
            color: var(--text-muted);
        }

        .sympathy-handle {
            color: rgba(218, 218, 222, 0.7);
        }

        @media (max-width: 720px) {
            .sympathy-card {
                padding: 8px 14px;
                border-radius: 999px;
            }

            .sympathy-title {
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

            .sympathy-footer {
                font-size: 14px;
                gap: 10px;
            }
        }
    </style>
</head>
<body>
    <div class="overlay-root is-hidden" id="overlayRoot">
        <div class="sympathy-wrap" id="sympathyWrap">
            <div class="sympathy-header">
                <div class="sympathy-badge" id="stateBadge">LIVE</div>
                <div class="sympathy-title">Зрительские симпатии</div>
            </div>

            <section class="sympathy-card">
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

            <div class="sympathy-footer">
                <span id="summaryMeta">0 прогнозов через</span>
                <span class="sympathy-handle">@mafiapickembot</span>
            </div>
        </div>
    </div>

    <script>
        const tournamentId = {{tournamentId}};
        const overlayRoot = document.getElementById('overlayRoot');
        const sympathyWrap = document.getElementById('sympathyWrap');
        const stateBadge = document.getElementById('stateBadge');
        const townPercent = document.getElementById('townPercent');
        const mafiaPercent = document.getElementById('mafiaPercent');
        const townFill = document.getElementById('townFill');
        const mafiaFill = document.getElementById('mafiaFill');
        const summaryMeta = document.getElementById('summaryMeta');
        const rootStyles = document.documentElement;

        const defaultOverlaySettings = {
            viewerSympathyBlock: {
                horizontalOffset: 0,
                verticalOffset: 24,
                scale: 10,
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

        const clamp = (value, min, max) => Math.min(Math.max(value, min), max);

        const normalizeViewerSympathyBlock = (settings) => ({
            horizontalOffset: clamp(Number(settings?.horizontalOffset ?? defaultOverlaySettings.viewerSympathyBlock.horizontalOffset) || 0, -4000, 4000),
            verticalOffset: clamp(Number(settings?.verticalOffset ?? defaultOverlaySettings.viewerSympathyBlock.verticalOffset) || 0, -4000, 4000),
            scale: clamp(Number(settings?.scale ?? defaultOverlaySettings.viewerSympathyBlock.scale) || 10, 1, 10),
        });

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
        };

        const setSummary = (redSide, blackSide, totalPredictions) => {
            const redValue = clamp(Number(redSide?.percent || 0), 0, 100);
            const blackValue = clamp(Number(blackSide?.percent || 0), 0, 100);

            townPercent.textContent = formatPercent(redValue);
            mafiaPercent.textContent = formatPercent(blackValue);
            townFill.style.width = `${redValue}%`;
            mafiaFill.style.width = `${blackValue}%`;
            summaryMeta.textContent = formatPredictionsCount(totalPredictions) + ' через';
        };

        const setPendingState = () => {
            sympathyWrap.classList.remove('is-finished');
            stateBadge.textContent = 'LIVE';
            setSummary(null, null, 0);
        };

        const renderPayload = (payload) => {
            applyOverlaySettings(payload);

            if (payload?.status === 'no-match') {
                overlayRoot.classList.add('is-hidden');
                return;
            }

            overlayRoot.classList.remove('is-hidden');

            if (payload?.status !== 'ready') {
                setPendingState();
                return;
            }

            sympathyWrap.classList.toggle('is-finished', payload?.matchState === 'Resolved');
            stateBadge.textContent = payload?.matchState === 'Resolved' ? 'ИТОГ' : 'LIVE';
            setSummary(payload?.redSide, payload?.blackSide, payload?.totalPredictions);
        };

        const applyDisconnectedState = () => {
            applyOverlaySettings(null);
            overlayRoot.classList.remove('is-hidden');
            sympathyWrap.classList.remove('is-finished');
            stateBadge.textContent = 'OFFLINE';
            setSummary(null, null, 0);
            summaryMeta.textContent = 'Нет связи через';
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