document.addEventListener("DOMContentLoaded", () => {
    // ... [slideshow code] ...

    const registerForm = document.getElementById("registerForm");
    const registerError = document.getElementById("registerError");

    // ✅ CORRECT ORDER: DEFINE FUNCTIONS FIRST
    function clearRegisterErrors() {
        registerForm.querySelectorAll("[data-valmsg-for]").forEach(el => el.innerHTML = "");
        registerForm.querySelectorAll(".is-invalid").forEach(el => el.classList.remove("is-invalid"));
        if (registerError) {
            registerError.classList.add("d-none");
            registerError.innerHTML = "";
        }
    }

    function setRegisterFieldError(field, messages) {
        const message = Array.isArray(messages) ? messages.join("<br />") : messages;
        const input = registerForm.querySelector(`[name='${field}']`);
        if (input) input.classList.add("is-invalid");
        const target = registerForm.querySelector(`[data-valmsg-for='${field}']`);
        if (target) target.innerHTML = message;
    }

    function showRegisterGlobalError(message) {
        if (!message || !registerError) return;
        registerError.classList.remove("d-none");
        registerError.innerHTML = message;
    }

    registerForm.addEventListener("submit", async function (e) {
        e.preventDefault();
        clearRegisterErrors();


        const formData = new FormData(registerForm);

        try {
            const res = await fetch("/Auth/SignUp", {
                method: "POST",
                body: formData, // ✅ SEND FORM DATA (not JSON)
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const result = await res.json(); // ✅ Now works because server returns JSON

            if (result.success) {
                window.location.href = result.redirectUrl;
            } else {
                // Handle errors from server
                Object.entries(result.errors || {}).forEach(([key, messages]) => {
                    setRegisterFieldError(key, messages);
                });
                showRegisterGlobalError("لطفاً خطاهای مشخص‌شده را برطرف کنید.");
            }
        } catch (err) {
            console.error("Registration failed:", err);
            showRegisterGlobalError("خطای سیستمی رخ داد. لطفاً دوباره تلاش کنید.");
        }
    });
});