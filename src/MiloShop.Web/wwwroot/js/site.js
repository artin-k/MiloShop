(() => {
    const rejectKey = "cookie-consent-choice";

    const safeGet = (key) => {
        try { return localStorage.getItem(key); } catch { return null; }
    };

    const safeSet = (key, value) => {
        try { localStorage.setItem(key, value); } catch { }
    };

    document.addEventListener("DOMContentLoaded", () => {
        const banner = document.getElementById("cookieConsentBanner");
        if (!banner) return;

        if (safeGet(rejectKey) === "necessary-only") {
            banner.remove();
            return;
        }

        const acceptBtn = document.getElementById("cookieAcceptBtn");
        const rejectBtn = document.getElementById("cookieRejectBtn");

        acceptBtn?.addEventListener("click", () => {
            const cookieString = banner.dataset.cookieString;
            if (cookieString) {
                document.cookie = cookieString;
            }

            safeSet(rejectKey, "accepted");
            banner.remove();
        });

        rejectBtn?.addEventListener("click", () => {
            safeSet(rejectKey, "necessary-only");
            banner.remove();
        });
    });
})();
