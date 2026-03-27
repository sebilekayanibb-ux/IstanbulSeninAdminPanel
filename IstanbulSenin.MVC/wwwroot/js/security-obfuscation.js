// Production security - Developer tools protection
(function () {
    'use strict';

    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
        return;
    }

    const checkDevTools = setInterval(() => {
        const widthThreshold = window.outerWidth - window.innerWidth > 160;
        const heightThreshold = window.outerHeight - window.innerHeight > 160;

        if (widthThreshold || heightThreshold) {
            console.clear();
            console.log('%cDeveloper tools are not allowed in production.', 'color: red; font-weight: bold;');
        }
    }, 1000);

    window.addEventListener('beforeunload', () => {
        clearInterval(checkDevTools);
    });
})();
