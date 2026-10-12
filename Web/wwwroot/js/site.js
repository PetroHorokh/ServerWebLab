(function () {
    'use strict';

    const ajaxHeaders = { 'X-Requested-With': 'XMLHttpRequest' };
    const sessionEndedMessage = 'Your session has ended or you no longer have access. Reload the page and log in again.';
    const offlineMessage = 'Could not reach the server. Check that the application is running and try again.';

    function isAuthFailure(response) {
        return response.status === 401 || response.status === 403;
    }

    function localizeTimes(root) {
        root.querySelectorAll('time[data-local-time]').forEach(function (element) {
            const date = new Date(element.getAttribute('datetime'));
            if (!isNaN(date.getTime())) {
                element.textContent = date.toLocaleString([], { dateStyle: 'medium', timeStyle: 'short' });
                element.removeAttribute('data-local-time');
            }
        });
    }

    function initMultiselects(root) {
        root.querySelectorAll('[data-multiselect]').forEach(function (container) {
            const summary = container.querySelector('[data-multiselect-summary]');
            const count = container.querySelector('[data-multiselect-count]');
            const search = container.querySelector('[data-multiselect-search]');
            const noResults = container.querySelector('[data-multiselect-no-results]');
            const options = Array.from(container.querySelectorAll('[data-multiselect-option]'));
            const placeholder = container.getAttribute('data-placeholder');

            function labelOf(option) {
                return option.querySelector('label').textContent.trim();
            }

            function updateSummary() {
                const selected = options
                    .filter(function (option) { return option.querySelector('input').checked; })
                    .map(labelOf);

                summary.textContent = selected.length ? selected.join(', ') : placeholder;
                summary.classList.toggle('is-placeholder', selected.length === 0);
                count.textContent = selected.length;
                count.classList.toggle('d-none', selected.length === 0);
            }

            options.forEach(function (option) {
                option.querySelector('input').addEventListener('change', updateSummary);
            });

            if (search) {
                search.addEventListener('input', function () {
                    const term = search.value.trim().toLowerCase();
                    let visible = 0;
                    options.forEach(function (option) {
                        const matches = labelOf(option).toLowerCase().includes(term);
                        option.classList.toggle('d-none', !matches);
                        visible += matches ? 1 : 0;
                    });
                    noResults.classList.toggle('d-none', visible > 0);
                });

                search.addEventListener('keydown', function (event) {
                    if (event.key === 'Enter') {
                        event.preventDefault();
                    }
                });
            }

            updateSummary();
        });
    }

    function initModals() {
        const modalElement = document.getElementById('app-modal');
        if (!modalElement || !window.bootstrap) {
            return;
        }

        const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
        const modalContent = modalElement.querySelector('[data-modal-content]');

        const loadingHtml =
            '<div class="modal-body py-5 text-center">' +
            '<div class="spinner-border text-secondary" role="status"><span class="visually-hidden">Loading…</span></div>' +
            '</div>';

        const errorHtml =
            '<div class="modal-header">' +
            '<h2 class="modal-title h5" id="app-modal-title">Something went wrong</h2>' +
            '<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>' +
            '</div>' +
            '<div class="modal-body"><p class="mb-0" data-error-message></p></div>' +
            '<div class="modal-footer">' +
            '<button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Close</button>' +
            '</div>';

        function renderError(message) {
            modalContent.innerHTML = errorHtml;
            modalContent.querySelector('[data-error-message]').textContent = message;
        }

        function describeFailure(response) {
            if (isAuthFailure(response)) {
                return sessionEndedMessage;
            }

            if (response.status === 404) {
                return 'This item no longer exists. It may have been deleted already; reload the page to see the current data.';
            }

            return 'The server could not complete the request (HTTP ' + response.status + '). Please try again.';
        }

        function focusFirstField() {
            const field = modalContent.querySelector(
                '.input-validation-error, .modal-body input:not([type=hidden]):not([type=search]):not([disabled]), .modal-body select, .modal-body textarea');
            if (field) {
                field.focus();
            }
        }

        function render(html) {
            modalContent.innerHTML = html;

            const form = modalContent.querySelector('form[data-ajax-form]');
            if (form && window.jQuery && jQuery.validator && jQuery.validator.unobtrusive) {
                jQuery.validator.unobtrusive.parse(form);
            }

            initMultiselects(modalContent);

            if (modalElement.classList.contains('show')) {
                focusFirstField();
            }
        }

        async function openModal(url) {
            modalContent.innerHTML = loadingHtml;
            modal.show();

            try {
                const response = await fetch(url, { headers: Object.assign({ Accept: 'text/html' }, ajaxHeaders) });
                if (!response.ok) {
                    renderError(describeFailure(response));
                    return;
                }

                render(await response.text());
            } catch {
                renderError(offlineMessage);
            }
        }

        async function submitForm(form) {
            const submitButton = form.querySelector('[type=submit]');
            if (submitButton) {
                submitButton.disabled = true;
            }

            try {
                const response = await fetch(form.action, { method: 'POST', body: new FormData(form), headers: ajaxHeaders });
                const contentType = response.headers.get('content-type') || '';

                if (response.ok && contentType.includes('application/json')) {
                    const result = await response.json();
                    if (result.success) {
                        window.location.reload();
                        return;
                    }
                }

                if ((response.ok || response.status === 400) && contentType.includes('text/html')) {
                    render(await response.text());
                    return;
                }

                renderError(describeFailure(response));
            } catch {
                renderError(offlineMessage);
            } finally {
                if (submitButton) {
                    submitButton.disabled = false;
                }
            }
        }

        document.addEventListener('click', function (event) {
            const trigger = event.target.closest('[data-modal-url]');
            if (!trigger) {
                return;
            }

            event.preventDefault();
            openModal(trigger.getAttribute('data-modal-url'));
        });

        document.addEventListener('submit', function (event) {
            const form = event.target.closest('form[data-ajax-form]');
            if (!form) {
                return;
            }

            event.preventDefault();
            if (window.jQuery && typeof jQuery(form).valid === 'function' && !jQuery(form).valid()) {
                return;
            }

            submitForm(form);
        });

        modalElement.addEventListener('shown.bs.modal', focusFirstField);
        modalElement.addEventListener('hidden.bs.modal', function () {
            modalContent.innerHTML = '';
        });
    }

    function initChat() {
        const list = document.querySelector('[data-chat-messages]');
        const form = document.querySelector('[data-chat-form]');
        if (!list) {
            return;
        }

        const messagesUrl = list.getAttribute('data-messages-url');
        const input = form ? form.querySelector('textarea') : null;
        const sendButton = form ? form.querySelector('[type=submit]') : null;
        const errorElement = form ? form.querySelector('[data-chat-error]') : null;
        const emptyElement = list.querySelector('[data-chat-empty]');
        const pollIntervalMs = 4000;
        let syncedTicks = list.getAttribute('data-chat-synced');
        let polling = false;
        let closed = false;

        function showError(message) {
            if (errorElement) {
                errorElement.textContent = message || '';
            }
        }

        function lastMessageId() {
            const messages = list.querySelectorAll('[data-message-id]');
            return messages.length ? messages[messages.length - 1].getAttribute('data-message-id') : null;
        }

        function isNearBottom() {
            return list.scrollHeight - list.scrollTop - list.clientHeight < 80;
        }

        function scrollToBottom() {
            list.scrollTop = list.scrollHeight;
        }

        function appendMessages(html, forceScroll) {
            const stickToBottom = forceScroll || isNearBottom();
            const template = document.createElement('template');
            template.innerHTML = html;

            let added = 0;
            template.content.querySelectorAll('[data-message-id]').forEach(function (message) {
                const id = message.getAttribute('data-message-id');
                if (!list.querySelector('[data-message-id="' + id + '"]')) {
                    localizeTimes(message);
                    list.appendChild(message);
                    added++;
                }
            });

            if (added > 0) {
                emptyElement.classList.add('d-none');
                if (stickToBottom) {
                    scrollToBottom();
                }
            }
        }

        function removeMessages(idList) {
            idList.split(',').filter(Boolean).forEach(function (id) {
                const message = list.querySelector('[data-message-id="' + id + '"]');
                if (message) {
                    message.remove();
                }
            });

            emptyElement.classList.toggle('d-none', list.querySelector('[data-message-id]') !== null);
        }

        function close(message) {
            closed = true;
            if (form) {
                input.disabled = true;
                sendButton.disabled = true;
            }

            showError(message);
        }

        async function poll() {
            if (polling || closed || document.hidden) {
                return;
            }

            polling = true;
            try {
                const url = new URL(messagesUrl, window.location.origin);
                const lastId = lastMessageId();
                if (lastId) {
                    url.searchParams.set('afterId', lastId);
                }

                if (syncedTicks) {
                    url.searchParams.set('since', syncedTicks);
                }

                const response = await fetch(url, { headers: ajaxHeaders });
                if (isAuthFailure(response)) {
                    close(sessionEndedMessage);
                } else if (response.ok) {
                    removeMessages(response.headers.get('X-Chat-Removed') || '');
                    appendMessages(await response.text(), false);
                    syncedTicks = response.headers.get('X-Chat-Synced') || syncedTicks;
                }
            } catch {
            } finally {
                polling = false;
            }
        }

        async function send() {
            if (closed) {
                return;
            }

            if (!input.value.trim()) {
                showError('Write a message first.');
                input.focus();
                return;
            }

            sendButton.disabled = true;
            showError('');
            try {
                const response = await fetch(form.action, { method: 'POST', body: new FormData(form), headers: ajaxHeaders });
                if (response.ok) {
                    appendMessages(await response.text(), true);
                    input.value = '';
                    input.focus();
                } else if (isAuthFailure(response)) {
                    close(sessionEndedMessage);
                } else if (response.status === 404) {
                    close('This movie no longer exists, so its chat is closed.');
                } else if (response.status === 400) {
                    showError((await response.text()) || 'The message could not be sent. Reload the page and try again.');
                } else {
                    showError('The message could not be sent (HTTP ' + response.status + '). Please try again.');
                }
            } catch {
                showError(offlineMessage);
            } finally {
                if (!closed) {
                    sendButton.disabled = false;
                }
            }
        }

        if (form) {
            form.addEventListener('submit', function (event) {
                event.preventDefault();
                send();
            });

            input.addEventListener('keydown', function (event) {
                if (event.key === 'Enter' && !event.shiftKey && !event.isComposing) {
                    event.preventDefault();
                    send();
                }
            });
        }

        document.addEventListener('visibilitychange', function () {
            if (!document.hidden) {
                poll();
            }
        });

        scrollToBottom();
        window.setInterval(poll, pollIntervalMs);
    }

    function initPasswordToggles(root) {
        root.querySelectorAll('[data-password-toggle]').forEach(function (button) {
            const input = document.getElementById(button.getAttribute('aria-controls'));
            if (!input) {
                return;
            }

            const showIcon = button.querySelector('[data-password-icon-show]');
            const hideIcon = button.querySelector('[data-password-icon-hide]');

            function setVisible(visible) {
                input.type = visible ? 'text' : 'password';
                button.setAttribute('aria-pressed', String(visible));
                button.title = visible ? 'Hide password' : 'Show password';
                showIcon.classList.toggle('d-none', visible);
                hideIcon.classList.toggle('d-none', !visible);
            }

            button.addEventListener('click', function () {
                setVisible(input.type === 'password');
                input.focus();
                const end = input.value.length;
                input.setSelectionRange(end, end);
            });

            if (input.form) {
                input.form.addEventListener('submit', function () {
                    setVisible(false);
                });
            }
        });
    }

    localizeTimes(document);
    initModals();
    initChat();
    initPasswordToggles(document);
})();
