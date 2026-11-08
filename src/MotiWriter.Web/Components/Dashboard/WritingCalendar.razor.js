// Calcule combien de semaines tiennent dans la largeur du calendrier, avec des cases
// d'environ `targetSize` px séparées de `gap` px, et le signale au composant Blazor.
// Les cases s'étirent ensuite (colonnes 1fr, aspect-ratio 1) pour remplir exactement la largeur.
export function observe(element, dotnet, targetSize, gap, maxWeeks) {
    let last = 0;
    let frame = 0;

    const measure = () => {
        frame = 0;
        const style = getComputedStyle(element);
        const inner = element.clientWidth
            - parseFloat(style.paddingLeft)
            - parseFloat(style.paddingRight);
        const label = element.querySelector('.label');
        const labelWidth = label ? label.getBoundingClientRect().width : 0;

        // Largeur disponible pour les semaines (l'écart après la colonne des libellés compris).
        const available = inner - labelWidth;
        const weeks = Math.max(1, Math.min(maxWeeks, Math.floor(available / (targetSize + gap))));

        if (weeks !== last) {
            last = weeks;
            dotnet.invokeMethodAsync('SetVisibleWeeks', weeks);
        }
    };

    const observer = new ResizeObserver(() => {
        if (!frame) {
            frame = requestAnimationFrame(measure);
        }
    });
    observer.observe(element);
    measure();

    return {
        dispose() {
            observer.disconnect();
            if (frame) {
                cancelAnimationFrame(frame);
            }
        },
    };
}
