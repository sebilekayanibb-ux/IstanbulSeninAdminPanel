// Advanced Frontend Security Layer
(function () {
    'use strict';

    const isDevelopment = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';

    // 1. DevTools Detection (Production only)
    if (!isDevelopment) {
        let devToolsOpen = false;

        const detectDevTools = () => {
            const threshold = 160;
            devToolsOpen = window.outerWidth - window.innerWidth > threshold ||
                          window.outerHeight - window.innerHeight > threshold;

            if (devToolsOpen) {
                // Developer tools açıldı - potansiyel tehdit
                logSecurityEvent('devtools_detected', {
                    width: window.innerWidth,
                    height: window.innerHeight
                });
                
                // Kritik form'ları devre dışı bırak
                disableCriticalForms();
            }
        };

        setInterval(detectDevTools, 500);

        // 2. Form Integrity Monitoring
        function monitorFormIntegrity() {
            document.querySelectorAll('form').forEach(form => {
                const originalHTML = form.innerHTML;
                const observer = new MutationObserver(() => {
                    if (form.innerHTML !== originalHTML) {
                        logSecurityEvent('form_manipulation_detected', {
                            formId: form.id,
                            formAction: form.action
                        });
                        disableCriticalForms();
                    }
                });

                observer.observe(form, {
                    childList: true,
                    subtree: true,
                    attributes: true
                });
            });
        }

        // 3. Validation Bypass Prevention
        function protectFormValidation() {
            document.querySelectorAll('input, textarea, select').forEach(input => {
                const originalSetAttribute = input.setAttribute;

                Object.defineProperty(input, 'required', {
                    get() {
                        return this.hasAttribute('required');
                    },
                    set(value) {
                        if (!value) {
                            logSecurityEvent('validation_bypass_attempt', {
                                inputName: this.name,
                                inputType: this.type
                            });
                        }
                    },
                    configurable: false
                });

                // Monitor attribute changes
                const observer = new MutationObserver((mutations) => {
                    mutations.forEach((mutation) => {
                        if (mutation.type === 'attributes' && mutation.attributeName === 'required') {
                            logSecurityEvent('required_attribute_removed', {
                                element: mutation.target.name
                            });
                        }
                    });
                });

                observer.observe(input, { attributes: true });
            });
        }

        // 4. Request Tampering Prevention
        function protectAjaxRequests() {
            const originalFetch = window.fetch;

            window.fetch = function (...args) {
                const [resource, config] = args;

                // Suspicious headers check
                if (config?.headers) {
                    const suspicious = ['x-admin-override', 'x-bypass-auth', 'x-skip-validation'];
                    const headerKeys = Object.keys(config.headers || {}).map(h => h.toLowerCase());

                    if (suspicious.some(s => headerKeys.includes(s))) {
                        logSecurityEvent('suspicious_request_headers', {
                            url: resource,
                            headers: Object.keys(config.headers)
                        });
                        return Promise.reject(new Error('Invalid request'));
                    }
                }

                // Body tampering check
                if (config?.body && typeof config.body === 'string') {
                    try {
                        const bodyObj = JSON.parse(config.body);
                        // Check for suspicious fields
                        if (bodyObj.userId || bodyObj.isAdmin || bodyObj.role) {
                            logSecurityEvent('body_tampering_detected', {
                                url: resource,
                                suspiciousFields: Object.keys(bodyObj)
                            });
                        }
                    } catch (e) {
                    }
                }

                return originalFetch.apply(this, args);
            };
        }

        function protectConsole() {
            const consoleProxy = new Proxy(console, {
                get(target, prop) {
                    if (prop === 'log' || prop === 'warn' || prop === 'error') {
                        return function (...args) {
                            logSecurityEvent('console_command', {
                                command: prop,
                                args: args.toString().substring(0, 100)
                            });

                            if (devToolsOpen) {
                                return;  // Ignore console commands
                            }
                            return target[prop].apply(target, args);
                        };
                    }
                    return target[prop];
                }
            });

            Object.defineProperty(window, 'console', {
                value: consoleProxy,
                writable: false,
                configurable: false
            });
        }


        function disableCriticalForms() {
            document.querySelectorAll('form[data-critical]').forEach(form => {
                form.style.opacity = '0.5';
                form.style.pointerEvents = 'none';
                Array.from(form.elements).forEach(input => {
                    input.disabled = true;
                    input.style.cursor = 'not-allowed';
                });
            });
        }

        function logSecurityEvent(eventType, details = {}) {
            const event = {
                type: eventType,
                timestamp: new Date().toISOString(),
                url: window.location.href,
                userAgent: navigator.userAgent,
                details: details
            };

            // Log to backend
            fetch('/api/security-log', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(event),
                credentials: 'include'
            }).catch(() => {
                // Silently fail if logging endpoint unavailable
            });

            // Local logging
            console.warn('[SECURITY]', eventType, details);
        }

  
        function verifyDOMIntegrity() {
            // Check for injected scripts
            const scripts = document.querySelectorAll('script');
            const originalScripts = new Set();

            scripts.forEach(script => {
                if (script.src === '' || !script.src) {
                    originalScripts.add(script.textContent.substring(0, 100));
                }
            });

            // Monitor for new scripts
            const observer = new MutationObserver((mutations) => {
                mutations.forEach((mutation) => {
                    if (mutation.addedNodes.length) {
                        mutation.addedNodes.forEach((node) => {
                            if (node.nodeName === 'SCRIPT') {
                                logSecurityEvent('injected_script_detected', {
                                    src: node.src,
                                    inline: !node.src
                                });
                            }
                        });
                    }
                });
            });

            observer.observe(document.body, {
                childList: true,
                subtree: true
            });
        }


        function protectStorage() {
            const sensitiveKeys = ['token', 'auth', 'password', 'secret', 'admin'];

            const handler = {
                get(target, prop) {
                    return target[prop];
                },
                set(target, prop, value) {
                    sensitiveKeys.forEach(key => {
                        if (prop.toLowerCase().includes(key)) {
                            logSecurityEvent('sensitive_data_storage', {
                                key: prop,
                                value: value.substring(0, 20) + '...'
                            });
                        }
                    });
                    target[prop] = value;
                    return true;
                }
            };

            if (window.localStorage) {
                const localStorageProxy = new Proxy(window.localStorage, handler);
                Object.defineProperty(window, 'localStorage', {
                    value: localStorageProxy,
                    writable: false
                });
            }
        }

        monitorFormIntegrity();
        protectFormValidation();
        protectAjaxRequests();
        protectConsole();
        verifyDOMIntegrity();
        protectStorage();

        // Log security initialization
        logSecurityEvent('security_layer_initialized', {
            protections: [
                'form_integrity',
                'validation_bypass_prevention',
                'ajax_tampering_prevention',
                'console_protection',
                'dom_verification',
                'storage_protection'
            ]
        });
    }
})();
