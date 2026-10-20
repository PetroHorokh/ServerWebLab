(function () {
    'use strict';

    const root = document.querySelector('[data-chat]');
    const shared = window.cinema;
    if (!root || !shared || !window.signalR) {
        return;
    }

    const isInbox = root.getAttribute('data-chat') === 'inbox';
    const movieId = (root.getAttribute('data-movie-id') || '').toLowerCase();
    const currentUserId = (root.getAttribute('data-current-user-id') || '').toLowerCase();
    const canDelete = root.getAttribute('data-can-delete') === 'true';
    const urls = {
        delete: root.getAttribute('data-delete-url'),
        attachment: root.getAttribute('data-attachment-url'),
        movie: root.getAttribute('data-movie-url'),
    };

    const list = root.querySelector('[data-chat-messages]');
    const emptyElement = root.querySelector('[data-chat-empty]');
    const statusElement = root.querySelector('[data-chat-status]');
    const form = root.querySelector('[data-chat-form]');
    const privateTitle = 'Only the sender and admins can see this message';
    const restartDelayMs = 10000;

    let syncedAt = root.getAttribute('data-chat-synced');
    let closed = false;
    let restartTimer = null;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(root.getAttribute('data-hub-url'))
        .withAutomaticReconnect([0, 2000, 5000, 10000, 20000, 30000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    function fillUrl(template, id) {
        return template.replace('__id__', encodeURIComponent(id));
    }

    function element(tagName, className, text) {
        const node = document.createElement(tagName);
        if (className) {
            node.className = className;
        }

        if (text !== undefined) {
            node.textContent = text;
        }

        return node;
    }

    function renderAttachment(attachment) {
        const url = fillUrl(urls.attachment, attachment.id);
        if (attachment.isImage) {
            const link = element('a', 'chat-image');
            link.href = url;
            link.target = '_blank';
            link.rel = 'noopener';
            link.title = attachment.fileName + ' (' + attachment.sizeText + ')';
            const image = element('img');
            image.src = url;
            image.alt = attachment.fileName;
            image.loading = 'lazy';
            link.appendChild(image);
            return link;
        }

        const link = element('a', 'chat-file');
        link.href = url;
        link.setAttribute('download', attachment.fileName);
        link.appendChild(element('span', 'chat-file-name', attachment.fileName));
        link.appendChild(element('span', 'chat-file-size', attachment.sizeText));
        return link;
    }

    function renderMessage(message) {
        const article = element('article', 'chat-message');
        article.classList.toggle('is-mine', message.authorId.toLowerCase() === currentUserId);
        article.classList.toggle('is-private', message.isPrivate);
        article.setAttribute('data-message-id', message.id);

        const header = element('header', 'chat-meta');
        header.appendChild(element('span', 'chat-author', message.authorName));
        header.appendChild(element('span', 'badge ' + (message.isAdmin ? 'bg-dark' : 'bg-light text-dark border'), message.roleTag));
        if (message.isPrivate) {
            const badge = element('span', 'badge chat-private-badge', 'Private');
            badge.title = privateTitle;
            header.appendChild(badge);
        }

        const time = element('time', 'chat-time');
        time.setAttribute('datetime', message.postedDateTime);
        time.setAttribute('data-local-time', '');
        header.appendChild(time);

        if (canDelete) {
            const button = element('button', 'chat-delete', 'Delete');
            button.type = 'button';
            button.setAttribute('data-modal-url', fillUrl(urls.delete, message.id));
            button.setAttribute('aria-label', 'Delete the message from ' + message.authorName);
            header.appendChild(button);
        }

        article.appendChild(header);

        if (isInbox) {
            const context = element('div', 'chat-context', 'in ');
            const link = element('a', null, message.movieTitle);
            link.href = fillUrl(urls.movie, message.movieId);
            context.appendChild(link);
            article.appendChild(context);
        }

        if (message.message) {
            article.appendChild(element('p', 'chat-text', message.message));
        }

        if (message.attachments.length > 0) {
            const attachments = element('div', 'chat-attachments');
            message.attachments.forEach(function (attachment) {
                attachments.appendChild(renderAttachment(attachment));
            });
            article.appendChild(attachments);
        }

        shared.localizeTimes(article);
        return article;
    }

    function messageElements() {
        return list.querySelectorAll('[data-message-id]');
    }

    function findMessage(id) {
        return list.querySelector('[data-message-id="' + CSS.escape(id) + '"]');
    }

    function postedAt(node) {
        const time = node.querySelector('time[datetime]');
        return time ? Date.parse(time.getAttribute('datetime')) : 0;
    }

    function lastMessageId() {
        const messages = messageElements();
        return messages.length ? messages[messages.length - 1].getAttribute('data-message-id') : null;
    }

    function isNearBottom() {
        return list.scrollHeight - list.scrollTop - list.clientHeight < 80;
    }

    function scrollToBottom() {
        list.scrollTop = list.scrollHeight;
    }

    function updateEmptyState() {
        emptyElement.classList.toggle('d-none', messageElements().length > 0);
    }

    function insertInOrder(node) {
        const time = postedAt(node);
        const messages = Array.from(messageElements());

        if (isInbox) {
            const older = messages.find(function (other) { return postedAt(other) < time; });
            list.insertBefore(node, older || null);
            return;
        }

        for (let i = messages.length - 1; i >= 0; i--) {
            if (postedAt(messages[i]) <= time) {
                messages[i].after(node);
                return;
            }
        }

        list.insertBefore(node, messages.length ? messages[0] : null);
    }

    function addMessages(messages, forceScroll) {
        const stickToBottom = forceScroll || isNearBottom();
        let added = 0;

        messages.forEach(function (message) {
            if (!findMessage(message.id)) {
                insertInOrder(renderMessage(message));
                added++;
            }
        });

        if (added > 0) {
            updateEmptyState();
            if (!isInbox && stickToBottom) {
                scrollToBottom();
            }
        }
    }

    function removeMessage(id) {
        const node = findMessage(id);
        if (node) {
            node.remove();
            updateEmptyState();
        }
    }

    function setStatus(state, text) {
        if (!statusElement) {
            return;
        }

        statusElement.classList.remove('is-live', 'is-reconnecting', 'is-offline');
        statusElement.classList.add('is-' + state);
        statusElement.textContent = text;
    }

    function belongsHere(message) {
        return isInbox
            ? message.isPrivate && (!movieId || message.movieId === movieId)
            : message.movieId === movieId;
    }

    connection.on('messagePosted', function (message) {
        if (belongsHere(message)) {
            addMessages([message], false);
        }
    });

    connection.on('messageDeleted', function (deletedMovieId, messageId) {
        removeMessage(messageId);
    });

    async function join() {
        if (isInbox) {
            return;
        }

        const updates = await connection.invoke('JoinMovie', movieId, lastMessageId(), syncedAt);
        if (!updates) {
            closeChat('This movie no longer exists, so its chat is closed.');
            await connection.stop();
            return;
        }

        updates.removedMessageIds.forEach(removeMessage);
        addMessages(updates.messages, false);
        syncedAt = updates.syncedAt;
    }

    function scheduleRestart() {
        if (closed || restartTimer !== null) {
            return;
        }

        setStatus('offline', 'Offline, trying again shortly…');
        restartTimer = window.setTimeout(function () {
            restartTimer = null;
            start();
        }, restartDelayMs);
    }

    async function start() {
        if (closed || connection.state !== signalR.HubConnectionState.Disconnected) {
            return;
        }

        try {
            setStatus('reconnecting', 'Connecting…');
            await connection.start();
            setStatus('live', 'Live');
            await join();
        } catch {
            scheduleRestart();
        }
    }

    connection.onreconnecting(function () {
        setStatus('reconnecting', 'Reconnecting…');
    });

    connection.onreconnected(async function () {
        setStatus('live', isInbox ? 'Live (reload to see messages sent while you were offline)' : 'Live');
        try {
            await join();
        } catch {
        }
    });

    connection.onclose(function () {
        scheduleRestart();
    });

    document.addEventListener('visibilitychange', function () {
        if (!document.hidden && restartTimer !== null) {
            window.clearTimeout(restartTimer);
            restartTimer = null;
            start();
        }
    });

    function closeChat(message) {
        closed = true;
        setStatus('offline', 'Closed');
        if (composer) {
            composer.close(message);
        }
    }

    const composer = form ? createComposer(form) : null;

    function createComposer(chatForm) {
        const input = chatForm.querySelector('textarea');
        const sendButton = chatForm.querySelector('[type=submit]');
        const attachButton = chatForm.querySelector('[data-chat-attach]');
        const fileInput = chatForm.querySelector('[data-chat-file-input]');
        const selectedList = chatForm.querySelector('[data-chat-selected-files]');
        const privateToggle = chatForm.querySelector('[data-chat-private]');
        const errorElement = chatForm.querySelector('[data-chat-error]');
        const tokenInput = chatForm.querySelector('input[name="__RequestVerificationToken"]');
        const acceptedExtensions = (fileInput.getAttribute('accept') || '').toLowerCase().split(',');
        const maxFiles = Number(root.getAttribute('data-max-files'));
        const maxFileSize = Number(root.getAttribute('data-max-file-size'));
        const maxFileSizeText = root.getAttribute('data-max-file-size-text');

        let selected = [];
        let sending = false;

        function showError(message) {
            errorElement.textContent = message || '';
        }

        function extensionOf(name) {
            const dot = name.lastIndexOf('.');
            return dot >= 0 ? name.slice(dot).toLowerCase() : '';
        }

        function formatSize(bytes) {
            if (bytes < 1024) {
                return bytes + ' B';
            }

            const units = bytes < 1024 * 1024 ? [1024, ' KB'] : [1024 * 1024, ' MB'];
            return Number((bytes / units[0]).toFixed(1)) + units[1];
        }

        function renderSelected() {
            selectedList.replaceChildren();
            selected.forEach(function (item, index) {
                const entry = element('li', 'chat-selected-file');
                if (item.previewUrl) {
                    const preview = element('img');
                    preview.src = item.previewUrl;
                    preview.alt = '';
                    entry.appendChild(preview);
                }

                entry.appendChild(element('span', 'chat-selected-file-name', item.file.name));
                entry.appendChild(element('span', 'chat-selected-file-size', formatSize(item.file.size)));

                const remove = element('button', 'btn-close');
                remove.type = 'button';
                remove.setAttribute('aria-label', 'Remove ' + item.file.name);
                remove.addEventListener('click', function () {
                    removeSelected(index);
                    input.focus();
                });
                entry.appendChild(remove);
                selectedList.appendChild(entry);
            });

            selectedList.hidden = selected.length === 0;
        }

        function removeSelected(index) {
            const [item] = selected.splice(index, 1);
            if (item && item.previewUrl) {
                URL.revokeObjectURL(item.previewUrl);
            }

            renderSelected();
        }

        function clearSelected() {
            selected.forEach(function (item) {
                if (item.previewUrl) {
                    URL.revokeObjectURL(item.previewUrl);
                }
            });

            selected = [];
            renderSelected();
        }

        function addFiles(files) {
            let error = '';
            Array.from(files).forEach(function (file) {
                if (selected.length >= maxFiles) {
                    error = error || 'You can attach at most ' + maxFiles + ' files to a message.';
                } else if (!acceptedExtensions.includes(extensionOf(file.name))) {
                    error = error || '“' + file.name + '” can’t be attached. Allowed file types: ' + acceptedExtensions.join(', ') + '.';
                } else if (file.size === 0) {
                    error = error || '“' + file.name + '” is empty.';
                } else if (file.size > maxFileSize) {
                    error = error || '“' + file.name + '” is larger than ' + maxFileSizeText + '.';
                } else {
                    selected.push({
                        file: file,
                        previewUrl: file.type.startsWith('image/') ? URL.createObjectURL(file) : null,
                    });
                }
            });

            renderSelected();
            showError(error);
        }

        function setPrivate(isPrivate) {
            privateToggle.checked = isPrivate;
            chatForm.classList.toggle('is-private', isPrivate);
            input.placeholder = input.getAttribute(isPrivate ? 'data-private-placeholder' : 'data-public-placeholder');
        }

        function close(message) {
            input.disabled = true;
            sendButton.disabled = true;
            attachButton.disabled = true;
            privateToggle.disabled = true;
            showError(message);
        }

        async function sendWithHub(text) {
            const response = await connection.invoke('SendMessage', movieId, text, privateToggle.checked);
            if (response.message) {
                return response.message;
            }

            if (response.chatClosed) {
                closeChat(response.error);
            } else {
                showError(response.error || 'The message could not be sent. Please try again.');
            }

            return null;
        }

        async function sendWithHttp(text) {
            const data = new FormData();
            data.append('Message', text);
            data.append('IsPrivate', privateToggle.checked ? 'true' : 'false');
            data.append('__RequestVerificationToken', tokenInput.value);
            selected.forEach(function (item) {
                data.append('Files', item.file, item.file.name);
            });

            const response = await fetch(chatForm.action, { method: 'POST', body: data, headers: shared.ajaxHeaders });
            if (response.ok) {
                return await response.json();
            }

            if (response.status === 401) {
                closeChat(shared.sessionEndedMessage);
            } else if (response.status === 403) {
                closeChat((await response.text()) || shared.sessionEndedMessage);
            } else if (response.status === 404) {
                closeChat('This movie no longer exists, so its chat is closed.');
            } else if (response.status === 413) {
                showError('The files are too large to send together. Remove some of them and try again.');
            } else if (response.status === 400) {
                showError((await response.text()) || 'The message could not be sent. Reload the page and try again.');
            } else {
                showError('The message could not be sent (HTTP ' + response.status + '). Please try again.');
            }

            return null;
        }

        async function send() {
            if (closed || sending) {
                return;
            }

            const text = input.value.trim();
            if (!text && selected.length === 0) {
                showError('Write a message or attach a file.');
                input.focus();
                return;
            }

            sending = true;
            sendButton.disabled = true;
            sendButton.textContent = selected.length > 0 ? 'Uploading…' : 'Sending…';
            showError('');

            try {
                const useHub = selected.length === 0 && connection.state === signalR.HubConnectionState.Connected;
                const message = useHub ? await sendWithHub(text) : await sendWithHttp(text);
                if (message) {
                    addMessages([message], true);
                    input.value = '';
                    clearSelected();
                    setPrivate(false);
                    input.focus();
                }
            } catch {
                showError(shared.offlineMessage);
            } finally {
                sending = false;
                sendButton.textContent = 'Send';
                sendButton.disabled = closed;
            }
        }

        chatForm.addEventListener('submit', function (event) {
            event.preventDefault();
            send();
        });

        input.addEventListener('keydown', function (event) {
            if (event.key === 'Enter' && !event.shiftKey && !event.isComposing) {
                event.preventDefault();
                send();
            }
        });

        input.addEventListener('paste', function (event) {
            const files = event.clipboardData ? event.clipboardData.files : null;
            if (files && files.length > 0) {
                event.preventDefault();
                addFiles(files);
            }
        });

        attachButton.addEventListener('click', function () {
            fileInput.click();
        });

        fileInput.addEventListener('change', function () {
            addFiles(fileInput.files);
            fileInput.value = '';
        });

        privateToggle.addEventListener('change', function () {
            setPrivate(privateToggle.checked);
        });

        chatForm.addEventListener('dragover', function (event) {
            if (event.dataTransfer && Array.from(event.dataTransfer.types).includes('Files')) {
                event.preventDefault();
                chatForm.classList.add('is-dragging');
            }
        });

        chatForm.addEventListener('dragleave', function (event) {
            if (!chatForm.contains(event.relatedTarget)) {
                chatForm.classList.remove('is-dragging');
            }
        });

        chatForm.addEventListener('drop', function (event) {
            chatForm.classList.remove('is-dragging');
            if (event.dataTransfer && event.dataTransfer.files.length > 0) {
                event.preventDefault();
                if (!closed) {
                    addFiles(event.dataTransfer.files);
                }
            }
        });

        return { close: close };
    }

    if (!isInbox) {
        scrollToBottom();
    }

    start();
})();
