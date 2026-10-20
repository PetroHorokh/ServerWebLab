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
                const full = date.toLocaleString([], { dateStyle: 'medium', timeStyle: 'short' });
                if (element.getAttribute('data-local-time') === 'date') {
                    element.textContent = date.toLocaleDateString([], { dateStyle: 'medium' });
                    element.title = full;
                } else {
                    element.textContent = full;
                }
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

    const draftKeyPrefix = 'cinema-draft:';

    function readStoredDraft(key) {
        try {
            const raw = window.sessionStorage.getItem(draftKeyPrefix + key);
            return raw ? JSON.parse(raw) : null;
        } catch {
            return null;
        }
    }

    function storeDraft(key, draft) {
        try {
            window.sessionStorage.setItem(draftKeyPrefix + key, JSON.stringify(draft));
        } catch {
        }
    }

    function discardDraft(key) {
        try {
            window.sessionStorage.removeItem(draftKeyPrefix + key);
        } catch {
        }
    }

    function discardAllDrafts() {
        try {
            Object.keys(window.sessionStorage)
                .filter(function (key) { return key.startsWith(draftKeyPrefix); })
                .forEach(function (key) { window.sessionStorage.removeItem(key); });
        } catch {
        }
    }

    function isDraftForm(form) {
        return !!form && form.hasAttribute('data-draft-key');
    }

    function isEditDraftForm(form) {
        return isDraftForm(form) && form.getAttribute('data-draft-mode') === 'edit';
    }

    function draftSignature(state) {
        const names = Object.keys(state.values).sort();
        return JSON.stringify([
            names.map(function (name) { return [name, state.values[name]]; }),
            state.checked.slice().sort()
        ]);
    }

    function sameDraftState(first, second) {
        return draftSignature(first) === draftSignature(second);
    }

    function draftFields(form) {
        return Array.from(form.elements).filter(function (field) {
            return field.name && !field.disabled && !field.hasAttribute('data-draft-ignore')
                && !['hidden', 'search', 'submit', 'button', 'password'].includes(field.type);
        });
    }

    function captureDraft(form) {
        const draft = { values: {}, checked: [] };
        draftFields(form).forEach(function (field) {
            if (field.type === 'checkbox' || field.type === 'radio') {
                if (field.checked) {
                    draft.checked.push(field.name + '=' + field.value);
                }
            } else {
                draft.values[field.name] = field.value;
            }
        });
        return draft;
    }

    function applyDraft(form, draft) {
        draftFields(form).forEach(function (field) {
            if (field.type === 'checkbox' || field.type === 'radio') {
                field.checked = draft.checked.includes(field.name + '=' + field.value);
            } else if (Object.prototype.hasOwnProperty.call(draft.values, field.name)) {
                field.value = draft.values[field.name];
            }
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

        const draftBaselines = new WeakMap();

        function showDraftNotice(form, message, variant) {
            const body = form.querySelector('.modal-body');
            if (!body) {
                return;
            }

            const notice = document.createElement('div');
            notice.className = 'alert ' + variant + ' py-2 small';
            notice.setAttribute('role', 'status');
            notice.setAttribute('data-draft-notice', '');
            notice.textContent = message;
            body.prepend(notice);
        }

        function prepareDraft(form, restoreDraft) {
            const draftKey = form.getAttribute('data-draft-key');
            const stored = readStoredDraft(draftKey);

            if (!restoreDraft) {
                draftBaselines.set(form, stored ? stored.baseline : null);
                return;
            }

            const current = captureDraft(form);
            draftBaselines.set(form, current);
            if (!stored) {
                return;
            }

            if (isEditDraftForm(form) && stored.baseline && !sameDraftState(stored.baseline, current)) {
                discardDraft(draftKey);
                showDraftNotice(form,
                    'This item was changed after you started editing it, so its current saved version is shown and your unsaved changes were discarded.',
                    'alert-warning');
                return;
            }

            applyDraft(form, stored);
            showDraftNotice(form, 'Your unsaved changes were restored. Press Close to discard them.', 'alert-info');
        }

        function render(html, restoreDraft) {
            modalContent.innerHTML = html;

            const form = modalContent.querySelector('form[data-ajax-form]');
            if (isDraftForm(form)) {
                prepareDraft(form, restoreDraft);
            }

            if (form && window.jQuery && jQuery.validator && jQuery.validator.unobtrusive) {
                jQuery.validator.unobtrusive.parse(form);
            }

            initMultiselects(modalContent);
            initPasswordToggles(modalContent);

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

                render(await response.text(), true);
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
                        if (isDraftForm(form)) {
                            discardDraft(form.getAttribute('data-draft-key'));
                        }

                        window.location.reload();
                        return;
                    }
                }

                if ((response.ok || response.status === 400) && contentType.includes('text/html')) {
                    render(await response.text(), false);
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

        function saveDraftFromEvent(event) {
            const form = event.target.closest('form');
            if (!event.target.name || !isDraftForm(form)) {
                return;
            }

            const draftKey = form.getAttribute('data-draft-key');
            const state = captureDraft(form);
            const baseline = draftBaselines.get(form) || null;
            if (baseline && sameDraftState(state, baseline)) {
                discardDraft(draftKey);
                return;
            }

            storeDraft(draftKey, { values: state.values, checked: state.checked, baseline: baseline });
        }

        modalContent.addEventListener('input', saveDraftFromEvent);
        modalContent.addEventListener('change', saveDraftFromEvent);
        modalContent.addEventListener('click', function (event) {
            const discardButton = event.target.closest('[data-draft-discard]');
            const form = discardButton && discardButton.closest('form');
            if (isDraftForm(form)) {
                discardDraft(form.getAttribute('data-draft-key'));
            }
        });

        document.querySelectorAll('form[data-clear-drafts]').forEach(function (form) {
            form.addEventListener('submit', discardAllDrafts);
        });

        modalElement.addEventListener('shown.bs.modal', focusFirstField);
        modalElement.addEventListener('hidden.bs.modal', function () {
            modalContent.innerHTML = '';
        });
    }

    function initAutoSubmitForms() {
        document.querySelectorAll('form[data-auto-submit] select').forEach(function (select) {
            select.addEventListener('change', function () {
                select.form.requestSubmit();
            });
        });
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

    window.cinema = {
        ajaxHeaders: ajaxHeaders,
        isAuthFailure: isAuthFailure,
        sessionEndedMessage: sessionEndedMessage,
        offlineMessage: offlineMessage,
        localizeTimes: localizeTimes,
    };

    localizeTimes(document);
    initModals();
    initAutoSubmitForms();
    initPasswordToggles(document);
})();
