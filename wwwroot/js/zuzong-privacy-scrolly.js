(function () {
    const root = document.getElementById('privacyScrolly');
    const canvas = document.getElementById('privacySequenceCanvas');

    if (!root || !canvas) return;

    const context = canvas.getContext('2d', { alpha: false });
    const frameCount = Number(root.dataset.frameCount || 220);
    const frameBase = root.dataset.frameBase || '/images/14861093_3840_2160_30fps_';
    const chapters = Array.from(document.querySelectorAll('[data-policy-step]'));
    const cache = new Map();
    let currentFrame = 0;
    let lastDrawableFrame = 0;
    let ticking = false;

    function framePath(index) {
        return `${frameBase}${String(index).padStart(3, '0')}.jpg`;
    }

    function loadFrame(index) {
        const safeIndex = Math.max(0, Math.min(frameCount - 1, index));
        if (cache.has(safeIndex)) return cache.get(safeIndex);

        const image = new Image();
        image.decoding = 'async';
        image.loading = 'eager';
        image.src = framePath(safeIndex);
        cache.set(safeIndex, image);
        return image;
    }

    function resizeCanvas() {
        const dpr = Math.min(window.devicePixelRatio || 1, 2);
        const width = Math.max(1, canvas.clientWidth);
        const height = Math.max(1, canvas.clientHeight);

        canvas.width = Math.round(width * dpr);
        canvas.height = Math.round(height * dpr);
        context.setTransform(dpr, 0, 0, dpr, 0, 0);
        drawFrame(currentFrame);
    }

    function drawCover(image) {
        const width = canvas.clientWidth;
        const height = canvas.clientHeight;
        const scale = Math.max(width / image.naturalWidth, height / image.naturalHeight);
        const drawWidth = image.naturalWidth * scale;
        const drawHeight = image.naturalHeight * scale;
        const x = (width - drawWidth) / 2;
        const y = (height - drawHeight) / 2;

        context.drawImage(image, x, y, drawWidth, drawHeight);
    }

    function paintImage(image, index) {
        context.fillStyle = '#08090b';
        context.fillRect(0, 0, canvas.clientWidth, canvas.clientHeight);
        drawCover(image);
        lastDrawableFrame = index;
    }

    function drawFrame(index) {
        const image = loadFrame(index);
        if (!image.complete || !image.naturalWidth) {
            if (index !== lastDrawableFrame) {
                const fallback = loadFrame(lastDrawableFrame);
                if (fallback.complete && fallback.naturalWidth) {
                    paintImage(fallback, lastDrawableFrame);
                }
            }
            image.onload = () => drawFrame(index);
            image.onerror = () => drawFrame(lastDrawableFrame);
            return;
        }

        paintImage(image, index);
    }

    function preloadAround(index) {
        const importantFrames = [0, Math.floor(frameCount * 0.25), Math.floor(frameCount * 0.5), Math.floor(frameCount * 0.75), frameCount - 1];
        importantFrames.forEach(loadFrame);

        for (let offset = -8; offset <= 10; offset += 1) {
            loadFrame(index + offset);
        }
    }

    function updateFrame() {
        ticking = false;

        const rect = root.getBoundingClientRect();
        const scrollable = Math.max(1, root.offsetHeight - window.innerHeight);
        const progress = Math.max(0, Math.min(1, -rect.top / scrollable));
        const nextFrame = Math.round(progress * (frameCount - 1));

        if (nextFrame !== currentFrame) {
            currentFrame = nextFrame;
            drawFrame(currentFrame);
            preloadAround(currentFrame);
        }
    }

    function requestFrameUpdate() {
        if (ticking) return;
        ticking = true;
        requestAnimationFrame(updateFrame);
    }

    const chapterObserver = new IntersectionObserver((entries) => {
        const visible = entries
            .filter((entry) => entry.isIntersecting)
            .sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0];

        if (!visible) return;

        chapters.forEach((chapter) => {
            chapter.classList.toggle('is-active', chapter === visible.target);
        });
    }, {
        rootMargin: '-38% 0px -38% 0px',
        threshold: [0.2, 0.45, 0.7]
    });

    chapters.forEach((chapter) => chapterObserver.observe(chapter));
    window.addEventListener('scroll', requestFrameUpdate, { passive: true });
    window.addEventListener('resize', resizeCanvas);

    loadFrame(0).onload = () => drawFrame(0);
    preloadAround(0);
    resizeCanvas();
    updateFrame();
})();
