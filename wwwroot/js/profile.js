    (function () {
    const overlay = document.getElementById("editProfileOverlay");
    const openBtn = document.getElementById("openEditProfile");
    const closeBtn = document.getElementById("closeEditProfile");
    const cancelBtn = document.getElementById("cancelEditProfile");
    const form = document.getElementById("editProfileForm");
    const alertBox = document.getElementById("profileAlert");
    const saveBtn = document.getElementById("saveProfileBtn");
    const spinner = saveBtn.querySelector(".spinner");

    function showModal() {
        overlay.hidden = false;
      // Focus first input for nicer UX
      setTimeout(() => document.getElementById("UserName")?.focus(), 0);
    document.addEventListener("keydown", onKeyDown);
    }

    function hideModal() {
        overlay.hidden = true;
    hideAlert();
    document.removeEventListener("keydown", onKeyDown);
    }

    function onKeyDown(e) {
      if (e.key === "Escape") hideModal();
    }

    function showAlert(type, message) {
        alertBox.hidden = false;
    alertBox.className = "alert " + (type === "success" ? "success" : "error");
    alertBox.textContent = message;
    }

    function hideAlert() {
        alertBox.hidden = true;
    alertBox.textContent = "";
    alertBox.className = "alert";
    }

    function setLoading(isLoading) {
        saveBtn.disabled = isLoading;
    spinner.hidden = !isLoading;
    }

    function getAntiForgeryToken() {
      const tokenInput = form.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : "";
    }

    openBtn.addEventListener("click", showModal);
    closeBtn.addEventListener("click", hideModal);
    cancelBtn.addEventListener("click", hideModal);

    // Close if user clicks backdrop
    overlay.addEventListener("click", function (e) {
      if (e.target === overlay) hideModal();
    });

    form.addEventListener("submit", async function (e) {
        e.preventDefault();
    hideAlert();
    setLoading(true);

    try {
        const formData = new FormData(form);

    const res = await fetch("/Profile/UpdateProfile", {
        method: "POST",
    body: formData,
    headers: {
        "RequestVerificationToken": getAntiForgeryToken()
          }
        });

    if (!res.ok) {
          const text = await res.text();
    showAlert("error", text || "Could not save changes.");
    return;
        }

        // Expect JSON {success: true, message?: "...", updated?: {...} }
        const data = await res.json().catch(() => null);

    showAlert("success", data?.message || "Profile updated ✅");

    // Optional: update visible UI without reload if you show name/phone outside modal.
    // Example: document.getElementById("profileName").textContent = formData.get("FullName");

    setTimeout(hideModal, 600);
      } catch (err) {
        showAlert("error", "Network error. Please try again.");
      } finally {
        setLoading(false);
      }
    });
  })();
