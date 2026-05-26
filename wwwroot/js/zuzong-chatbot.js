(function () {
    const chat = document.getElementById('zzAiChat');
    const toggle = document.getElementById('zzChatToggle');
    const close = document.getElementById('zzChatClose');
    const messages = document.getElementById('zzChatMessages');
    const input = document.getElementById('zzChatInput');
    const form = document.querySelector('[data-chat-form]');

    if (!chat || !toggle || !messages || !input || !form) return;

    function setChatOpen(isOpen) {
        chat.classList.toggle('is-open', isOpen);
        chat.setAttribute('aria-hidden', String(!isOpen));
        toggle.setAttribute('aria-expanded', String(isOpen));

        if (isOpen) {
            setTimeout(() => input.focus(), 120);
        }
    }

    function addMessage(type, text, products) {
        const row = document.createElement('div');
        row.className = `zz-chatbox__msg zz-chatbox__msg--${type}`;

        const bubble = document.createElement('div');
        bubble.className = 'zz-chatbox__bubble';

        const paragraph = document.createElement('p');
        paragraph.textContent = text;
        bubble.appendChild(paragraph);

        if (Array.isArray(products) && products.length > 0) {
            const productList = document.createElement('div');
            productList.className = 'zz-chatbox__products';

            products.forEach((product) => {
                productList.appendChild(createProductCard(product));
            });

            bubble.appendChild(productList);
        }

        row.appendChild(bubble);
        messages.appendChild(row);
        messages.scrollTop = messages.scrollHeight;
        return row;
    }

    function createProductCard(product) {
        const link = document.createElement('a');
        link.className = 'zz-chatbox__product';
        link.href = product.url || '#';

        const imageWrap = document.createElement('span');
        imageWrap.className = 'zz-chatbox__product-image';

        if (product.image) {
            const image = document.createElement('img');
            image.src = product.image;
            image.alt = product.name || 'Sản phẩm';
            image.loading = 'lazy';
            imageWrap.appendChild(image);
        } else {
            imageWrap.textContent = 'Z';
        }

        const body = document.createElement('span');
        body.className = 'zz-chatbox__product-body';

        const name = document.createElement('strong');
        name.textContent = product.name || 'Điện thoại';

        const meta = document.createElement('small');
        meta.textContent = product.reason || product.brand || 'Gợi ý phù hợp';

        const price = document.createElement('span');
        price.className = 'zz-chatbox__product-price';
        price.textContent = product.price || '';

        body.appendChild(name);
        body.appendChild(meta);
        body.appendChild(price);

        if (product.oldPrice) {
            const oldPrice = document.createElement('del');
            oldPrice.textContent = product.oldPrice;
            body.appendChild(oldPrice);
        }

        link.appendChild(imageWrap);
        link.appendChild(body);
        return link;
    }

    async function askAssistant(text) {
        const response = await fetch(`/SanPham/TuVanChat?message=${encodeURIComponent(text)}`, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        if (!response.ok) {
            throw new Error('Chat request failed');
        }

        return response.json();
    }

    toggle.addEventListener('click', () => {
        setChatOpen(!chat.classList.contains('is-open'));
    });

    if (close) {
        close.addEventListener('click', () => setChatOpen(false));
    }

    document.addEventListener('click', (event) => {
        const suggestion = event.target.closest('[data-chat-suggest]');
        if (!suggestion) return;

        setChatOpen(true);
        input.value = suggestion.dataset.chatSuggest || '';
        form.requestSubmit();
    });

    form.addEventListener('submit', async (event) => {
        event.preventDefault();

        const text = input.value.trim();
        if (!text) return;

        addMessage('user', text);
        input.value = '';
        input.disabled = true;
        form.querySelector('button[type="submit"]').disabled = true;

        const typing = addMessage('bot', 'Zuzong AI đang đọc dữ liệu sản phẩm phù hợp cho bạn...', []);

        try {
            const data = await askAssistant(text);
            typing.remove();
            addMessage(
                'bot',
                data.reply || 'Mình đã xem qua sản phẩm phù hợp cho bạn.',
                data.products || []
            );
        } catch {
            typing.remove();
            addMessage(
                'bot',
                'Mình chưa kết nối được dữ liệu sản phẩm lúc này. Bạn thử lại sau hoặc bấm Facebook bên dưới để được tư vấn trực tiếp nhé.',
                []
            );
        } finally {
            input.disabled = false;
            form.querySelector('button[type="submit"]').disabled = false;
            input.focus();
        }
    });
})();
