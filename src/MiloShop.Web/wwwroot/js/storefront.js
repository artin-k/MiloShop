// Optional enhancement: every shopping link and form works without JavaScript.
document.addEventListener("click", async (event) => {
    const button = event.target.closest("[data-code]");
    if (!button) return;
    try {
        await navigator.clipboard.writeText(button.dataset.code);
        button.textContent = "کپی شد";
    } catch {
        button.textContent = button.dataset.code;
    }
});
