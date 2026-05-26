(function () {
    const form = document.querySelector('[data-product-search-form]');
    if (!form) return;

    const input = form.querySelector('[data-product-search-input]');
    const panel = document.getElementById('zzSearchSuggestions');
    const brandInput = form.querySelector('input[name="maThuongHieu"]');
    if (!input || !panel) return;

    let timer = 0;
    let abortController = null;
    let activeIndex = -1;
    let suggestions = [];

    const endpoint = '/SanPham/GoiYTimKiem';

    function setOpen(open) {
        panel.classList.toggle('is-open', open);
        input.setAttribute('aria-expanded', open ? 'true' : 'false');
    }

    function clearPanel() {
        suggestions = [];
        activeIndex = -1;
        panel.replaceChildren();
        setOpen(false);
    }

    function setActive(index) {
        activeIndex = index;
        Array.from(panel.querySelectorAll('.zz-search-suggestion')).forEach((item, itemIndex) => {
            item.classList.toggle('is-active', itemIndex === activeIndex);
            item.setAttribute('aria-selected', itemIndex === activeIndex ? 'true' : 'false');
        });
    }

    function makeSuggestion(item, index) {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'zz-search-suggestion';
        button.id = `zzSearchSuggestion${index}`;
        button.setAttribute('role', 'option');
        button.setAttribute('aria-selected', 'false');

        const thumb = document.createElement('span');
        thumb.className = 'zz-search-suggestion__thumb';
        if (item.image) {
            const image = document.createElement('img');
            image.src = item.image;
            image.alt = item.name || 'Sản phẩm';
            image.loading = 'lazy';
            thumb.appendChild(image);
        } else {
            thumb.textContent = 'Z';
        }

        const body = document.createElement('span');
        body.className = 'zz-search-suggestion__body';

        const name = document.createElement('strong');
        name.className = 'zz-search-suggestion__name';
        name.textContent = item.name || 'Sản phẩm';

        const meta = document.createElement('span');
        meta.className = 'zz-search-suggestion__meta';
        meta.textContent = item.brand || 'Zuzong Store';

        const price = document.createElement('span');
        price.className = 'zz-search-suggestion__price';
        price.textContent = item.price || '';

        body.append(name, meta);
        button.append(thumb, body, price);

        button.addEventListener('mousedown', event => event.preventDefault());
        button.addEventListener('click', () => {
            if (item.url) window.location.href = item.url;
        });

        return button;
    }

    function render(items) {
        panel.replaceChildren();
        suggestions = Array.isArray(items) ? items : [];
        activeIndex = -1;

        if (!suggestions.length) {
            const empty = document.createElement('div');
            empty.className = 'zz-search-suggestion zz-search-suggestion--empty';
            empty.textContent = 'Chưa có sản phẩm phù hợp';
            panel.appendChild(empty);
            setOpen(true);
            return;
        }

        suggestions.forEach((item, index) => {
            panel.appendChild(makeSuggestion(item, index));
        });
        setOpen(true);
    }

    async function loadSuggestions() {
        const keyword = input.value.trim();
        const params = new URLSearchParams();
        params.set('keyword', keyword);
        if (brandInput && brandInput.value) params.set('maThuongHieu', brandInput.value);

        if (abortController) abortController.abort();
        abortController = new AbortController();

        try {
            const response = await fetch(`${endpoint}?${params.toString()}`, {
                headers: { 'Accept': 'application/json' },
                signal: abortController.signal
            });
            if (!response.ok) throw new Error('Search failed');
            const data = await response.json();
            render(data.items || []);
        } catch (error) {
            if (error.name !== 'AbortError') clearPanel();
        }
    }

    function queueLoad() {
        window.clearTimeout(timer);
        timer = window.setTimeout(loadSuggestions, 180);
    }

    input.addEventListener('focus', loadSuggestions);
    input.addEventListener('input', queueLoad);

    input.addEventListener('keydown', event => {
        if (!panel.classList.contains('is-open')) return;

        if (event.key === 'ArrowDown') {
            event.preventDefault();
            setActive(Math.min(activeIndex + 1, suggestions.length - 1));
        } else if (event.key === 'ArrowUp') {
            event.preventDefault();
            setActive(Math.max(activeIndex - 1, 0));
        } else if (event.key === 'Enter' && activeIndex >= 0 && suggestions[activeIndex]?.url) {
            event.preventDefault();
            window.location.href = suggestions[activeIndex].url;
        } else if (event.key === 'Escape') {
            clearPanel();
            input.blur();
        }
    });

    form.addEventListener('submit', event => {
        if (activeIndex >= 0 && suggestions[activeIndex]?.url) {
            event.preventDefault();
            window.location.href = suggestions[activeIndex].url;
        }
    });

    document.addEventListener('click', event => {
        if (!form.contains(event.target)) clearPanel();
    });
})();